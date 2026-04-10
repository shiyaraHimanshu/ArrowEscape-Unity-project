using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("UI/VayuPopup/VPObject/VPCanvasGroupObject")]
    public class VPCanvasGroupObject : VPObject
    {
        [Range(0f, 1f)]
        public float openAlpha = 1f;

        [Range(0f, 1f)]
        public float closeAlpha = 0f;
        [SerializeField] protected CanvasGroup canvasGroup;
        protected virtual void OnValidate()
        {
            AutoAssignCanvasGroup();
        }
        void Awake()
        {
            AutoAssignCanvasGroup();
        }
        void AutoAssignCanvasGroup()
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
        }
        // =========================
        // DOTWEEN CREATION
        // =========================
        protected override Tween CreateOpenTween()
        {
            canvasGroup.alpha = closeAlpha;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            return canvasGroup.DOFade(openAlpha, animTime);
        }

        protected override Tween CreateCloseTween()
        {
            canvasGroup.alpha = openAlpha;

            Tween tween = canvasGroup.DOFade(closeAlpha, animTime);

            // Disable interaction AFTER animation completes
            tween.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            });

            return tween;
        }

        // =========================
        // STATE
        // =========================
        public override void SetToOpen()
        {
            KillTween();
            canvasGroup.alpha = openAlpha;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public override void SetToClose()
        {
            KillTween();
            canvasGroup.alpha = closeAlpha;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public override void SetCurrentAsOpen()
        {
            openAlpha = canvasGroup.alpha;
        }

        public override void SetCurrentAsClose()
        {
            closeAlpha = canvasGroup.alpha;
        }
    }

}