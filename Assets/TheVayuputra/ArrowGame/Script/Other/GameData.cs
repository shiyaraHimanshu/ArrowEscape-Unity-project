using UnityEngine;
using UnityEngine.Rendering;
using TheVayuputra.Core;
namespace ArrowGame
{
    public class GameData
    {
        private const string SAVE_PREFIX = "ArrowGame_";
        public const int LevelWonCoins = 20;
        public const int BuyHintCoins = 100;
        public const int BuyGridCoins = 100;
        public const int BuyAgentStars = 10;
        public const int RewardedAdCoinMultiplier = 3;
        public const int NoInterstitialAdsFirstGameCount = 0; // 0 - No initial grace period
        public const int ShowInterstitialAdsParGameCount = 3; // Show an ad every 3 levels
        public static int TotalGamePlay // Use For Ads TotalGamePlay Level Success and Level Failed Bot Count
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}TotalGamePlay", 0);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}TotalGamePlay", value);
            }
        }
        public static ObservableValue<bool> IsDayTheme = new ObservableValue<bool>(SavedTheme, v => SavedTheme = v);
        private static bool SavedTheme
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}IsDayTheme", 0) == 1;
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}IsDayTheme", value ? 1 : 0);
            }
        }
        public static ObservableValue<int> Coins = new ObservableValue<int>(SavedCoins, (v) => SavedCoins = v);
        private static int SavedCoins
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}Coins", 0);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}Coins", value);
            }
        }
        public static ObservableValue<int> Hint = new ObservableValue<int>(HintCount, (v) => HintCount = v);
        private static int HintCount
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}HintArrow", 0);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}HintArrow", value);
            }
        }
        public static ObservableValue<int> GridLines = new ObservableValue<int>(GridLineCount, (v) => GridLineCount = v);
        private static int GridLineCount
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}GridLineCount", 0);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}GridLineCount", value);
            }
        }
        public static ObservableValue<int> Agent = new ObservableValue<int>(AgentCount, (v) => AgentCount = v);
        private static int AgentCount
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}AgentCount", 0);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}AgentCount", value);
            }
        }
        public static int CurrentLevel
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}CurrentLevel", 1);
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}CurrentLevel", value);
            }
        }
        public static ObservableValue<bool> MusicOn = new ObservableValue<bool>(SavedMusicOn, v => SavedMusicOn = v);
        public static ObservableValue<bool> SfxOn = new ObservableValue<bool>(SavedSfxOn, v => SavedSfxOn = v);
        public static ObservableValue<bool> VibrationOn = new ObservableValue<bool>(SavedVibrationOn, v => SavedVibrationOn = v);

        private static bool SavedMusicOn
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}MusicOn", 1) == 1;
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}MusicOn", value ? 1 : 0);
            }
        }
        private static bool SavedSfxOn
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}SfxOn", 1) == 1;
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}SfxOn", value ? 1 : 0);
            }
        }

        private static bool SavedVibrationOn
        {
            get => PlayerPrefs.GetInt($"{SAVE_PREFIX}VibrationOn", 1) == 1;
            set
            {
                PlayerPrefs.SetInt($"{SAVE_PREFIX}VibrationOn", value ? 1 : 0);
            }
        }
    }
}