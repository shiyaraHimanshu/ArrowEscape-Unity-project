using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/Popup")]
    public class VayuPopup : MonoBehaviour
    {
        public bool isOpen;
        public bool closeOnEsc = false;

        public UnityEvent onOpen;
        public UnityEvent onClose;

        public List<VPObject> lPObjects;

        private Sequence openSequence;
        private Sequence closeSequence;

        public bool isPlaying
        {
            get
            {
                return (openSequence != null && openSequence.IsActive() && openSequence.IsPlaying()) ||
               (closeSequence != null && closeSequence.IsActive() && closeSequence.IsPlaying());
            }
        }

        void OnEnable()
        {
            VayuPopupManager.intance.RegisterPopup(this);
        }

        void Start()
        {
            lPObjects.RemoveAll(x => x == null);

            if (isOpen)
                SetToOpen();
            else
                SetToClose();
        }

        // =========================
        // OPEN
        // =========================
        public void Open()
        {
            if (isPlaying) return;

            isOpen = true;
            VayuPopupManager.intance.AddToOpenPopup(this);
            KillSequences();
            openSequence = DOTween.Sequence();
            for (int i = 0; i < lPObjects.Count; i++)
            {
                lPObjects[i].AppendOpen(openSequence);
            }
            openSequence.OnComplete(() =>
            {
                onOpen?.Invoke();
            });
        }

        // =========================
        // CLOSE
        // =========================
        public void Close()
        {
            if (isPlaying) return;

            KillSequences();
            closeSequence = DOTween.Sequence();

            for (int i = 0; i < lPObjects.Count; i++)
            {
                lPObjects[i].AppendClose(closeSequence);
            }

            closeSequence.OnComplete(() =>
            {
                isOpen = false;
                onClose?.Invoke();
            });

            VayuPopupManager.intance.RemoveToOpenPopup(this);
        }

        // =========================
        // STATE
        // =========================
        public void SetToOpen()
        {
            KillSequences();
            isOpen = true;

            for (int i = 0; i < lPObjects.Count; i++)
                lPObjects[i].SetToOpen();
        }

        public void SetToClose()
        {
            KillSequences();
            isOpen = false;

            for (int i = 0; i < lPObjects.Count; i++)
                lPObjects[i].SetToClose();
        }

        // =========================
        // UTILS
        // =========================
        void KillSequences()
        {
            openSequence?.Kill();
            closeSequence?.Kill();
            openSequence = null;
            closeSequence = null;
        }
    }
}