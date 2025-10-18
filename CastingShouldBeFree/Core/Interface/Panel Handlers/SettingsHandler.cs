using System;
using System.Globalization;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Nametags;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class SettingsHandler : PanelHandlerBase
{
    private const string ThirdPersonXKey = "ThirdPersonX";

    private void Awake()
    {
        Transform leaderboard = GUIHandler.Instance.Canvas.transform.Find("Leaderboard");

        SetUpSetting("SettingsGrid/Viewport/Content/AntiAFKKick", "Anti AFK Kick",
                () => PhotonNetworkController.Instance.disableAFKKick,
                () => PhotonNetworkController.Instance.disableAFKKick =
                              !PhotonNetworkController.Instance.disableAFKKick);

        SetUpSetting("SettingsGrid/Viewport/Content/RollLock", "Roll Lock", () => ModeHandlerBase.RollLock,
                () => ModeHandlerBase.RollLock = !ModeHandlerBase.RollLock);

        SetUpSetting("SettingsGrid/Viewport/Content/AutoCasting", "Auto Casting", () => AutoCaster.Instance.IsEnabled,
                () => AutoCaster.Instance.IsEnabled = !AutoCaster.Instance.IsEnabled);

        SetUpSetting("SettingsGrid/Viewport/Content/ThirdPersonBodyLock", "Third Person Body Lock",
                () => ThirdPersonHandler.BodyLocked,
                () => ThirdPersonHandler.BodyLocked = !ThirdPersonHandler.BodyLocked);

        SetUpSetting("SettingsGrid/Viewport/Content/SnappySmoothing", "Snappy Smoothing",
                () => ModeHandlerBase.SnappySmoothing,
                () => ModeHandlerBase.SnappySmoothing = !ModeHandlerBase.SnappySmoothing);

        SetUpSetting("SettingsGrid/Viewport/Content/Leaderboard", "Leaderboard",
                () => leaderboard.gameObject.activeSelf,
                () => leaderboard.gameObject.SetActive(!leaderboard.gameObject.activeSelf));

        SetUpSetting("SettingsGrid/Viewport/Content/Scoreboard", "Scoreboard",
                () => GUIHandler.Instance.Canvas.transform.Find("Scoreboard").gameObject.activeSelf,
                () => GUIHandler.Instance.Canvas.transform.Find("Scoreboard").gameObject
                                .SetActive(!GUIHandler.Instance.Canvas.transform.Find("Scoreboard").gameObject
                                                      .activeSelf));

        SetUpSetting("SettingsGrid/Viewport/Content/MiniMap", "Mini Map",
                () => GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject.activeSelf,
                () => GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject
                                .SetActive(!GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject
                                                      .activeSelf));

        SetUpSetting("SettingsGrid/Viewport/Content/ClosestLava", "Closest Lava",
                () => ClosestTaggedHandler.Instance.gameObject.activeSelf,
                () => ClosestTaggedHandler.Instance.gameObject.SetActive(
                        !ClosestTaggedHandler.Instance.gameObject.activeSelf));

        SetUpSetting("SettingsGrid/Viewport/Content/Nametags", "Nametags",
                () => NametagHandler.Instance.NametagsEnabled,
                () => NametagHandler.Instance.NametagsEnabled = !NametagHandler.Instance.NametagsEnabled);

        Transform thirdPersonSliderPanel = transform.Find("ThirdPersonPanel");

        thirdPersonSliderPanel.GetComponentInChildren<Slider>().onValueChanged.AddListener(value =>
        {
            thirdPersonSliderPanel.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Third Person Right: {value.ToString("F", CultureInfo.InvariantCulture)}";

            ThirdPersonHandler.X = value;
            PlayerPrefs.SetFloat(ThirdPersonXKey, value);
        });

        thirdPersonSliderPanel.GetComponentInChildren<Slider>().onValueChanged
                             ?.Invoke(PlayerPrefs.GetFloat(ThirdPersonXKey, 0f));

        thirdPersonSliderPanel.GetComponentInChildren<Slider>().value = PlayerPrefs.GetFloat(ThirdPersonXKey, 0f);
    }

    private void SetUpSetting(string settingPath, string settingName, Func<bool> getSetting, UnityAction setSetting)
    {
        void ChangeSetting()
        {
            bool localSetting = getSetting();
            transform.Find(settingPath).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{settingName}\n{(localSetting ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";

            PlayerPrefs.SetInt(settingName, localSetting ? 1 : 0);
            PlayerPrefs.Save();
        }

        Button button = transform.Find(settingPath).GetComponent<Button>();
        button.onClick.AddListener(setSetting);
        button.onClick.AddListener(ChangeSetting);

        if (PlayerPrefs.GetInt(settingName, 0) == 1 == getSetting())
            setSetting?.Invoke();

        transform.Find(settingPath).GetComponent<Button>().onClick?.Invoke();
    }
}