using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VPObject/VPMoveObject")]
    public class VPMoveObject : VPObject
    {
        public Vector3 openPos;
        public Vector3 closePos;
        [SerializeField]
        protected RectTransform rect;

        void Awake()
        {
            AutoAssignRectTransform();
        }
        protected virtual void OnValidate()
        {
            AutoAssignRectTransform();
        }
        void AutoAssignRectTransform()
        {
            if (!rect)
                rect = GetComponent<RectTransform>();
        }
        // =========================
        // DOTWEEN CREATION
        // =========================
        protected override Tween CreateOpenTween()
        {
            rect.anchoredPosition3D = closePos;
            return rect.DOAnchorPos3D(openPos, animTime);
        }

        protected override Tween CreateCloseTween()
        {
            rect.anchoredPosition3D = openPos;
            return rect.DOAnchorPos3D(closePos, animTime);
        }

        // =========================
        // STATE
        // =========================
        public override void SetToOpen()
        {
            KillTween();
            rect.anchoredPosition3D = openPos;
        }

        public override void SetToClose()
        {
            KillTween();
            rect.anchoredPosition3D = closePos;
        }

        public override void SetCurrentAsOpen()
        {
            openPos = rect.anchoredPosition3D;
        }

        public override void SetCurrentAsClose()
        {
            closePos = rect.anchoredPosition3D;
        }
    }
}