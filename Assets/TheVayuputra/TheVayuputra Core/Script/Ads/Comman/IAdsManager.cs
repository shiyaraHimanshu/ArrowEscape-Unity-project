namespace TheVayuputra.Core
{
    public interface IAdsManager
    {
        void Initialize();

        bool IsReadyBanner();
        bool ShowBanner();
        bool HideBanner();

        bool IsReadyInterstitial();
        bool ShowInterstitial(System.Action onClosed = null);

        bool IsReadyRewarded();
        bool ShowRewarded(System.Action onRewarded = null);
    }
}