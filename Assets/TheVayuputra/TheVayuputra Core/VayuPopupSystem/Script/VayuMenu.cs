using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VayuMenu")]
    public class VayuMenu : MonoBehaviour
    {
        public VayuPopup popup;
        GameObject blocker;
        void Start()
        {
            if (popup == null)
                popup = GetComponentInChildren<VayuPopup>();

            popup.onOpen.AddListener(OpenMenuOnly);
            popup.onClose.AddListener(CloseMenuOnly);
        }
        public void OpenMenu()
        {
            popup.Open();
        }
        void OpenMenuOnly()
        {
            if (popup == null)
            {
                Debug.Log("Popup null");
                return;
            }


            var list = popup.GetComponentsInParent<Canvas>();
            if (list.Length == 0)
                return;
            Canvas rootCanvas = list[list.Length - 1];
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].isRootCanvas)
                {
                    rootCanvas = list[i];
                    break;
                }
            }


            Canvas popupCanvas = GetOrAddComponent<Canvas>(popup.gameObject);
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingLayerID = rootCanvas.sortingLayerID;
            popupCanvas.sortingOrder = 30000;
            GetOrAddComponent<GraphicRaycaster>(popupCanvas.gameObject);

            if (blocker == null)
                blocker = new GameObject("Blocker");

            RectTransform blockerRect = GetOrAddComponent<RectTransform>(blocker);
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            Canvas blockerCanvas = GetOrAddComponent<Canvas>(blocker);
            blockerCanvas.overrideSorting = true;
            blockerCanvas.sortingLayerID = popupCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = popupCanvas.sortingOrder - 1;
            GetOrAddComponent<GraphicRaycaster>(blocker);



            // Add image since it's needed to block, but make it clear.
            Image blockerImage = GetOrAddComponent<Image>(blocker);
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(CloseMenu);
        }
        public void CloseMenu()
        {
            popup.Close();
        }
        void CloseMenuOnly()
        {

            if (popup == null)
            {
                Debug.Log("Popup null");
                return;
            }
            Canvas popupCanvas = GetOrAddComponent<Canvas>(popup.gameObject);
            popupCanvas.overrideSorting = false;
            DestroyImmediate(blocker);
        }
        T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}