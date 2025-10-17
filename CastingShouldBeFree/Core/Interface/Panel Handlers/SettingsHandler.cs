using System.Globalization;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Nametags;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class SettingsHandler : Singleton<SettingsHandler>
{
    protected override void Awake()
    {
        Transform leaderboard = GUIHandler.Instance.Canvas.transform.Find("Leaderboard");
   
        SetUpSetting("SettingsGrid/Viewport/Content/AntiAFKKick", "Anti AFK Kick", ref PhotonNetworkController.Instance.disableAFKKick);
        SetUpSetting("SettingsGrid/Viewport/Content/RollLock", "Roll Lock", ref ModeHandlerBase.RollLock);
        SetUpSetting("SettingsGrid/Viewport/Content/AutoCasting", "Auto Casting", ref AutoCaster.Instance.IsEnabled);
        SetUpSetting("SettingsGrid/Viewport/Content/ThirdPersonBodyLock", "Third Person Body Lock", ref ThirdPersonHandler.BodyLocked);
        SetUpSetting("SettingsGrid/Viewport/Content/SnappySmoothing", "Snappy Smoothing", ref ModeHandlerBase.SnappySmoothing);

        SetUpSetting("SettingsGrid/Viewport/Content/Leaderboard", "Leaderboard", leaderboard.gameObject);
        SetUpSetting("SettingsGrid/Viewport/Content/Scoreboard", "Scoreboard", GUIHandler.Instance.Canvas.transform.Find("Scoreboard").gameObject);
        SetUpSetting("SettingsGrid/Viewport/Content/MiniMap", "Mini Map", GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject);
        SetUpSetting("SettingsGrid/Viewport/Content/ClosestLava", "Closest Lava", ClosestTaggedHandler.Instance.gameObject);
        
        SetUpSetting("SettingsGrid/Viewport/Content/Nametags", "Nametags",
                () => NametagHandler.Instance.NametagsEnabled,
                value => NametagHandler.Instance.NametagsEnabled = value);

        Transform thirdPersonSliderPanel = transform.Find("ThirdPersonPanel");

        thirdPersonSliderPanel.GetComponentInChildren<Slider>().onValueChanged.AddListener(value =>
            {
                thirdPersonSliderPanel.GetComponentInChildren<TextMeshProUGUI>().text =
                        $"Third Person Right: {value.ToString("F", CultureInfo.InvariantCulture)}";

                ThirdPersonHandler.X = value;
            });
    }

    // A bunch of fucking overloads and shit
    private void SetUpSetting(string settingPath, string settingName, ref bool setting)
    {
        bool localSetting = setting;

        void ChangeSetting()
        {
            localSetting = !localSetting;
            transform.Find(settingPath).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{settingName}\n{(localSetting ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
            PlayerPrefs.SetInt(settingName, localSetting ? 1 : 0);
        }

        transform.Find(settingPath).GetComponent<Button>().onClick.AddListener(ChangeSetting);

        if (PlayerPrefs.GetInt(settingName, 0) == 1 != localSetting)
            transform.Find(settingPath).GetComponent<Button>().onClick?.Invoke();

        setting = localSetting;
    }

    private void SetUpSetting(string settingPath, string settingName, GameObject target)
    {
        bool localSetting = target.activeSelf;

        void ChangeSetting()
        {
            localSetting = !localSetting;
            target.SetActive(localSetting);
            transform.Find(settingPath).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{settingName}\n{(localSetting ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
            PlayerPrefs.SetInt(settingName, localSetting ? 1 : 0);
        }

        transform.Find(settingPath).GetComponent<Button>().onClick.AddListener(ChangeSetting);

        if (PlayerPrefs.GetInt(settingName, 0) == 1 != localSetting)
            transform.Find(settingPath).GetComponent<Button>().onClick?.Invoke();

        target.SetActive(localSetting);
    }
    
    private void SetUpSetting(string settingPath, string settingName, Func<bool> getter, Action<bool> setter)
    {
        bool localSetting = getter();

        void ChangeSetting()
        {
            localSetting = !localSetting;
            setter(localSetting);
            transform.Find(settingPath).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{settingName}\n{(localSetting ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
            PlayerPrefs.SetInt(settingName, localSetting ? 1 : 0);
        }

        transform.Find(settingPath).GetComponent<Button>().onClick.AddListener(ChangeSetting);

        if (PlayerPrefs.GetInt(settingName, 0) == 1 != localSetting)
            transform.Find(settingPath).GetComponent<Button>().onClick?.Invoke();

        setter(localSetting);
    }
}