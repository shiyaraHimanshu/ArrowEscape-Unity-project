using System.Collections;
using UnityEngine;
using TheVayuputra.Core;
namespace ArrowGame
{
    public class LoadingManager : MonoBehaviour
    {

        [SerializeField] private VayuPopup loadingPanel;
        private bool isOpen = false;
        private Coroutine hideRoutine;
        public void Show(bool isInstant = false)
        {
            if (isOpen)
                return;
            isOpen = true;
            if (isInstant)
                loadingPanel.SetToOpen();
            else
                loadingPanel.Open();
        }
        public void Hide(bool isInstant = false)
        {
            if (!isOpen)
                return;
            isOpen = false;
            if (isInstant)
                loadingPanel.SetToClose();
            else
                loadingPanel.Close();
        }

        public void ShowFor(float duration)
        {
            Show();
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideAfter(duration));
        }

        IEnumerator HideAfter(float t)
        {
            yield return new WaitForSeconds(t);
            Hide();
            hideRoutine = null;
        }
    }
}