using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.Interface.Panel_Handlers;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface;

public class GUIHandler : Singleton<GUIHandler>
{
    public bool HasInitEventSystem;

    public TextMeshProUGUI FOVText;
    public TextMeshProUGUI NearClipText;
    public TextMeshProUGUI SmoothingText;

    public           Slider                        FOVSlider;
    public           Slider                        NearClipSlider;
    public           Slider                        SmoothingSlider;
    private readonly Dictionary<VRRig, GameObject> leaderboardEntries = new();

    private readonly Dictionary<VRRig, GameObject> rigButtons = new();
    private          TextMeshProUGUI               currentModeText;

    private TextMeshProUGUI currentPlayerText;

    private float           initTime;
    private TextMeshProUGUI isPlayerTaggedText;
    private Transform       leaderboard;
    private GameObject      leaderboardEntryPrefab;

    private GameObject mainPanel;

    private Camera miniMapCamera;

    private GameObject playerButtonPrefab;

    private Transform  playerContent;
    public  GameObject Canvas { get; private set; }

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("Canvas");
        Canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        Canvas.name = "Casting Should Be Free Canvas";

        Canvas.transform.Find("ClosestLava").AddComponent<ClosestTaggedHandler>();

        leaderboardEntryPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("LeaderboardEntry");

        leaderboard = Canvas.transform.Find("Leaderboard");
        mainPanel   = Canvas.transform.Find("MainPanel").gameObject;

        playerContent = mainPanel.transform.Find("Players/Viewport/Content");

        SetUpPlayerInformation(mainPanel);
        SetUpCameraSettings(mainPanel);

        playerButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("PlayerButton");

        if (SetColourPatch.SpawnedRigs.Contains(VRRig.LocalRig))
            OnRigSpawned(VRRig.LocalRig);

        RigUtils.OnRigSpawned      += OnRigSpawned;
        RigUtils.OnRigCached       += OnRigCached;
        RigUtils.OnRigNameChange   += UpdatePlayerName;
        RigUtils.OnMatIndexChange  += UpdatePlayerTagState;
        RigUtils.OnRigColourChange += UpdatePlayerColour;

        CoreHandler.Instance.OnCastedRigChange += OnCastedRigChange;
        CoreHandler.Instance.OnCurrentHandlerChange +=
                handlerName => currentModeText.text = $"Current Mode: {handlerName}";

        SetUpOtherPanels(mainPanel);

        Canvas.SetActive(false);

        GameObject modeButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("CameraModeButton");
        Transform  modeContent      = mainPanel.transform.Find("CameraModes/Viewport/Content");

        foreach (KeyValuePair<string, ModeHandlerBase> modeHandlerPair in CoreHandler.Instance.ModeHandlers)
        {
            GameObject modeButton = Instantiate(modeButtonPrefab, modeContent);
            modeButton.GetComponentInChildren<TextMeshProUGUI>().text = modeHandlerPair.Value.HandlerName;
            modeButton.GetComponent<Button>().onClick
                      .AddListener(() => CoreHandler.Instance.SetCurrentHandler(modeHandlerPair.Value.HandlerName));
        }

        CoreHandler.Instance.CurrentHandlerName = PlayerPrefs.GetString(CoreHandler.CurrentHandlerKey,
                FirstPersonModeHandler.HandlerNameStatic());

        RenderTexture miniMapRenderTexture =
                Instantiate(Plugin.Instance.CastingBundle.LoadAsset<RenderTexture>("MiniMapRenderTexture"));

        Canvas.transform.Find("MiniMap").GetComponent<RawImage>().texture = miniMapRenderTexture;

        miniMapCamera               = new GameObject("hi im a miinimap camera so cool").AddComponent<Camera>();
        miniMapCamera.fieldOfView   = 100;
        miniMapCamera.nearClipPlane = 10f;
        miniMapCamera.orthographic  = true;
        miniMapCamera.targetTexture = miniMapRenderTexture;

        initTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - initTime < 5f)
            return;

        if (UnityInput.Current.GetKeyDown(KeyCode.C))
        {
            if (!HasInitEventSystem)
                InitEventSystem();
            else
                mainPanel.SetActive(!mainPanel.activeSelf);
        }

        if (UnityInput.Current.GetKeyDown(KeyCode.P))
        {
            string firstPersonHandlerName = FirstPersonModeHandler.HandlerNameStatic();
            CoreHandler.Instance.CurrentHandlerName = CoreHandler.Instance.CurrentHandlerName == firstPersonHandlerName
                                                              ? ThirdPersonHandler.HandlerNameStatic()
                                                              : firstPersonHandlerName;
        }

        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (UnityInput.Current.GetKeyDown(key) && SetColourPatch.SpawnedRigs.Count > i)
            {
                CoreHandler.Instance.CastedRig = SetColourPatch.SpawnedRigs[i];

                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (CameraHandler.Instance != null)
        {
            miniMapCamera.transform.position = CameraHandler.Instance.transform.position + Vector3.up * 17f;
            miniMapCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);
        }
    }

    private void OnGUI()
    {
        if (HasInitEventSystem || Time.time - initTime < 5f)
            return;

        GUI.Label(new Rect(0f, 0f, 500f, 100f), "Press 'C' to Open the Casting GUI");
    }

    private void SetUpCameraSettings(GameObject mainPanel)
    {
        Transform fovPanel = mainPanel.transform.Find("FOVPanel");
        FOVSlider = fovPanel.GetComponentInChildren<Slider>();
        FOVText   = fovPanel.GetComponentInChildren<TextMeshProUGUI>();

        CoreHandler.Instance.MaxFOV = (int)FOVSlider.maxValue;
        CoreHandler.Instance.MinFOV = (int)FOVSlider.minValue;

        FOVSlider.onValueChanged.AddListener(value => CoreHandler.Instance.SetFOV((int)value));

        Transform nearClipPanel = mainPanel.transform.Find("NearClipPanel");
        NearClipSlider = nearClipPanel.GetComponentInChildren<Slider>();
        NearClipText   = nearClipPanel.GetComponentInChildren<TextMeshProUGUI>();

        NearClipSlider.onValueChanged.AddListener(value => CoreHandler.Instance.SetNearClip((int)value));

        CoreHandler.Instance.MaxNearClip = (int)NearClipSlider.maxValue;
        CoreHandler.Instance.MinNearClip = (int)NearClipSlider.minValue;

        Transform smoothingPanel = mainPanel.transform.Find("SmoothingPanel");
        SmoothingSlider = smoothingPanel.GetComponentInChildren<Slider>();
        SmoothingText   = smoothingPanel.GetComponentInChildren<TextMeshProUGUI>();

        currentModeText = mainPanel.transform.Find("CurrentMode").GetComponent<TextMeshProUGUI>();

        CoreHandler.Instance.MaxSmoothing = (int)SmoothingSlider.maxValue;
        CoreHandler.Instance.MinSmoothing = (int)SmoothingSlider.minValue;

        SmoothingSlider.onValueChanged.AddListener(value => CoreHandler.Instance.SetSmoothing((int)value));
    }
    
    private void SetUpOtherPanels(GameObject mainPanel)
    {
        Transform panelButtonContent = mainPanel.transform.Find("Chin/Panels/Viewport/Content");
        Transform panels             = Canvas.transform.Find("Panels");
        Type[] panelHandlers = Assembly.GetExecutingAssembly().GetTypes()
                                       .Where(t => !t.IsAbstract || t.IsClass ||
                                                   typeof(PanelHandlerBase).IsAssignableFrom(t)).ToArray();

        foreach (Transform panel in panels)
        {
            panel.gameObject.SetActive(true);
            panel.AddComponent<DraggableUI>();
            panel.Find("Exit").GetComponent<Button>().onClick.AddListener(() => panel.gameObject.SetActive(false));
            Type chosenType =
                    panelHandlers.FirstOrDefault(type => type.Name ==
                                                         panel.gameObject.name.Replace(" ", "")[..^5] + "Handler");

            panel.gameObject.AddComponent(chosenType);
        }

        foreach (Transform panelButton in panelButtonContent)
            panelButton.GetComponent<Button>().onClick.AddListener(() =>
                                                                   {
                                                                       GameObject associatedPanel =
                                                                               panels.Find(
                                                                                       panelButton.gameObject.name +
                                                                                       "Panel").gameObject;

                                                                       associatedPanel.transform.localPosition =
                                                                               Vector3.zero;

                                                                       associatedPanel.SetActive(
                                                                               !associatedPanel.activeSelf);
                                                                   });
    }

    private void SetUpPlayerInformation(GameObject mainPanel)
    {
        Transform playerInformation = mainPanel.transform.Find("Chin/PlayerInformation");
        currentPlayerText  = playerInformation.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        isPlayerTaggedText = playerInformation.Find("IsTagged").GetComponent<TextMeshProUGUI>();

        currentPlayerText.text = "No Player Selected";

        Transform moreInfo = Canvas.transform.Find("PlayerInfoPanel");
        moreInfo.AddComponent<DraggableUI>();
        moreInfo.AddComponent<MoreInfoHandler>();
        moreInfo.Find("Exit").GetComponent<Button>().onClick.AddListener(() => moreInfo.gameObject.SetActive(false));

        playerInformation.Find("MoreInfo").GetComponent<Button>().onClick.AddListener(() =>
        {
            moreInfo.localPosition = Vector3.zero;
            moreInfo.gameObject.SetActive(!moreInfo.gameObject.activeSelf);
        });
    }

    public void InitEventSystem()
    {
        HasInitEventSystem             = true;
        CoreHandler.Instance.CastedRig = VRRig.LocalRig;
        Canvas.SetActive(true);
        CameraHandler.Instance.Initialize();
        WorldSpaceHandler.Instance.SetUpRenderTexture();
        FOVSlider.onValueChanged?.Invoke(PlayerPrefs.GetInt(CoreHandler.FovKey,             100));
        NearClipSlider.onValueChanged?.Invoke(PlayerPrefs.GetInt(CoreHandler.NearClipKey,   1));
        SmoothingSlider.onValueChanged?.Invoke(PlayerPrefs.GetInt(CoreHandler.SmoothingKey, 18));
    }

    private void OnRigSpawned(VRRig rig)
    {
        GameObject button = Instantiate(playerButtonPrefab, playerContent);
        button.GetComponent<Button>().onClick.AddListener(() => CoreHandler.Instance.CastedRig = rig);
        button.GetComponentInChildren<TextMeshProUGUI>().text = rig.OwningNetPlayer?.NickName;
        rigButtons[rig]                                       = button;

        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboard);
        leaderboardEntry.GetComponentInChildren<TextMeshProUGUI>().text =
                $"{SetColourPatch.SpawnedRigs.Count - 1}.{rig.OwningNetPlayer?.NickName}";

        leaderboardEntry.transform.Find("ColourPanel").GetComponent<Image>().color = rig.playerColor;
        leaderboardEntries[rig]                                                    = leaderboardEntry;
    }

    private void OnRigCached(VRRig rig)
    {
        if (rigButtons.ContainsKey(rig))
        {
            Destroy(rigButtons[rig]);
            rigButtons.Remove(rig);
        }

        if (CoreHandler.Instance.CastedRig == rig)
            CoreHandler.Instance.CastedRig = VRRig.LocalRig;

        if (leaderboardEntries.ContainsKey(rig))
        {
            Destroy(leaderboardEntries[rig]);
            leaderboardEntries.Remove(rig);
        }

        foreach (KeyValuePair<VRRig, GameObject> kvp in leaderboardEntries)
            kvp.Value.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{SetColourPatch.SpawnedRigs.IndexOf(kvp.Key)}.{(kvp.Key.OwningNetPlayer != null ? kvp.Key.OwningNetPlayer.NickName : kvp.Key.playerText1.text)}";
    }

    private void UpdatePlayerName(VRRig rig, string playerName)
    {
        if (rigButtons.TryGetValue(rig, out GameObject button))
            button.GetComponentInChildren<TextMeshProUGUI>().text = playerName;

        if (CoreHandler.Instance.CastedRig == rig)
            currentPlayerText.text =
                    $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";

        if (leaderboardEntries.TryGetValue(rig, out GameObject entry))
            entry.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{SetColourPatch.SpawnedRigs.IndexOf(rig)}.{playerName}";
    }

    private void UpdatePlayerTagState(VRRig rig)
    {
        if (leaderboardEntries.TryGetValue(rig, out GameObject button))
            button.transform.Find("ColourPanel").GetComponent<Image>().color =
                    rig.IsTagged() ? new Color(1f, 0.3288f, 0f, 1f) : rig.playerColor;

        if (CoreHandler.Instance.CastedRig == rig)
            isPlayerTaggedText.text =
                    $"Is Tagged? {(rig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
    }

    private void UpdatePlayerColour(VRRig rig, Color colour)
    {
        if (leaderboardEntries.TryGetValue(rig, out GameObject button))
            button.transform.Find("ColourPanel").GetComponent<Image>().color =
                    rig.IsTagged() ? new Color(1f, 0.3288f, 0f, 1f) : rig.playerColor;

        if (CoreHandler.Instance.CastedRig == rig)
        {
            string playerName = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : rig.playerText1.text;
            currentPlayerText.text =
                    $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
        }
    }

    private void OnCastedRigChange(VRRig currentRig, VRRig lastRig)
    {
        string playerName = currentRig.OwningNetPlayer != null
                                    ? currentRig.OwningNetPlayer.NickName
                                    : currentRig.playerText1.text;

        currentPlayerText.text =
                $"Name: <color=#{ColorUtility.ToHtmlStringRGB(currentRig.playerColor)}>{playerName}</color>";

        isPlayerTaggedText.text =
                $"Is Tagged? {(currentRig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
    }
}