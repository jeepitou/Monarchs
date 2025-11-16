using System;
using Monarchs.Client;
using Monarchs.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class GameTimer : MonoBehaviour
    {
        public TMP_Text timerTextTMP;
        public Text timerText;
        public TimerFormat timerFormat = TimerFormat.HHMMSS;
        public IncrementType incrementType = IncrementType.AddTime;
        public int initialSeconds = 0;
        public int initialMinutes = 0;
        public int initialHours = 0;

        // Timer variables


        private int hours = 0;
        private int minutes = 0;
        private float seconds = 0;

        // Start is called before the first frame update
        void Start()
        {
            hours = Mathf.Max(0, initialHours);
            minutes = Mathf.Max(0, initialMinutes);
            seconds = Mathf.Max(0, initialSeconds);
        }
        
        public void SetTimer(int hours, int minutes, float seconds)
        {
            this.hours = Mathf.Max(0, hours);
            this.minutes = Mathf.Max(0, minutes);
            this.seconds = Mathf.Max(0, seconds);
        }

        private void OnEnable()
        {
            hours = Mathf.Max(0, initialHours);
            minutes = Mathf.Max(0, initialMinutes);
            seconds = Mathf.Max(0, initialSeconds);
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameClient.Get().IsReady() || GameClient.GetGameData().State == GameState.Connecting)
            {
                return;
            }

            if (incrementType == IncrementType.AddTime)
            {
                seconds += Time.deltaTime;
            }
            else if (incrementType == IncrementType.SubtractTime)
            {
                seconds -= Time.deltaTime;
            }

            if (timerFormat == TimerFormat.HHMMSS)
            {
                UpdateText(GetSringHHMMSS());
            }
            else if (timerFormat == TimerFormat.MMSS)
            {
                UpdateText(GetStringMMSS());
            }
            else if (timerFormat == TimerFormat.SS)
            {
                UpdateText(GetStringSS());
            }
        }
        
        string GetSringHHMMSS()
        {
            if (seconds >= 60)
            {
                minutes++;
                seconds = 0;
            }

            if (seconds < 0)
            {
                seconds += 60;
                minutes--;
            }

            if (minutes >= 60)
            {
                hours++;
                minutes = 0;
            }
            
            if (minutes < 0)
            {
                minutes += 60;
                hours--;
            }
            
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
        }
        
        string GetStringMMSS()
        {
            if (seconds >= 60)
            {
                minutes++;
                seconds = 0;
            }
            
            if (seconds < 0)
            {
                seconds += 60;
                minutes--;
            }

            return minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
        }
        
        string GetStringSS()
        {
            return ((int)seconds).ToString("00");
        }

        void UpdateText(string text)
        {
            if (timerTextTMP != null)
            {
                timerTextTMP.text = text;
            }
            else if (timerText != null)
            {
                timerText.text = text;
            }
        }
        
        public enum TimerFormat
        {
            HHMMSS,
            MMSS,
            SS
        }

        public enum IncrementType
        {
            AddTime,
            SubtractTime
        }
    }
}
