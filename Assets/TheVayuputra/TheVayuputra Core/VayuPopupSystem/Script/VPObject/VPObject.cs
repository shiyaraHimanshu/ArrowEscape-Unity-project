using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace TheVayuputra.Core
{
    [AddComponentMenu("UI/VayuPopup/VPObject/VPObject")]
    public abstract class VPObject : MonoBehaviour
    {
        [Header("Timing")]
        [Range(0.01f, 5f)]
        public float animTime = 0.3f;

        [Range(0f, 5f)]
        public float animDelayTimeOpen = 0f;

        [Range(0f, 5f)]
        public float animDelayTimeClose = 0f;

        [Header("Ease")]
        public Ease openEase = Ease.OutBack;
        public Ease closeEase = Ease.InBack;

        protected Tween currentTween;

        // =========================
        // DOTWEEN APPEND API
        // =========================
        public void AppendOpen(Sequence seq)
        {
            Tween tween = CreateOpenTween();
            if (tween == null) return;

            tween.SetDelay(animDelayTimeOpen)
                 .SetEase(openEase);

            seq.Join(tween);
            currentTween = tween;
        }

        public void AppendClose(Sequence seq)
        {
            Tween tween = CreateCloseTween();
            if (tween == null) return;

            tween.SetDelay(animDelayTimeClose)
                 .SetEase(closeEase);

            seq.Join(tween);
            currentTween = tween;
        }

        // =========================
        // ABSTRACT CREATION
        // =========================
        protected abstract Tween CreateOpenTween();
        protected abstract Tween CreateCloseTween();

        // =========================
        // STATE HELPERS
        // =========================
        public virtual void SetToOpen() { }
        public virtual void SetToClose() { }

        public virtual void SetCurrentAsOpen() { }
        public virtual void SetCurrentAsClose() { }

        public bool IsPlaying =>
            currentTween != null && currentTween.IsPlaying();

        protected void KillTween()
        {
            if (currentTween != null && currentTween.IsActive())
                currentTween.Kill();

            currentTween = null;
        }
    }
}