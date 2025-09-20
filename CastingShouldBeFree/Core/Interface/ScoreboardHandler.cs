using System.Collections;
using System.Globalization;
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
        while (currentTimerMode == TimerMode.Timing)
        {
            timerTime += Time.deltaTime;
            timer.text = timerTime.ToString("F", CultureInfo.InvariantCulture);
            
            if (TagManager.Instance.UnTaggedRigs.Count < 1)
                yield break;
            
            yield return new WaitForFixedUpdate();
        }

        if (currentTimerMode == TimerMode.Timing)
            timer.GetComponentInChildren<Button>().onClick?.Invoke();
    }
}