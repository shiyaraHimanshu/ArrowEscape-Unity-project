using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace TheVayuputra.Core
{

    public class BlinkImage : MonoBehaviour
    {
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] float minAlpha = 0.2f;
        Image image;
        Tween blinkTween;
        Color originalColor;
        void Awake()
        {
            image = GetComponent<Image>();
            originalColor = image.color;
        }
        void OnEnable()
        {
            StartBlink();
        }
        void OnDisable()
        {
            StopBlink();
        }
        public void StartBlink()
        {
            StopBlink();

            blinkTween = image
                .DOFade(minAlpha, fadeDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        public void StopBlink()
        {
            if (blinkTween != null && blinkTween.IsActive())
                blinkTween.Kill();

            // Reset alpha
            image.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                originalColor.a
            );
        }
    }
}