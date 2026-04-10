using UnityEngine;
using UnityEngine.UI;
namespace ArrowGame
{
    [RequireComponent(typeof(Graphic))]
    public class UIThemeColorObject : MonoBehaviour
    {
        public Color dayColor = Color.white;
        public Color nightColor = Color.white;

        private Graphic graphic;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
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
            graphic.color = isDayTheme ? dayColor : nightColor;
        }
    }
}