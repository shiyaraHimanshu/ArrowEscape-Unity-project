using UnityEngine;
namespace TheVayuputra.Core
{
    public abstract class AdsManagerBase : MonoBehaviour, IAdsManager
    {
        public static IAdsManager Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && (Object)Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        protected virtual void Start()
        {
            Initialize();
        }

        public abstract void Initialize();

        // ---- Banner ----
        public abstract bool IsReadyBanner();
        public abstract bool ShowBanner();
        public abstract bool HideBanner();

        // ---- Interstitial ----
        public abstract bool IsReadyInterstitial();
        public abstract bool ShowInterstitial(System.Action onClosed = null);

        // ---- Rewarded ----
        public abstract bool IsReadyRewarded();
        public abstract bool ShowRewarded(System.Action onRewarded = null);
    }
}