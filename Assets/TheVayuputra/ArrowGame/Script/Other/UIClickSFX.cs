using System.Collections;
using System.Collections.Generic;
using ArrowGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ArrowGame
{
    public class UIClickSFX : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            SoundManager.Instance.PlayUIClickSFX();
        }
    }
}