using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VPObject/VPColorObject")]
    [RequireComponent(typeof(Image))]
    public class VPImageColorObject : VPObject
    {
        public Color openColor = Color.white;
        public Color closeColor = Color.clear;
        [SerializeField] protected Image image;

        void Awake()
        {
            AutoAssignImage();
        }
        protected virtual void OnValidate()
        {
            AutoAssignImage();
        }
        void AutoAssignImage()
        {
            if (!image)
                image = GetComponent<Image>();
        }
        // =========================
        // DOTWEEN CREATION
        // =========================
        protected override Tween CreateOpenTween()
        {
            image.color = closeColor;
            image.raycastTarget = true;

            return image.DOColor(openColor, animTime);
        }

        protected override Tween CreateCloseTween()
        {
            image.color = openColor;
            image.raycastTarget = false;

            return image.DOColor(closeColor, animTime);
        }

        // =========================
        // STATE
        // =========================
        public override void SetToOpen()
        {
            KillTween();
            image.color = openColor;
            image.raycastTarget = true;
        }

        public override void SetToClose()
        {
            KillTween();
            image.color = closeColor;
            image.raycastTarget = false;
        }

        public override void SetCurrentAsOpen()
        {
            openColor = image.color;
        }

        public override void SetCurrentAsClose()
        {
            closeColor = image.color;
        }
    }
}