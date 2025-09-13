using System.IO;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.Interface;
using HarmonyLib;
using UnityEngine;

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
        Destroy(PCCamera.GetChild(0).gameObject);
        
        GameObject cameraPrefab = CastingBundle.LoadAsset<GameObject>("CardboardCamera");
        GameObject camera = Instantiate(cameraPrefab);
        camera.AddComponent<CameraHandler>();
        camera.name = "CardboardCamera";
        Destroy(cameraPrefab);

        GameObject componentHolder = new GameObject("Casting Should Be Free");
        componentHolder.AddComponent<GUIHandler>();
    }
}