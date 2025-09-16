using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.Interface.PanelHandlers;
using CastingShouldBeFree.Core.ModeHandlers;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface;

public class GUIHandler : Singleton<GUIHandler>
{
    #region Backing Fields

    private VRRig _castedRig;
    private string _currentHandlerName;

    #endregion

    #region Getters and Setters

    public VRRig CastedRig
    {
        get => _castedRig;
        set
        {
            if (_castedRig != value)
            {
                string playerName = value.OwningNetPlayer != null ? value.OwningNetPlayer.NickName : value.playerText1.text;
                currentPlayerText.text = $"Name: <color=#{ColorUtility.ToHtmlStringRGB(value.playerColor)}>{playerName}</color>";
                isPlayerTaggedText.text =
                    $"Is Tagged? {(value.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
                OnCastedRigChange?.Invoke(value, _castedRig);
                _castedRig = value;
            }
        }
    }

    public string CurrentHandlerName
    {
        get => _currentHandlerName;
        set
        {
            if (_currentHandlerName != value)
            {
                _currentHandlerName = value;
                currentModeText.text = $"Current Mode: {value}";

                foreach (string modeHandlerName in modeHandlers.Keys)
                    modeHandlers[modeHandlerName].enabled = modeHandlerName == value;
                
                OnCurrentHandlerChange?.Invoke(value);
            }
        }
    }

    #endregion

    public Action<string> OnCurrentHandlerChange;
    public Action<VRRig, VRRig> OnCastedRigChange;

    public int MaxSmoothing { get; private set; }

    public GameObject Canvas { get; private set; }

    private Dictionary<string, ModeHandlerBase> modeHandlers = new();

    private bool hasInitEventSystem;

    private float initTime;

    private GameObject mainPanel;

    private Camera miniMapCamera;

    private Transform playerContent;
    private Transform leaderboard;

    private TextMeshProUGUI currentPlayerText;
    private TextMeshProUGUI isPlayerTaggedText;
    private TextMeshProUGUI currentModeText;

    private GameObject playerButtonPrefab;
    private GameObject leaderboardEntryPrefab;

    private Dictionary<VRRig, GameObject> rigButtons = new();
    private Dictionary<VRRig, GameObject> leaderboardEntries = new();

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("Canvas");
        Canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        Canvas.name = "Casting Should Be Free Canvas";

        Canvas.transform.Find("Scoreboard").AddComponent<ScoreboardHandler>();
        Canvas.transform.Find("ClosestLava").AddComponent<ClosestTaggedHandler>();

        leaderboardEntryPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("LeaderboardEntry");

        leaderboard = Canvas.transform.Find("Leaderboard");
        mainPanel = Canvas.transform.Find("MainPanel").gameObject;

        playerContent = mainPanel.transform.Find("Players/Viewport/Content");

        SetUpPlayerInformation(mainPanel);
        SetUpCameraSettings(mainPanel);

        playerButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("PlayerButton");

        if (SetColourPatch.SpawnedRigs.Contains(VRRig.LocalRig))
            OnRigSpawned(VRRig.LocalRig);

        RigUtils.OnRigSpawned += OnRigSpawned;
        RigUtils.OnRigCached += OnRigCached;
        RigUtils.OnRigNameChange += UpdatePlayerName;
        RigUtils.OnMatIndexChange += UpdatePlayerTagState;
        RigUtils.OnRigColourChange += UpdatePlayerColour;

        SetUpOtherPanels(mainPanel);

        Canvas.SetActive(false);

        GameObject modeHandlersComponents = new GameObject("Casting Should Be Free Mode Handlers");

        GameObject modeButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("CameraModeButton");
        Transform modeContent = mainPanel.transform.Find("CameraModes/Viewport/Content");

        Type[] modeHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract && typeof(ModeHandlerBase).IsAssignableFrom(type)).ToArray();

        foreach (Type modeHandlerType in modeHandlerTypes)
        {
            Component modeHandlerComponent = modeHandlersComponents.AddComponent(modeHandlerType);

            if (modeHandlerComponent is ModeHandlerBase modeHandler)
            {
                modeHandler.enabled = false;
                modeHandlers[modeHandler.HandlerName] = modeHandler;

                GameObject modeButton = Instantiate(modeButtonPrefab, modeContent);
                modeButton.GetComponentInChildren<TextMeshProUGUI>().text = modeHandler.HandlerName;
                modeButton.GetComponent<Button>().onClick
                    .AddListener(() => CurrentHandlerName = modeHandler.HandlerName);
            }
            else
            {
                Debug.Log(modeHandlerType.Name + " isn't a mode handler, removing.");
                Destroy(modeHandlerComponent);
            }
        }

        CurrentHandlerName = FirstPersonModeHandler.HandlerNameStatic();

        RenderTexture miniMapRenderTexture =
            Instantiate(Plugin.Instance.CastingBundle.LoadAsset<RenderTexture>("MiniMapRenderTexture"));
        Canvas.transform.Find("MiniMap").gameObject.SetActive(true);
        Canvas.transform.Find("MiniMap").GetComponent<RawImage>().texture = miniMapRenderTexture;

        miniMapCamera = new GameObject("hi im a miinimap camera so cool").AddComponent<Camera>();
        miniMapCamera.fieldOfView = 100;
        miniMapCamera.nearClipPlane = 10f;
        miniMapCamera.orthographic = true;
        miniMapCamera.targetTexture = miniMapRenderTexture;

        initTime = Time.time;
    }

    private void SetUpCameraSettings(GameObject mainPanel)
    {
        Transform fovPanel = mainPanel.transform.Find("FOVPanel");
        Slider fovSlider = fovPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI fovText = fovPanel.GetComponentInChildren<TextMeshProUGUI>();

        fovSlider.onValueChanged.AddListener((value) =>
        {
            Plugin.Instance.PCCamera.GetComponent<Camera>().fieldOfView = value;
            fovText.text = $"FOV: {value:N0}";
        });

        Transform nearClipPanel = mainPanel.transform.Find("NearClipPanel");
        Slider nearClipSlider = nearClipPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI nearClipText = nearClipPanel.GetComponentInChildren<TextMeshProUGUI>();

        nearClipSlider.onValueChanged.AddListener((value) =>
        {
            Plugin.Instance.PCCamera.GetComponent<Camera>().nearClipPlane = value / 100f;
            nearClipText.text = $"Near Clip: {value.ToString("N0", CultureInfo.InvariantCulture)}";
        });

        Transform smoothingPanel = mainPanel.transform.Find("SmoothingPanel");
        Slider smoothingSlider = smoothingPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI smoothingText = smoothingPanel.GetComponentInChildren<TextMeshProUGUI>();

        currentModeText = mainPanel.transform.Find("CurrentMode").GetComponent<TextMeshProUGUI>();

        MaxSmoothing = (int)smoothingSlider.maxValue + 1;

        smoothingSlider.onValueChanged.AddListener((value) =>
        {
            CameraHandler.Instance.SmoothingFactor = (int)value;
            smoothingText.text = $"Smoothing: {value:N0}";
        });

        fovSlider.onValueChanged?.Invoke(fovSlider.value);
        nearClipSlider.onValueChanged?.Invoke(nearClipSlider.value);
        smoothingSlider.onValueChanged?.Invoke(smoothingSlider.value);
    }

    private void SetUpOtherPanels(GameObject mainPanel)
    {
        Transform panelButtonContent = mainPanel.transform.Find("Chin/Panels/Viewport/Content");
        Transform panels = Canvas.transform.Find("Panels");

        foreach (Transform panel in panels)
        {
            panel.AddComponent<DraggableUI>();
            panel.Find("Exit").GetComponent<Button>().onClick.AddListener(() => panel.gameObject.SetActive(false));

            switch (panel.gameObject.name)
            {
                case "SettingsPanel":
                    panel.AddComponent<SettingsHandler>();
                    break;

                case "RoomStuffPanel":
                    panel.AddComponent<RoomStuffHandler>();
                    break;
            }
        }

        foreach (Transform panelButton in panelButtonContent)
        {
            panelButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject associatedPanel = panels.Find(panelButton.gameObject.name + "Panel").gameObject;
                associatedPanel.transform.localPosition = Vector3.zero;
                associatedPanel.SetActive(!associatedPanel.activeSelf);
            });
        }
    }

    private void SetUpPlayerInformation(GameObject mainPanel)
    {
        Transform playerInformation = mainPanel.transform.Find("Chin/PlayerInformation");
        currentPlayerText = playerInformation.Find("PlayerName").GetComponent<TextMeshProUGUI>();
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

    private void OnEventSysemInit()
    {
        CastedRig = VRRig.LocalRig;
    }

    private void OnGUI()
    {
        if (hasInitEventSystem || Time.time - initTime < 5f)
            return;

        GUI.Label(new Rect(0f, 0f, 500f, 100f), "Press 'C' to Open the Casting GUI");
    }

    private void Update()
    {
        if (Time.time - initTime < 5f)
            return;

        if (UnityInput.Current.GetKeyDown(KeyCode.C))
        {
            if (!hasInitEventSystem)
            {
                hasInitEventSystem = true;
                OnEventSysemInit();
                Canvas.SetActive(true);
            }
            else
            {
                mainPanel.SetActive(!mainPanel.activeSelf);
            }
        }

        if (UnityInput.Current.GetKeyDown(KeyCode.P))
        {
            string firstPersonHandlerName = FirstPersonModeHandler.HandlerNameStatic();
            CurrentHandlerName = (CurrentHandlerName == firstPersonHandlerName ? ThirdPersonHandler.HandlerNameStatic() : firstPersonHandlerName);
        }

        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (UnityInput.Current.GetKeyDown(key) && SetColourPatch.SpawnedRigs.Count > i)
            {
                CastedRig = SetColourPatch.SpawnedRigs[i];
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

    private void OnRigSpawned(VRRig rig)
    {
        GameObject button = Instantiate(playerButtonPrefab, playerContent);
        button.GetComponent<Button>().onClick.AddListener(() => CastedRig = rig);
        button.GetComponentInChildren<TextMeshProUGUI>().text = rig.OwningNetPlayer?.NickName;
        rigButtons[rig] = button;

        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboard);
        leaderboardEntry.GetComponentInChildren<TextMeshProUGUI>().text =
            $"{SetColourPatch.SpawnedRigs.Count - 1}.{rig.OwningNetPlayer?.NickName}";
        leaderboardEntry.transform.Find("ColourPanel").GetComponent<Image>().color = rig.playerColor;
        leaderboardEntries[rig] = leaderboardEntry;
    }

    private void OnRigCached(VRRig rig)
    {
        if (rigButtons.ContainsKey(rig))
        {
            Destroy(rigButtons[rig]);
            rigButtons.Remove(rig);
        }

        if (CastedRig == rig)
            CastedRig = VRRig.LocalRig;

        if (leaderboardEntries.ContainsKey(rig))
        {
            Destroy(leaderboardEntries[rig]);
            leaderboardEntries.Remove(rig);
        }

        foreach (var kvp in leaderboardEntries)
        {
            kvp.Value.GetComponentInChildren<TextMeshProUGUI>().text =
                $"{SetColourPatch.SpawnedRigs.IndexOf(kvp.Key)}.{(kvp.Key.OwningNetPlayer != null ? kvp.Key.OwningNetPlayer.NickName : kvp.Key.playerText1.text)}";
        }
    }

    private void UpdatePlayerName(VRRig rig, string playerName)
    {
        if (rigButtons.TryGetValue(rig, out GameObject button))
            button.GetComponentInChildren<TextMeshProUGUI>().text = playerName;

        if (CastedRig == rig)
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

        if (CastedRig == rig)
            isPlayerTaggedText.text =
                $"Is Tagged? {(rig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
    }

    private void UpdatePlayerColour(VRRig rig, Color colour)
    {
        if (leaderboardEntries.TryGetValue(rig, out GameObject button))
            button.transform.Find("ColourPanel").GetComponent<Image>().color =
                rig.IsTagged() ? new Color(1f, 0.3288f, 0f, 1f) : rig.playerColor;

        if (CastedRig == rig)
        {
            string playerName = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : rig.playerText1.text;
            currentPlayerText.text =
                $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
        }
    }
}