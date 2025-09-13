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

                foreach (string modeHandlerName in modeHandlers.Keys)
                    modeHandlers[modeHandlerName].enabled = modeHandlerName == value;
            }
        }
    }

    #endregion

    public Action<VRRig, VRRig> OnCastedRigChange;
    
    private Dictionary<string, ModeHandlerBase> modeHandlers = new();

    private GameObject canvas;
    private Transform playerContent;

    private TextMeshProUGUI currentPlayerText;
    private TextMeshProUGUI isPlayerTaggedText;

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

        playerButtonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("PlayerButton");

        RigUtils.OnRigSpawned += OnRigSpawned;
        RigUtils.OnRigCached += OnRigCached;
        RigUtils.OnRigNameChange += UpdatePlayerName;

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
    }

    private void UpdatePlayerName(VRRig rig, string playerName)
    {
        if (rigButtons.TryGetValue(rig, out GameObject button))
            button.GetComponentInChildren<TextMeshProUGUI>().text = playerName;

        if (Equals(CastedRig, rig))
            currentPlayerText.text =
                $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
    }

    private void ChangePlayer(VRRig rig)
    {
        string playerName = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : rig.playerText1.text;
        currentPlayerText.text = $"Name: <color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{playerName}</color>";
        CastedRig = rig;
    }
}