
using UnityEngine;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VPObject/VPScaleObject")]
    public class VPScaleObject : VPObject
    {
        public Vector3 openScale = Vector3.one;
        public Vector3 closeScale = Vector3.zero;

        protected override Tween CreateOpenTween()
        {
            transform.localScale = closeScale;
            return transform.DOScale(openScale, animTime);
        }

        protected override Tween CreateCloseTween()
        {
            transform.localScale = openScale;
            return transform.DOScale(closeScale, animTime);
        }

        public override void SetToOpen()
        {
            KillTween();
            transform.localScale = openScale;
        }

        public override void SetToClose()
        {
            KillTween();
            transform.localScale = closeScale;
        }

        public override void SetCurrentAsOpen()
        {
            openScale = transform.localScale;
        }

        public override void SetCurrentAsClose()
        {
            closeScale = transform.localScale;
        }
    }
}