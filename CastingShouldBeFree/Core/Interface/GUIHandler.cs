using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.ModeHandlers;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using Microsoft.Win32.SafeHandles;
using Photon.Pun;
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
            }
        }
    }

    #endregion

    public Action<VRRig, VRRig> OnCastedRigChange;

    public int MaxSmoothing { get; private set; }

    private Dictionary<string, ModeHandlerBase> modeHandlers = new();

    private bool hasInitEventSystem;
    private bool isInSettings;

    private GameObject canvas;
    private GameObject mainPanel;
    private GameObject settingsPanel;

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
        canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        canvas.name = "Casting Should Be Free Canvas";

        canvas.transform.Find("Scoreboard").AddComponent<ScoreboardHandler>();

        leaderboardEntryPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("LeaderboardEntry");

        leaderboard = canvas.transform.Find("Leaderboard");
        mainPanel = canvas.transform.Find("MainPanel").gameObject;
        settingsPanel = canvas.transform.Find("SettingsPanel").gameObject;
        
        settingsPanel.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() =>
        {
            isInSettings = false;
            settingsPanel.SetActive(false);
            mainPanel.SetActive(true);
        });

        playerContent = mainPanel.transform.Find("Players/Viewport/Content");

        Transform playerInformation = mainPanel.transform.Find("Chin/PlayerInformation");
        currentPlayerText = playerInformation.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        isPlayerTaggedText = playerInformation.Find("IsTagged").GetComponent<TextMeshProUGUI>();

        mainPanel.transform.Find("Chin/Settings").GetComponent<Button>().onClick.AddListener(() =>
        {
            isInSettings = true;
            settingsPanel.SetActive(true);
            mainPanel.SetActive(false);
        });

        currentPlayerText.text = "No Player Selected";

        playerButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("PlayerButton");

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
            Plugin.Instance.PCCamera.GetComponent<Camera>().nearClipPlane = value;
            nearClipText.text = $"Near Clip: {value.ToString("F", CultureInfo.InvariantCulture)}";
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
        
        settingsPanel.transform.Find("AntiAFKKick").GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonNetworkController.Instance.disableAFKKick = !PhotonNetworkController.Instance.disableAFKKick;
            settingsPanel.transform.Find("AntiAFKKick").GetComponentInChildren<TextMeshProUGUI>().text =
                $"Anti AFK Kick\n{(PhotonNetworkController.Instance.disableAFKKick ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
        });

        settingsPanel.transform.Find("Leaderboard").GetComponent<Button>().onClick.AddListener(() =>
        {
            leaderboard.gameObject.SetActive(!leaderboard.gameObject.activeSelf);
            settingsPanel.transform.Find("Leaderboard").GetComponentInChildren<TextMeshProUGUI>().text =
                $"Leaderboard\n{(leaderboard.gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
        });
        
        settingsPanel.transform.Find("Scoreboard").GetComponent<Button>().onClick.AddListener(() =>
        {
            ScoreboardHandler.Instance.gameObject.SetActive(!ScoreboardHandler.Instance.gameObject.activeSelf);
            settingsPanel.transform.Find("Scoreboard").GetComponentInChildren<TextMeshProUGUI>().text =
                $"Scoreboard\n{(ScoreboardHandler.Instance.gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
        });

        settingsPanel.transform.Find("MiniMap").GetComponent<Button>().onClick.AddListener(() =>
        {
            canvas.transform.Find("MiniMap").gameObject.SetActive(!canvas.transform.Find("MiniMap").gameObject.activeSelf);
            settingsPanel.transform.Find("MiniMap").GetComponentInChildren<TextMeshProUGUI>().text =
                $"Mini Map\n{(canvas.transform.Find("MiniMap").gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
        });

        RigUtils.OnRigSpawned += OnRigSpawned;
        RigUtils.OnRigCached += OnRigCached;
        RigUtils.OnRigNameChange += UpdatePlayerName;
        RigUtils.OnMatIndexChange += UpdatePlayerTagState;
        RigUtils.OnRigColourChange += UpdatePlayerColour;

        if (VRRig.LocalRig != null)
            OnRigSpawned(VRRig.LocalRig);
        
        canvas.SetActive(false);

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

        RenderTexture miniMapRenderTexture =
            Instantiate(Plugin.Instance.CastingBundle.LoadAsset<RenderTexture>("MiniMapRenderTexture"));
        canvas.transform.Find("MiniMap").gameObject.SetActive(true);
        canvas.transform.Find("MiniMap").GetComponent<RawImage>().texture = miniMapRenderTexture;

        miniMapCamera = new GameObject("hi im a miinimap camera so cool").AddComponent<Camera>();
        miniMapCamera.fieldOfView = 100;
        miniMapCamera.orthographic = true;
        miniMapCamera.targetTexture = miniMapRenderTexture;
    }

    private void OnGUI()
    {
        if (hasInitEventSystem)
            return;

        GUI.Label(new Rect(0f, 0f, 500f, 100f), "Press 'C' to Open the Casting GUI!");
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.C))
        {
            if (!hasInitEventSystem)
            {
                hasInitEventSystem = true;
                canvas.SetActive(true);
            }
            else
            {
                if (isInSettings)
                    settingsPanel.SetActive(!settingsPanel.activeSelf);
                else
                    mainPanel.SetActive(!mainPanel.activeSelf);
            }
        }

        for (int i = 0; i <= 9; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (UnityInput.Current.GetKeyDown(key) && SetColourPatch.SpawnedRigs.Count > i)
            {
                ChangePlayer(SetColourPatch.SpawnedRigs[i]);
                break;
            }
        }
    }

    private void OnRigSpawned(VRRig rig)
    {
        GameObject button = Instantiate(playerButtonPrefab, playerContent);
        button.GetComponent<Button>().onClick.AddListener(() => ChangePlayer(rig));
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

    private void ChangePlayer(VRRig rig)
    {
        string playerName = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : rig.playerText1.text;
        currentPlayerText.text = $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
        isPlayerTaggedText.text =
            $"Is Tagged? {(rig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";

        miniMapCamera.transform.SetParent(rig.bodyRenderer.transform);
        miniMapCamera.transform.localPosition = new Vector3(0f, 20f, 0f);
        miniMapCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);

        CastedRig = rig;
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