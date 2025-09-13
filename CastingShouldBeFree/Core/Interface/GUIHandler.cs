using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using CastingShouldBeFree.Core.ModeHandlers;
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

    private GameObject canvas;
    private Transform playerContent;

    private TextMeshProUGUI currentPlayerText;
    private TextMeshProUGUI isPlayerTaggedText;
    private TextMeshProUGUI currentModeText;

    private GameObject playerButtonPrefab;

    private Dictionary<VRRig, GameObject> rigButtons = new();

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("Canvas");
        canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        canvas.name = "Casting Should Be Free Canvas";

        playerContent = canvas.transform.Find("MainPanel/Players/Viewport/Content");

        Transform playerInformation = canvas.transform.Find("MainPanel/Chin/PlayerInformation");
        currentPlayerText = playerInformation.Find("PlayerName").GetComponent<TextMeshProUGUI>();
        isPlayerTaggedText = playerInformation.Find("IsTagged").GetComponent<TextMeshProUGUI>();

        currentPlayerText.text = "No Player Selected";

        playerButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("PlayerButton");

        Transform fovPanel = canvas.transform.Find("MainPanel/FOVPanel");
        Slider fovSlider = fovPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI fovText = fovPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        fovSlider.onValueChanged.AddListener((value) =>
        {
            Plugin.Instance.PCCamera.GetComponent<Camera>().fieldOfView = value;
            fovText.text = $"FOV: {value:N0}";
        });

        Transform nearClipPanel = canvas.transform.Find("MainPanel/NearClipPanel");
        Slider nearClipSlider = nearClipPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI nearClipText = nearClipPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        nearClipSlider.onValueChanged.AddListener((value) =>
        {
            Plugin.Instance.PCCamera.GetComponent<Camera>().nearClipPlane = value;
            nearClipText.text = $"Near Clip: {value:F}";
        });

        Transform smoothingPanel = canvas.transform.Find("MainPanel/SmoothingPanel");
        Slider smoothingSlider = smoothingPanel.GetComponentInChildren<Slider>();
        TextMeshProUGUI smoothingText = smoothingPanel.GetComponentInChildren<TextMeshProUGUI>();

        currentModeText = canvas.transform.Find("MainPanel/CurrentMode").GetComponent<TextMeshProUGUI>();

        MaxSmoothing = (int)smoothingSlider.maxValue + 1;
        
        smoothingSlider.onValueChanged.AddListener((value) =>
        {
            CameraHandler.Instance.SmoothingFactor = (int)value;
            smoothingText.text = $"Smoothing: {value:N0}";
        });
        
        fovSlider.onValueChanged?.Invoke(fovSlider.value);
        nearClipSlider.onValueChanged?.Invoke(nearClipSlider.value);

        RigUtils.OnRigSpawned += OnRigSpawned;
        RigUtils.OnRigCached += OnRigCached;
        RigUtils.OnRigNameChange += UpdatePlayerName;
        RigUtils.OnMatIndexChange += UpdatePlayerTagState;

        if (VRRig.LocalRig != null)
            OnRigSpawned(VRRig.LocalRig);

        canvas.SetActive(false);

        GameObject modeHandlersComponents = new GameObject("Casting Should Be Free Mode Handlers");

        GameObject modeButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("CameraModeButton");
        Transform modeContent = canvas.transform.Find("MainPanel/CameraModes/Viewport/Content");

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
                modeButton.GetComponent<Button>().onClick.AddListener(() => CurrentHandlerName = modeHandler.HandlerName);
            }
            else
            {
                Debug.Log(modeHandlerType.Name + " isn't a mode handler, removing.");
                Destroy(modeHandlerComponent);
            }
        }
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(KeyCode.C))
            canvas.SetActive(!canvas.activeSelf);
    }

    private void OnRigSpawned(VRRig rig)
    {
        GameObject button = Instantiate(playerButtonPrefab, playerContent);
        button.GetComponent<Button>().onClick.AddListener(() => ChangePlayer(rig));
        button.GetComponentInChildren<TextMeshProUGUI>().text = rig.OwningNetPlayer?.NickName;
        rigButtons[rig] = button;
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
    }

    private void UpdatePlayerName(VRRig rig, string playerName)
    {
        if (rigButtons.TryGetValue(rig, out GameObject button))
            button.GetComponentInChildren<TextMeshProUGUI>().text = playerName;

        if (CastedRig == rig)
            currentPlayerText.text =
                $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
    }

    private void ChangePlayer(VRRig rig)
    {
        string playerName = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : rig.playerText1.text;
        currentPlayerText.text = $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
        isPlayerTaggedText.text =
            $"Is Tagged? {(rig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
        CastedRig = rig;
    }

    private void UpdatePlayerTagState(VRRig rig)
    {
        if (CastedRig != rig)
            return;
        
        isPlayerTaggedText.text =
            $"Is Tagged? {(rig.IsTagged() ? "<color=green>Yes!</color>" : "<color=red>No!</color>")}";
    }
}