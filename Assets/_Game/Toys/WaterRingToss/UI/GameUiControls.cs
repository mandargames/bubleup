using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class PumpPress : MonoBehaviour, IPointerDownHandler
    {
        public Action Action;
        public void OnPointerDown(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) Action?.Invoke(); }
    }
    public sealed class CenterOnRelease : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData e) { GetComponent<Slider>().value = 0f; }
    }
    public sealed class StarGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear(); var rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * .48f;
            helper.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? radius : radius * .45f;
                helper.AddVert(rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r, color, Vector2.zero);
            }
            for (int i = 0; i < 10; i++) helper.AddTriangle(0, i + 1, (i + 1) % 10 + 1);
        }
    }
}
