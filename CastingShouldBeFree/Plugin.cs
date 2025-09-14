using System.IO;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Nametags;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace CastingShouldBeFree;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }

    public AssetBundle CastingBundle { get; private set; }

    public Transform PCCamera { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);

        Harmony harmony = new(Constants.PluginGuid);
        harmony.PatchAll();
    }

    private void OnGameInitialized()
    {
        Stream bundleStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CastingShouldBeFree.Resources.castingshouldbefree");
        CastingBundle = AssetBundle.LoadFromStream(bundleStream);
        bundleStream?.Close();

        PCCamera = GorillaTagger.Instance.thirdPersonCamera.transform.GetChild(0);
        
        foreach (Transform child in PCCamera)
            Destroy(child.gameObject);

        if (!XRSettings.isDeviceActive)
            PCCamera.AddComponent<AudioListener>();
        
        GameObject cameraPrefab = CastingBundle.LoadAsset<GameObject>("CardboardCamera");
        GameObject camera = Instantiate(cameraPrefab);
        camera.AddComponent<CameraHandler>();
        camera.name = "CardboardCamera";
        Destroy(cameraPrefab);

        GameObject componentHolder = new GameObject("Casting Should Be Free");
        componentHolder.AddComponent<TagManager>();
        componentHolder.AddComponent<GUIHandler>();
        componentHolder.AddComponent<NametagHandler>();
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.I))
            PhotonNetworkController.Instance.disableAFKKick = true;
    }
}