using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace TheVayuputra.Core
{
    public class AdsManagerGoogleAds : AdsManagerBase
    {
        // Import the JavaScript function from our .jslib file (AdsgramBridge.jslib)
        [DllImport("__Internal")]
        private static extern void ShowAdInternal(string blockId, bool isRewarded);

        [Header("Adsgram Setting")]
        [SerializeField] private string videoAdId = "int-27021"; // Interstitial ID
        [SerializeField] private string rewardAdId = "27022";    // Rewarded ID

        private Action interstitialClosedCallback;
        private Action rewardedEarnedCallback;

        protected override void Awake()
        {
            base.Awake();
            // Crucial for WebGL SendMessage! The GameObject name must match the name in AdsgramBridge.jslib
            gameObject.name = "AdsManager";
        }

        public override void Initialize()
        {
            Debug.Log("Adsgram AdsManager Initialized via GoogleAds Bridge.");
        }

        // ---- Banner ----
        public override bool IsReadyBanner() => false;
        public override bool ShowBanner() => false;
        public override bool HideBanner() => false;

        // ---- Interstitial ----
        public override bool IsReadyInterstitial()
        {
            return true; // Adsgram handles availability internally
        }

        public override bool ShowInterstitial(Action onClosed = null)
        {
            interstitialClosedCallback = onClosed;
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowAdInternal(videoAdId, false);
#else
            Debug.Log("Video Ad triggered (Editor Mode - Bypassing)");
            OnVideoFinished();
#endif
            return true;
        }

        // ---- Rewarded ----
        public override bool IsReadyRewarded()
        {
            return true; // Adsgram handles availability internally
        }

        public override bool ShowRewarded(Action onRewarded = null)
        {
            rewardedEarnedCallback = onRewarded;
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowAdInternal(rewardAdId, true);
#else
            Debug.Log("Rewarded Ad triggered (Editor Mode - Bypassing)");
            OnRewardedFinished(1);
#endif
            return true;
        }

        // ---- Callbacks from JavaScript ----

        // Called via SendMessage('AdsManager', 'OnVideoFinished')
        public void OnVideoFinished()
        {
            Debug.Log("Video Ad (Interstitial) Finished.");
            var cb = interstitialClosedCallback;
            interstitialClosedCallback = null;
            cb?.Invoke();
        }

        // Called via SendMessage('AdsManager', 'OnRewardedFinished', success)
        public void OnRewardedFinished(int success)
        {
            if (success == 1)
            {
                Debug.Log("Rewarded Ad Success. Giving Reward.");
                var cb = rewardedEarnedCallback;
                rewardedEarnedCallback = null;
                cb?.Invoke();
            }
            else
            {
                Debug.Log("Rewarded Ad Skipped or Failed.");
                rewardedEarnedCallback = null;
            }
        }
    }
}