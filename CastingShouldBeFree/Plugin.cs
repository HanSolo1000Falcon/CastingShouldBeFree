using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Nametags;
using CastingShouldBeFree.Utils;
using CastingShouldBeFree.Version_Checking;
using GorillaNetworking;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace CastingShouldBeFree;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const  string GorillaInfoURL = "https://raw.githubusercontent.com/HanSolo1000Falcon/GorillaInfo/main/";
    public static Plugin Instance { get; private set; }

    public AssetBundle CastingBundle { get; private set; }

    public Transform PCCamera { get; private set; }

    public TMP_FontAsset CasterFontBold { get; private set; }

    public Shader TMP_DistanceField { get; private set; }

    public Dictionary<string, string> KnownMods   { get; private set; }
    public Dictionary<string, string> KnownCheats { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);

        Harmony harmony = new(Constants.PluginGuid);
        harmony.PatchAll();
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.I))
            PhotonNetworkController.Instance.disableAFKKick = true;
    }

    private void OnGameInitialized()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly()
                                      .GetManifestResourceStream("CastingShouldBeFree.Resources.castingshouldbefree");

        CastingBundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();

        CasterFontBold = Instantiate(CastingBundle.LoadAsset<TMP_FontAsset>("JetBrainsMono-Bold SDF"));

        TMP_DistanceField              = Shader.Find("TextMeshPro/Mobile/Distance Field");
        CasterFontBold.material.shader = TMP_DistanceField;

        PCCamera = GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0);

        GameObject cameraPrefab = CastingBundle.LoadAsset<GameObject>("CardboardCamera");
        GameObject camera       = Instantiate(cameraPrefab);
        camera.AddComponent<CameraHandler>();
        camera.name = "CardboardCamera";
        Destroy(cameraPrefab);

        GameObject componentHolder = new("Casting Should Be Free");
        componentHolder.AddComponent<VersionChecker>();
        componentHolder.AddComponent<TagManager>();
        componentHolder.AddComponent<CoreHandler>();
        componentHolder.AddComponent<NametagHandler>();
        componentHolder.AddComponent<AutoCaster>();

        using HttpClient    httpClient          = new();
        HttpResponseMessage knownModsResponse   = httpClient.GetAsync(GorillaInfoURL + "KnownMods.txt").Result;
        HttpResponseMessage knownCheatsResponse = httpClient.GetAsync(GorillaInfoURL + "KnownCheats.txt").Result;

        knownModsResponse.EnsureSuccessStatusCode();
        knownCheatsResponse.EnsureSuccessStatusCode();

        using (Stream stream = knownModsResponse.Content.ReadAsStreamAsync().Result)
        {
            using (StreamReader reader = new(stream))
            {
                KnownMods = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            }
        }

        using (Stream stream = knownCheatsResponse.Content.ReadAsStreamAsync().Result)
        {
            using (StreamReader reader = new(stream))
            {
                KnownCheats = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            }
        }

        RigUtils.OnRigCosmeticsLoad += rig =>
                                       {
                                           Nametag nametag = rig.GetComponent<Nametag>();

                                           if (nametag is not null)
                                               nametag.UpdatePlayerPlatform();
                                       };
    }
}