using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ArrowGame
{
    public class DamageOverlayUI : MonoBehaviour
    {
        [SerializeField] Image damageImage;

        [Header("Flash Look")]
        [SerializeField] Color flashColor = new Color(1f, 0f, 0f, 1f);

        [Header("Vibration Settings")]
        [SerializeField] int flashes = 4;          // number of rapid flashes
        [SerializeField] float flashInterval = 0.03f; // speed of vibration
        [SerializeField] float maxAlpha = 0.8f;

        Tween flashTween;

        void Awake()
        {
            damageImage.enabled = true;
            SetAlpha(0f);
        }
        [ContextMenu("Play Damage Flash")]
        public void PlayDamageFlash()
        {
            flashTween?.Kill();
            SetAlpha(0f);

            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < flashes; i++)
            {
                seq.Append(DOTween.To(
                    () => damageImage.color.a,
                    a => SetAlpha(a),
                    maxAlpha,
                    flashInterval));

                seq.Append(DOTween.To(
                    () => damageImage.color.a,
                    a => SetAlpha(a),
                    0f,
                    flashInterval));
            }

            flashTween = seq.SetUpdate(true);
        }

        void SetAlpha(float a)
        {
            damageImage.color = new Color(
                flashColor.r,
                flashColor.g,
                flashColor.b,
                a
            );
        }

        void OnDisable()
        {
            flashTween?.Kill();
        }
    }
}
