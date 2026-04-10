using UnityEngine;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArrowGameLevelEditor
{
    public class LevelBatchScreenshotter : MonoBehaviour
    {
        [Header("Folders")]
        [Tooltip("Folder that contains level JSON files")]
        public string levelsFolderPath;

        [Tooltip("Folder where screenshots will be saved")]
        public string screenshotFolderPath;

        [Header("Settings")]
        public float delayAfterLoad = 0.3f;
        public float delayAfterScreenshot = 0.5f;

        private LevelEditorController levelEditor;

#if UNITY_EDITOR
        [ContextMenu("📸 Start Batch Screenshot")]
        public void StartBatchScreenshot()
        {
            if (string.IsNullOrEmpty(levelsFolderPath))
            {
                levelsFolderPath = EditorUtility.OpenFolderPanel(
                    "Select Levels Folder",
                    "",
                    ""
                );
            }

            if (string.IsNullOrEmpty(levelsFolderPath))
            {
                Debug.LogError("Levels folder not selected.");
                return;
            }

            if (string.IsNullOrEmpty(screenshotFolderPath))
            {
                screenshotFolderPath = Path.Combine(levelsFolderPath, "Screenshots");
            }

            if (!Directory.Exists(screenshotFolderPath))
                Directory.CreateDirectory(screenshotFolderPath);

            levelEditor = FindObjectOfType<LevelEditorController>();

            if (levelEditor == null)
            {
                Debug.LogError("LevelEditorController not found in scene!");
                return;
            }

            StartCoroutine(BatchScreenshotCoroutine());
        }
#endif

        IEnumerator BatchScreenshotCoroutine()
        {
            string[] levelFiles = Directory.GetFiles(levelsFolderPath, "*.json");

            if (levelFiles.Length == 0)
            {
                Debug.LogWarning("No level JSON files found.");
                yield break;
            }

            Debug.Log($"📸 Starting screenshot for {levelFiles.Length} levels");

            foreach (string levelPath in levelFiles)
            {
                Debug.Log("Loading level: " + levelPath);

                yield return levelEditor.StartCoroutine(
                    levelEditor.OpeningLevel(levelPath)
                );

                // Allow UI + camera to settle
                yield return new WaitForSeconds(delayAfterLoad);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                string levelName = Path.GetFileNameWithoutExtension(levelPath);
                string screenshotPath = Path.Combine(
                    screenshotFolderPath,
                    levelName + ".png"
                );

                ScreenCapture.CaptureScreenshot(screenshotPath);
                Debug.Log("📸 Saved screenshot: " + screenshotPath);

                yield return new WaitForSeconds(delayAfterScreenshot);
            }

            Debug.Log("✅ Batch screenshot process completed");
        }
    }
}
