using UnityEngine;
using TheVayuputra.Core;

namespace ArrowGame
{
    [AddComponentMenu("UI/Developer Panel")]
    public class DeveloperPanel : MonoBehaviour
    {
        public VayuMenu menu;
        void Start()
        {
            menu.popup.SetToClose();
            menu.CloseMenu();
        }
        public void OnClick_NextLevel()
        {
            LevelManager.Instance?.LoadNextLevel();
            menu.CloseMenu();
        }
        public void OnClick_PreviousLevel()
        {
            LevelManager.Instance?.LoadPreLevel();
            menu.CloseMenu();
        }
        public void OnClick_ReloadLevel()
        {
            LevelManager.Instance?.LoadCurrentLevel();
            menu.CloseMenu();
        }
        public void OnClick_CompleteLevel()
        {
            LevelManager.Instance?.QuickCompeteLevelForTesting();
            menu.CloseMenu();
        }
        public void OnClick_AddCoin()
        {
            GameData.Coins.Value += 500;
            LevelManager.Instance.uiManager.ShowToast("500 Coin Added");
            menu.CloseMenu();
        }
        public void OnClick_AddHint()
        {
            GameData.Hint.Value += 2;
            menu.CloseMenu();
        }
        public void OnClick_AddGrindLines()
        {
            GameData.GridLines.Value += 2;
            menu.CloseMenu();
        }
        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.N))
            {
                OnClick_NextLevel();
            }
            else if (Input.GetKeyUp(KeyCode.P))
            {
                OnClick_PreviousLevel();
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                OnClick_ReloadLevel();
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                OnClick_CompleteLevel();
            }
        }
    }
}
