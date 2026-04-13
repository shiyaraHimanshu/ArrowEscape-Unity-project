using System.Collections;
using System.Collections.Generic;
using ArrowGameLevelEditor;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheVayuputra.Core;
namespace ArrowGame
{

    public class UiManager : MonoBehaviour
    {
        public TextMeshProUGUI errorMsg;

        /// <summary>
        /// Appends a timestamped log line to the on-screen errorMsg text.
        /// Use this instead of Debug.Log when debugging in Telegram browser.
        /// </summary>
        public void LogToScreen(string msg)
        {
            Debug.Log("[SCREEN] " + msg);
            if (errorMsg != null)
            {
                // Keep only last 15 lines to avoid overflow
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
                string newLine = $"[{timestamp}] {msg}";
                string current = errorMsg.text ?? "";
                string[] lines = current.Split('\n');
                if (lines.Length > 15)
                {
                    // Keep last 14 lines + new one
                    current = string.Join("\n", lines, lines.Length - 14, 14);
                }
                errorMsg.text = current + (string.IsNullOrEmpty(current) ? "" : "\n") + newLine;
            }
        }
        public LoadingManager loadingManager;
        [Header("Heart Setup")]
        [SerializeField] private GameObject heartPrefab;
        [SerializeField] private Transform heartParent;
        [SerializeField] private Color deactiveColor = Color.gray;
        public List<GameObject> heartsObject;
        public TextMeshProUGUI arrowText;
        public TextMeshProUGUI levelText;
        public Button btnRecenter;
        public Toggle themeToggle;
        public Slider zoomSlider;
        [Header("Level Complete Panel")]
        public VayuPopup coinPanel;
        public VayuPopup levelCompletePanel;
        public TextMeshProUGUI gc_EarnCoinText;
        public TextMeshProUGUI totalCoinText;
        public UIParticle coinRewardParticle;
        public GameObject winParticle;

        public Button btn2xRewards;
        public Button btnContinue;


        [Header("Agent Booster Panel")]
        public VayuPopup agentBoosterPanel;
        public Button btnBuyAgent;

        [Header("Level Failed Panel")]
        public VayuPopup levelFailedPanel;
        public Button btnPlayOn;
        [Header("Setting Panel")]
        public VayuPopup settingPanel;
        public Toggle musicToggle;
        public Toggle sfxToggle;
        public Toggle vibrationToggle;
        [Header("Boosters")]
        public TextMeshProUGUI totalHintText;
        public TextMeshProUGUI totalGridLineText;
        public TextMeshProUGUI totalAgentText;
        public Button btnHint, btnGridLines, btnAgent;
        public VayuPopup hintPopup;
        public Button btnBuyHint;
        public Button btnWatchAd_Hint;
        public VayuPopup gridlinePopup;
        public Button btnBuyGridline;
        public Button btnWatchAd_Gridline;
        public GameObject gridCounterObj;
        public DamageOverlayUI damageOverlayUI;
        public TMPro.TextMeshProUGUI debugStatusText;
        
        [Header("Toast View")]
        public CanvasGroup toastView;
        public TextMeshProUGUI toastLbl;
        private int previousLife = -1;
        private LevelManager levelManager => LevelManager.Instance;
        public static UiManager Instance { get; private set; }
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            loadingManager.Show(true);
        }
        public void SetupUI()
        {
            GameData.Coins.AddListener(UpdateTotalCoins);
            GameData.Hint.AddListener(UpdateTotalHintArrow);
            GameData.GridLines.AddListener(UpdateTotalHintGridLine);
            GameData.Agent.AddListener(UpdateTotalAgentCount);
            levelManager.currentLife.AddListener(UpdateLife);
            levelManager.arrowManager.remandingArrow.AddListener(UpdateArrow);
            levelManager.OnLevelCompleted += OnLevelCompleted;
            levelManager.OnLevelFailed += OnLevelFailed;
            levelManager.OnLevelInit += OnLevelInit;
            levelManager.playerInputManager.enableResetCameraUI.AddListener(HandleResetCameraButton);
            themeToggle.isOn = GameData.IsDayTheme.Value;
            themeToggle.onValueChanged.AddListener(OnThemeToggleChange);
            btn2xRewards.onClick.RemoveAllListeners();
            btn2xRewards.onClick.AddListener(On2xRewardButtonClicked);
            btnRecenter.onClick.RemoveAllListeners();
            btnRecenter.onClick.AddListener(ResetCameraToCenter);
            btnContinue.onClick.RemoveAllListeners();
            btnContinue.onClick.AddListener(OnContinueButton);
            btnPlayOn.onClick.RemoveAllListeners();
            btnPlayOn.onClick.AddListener(OnPlayOnClicked);
            musicToggle.isOn = GameData.MusicOn.Value;
            musicToggle.onValueChanged.AddListener(OnMusicToggle);
            sfxToggle.isOn = GameData.SfxOn.Value;
            sfxToggle.onValueChanged.AddListener(OnSfxToggle);
            vibrationToggle.isOn = GameData.VibrationOn.Value;
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggle);
            btnHint.onClick.RemoveAllListeners();
            btnHint.onClick.AddListener(OnHintClick);
            btnGridLines.onClick.RemoveAllListeners();
            btnGridLines.onClick.AddListener(OnGridLineClick);
            btnAgent.onClick.RemoveAllListeners();
            btnAgent.onClick.AddListener(OnAgentClick);
            btnBuyHint.onClick.RemoveAllListeners();
            btnBuyHint.onClick.AddListener(OnBuyHinFromCoins);
            btnWatchAd_Hint.onClick.RemoveAllListeners();
            btnWatchAd_Hint.onClick.AddListener(OnBuyHintFromAds);
            btnBuyGridline.onClick.RemoveAllListeners();
            btnBuyGridline.onClick.AddListener(OnBuyGridLineFromCoins);
            btnWatchAd_Gridline.onClick.RemoveAllListeners();
            btnWatchAd_Gridline.onClick.AddListener(OnBuyGridLineFromAds);
            btnBuyAgent.onClick.RemoveAllListeners();
            btnBuyAgent.onClick.AddListener(OnBuyAgentFromStars);

            levelManager.playerInputManager.SetupZoomSlider(zoomSlider);
            UpdateTotalCoins(GameData.Coins.Value);
            UpdateTotalHintArrow(GameData.Hint.Value);
            UpdateTotalHintGridLine(GameData.GridLines.Value);
            UpdateTotalAgentCount(GameData.Agent.Value);
            SpawnHearts();
        }

        private void HandleResetCameraButton(bool value)
        {
            DOTween.Kill(btnRecenter.gameObject);
            if (value)
            {
                btnRecenter.transform.localScale = Vector3.zero;
                btnRecenter.transform.DOScale(Vector3.one, 0.25f);
            }
            else
            {
                btnRecenter.transform.DOScale(Vector3.zero, 0.25f);
            }
        }
        public void ResetCameraToCenter()
        {
            GameManager.Instance.ReportEvent("CLICK_RESET_CAMERA");
            HandleResetCameraButton(false);
            levelManager.playerInputManager.LevelStartAnimation(.5f);
        }
        private void OnMusicToggle(bool value)
        {
            GameData.MusicOn.Value = value;
            GameManager.Instance.ReportEvent(
                "CLICK_MUSIC_TOGGLE",
                new Dictionary<string, string>() { { "MusicOn", $"{value}" } }
                );
        }

        private void OnSfxToggle(bool value)
        {
            GameData.SfxOn.Value = value;
            GameManager.Instance.ReportEvent(
                "CLICK_SFX_TOGGLE",
                new Dictionary<string, string>() { { "SfxOn", $"{value}" } }
                );
        }

        private void OnVibrationToggle(bool value)
        {
            GameData.VibrationOn.Value = value;
            GameManager.Instance.ReportEvent(
                "CLICK_VIBRATION_TOGGLE",
                new Dictionary<string, string>() { { "VibrationOn", $"{value}" } }
                );
        }

        void SpawnHearts()
        {
            heartsObject.Clear();
            for (int i = 0; i < levelManager.maxLife; i++)
            {
                GameObject heart = Instantiate(heartPrefab, heartParent);
                heartsObject.Add(heart);
            }
        }
        private void OnThemeToggleChange(bool value)
        {
            GameData.IsDayTheme.Value = value;
            GameManager.Instance.ReportEvent(
                "CLICK_THEME_TOGGLE",
                new Dictionary<string, string>() { { "IsDayTheme", $"{value}" } }
                );
        }

        void UpdateLife(int life)
        {
            for (int i = 0; i < heartsObject.Count; i++)
            {
                Image img = heartsObject[i].GetComponent<Image>();
                img.color = (i < life) ? Color.white : deactiveColor;
            }
            if (previousLife > life)
            {
                int lostHeartIndex = life;

                if (lostHeartIndex >= 0 && lostHeartIndex < heartsObject.Count)
                {
                    UIParticle particle = heartsObject[lostHeartIndex]
                        .GetComponentInChildren<UIParticle>();
                    if (particle != null)
                        particle.Play();
                }
            }
            previousLife = life;
        }
        int displayedCoins = 0;
        Tween coinTween;
        void UpdateTotalCoins(int targetCoins)
        {
            // Kill previous tween if running
            if (coinTween != null && coinTween.IsActive())
                coinTween.Kill();

            int startCoins = displayedCoins;
            float duration = Mathf.Clamp(
                Mathf.Abs(targetCoins - displayedCoins) * 0.02f,
                0.2f,
                1f
            );
            coinTween = DOTween.To(() => startCoins,
                x =>
                {
                    startCoins = x;
                    displayedCoins = x;
                    totalCoinText.text = x.ToString();
                },
                targetCoins,
                duration
            ).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                displayedCoins = targetCoins;
                totalCoinText.text = targetCoins.ToString();
            });
        }

        void UpdateArrow(int remainingArrow)
        {
            arrowText.text = remainingArrow.ToString();
        }
        void UpdateTotalHintArrow(int count)
        {
            totalHintText.text = count > 0 ? count.ToString() : "+";
        }
        void UpdateTotalHintGridLine(int count)
        {
            totalGridLineText.text = count > 0 ? count.ToString() : "+";
        }
        void UpdateTotalAgentCount(int count)
        {
            totalAgentText.text = count > 0 ? count.ToString() : "+";
        }
        void OnLevelInit()
        {
            levelText.text = $"Level {GameData.CurrentLevel}";
            levelManager.isGridLineUnlockedForLevel = false;
            gridCounterObj.SetActive(true);
        }
        void OnLevelCompleted()
        {
            GameData.TotalGamePlay++;
            gc_EarnCoinText.text = $"+{GameData.LevelWonCoins}";
            OpenPopup(levelCompletePanel, true);
            // Rewarded button setup
            btn2xRewards.gameObject.SetActive(true);
            btn2xRewards.interactable = true;
            btnContinue.gameObject.SetActive(true);
            SoundManager.Instance.PlayLevelCompletedSFX();
        }

        void OnLevelFailed()
        {
            OpenPopup(levelFailedPanel);
            btnPlayOn.gameObject.SetActive(true);
            btnPlayOn.interactable = true;
            SoundManager.Instance.PlayLevelFailedSFX();
        }

        public void OnContinueButton()
        {
            StartCoroutine(IGiveRewardAnimations(GameData.LevelWonCoins));
        }
        void On2xRewardButtonClicked()
        {
            if (AdsManagerBase.Instance == null)
                return;
            if (!AdsManagerBase.Instance.IsReadyRewarded())
            {
                ShowToast("No ads available right now!");
                return;
            }
            // Disable button immediately to avoid double clicks
            btn2xRewards.interactable = false;
            AdsManagerBase.Instance.ShowRewarded(() =>
            {
                GiveAdsRewards();
            });
            GameManager.Instance.ReportEvent("CLICK_LEVEL_COMPLETED_2X_REWARD");
        }
        void GiveAdsRewards()
        {
            int extraCoins = GameData.LevelWonCoins * GameData.RewardedAdCoinMultiplier;
            gc_EarnCoinText.text = $"+{extraCoins}";
            // Hide button after reward
            GameManager.Instance.ReportEvent(
                "GIVE_LEVEL_COMPLETED_2X_REWARD",
                new Dictionary<string, string>() { { "REWARD_COIN", $"{extraCoins}" } }
                );
            StartCoroutine(IGiveRewardAnimations(extraCoins));
        }
        IEnumerator IGiveRewardAnimations(int rewardsCoins)
        {
            coinRewardParticle.Play();
            btn2xRewards.gameObject.SetActive(false);
            btnContinue.gameObject.SetActive(false);
            yield return new WaitForSeconds(.8f);
            GameData.Coins.Value += rewardsCoins;
            yield return new WaitForSeconds(1.2f);
            levelManager.LoadCurrentLevel();
            ClosePopup(levelCompletePanel);
            TryShowInterstitial();
        }
        public void OnRetryButton()
        {
            levelManager.LoadCurrentLevel();
            ClosePopup(levelFailedPanel);
            GameData.TotalGamePlay++;
            TryShowInterstitial();
        }
        public void SettingPanelOpenClose(bool doOpen)
        {
            if (doOpen)
            {
                OpenPopup(settingPanel);
                GameManager.Instance.ReportEvent("CLICK_SETTING");
            }
            else
            {
                ClosePopup(settingPanel);
            }
        }

        void OnPlayOnClicked()
        {
            if (AdsManagerBase.Instance == null)
                return;
            if (!AdsManagerBase.Instance.IsReadyRewarded())
            {
                Debug.Log("2x Reward Granted via Rewarded Ad");
                return;
            }
            // Prevent double clicks
            btnPlayOn.interactable = false;
            AdsManagerBase.Instance.ShowRewarded(() =>
            {
                OnPlayOnRewardGranted();
            });
            GameManager.Instance.ReportEvent("CLICK_LEVEL_FAILED_PLAY_ON");
        }
        void OnPlayOnRewardGranted()
        {
            GameManager.Instance.ReportEvent(
                "GIVE_LEVEL_FAILED_PLAY_ON",
                new Dictionary<string, string>() { { "REWARD_LIFE", $"3" } }
                );
            LevelManager.Instance.ResetLife();
            ClosePopup(levelFailedPanel);
            btnPlayOn.gameObject.SetActive(false);
        }

        void OnHintClick()
        {
            GameManager.Instance.ReportEvent("CLICK_HINT");
            if (GameData.Hint.Value > 0)
            {
                GameData.Hint.Value--;
                levelManager.ShowEscapeArrowHint();
                GameManager.Instance.ReportEvent("USE_HINT");
                return;
            }
            btnWatchAd_Hint.interactable = true;
            btnBuyHint.interactable = true;
            GameManager.Instance.ReportEvent("SHOW_HINT_POPUP");
            OpenPopup(hintPopup, true);
        }
        void OnGridLineClick()
        {
            GameManager.Instance.ReportEvent("CLICK_GRID_LINE");
            if (levelManager.isGridLineUnlockedForLevel)
            {
                levelManager.isShowGridLines.Value =
                    !levelManager.isShowGridLines.Value;
                return;
            }
            if (GameData.GridLines.Value > 0)
            {
                GameData.GridLines.Value--;
                GameManager.Instance.ReportEvent("USE_GRID_LINE");
                levelManager.isShowGridLines.Value = true;
                levelManager.isGridLineUnlockedForLevel = true;
                gridCounterObj.SetActive(false);
                return;
            }
            btnWatchAd_Gridline.interactable = true;
            btnBuyGridline.interactable = true;
            GameManager.Instance.ReportEvent("SHOW_GRID_LINE_POPUP");
            OpenPopup(gridlinePopup, true);
        }
        void OnAgentClick()
        {
            LogToScreen("OnAgentClick called");
            GameManager.Instance.ReportEvent("CLICK_AGENT");
            if (GameData.Agent.Value > 0)
            {
                LogToScreen($"Agent available: {GameData.Agent.Value}. Using it.");
                GameData.Agent.Value--;
                levelManager.UseAgentBooster();
                GameManager.Instance.ReportEvent("USE_AGENT");
                return;
            }
            LogToScreen("No Agent owned. Opening buy popup.");
            btnBuyAgent.interactable = true;
            GameManager.Instance.ReportEvent("SHOW_AGENT_POPUP");
            OpenPopup(agentBoosterPanel, true);
        }

        void OnBuyHinFromCoins()
        {
            GameManager.Instance.ReportEvent("CLICK_BUY_HINT");
            if (GameData.Coins.Value >= GameData.BuyHintCoins)
            {
                GameData.Coins.Value -= GameData.BuyHintCoins;
                RewardHint();
                GameManager.Instance.ReportEvent("BUY_HINT_SUCCESS");
            }
            else
            {
                // Show Toast Not enough coins
                GameManager.Instance.ReportEvent("BUY_HINT_FAILED");
                ShowToast("You don't have enough coins!");
            }
        }
        void OnBuyHintFromAds()
        {
            GameManager.Instance.ReportEvent("CLICK_BUY_HINT_BY_ADS");

            if (AdsManagerBase.Instance == null)
                return;
            if (!AdsManagerBase.Instance.IsReadyRewarded())
            {

                ShowToast("No ads available right now!");
                return;
            }
            // Prevent double clicks
            btnWatchAd_Hint.interactable = false;
            AdsManagerBase.Instance.ShowRewarded(() =>
            {
                RewardHint();
            });
        }
        void RewardHint()
        {
            GameManager.Instance.ReportEvent(
                "GIVE_HINT",
                new Dictionary<string, string>() { { "REWARD_HINT", $"{1}" } }
                );
            GameData.Hint.Value++;
            ClosePopup(hintPopup);
        }

        void OnBuyGridLineFromCoins()
        {
            GameManager.Instance.ReportEvent("CLICK_BUY_GRID_LINE");
            if (GameData.Coins.Value >= GameData.BuyGridCoins)
            {
                GameManager.Instance.ReportEvent("CLICK_BUY_GRID_LINE_SUCCESS");
                GameData.Coins.Value -= GameData.BuyGridCoins;
                RewardGridLine();
            }
            else
            {
                // Show Toast Not enough coins
                GameManager.Instance.ReportEvent("CLICK_BUY_GRID_LINE_FAILED");
                ShowToast("You don't have enough coins!");

            }
        }
        void OnBuyGridLineFromAds()
        {
            GameManager.Instance.ReportEvent("CLICK_BUY_GRID_LINE_BY_ADS");
            if (AdsManagerBase.Instance == null)
                return;
            if (!AdsManagerBase.Instance.IsReadyRewarded())
            {
                ShowToast("No ads available right now!");
                return;
            }
            // Prevent double clicks
            btnWatchAd_Gridline.interactable = false;
            AdsManagerBase.Instance.ShowRewarded(() =>
            {
                RewardGridLine();
            });
        }
        void RewardGridLine()
        {
            GameManager.Instance.ReportEvent(
                "GIVE_GRID_LINE",
                new Dictionary<string, string>() { { "REWARD_HINT", $"{1}" } }
                );
            GameData.GridLines.Value++;
            ClosePopup(gridlinePopup);
        }

        public void UpdateDebugStatus(string status)
        {
            LogToScreen("[JS→Unity] " + status);
            if (debugStatusText != null)
            {
                debugStatusText.text = "Status: " + status;
            }
        }

        void OnBuyAgentFromStars()
        {
            LogToScreen("=== OnBuyAgentFromStars START ===");
            LogToScreen("Step 1: Button clicked in Unity");

            try
            {
                GameManager.Instance.ReportEvent("CLICK_BUY_AGENT_BY_STARS");
                LogToScreen("Step 2: ReportEvent sent");
            }
            catch (System.Exception e)
            {
                LogToScreen($"ERROR at ReportEvent: {e.Message}");
            }

            // Call JavaScript bridge for Telegram Stars
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                LogToScreen("Step 3: WEBGL build → Calling JS BuyAgentWithStars()");
                BuyAgentWithStars();
                LogToScreen("Step 4: JS call returned (no exception)");
            }
            catch (System.Exception e)
            {
                LogToScreen($"ERROR calling JS: {e.GetType().Name}: {e.Message}");
            }
#else
            LogToScreen("Step 3: EDITOR build → Calling RewardAgent() directly");
            RewardAgent();
#endif
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void BuyAgentWithStars();

        /// <summary>
        /// Called from JS via SendMessage('Canvas-UIManger', 'RewardAgent') after successful payment.
        /// </summary>
        public void RewardAgent()
        {
            LogToScreen("=== RewardAgent CALLED ===");
            try
            {
                LogToScreen($"Agent count BEFORE: {GameData.Agent.Value}");
                GameManager.Instance.ReportEvent(
                    "GIVE_AGENT",
                    new Dictionary<string, string>() { { "REWARD_AGENT", $"{1}" } }
                );
                GameData.Agent.Value++;
                LogToScreen($"Agent count AFTER: {GameData.Agent.Value}");
                ClosePopup(agentBoosterPanel);
                LogToScreen("RewardAgent completed successfully");
            }
            catch (System.Exception e)
            {
                LogToScreen($"ERROR in RewardAgent: {e.Message}");
            }
        }

        void OpenPopup(VayuPopup popup, bool showCoinPanel = false)
        {
            if (popup.isOpen)
                return;
            popup.Open();
            if (showCoinPanel)
            { 
                coinPanel.Open(); 
            }
            else
            {
                if(coinPanel.isOpen)
                    coinPanel.Close();
                else
                    coinPanel.SetToClose();
            }
        }
        void ClosePopup(VayuPopup popup)
        {
            popup.Close();
            coinPanel.SetToClose();
        }
        private void TryShowInterstitial()
        {

            if (GameData.TotalGamePlay <= GameData.NoInterstitialAdsFirstGameCount)
                return;

            int gamesAfterNoAds = GameData.TotalGamePlay - GameData.NoInterstitialAdsFirstGameCount;

            if (gamesAfterNoAds % GameData.ShowInterstitialAdsParGameCount != 0)
                return;

            if (AdsManagerBase.Instance.IsReadyInterstitial())
            {
                AdsManagerBase.Instance.ShowInterstitial();
            }
        }
        public void ShowToast(string msg, float time = 2.5f, bool playSound = true)
        {

            if (toastCoroutine != null)
                StopCoroutine(toastCoroutine);
            toastCoroutine = StartCoroutine(ShowingToastAnimation(msg, time, playSound));
        }
        Coroutine toastCoroutine;
        const float toastAnimTime = .5f;
        IEnumerator ShowingToastAnimation(string msg, float showTime = 1.5f, bool playSound = true)
        {
            Debug.Log("Toast : " + msg);
            toastLbl.text = LevelEditorExtensionMethods.WrapText(msg, 70);
            DOTween.Kill(toastView);
            toastView.alpha = 0;
            yield return new WaitForSecondsRealtime(.1f);
            var size = toastView.GetComponent<RectTransform>().sizeDelta;

            toastView.DOFade(1f, toastAnimTime * 0.5f).SetEase(Ease.InOutBack).SetUpdate(true); // ignore time scale
            yield return new WaitForSecondsRealtime(showTime + toastAnimTime);
            toastView.DOFade(0f, toastAnimTime * 0.5f).SetEase(Ease.InOutBack).SetUpdate(true); toastCoroutine = null;
        }
    }
}