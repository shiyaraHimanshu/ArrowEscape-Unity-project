using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

namespace ArrowGameLevelEditor
{
    public class LevelEditorController : MonoBehaviour
    {
       
        public GridManager gridManager;
        public bool enableScreenshotSave=true;
        [Header("UI")]
        public CanvasGroup levelContent;      // New / Open / Save
        public TextMeshProUGUI levelNameLbl;        
        [Space]
        public CanvasGroup loadingCanvas;  // Arrow inspector
        public CanvasGroup toastView;
        public TextMeshProUGUI toastLbl;
        public static LevelEditorController levelEditorController;
        void Awake()
        {
            if(levelEditorController==null)
                levelEditorController = this;
        }
        void Start()
        {
            OnClick_NewLevel();
            SetLoading(false);            
        }
        public void OnClick_NewLevel()
        {
            var emptyLevel = new LevelData
            {
                GridXSize = 10,
                GridYSize = 10,
                Arrows=new List<ArrowData>()
            };
            levelNameLbl.text="UntitledLevel";
            gridManager.CreateGrid(10,10);
        }
        public void OnClick_SaveLevel()
        {
            
            if(gridManager.arrows.Count == 0)
            {
                ShowToast("Invalid Level : No Arrow Added");
                return;
            }
            if(gridManager.arrows.Exists(x=>!x.isValidArrow))
            {
                ShowToast("Invalid Level : Dashed Arrow are Invalid");
                return;
            }


            // Build level data
            LevelData levelData = new LevelData
            {
                GridXSize = gridManager.gridX,
                GridYSize = gridManager.gridY,
                Arrows = new List<ArrowData>()
            };

            // Export arrows
            foreach (var arrow in gridManager.arrows)
            {
                if (arrow == null || arrow.length < 2)
                    continue;

                ArrowData arrowData = new ArrowData();
                arrow.GetData(arrowData);
                levelData.Arrows.Add(arrowData);
            }

            // Open save dialog (Editor) or fallback to persistent path (runtime)
            string path = "";
#if UNITY_EDITOR
            path = EditorUtility.SaveFilePanel("Save Level", "", levelNameLbl.text.Replace(".json", ""), "json");
#else
            string filename = levelNameLbl.text.Replace(".json", "") + ".json";
            path = Path.Combine(Application.persistentDataPath, filename);
#endif

            if (string.IsNullOrEmpty(path))
                return;

            // Serialize & save
            string json = JsonUtility.ToJson(levelData, true);
            File.WriteAllText(path, json);

            Debug.Log($"Level saved successfully: {path}");
        }
        public void OnClick_OpenLevel()
        {
            // Open file dialog (Editor) or fallback (runtime)
            string[] paths = new string[0];
#if UNITY_EDITOR
            string openPath = EditorUtility.OpenFilePanel("Open Level", "", "json");
            if (!string.IsNullOrEmpty(openPath))
                paths = new[] { openPath };
#else
            string defaultPath = Path.Combine(Application.persistentDataPath, levelNameLbl.text.Replace(".json", "") + ".json");
            if (File.Exists(defaultPath))
                paths = new[] { defaultPath };
#endif

            if (paths.Length == 0)
            {
                Debug.Log("Failed OnClick_OpenLevel");
                return;
            }

            StartCoroutine(OpeningLevel(paths[0]));
        }
        public  IEnumerator OpeningLevel(string path)
        {
            SetLoading(true);

            levelNameLbl.text= (new FileInfo(path)).Name;
            string json = File.ReadAllText(path);
            var loadedLevel = JsonUtility.FromJson<LevelData>(json);

            gridManager.CreateGrid(loadedLevel.GridXSize,loadedLevel.GridYSize);            
            yield return new WaitForSeconds(.5f);
            gridManager.SetupLevelArrows(loadedLevel.Arrows);        
            gridManager.UpdateControlUI();
            yield return new WaitForSeconds(.5f);
            SetLoading(false);            
            if(enableScreenshotSave)
            {
                // Wait for rendering to finish
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                string directory = Path.GetDirectoryName(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string screenshotPath = Path.Combine(directory, fileName + ".png");
                ScreenCapture.CaptureScreenshot(screenshotPath);
                Debug.Log("Screenshot saved: " + screenshotPath);
            }

        }
   
        public void SetLoading(bool b)
        {
            loadingCanvas.alpha=b?1:0;
            loadingCanvas.SetCanvasInteractable(b);
        }  
        public void ShowToast(string msg, float time = 2.5f, bool playSound = true)
        {

            if (toastCoroutine != null)
                StopCoroutine(toastCoroutine);
            toastCoroutine = StartCoroutine(ShowingToastAnimation(msg, time, playSound));
        }
        Coroutine toastCoroutine;
        const float toastAnimTime = .5f;
        IEnumerator ShowingToastAnimation(string msg, float showTime = 2.5f, bool playSound = true)
        {
            Debug.Log("Toast : " + msg);

            toastLbl.text = LevelEditorExtensionMethods.WrapText(msg,70);
            DOTween.Kill(toastView);
            toastView.alpha = 0;
            yield return new WaitForSecondsRealtime(.1f);
            var size = toastView.GetComponent<RectTransform>().sizeDelta;

            toastView.DOFade(1f, toastAnimTime * 0.5f).SetEase(Ease.InOutBack).SetUpdate(true); // ignore time scale
            yield return new WaitForSecondsRealtime(showTime + toastAnimTime);
            toastView.DOFade(0f, toastAnimTime * 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);toastCoroutine = null;
        }

    }
    public static class LevelEditorExtensionMethods
    {
        public static void SetCanvasInteractable(this CanvasGroup cg, bool isEnable)
        {
            cg.interactable = isEnable;
            cg.blocksRaycasts = isEnable;
        }        
        public static string WrapText(string sentence, int columnWidth)
        {

            string[] words = sentence.Split(' ');

            System.Text.StringBuilder newSentence = new System.Text.StringBuilder();

            string line = "";
            for (int i = 0; i < words.Length; i++)
            {
                if ((line + words[i]).Length > columnWidth)
                {
                    newSentence.AppendLine(line);
                    line = "";
                }

                line += string.Format("{0} ", words[i]);
            }

            if (line.Length > 0)
                newSentence.Append(line);

            return newSentence.ToString();
        }
    }
}
