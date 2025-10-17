using System.Collections;
using System.Globalization;
using BepInEx;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;
using static System.String;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class ScoreboardHandler : Singleton<ScoreboardHandler>
{
    public GameObject Scoreboard;

    private void Start()
    {
        Scoreboard = GUIHandler.Instance.Canvas.transform.Find("Scoreboard").gameObject;

        transform.Find("TeamOneName").GetComponent<TMP_InputField>().onValueChanged
                 .AddListener(text => Scoreboard.transform.Find("TeamOne").GetComponent<TextMeshProUGUI>().text =
                                              !IsNullOrEmpty(text) ? text : "Team 1");

        transform.Find("TeamTwoName").GetComponent<TMP_InputField>().onValueChanged
                 .AddListener(text => Scoreboard.transform.Find("TeamTwo").GetComponent<TextMeshProUGUI>().text =
                                              !IsNullOrEmpty(text) ? text : "Team 2");

        transform.Find("TeamOneScore").GetComponent<TMP_InputField>().onValueChanged
                 .AddListener(text => Scoreboard.transform.Find("TeamOne/ScoreCounter/Score")
                                                .GetComponent<TextMeshProUGUI>().text =
                                              !IsNullOrEmpty(text) ? text : "0");

        transform.Find("TeamTwoScore").GetComponent<TMP_InputField>().onValueChanged
                 .AddListener(text => Scoreboard.transform.Find("TeamTwo/ScoreCounter/Score")
                                                .GetComponent<TextMeshProUGUI>().text =
                                              !IsNullOrEmpty(text) ? text : "0");

        TextMeshProUGUI timerText    = Scoreboard.transform.Find("Timer").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI lastTimeText = Scoreboard.transform.Find("Timer/LastTime").GetComponent<TextMeshProUGUI>();
        Plugin.Instance.gameObject.AddComponent<TimerHandler>().Initialize(timerText, lastTimeText);
        
        gameObject.SetActive(false);
    }

    private class TimerHandler : MonoBehaviour
    {
        private int currentTimingIndex;

        private float           lastTime;
        private TextMeshProUGUI lastTimeText;
        private TextMeshProUGUI timerText;

        private void Update()
        {
            if (UnityInput.Current.GetKeyDown(KeyCode.V))
            {
                currentTimingIndex = (currentTimingIndex + 1) % 3;
                if (currentTimingIndex == 1) StartCoroutine(Timing());
            }
        }

        public void Initialize(TextMeshProUGUI timerText, TextMeshProUGUI lastTimeText)
        {
            this.timerText    = timerText;
            this.lastTimeText = lastTimeText;
        }

        private IEnumerator Timing()
        {
            float       startTime = Time.time;
            const float Offset    = 10f;
            float       elapsed   = Time.time - startTime - Offset;

            while (currentTimingIndex == 1)
            {
                yield return new WaitForEndOfFrame();
                elapsed        = Time.time - startTime - Offset;
                timerText.text = elapsed.ToString("F2", CultureInfo.InvariantCulture);
            }

            timerText.text = elapsed.ToString("F2", CultureInfo.InvariantCulture);

            while (currentTimingIndex == 2)
                yield return new WaitForEndOfFrame();

            timerText.text    = "-10.00";
            lastTimeText.text = elapsed.ToString("F2", CultureInfo.InvariantCulture);
            lastTime          = elapsed;
        }
    }
}