using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketToys.WaterRingToss
{
    public sealed class PressControl : MonoBehaviour, IPointerDownHandler
    {
        public Action Pressed;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && GetComponent<Button>().interactable) Pressed?.Invoke();
        }
    }

    public sealed class SpringSlider : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData) { GetComponent<Slider>().value = 0f; }
        void OnDisable() { GetComponent<Slider>().value = 0f; }
    }
}
