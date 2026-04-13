using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using TheVayuputra.Core;
namespace ArrowGame
{
    public class LevelManager : MonoBehaviour
    {
        [Header("ColorSetting")]
        public List<Color> arrowColors = new()
        {
            new Color32(94,230,255,255),
            new Color32(108,255,181,255),
            new Color32(111,168,255,255),
            new Color32(180,140,255,255),
            new Color32(255,158,122,255),
            new Color32(255,217,102,255),
            new Color32(255,127,168,255),
            new Color32(77,255,210,255),
            new Color32(201,182,255,255),
            new Color32(182,255,106,255)
        };
        [Header("Level Setting")]
        public int totalLevel = 5;
        public int repeatLevelFrom = 2;

        public GridManager gridManager;
        public ArrowManager arrowManager;
        public PlayerInputManager playerInputManager;
        public UiManager uiManager;
        [Header("Tutorial")]
        public int tutorialLevel = 1;
        public GameObject tutorialHandPrefab;

        [Header("Debug")]
        public LevelData levelData = null;
        public static LevelManager Instance;

        public int maxLife = 3;
        public ObservableValue<int> currentLife;
        public event Action OnLevelInit;
        public event Action OnLevelStart;
        public event Action OnLevelFailed;
        public event Action OnLevelCompleted;
        private bool isTutorialRunning = false;
        private GameObject tutorialHandObject;
        public ObservableValue<bool> isShowGridLines = new ObservableValue<bool>(false);
        public bool isGridLineUnlockedForLevel;
        //Sorting Order
        public const int gridLineSortingOrder = 0;
        public const int arrowSortingOrderNormal = 10; // Line =10 and Head= +1 = 11
        public const int arrowSortingOrderMoving = 20; // Line =20 and Head=+1 21
        bool isLevelFailed = false;
        private void Awake()
        {
            Instance = this;
        }

        public void LoadCurrentLevel()
        {
            GameManager.Instance.ReportEvent("LEVEL_START", new Dictionary<string, string>() { { "CurrentLevelIndex", $"{GameData.CurrentLevel}" } });
            StartCoroutine(StartingCurrentLevel());
        }
        private LevelData GetLevel(int levelNo)
        {
            if (totalLevel <= 0)
            {
                Debug.LogError("TotalLevel must be greater than 0");
                return null;
            }

            int loadLevelNo = levelNo;

            // If level exceeds total, repeat from repeatLevelFrom
            if (levelNo > totalLevel)
            {
                int repeatStartIndex = Mathf.Clamp(repeatLevelFrom, 1, totalLevel);
                int repeatCount = totalLevel - repeatStartIndex + 1;

                int offset = (levelNo - totalLevel - 1) % repeatCount;
                loadLevelNo = repeatStartIndex + offset;
            }

            string path = $"Levels/Level_{loadLevelNo}";
            TextAsset levelText = Resources.Load<TextAsset>(path);

            if (levelText == null)
            {
                Debug.LogError($"Level file not found at Resources/{path}");
                return null;
            }
            Debug.Log($"Level{levelNo} Load From {path}");
            return JsonUtility.FromJson<LevelData>(levelText.text);
        }
        [ContextMenu("LoadNextLevel")]
        public void LoadNextLevel()
        {
            UiManager.Instance.loadingManager.Show();
            GameData.CurrentLevel++;
            LoadCurrentLevel();
        }
        [ContextMenu("LoadPreLevel")]
        public void LoadPreLevel()
        {
            UiManager.Instance.loadingManager.Show();
            GameData.CurrentLevel = (GameData.CurrentLevel - 1) <= 1 ? 1 : (GameData.CurrentLevel - 1);
            LoadCurrentLevel();
        }
        private IEnumerator StartingCurrentLevel()
        {
            EndTutorial();
            isShowGridLines.Value = false;
            playerInputManager.SetInputBlocked(true);
            levelData = GetLevel(GameData.CurrentLevel);
            OnLevelInit?.Invoke();
            gridManager.CreateGrid(Mathf.Max(1, levelData.GridXSize), Mathf.Max(1, levelData.GridYSize));
            arrowManager.SetupArrowList(levelData.Arrows);
            ResetLife();
            uiManager.winParticle.SetActive(false);
            // Do Animation
            yield return new WaitForSeconds(.5f);
            UiManager.Instance.loadingManager.Hide();
            playerInputManager.LevelStartAnimation(1f);
            arrowManager.PlayArrowSetupEffect(0.05f, 0.75f);
            yield return new WaitForSeconds(0.5f);
            OnLevelStart?.Invoke();
            playerInputManager.SetInputBlocked(false);
            if (GameData.CurrentLevel == tutorialLevel)
            {
                StartTutorial();
            }
        }
        [ContextMenu("ResetAll")]
        private void ResetAll()
        {
            gridManager.ResetAll();
            arrowManager.ResetAll();
        }
        public void ResetLife()
        {
            currentLife.Value = maxLife;
            isLevelFailed = false;
            playerInputManager.SetInputBlocked(false);
        }
        public void DecreaseLife()
        {
            currentLife.Value--;
            Debug.Log($"Life Left : {currentLife}");
            CheckGameOver();
            uiManager.damageOverlayUI.PlayDamageFlash();
        }
        public void CheckGameOver()
        {
            if (currentLife.Value <= 0)
            {
                LevelFailed();
            }
            else if (arrowManager.remandingArrow.Value <= 0)
            {
                StartCoroutine(LevelCompleted());
            }
        }
        public void StartTutorial()
        {
            if (tutorialHandObject)
                Destroy(tutorialHandObject);

            GameManager.Instance.ReportEvent("SHOW_TUTORIAL_HAND");
            var arrow = arrowManager.GetEscapedPossibleArrow();
            if (arrow)
            {
                tutorialHandObject = Instantiate<GameObject>(tutorialHandPrefab, transform);
                Vector3 pos = arrow.headCell.transform.position;
                pos.y += 2;
                tutorialHandObject.transform.position = pos;
                isTutorialRunning = true;
            }
        }
        public void EndTutorial()
        {
            if (isTutorialRunning)
            {
                if (tutorialHandObject)
                    Destroy(tutorialHandObject);
                isTutorialRunning = false;
            }
        }
        void LevelFailed()
        {
            if (isLevelFailed)
                return;
            isLevelFailed = true;
            GameManager.Instance.ReportEvent("LEVEL_FAILED", new Dictionary<string, string>() { { "CurrentLevelIndex", $"{GameData.CurrentLevel}" } });
            OnLevelFailed?.Invoke();
            playerInputManager.SetInputBlocked(true);
            // Invoke(nameof(SetupCurrentLevel), 10f);
        }
        private IEnumerator LevelCompleted()
        {
            GameManager.Instance.ReportEvent("LEVEL_COMPLETED", new Dictionary<string, string>() { { "CurrentLevelIndex", $"{GameData.CurrentLevel}" } });
            isShowGridLines.Value = false;
            playerInputManager.SetInputBlocked(true);
            uiManager.winParticle.SetActive(true);
            playerInputManager.LevelCompletedAnimation(1f);
            yield return new WaitForSeconds(.2f);
            yield return gridManager.CloseGridFromCenter(.5f);
            yield return new WaitForSeconds(0.5f);
            GameData.CurrentLevel++;
            OnLevelCompleted?.Invoke();
        }

        [ContextMenu("ToggleGridLines")]
        public void ToggleGridLines()
        {
            isShowGridLines.Value = !isShowGridLines.Value;
        }
        public void ShowEscapeArrowHint()
        {
            var arrow = arrowManager.GetEscapedPossibleArrow();
            if (arrow)
            {
                playerInputManager.GetCameraFocusAtPosition(arrow.headCell.transform.position);
                arrow.isHighlighted.Value = true;
            }
        }

        public void UseAgentBooster()
        {
            if (solvingLevel != null)
                StopCoroutine(solvingLevel);
            solvingLevel = StartCoroutine(ExecutingAgentBooster());
        }

        IEnumerator ExecutingAgentBooster()
        {
            playerInputManager.SetInputBlocked(true);
            int totalArrows = arrowManager.remandingArrow.Value;
            int arrowsToClear = 10;

            // If 7 or fewer arrows, clear all to complete the level
            if (totalArrows <= 7)
            {
                arrowsToClear = totalArrows;
            }

            int cleared = 0;
            while (cleared < arrowsToClear && arrowManager.remandingArrow.Value > 0)
            {
                Arrow nextArrow = arrowManager.GetEscapedPossibleArrow();
                if (nextArrow)
                {
                    nextArrow.EscapeArrow();
                    cleared++;
                    yield return new WaitForSeconds(.25f);
                }
                else
                {
                    break;
                }
            }
            playerInputManager.SetInputBlocked(false);
            solvingLevel = null;
        }

        Coroutine solvingLevel;
        [ContextMenu("QuickCompeteLevelForTesting")]
        public void QuickCompeteLevelForTesting()
        {
            if (solvingLevel != null)
                StopCoroutine(solvingLevel);
            solvingLevel = StartCoroutine(AutoSolvingForTesting());
        }
        IEnumerator AutoSolvingForTesting()
        {
            playerInputManager.SetInputBlocked(true);
            Arrow nextArrow = null;
            do
            {
                nextArrow = arrowManager.GetEscapedPossibleArrow();
                if (nextArrow) nextArrow.EscapeArrow();
                yield return new WaitForSeconds(.2f);
            }
            while (nextArrow != null && arrowManager.remandingArrow.Value > 1);
            yield return new WaitForSeconds(1f);
            nextArrow = arrowManager.GetEscapedPossibleArrow();
            if (nextArrow) nextArrow.EscapeArrow();

        }
        public Color GetArrowColor(int colorIndex)
        {
            if (arrowColors == null || arrowColors.Count == 0)
                return Color.white;

            // Handle negative & large indices safely
            int safeIndex = colorIndex % arrowColors.Count;

            return arrowColors[safeIndex];
        }
    }

    [System.Serializable]
    public class LevelData
    {
        public int GridXSize;
        public int GridYSize;
        public List<DataArrow> Arrows;
    }
}