using System.Collections;
using System.Globalization;
using System.Linq;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface;

public class ScoreboardHandler : Singleton<ScoreboardHandler>
{
    private enum TimerMode
    {
        Timing,
        Paused,
        Reset
    }

    private TimerMode currentTimerMode = TimerMode.Reset;

    private TextMeshProUGUI timer;

    private float timerTime = -10f;

    private VRRig lastTaggedRig;

    private void Start()
    {
        timer = transform.Find("Timer").GetComponent<TextMeshProUGUI>();
        
        timer.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            switch (currentTimerMode)
            {
                case TimerMode.Reset:
                    currentTimerMode = TimerMode.Timing;
                    timer.GetComponentInChildren<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Stop Timer";
                    break;
                case TimerMode.Timing:
                    currentTimerMode = TimerMode.Paused;
                    timer.GetComponentInChildren<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Reset Timer";
                    break;
                default:
                    currentTimerMode = TimerMode.Reset;
                    timer.GetComponentInChildren<Button>().GetComponentInChildren<TextMeshProUGUI>().text = "Start Timer";
                    break;
            }

            if (currentTimerMode == TimerMode.Reset)
            {
                timer.text = "-10.00";
                timerTime = -10f;
            }
            else if (currentTimerMode == TimerMode.Timing)
            {
                StartCoroutine(StartTiming());
            }
        });
    }

    private IEnumerator StartTiming()
    {
        if (SetColourPatch.SpawnedRigs.Count < 4 && TagManager.Instance.TaggedRigs.Count > 0)
            lastTaggedRig = TagManager.Instance.TaggedRigs.ElementAt(0);
        
        while (currentTimerMode == TimerMode.Timing)
        {
            timerTime += Time.deltaTime;
            timer.text = timerTime.ToString("F", CultureInfo.InvariantCulture);

            if (TagManager.Instance.UnTaggedRigs.Count < 1)
            {
                timer.GetComponentInChildren<Button>().onClick?.Invoke();
                yield break;
            }

            if (SetColourPatch.SpawnedRigs.Count < 4 && TagManager.Instance.TaggedRigs.Count > 0)
            {
                if (lastTaggedRig != TagManager.Instance.TaggedRigs.ElementAt(0))
                {
                    timer.GetComponentInChildren<Button>().onClick?.Invoke();
                    yield break;
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
}