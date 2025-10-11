using System.Globalization;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Nametags;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class SettingsHandler : Singleton<SettingsHandler>
{
    protected override void Awake()
    {
        Transform leaderboard = GUIHandler.Instance.Canvas.transform.Find("Leaderboard");

        transform.Find("SettingsGrid/Viewport/Content/AntiAFKKick").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  PhotonNetworkController.Instance.disableAFKKick =
                                          !PhotonNetworkController.Instance.disableAFKKick;

                                  transform.Find("SettingsGrid/Viewport/Content/AntiAFKKick")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Anti AFK Kick\n{(PhotonNetworkController.Instance.disableAFKKick ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/Leaderboard").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  leaderboard.gameObject.SetActive(!leaderboard.gameObject.activeSelf);
                                  transform.Find("SettingsGrid/Viewport/Content/Leaderboard")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Leaderboard\n{(leaderboard.gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/Scoreboard").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  ScoreboardHandler.Instance.gameObject.SetActive(
                                          !ScoreboardHandler.Instance.gameObject.activeSelf);

                                  transform.Find("SettingsGrid/Viewport/Content/Scoreboard")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Scoreboard\n{(ScoreboardHandler.Instance.gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/MiniMap").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject
                                            .SetActive(!GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject
                                                                  .activeSelf);

                                  transform.Find("SettingsGrid/Viewport/Content/MiniMap")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Mini Map\n{(GUIHandler.Instance.Canvas.transform.Find("MiniMap").gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/ClosestLava").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  ClosestTaggedHandler.Instance.gameObject.SetActive(
                                          !ClosestTaggedHandler.Instance.gameObject
                                                               .activeSelf);

                                  transform.Find("SettingsGrid/Viewport/Content/ClosestLava")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Closest Lava\n{(ClosestTaggedHandler.Instance.gameObject.activeSelf ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/Nametags").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  NametagHandler.Instance.NametagsEnabled = !NametagHandler.Instance.NametagsEnabled;
                                  transform.Find("SettingsGrid/Viewport/Content/Nametags")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Nametags\n{(NametagHandler.Instance.NametagsEnabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/RollLock").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  ModeHandlerBase.RollLock = !ModeHandlerBase.RollLock;
                                  transform.Find("SettingsGrid/Viewport/Content/RollLock")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Roll Lock\n{(ModeHandlerBase.RollLock ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        transform.Find("SettingsGrid/Viewport/Content/AutoCasting").GetComponent<Button>().onClick
                 .AddListener(() =>
                              {
                                  AutoCaster.Instance.IsEnabled = !AutoCaster.Instance.IsEnabled;
                                  transform.Find("SettingsGrid/Viewport/Content/AutoCasting")
                                           .GetComponentInChildren<TextMeshProUGUI>().text =
                                          $"Auto Casting\n{(AutoCaster.Instance.IsEnabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>")}";
                              });

        Transform thirdPersonSliderPanel = transform.Find("ThirdPersonPanel");

        thirdPersonSliderPanel.GetComponentInChildren<Slider>().onValueChanged.AddListener(value =>
        {
            thirdPersonSliderPanel.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Third Person Right: {value.ToString("F", CultureInfo.InvariantCulture)}";

            ThirdPersonHandler.X = value;
        });
    }
}