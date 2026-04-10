using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VayuPopupManager")]
    public class VayuPopupManager : MonoBehaviour
    {
        public static VayuPopupManager intance
        {
            get
            {
                if (_intance == null)
                {
                    GameObject temp = new GameObject();
                    temp.name = "VayuPopupManager";
                    _intance = temp.AddComponent<VayuPopupManager>();
                }
                return _intance;
            }
        }
        static VayuPopupManager _intance = null;
        public List<VayuPopup> allPopupList;
        public List<VayuPopup> openPopupList;
        private void OnEnable()
        {
            if (_intance == null)
            {
                _intance = this;
                allPopupList = new List<VayuPopup>();
                openPopupList = new List<VayuPopup>();
                DontDestroyOnLoad(this.gameObject);
            }

        }
        public void RegisterPopup(VayuPopup popup)
        {
            if (!allPopupList.Contains(popup))
            {
                allPopupList.Add(popup);
            }
            allPopupList.RemoveAll(x => x == null);
        }
        public void AddToOpenPopup(VayuPopup popup)
        {
            allPopupList.RemoveAll(x => x == null);
            openPopupList.RemoveAll(x => x == null);
            openPopupList.Add(popup);
        }
        public void RemoveToOpenPopup(VayuPopup popup)
        {
            allPopupList.RemoveAll(x => x == null);
            openPopupList.RemoveAll(x => x == null);
            openPopupList.Remove(popup);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }
        }
        private void HandleEscapeKey()
        {
            VayuPopup popup = CurrentCloseOnEscPopup();
            if (popup != null)
            {
                popup.Close();
            }
        }
        public VayuPopup CurrentCloseOnEscPopup()
        {
            openPopupList.RemoveAll(x => x == null);
            if (openPopupList.Count > 0 && openPopupList[openPopupList.Count - 1].isOpen &&
            openPopupList[openPopupList.Count - 1].closeOnEsc && !openPopupList[openPopupList.Count - 1].isPlaying)
            {
                return openPopupList[openPopupList.Count - 1];
            }
            return null;
        }
    }
}