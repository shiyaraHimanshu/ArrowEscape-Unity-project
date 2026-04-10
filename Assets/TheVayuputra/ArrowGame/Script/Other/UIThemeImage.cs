using UnityEngine;
using UnityEngine.UI;
namespace ArrowGame
{
    [RequireComponent(typeof(Image))]
    public class UIThemeImage : MonoBehaviour
    {
        public Sprite dayImage;
        public Sprite nightImage;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            GameData.IsDayTheme.AddListener(OnThemeChange);
        }

        private void OnDisable()
        {
            GameData.IsDayTheme.RemoveListener(OnThemeChange);
        }

        private void OnThemeChange(bool isDayTheme)
        {
            image.sprite = isDayTheme ? dayImage : nightImage;
        }
    }
}
