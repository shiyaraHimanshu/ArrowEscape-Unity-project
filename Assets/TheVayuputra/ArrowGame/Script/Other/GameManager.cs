using System.Collections;
using System.Collections.Generic;
using TheVayuputra.Core;
using UnityEngine;
namespace ArrowGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        [SerializeField] private bool enableAnalyticsLog = true;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        IEnumerator Start()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout =SleepTimeout.NeverSleep;
            UiManager.Instance.SetupUI();
            yield return new WaitForSeconds(1f);
            //hide loading screen

            LevelManager.Instance.LoadCurrentLevel();

        }
        // --------------------------------------------------
        // ANALYTICS / REPORT EVENTS
        // --------------------------------------------------

        public void ReportEvent(string eventName)
        {
            if (!enableAnalyticsLog) return;

            if (enableAnalyticsLog)
                Debug.Log($"[Analytics] Event: {eventName}");

            // Example hooks (enable when needed)
            // FirebaseAnalytics.LogEvent(eventName);
            // GameAnalytics.NewDesignEvent(eventName);
        }
        public void ReportEvent(string eventName, Dictionary<string, string> data)
        {
            if (!enableAnalyticsLog) return;

            if (enableAnalyticsLog)
            {
                string dataLog = "";
                foreach (var pair in data)
                    dataLog += $"{pair.Key}:{pair.Value}, ";

                Debug.Log($"[Analytics] Event: {eventName} | Data: {dataLog}");
            }

            // Example hooks
            // foreach (var pair in data)
            //     FirebaseAnalytics.LogEvent(eventName, pair.Key, pair.Value);
        }

    }
}
