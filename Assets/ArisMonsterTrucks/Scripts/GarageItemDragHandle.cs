using UnityEngine;
using UnityEngine.EventSystems;

namespace ArisMonsterTrucks
{
    public sealed class GarageItemDragHandle :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private FrontEndController garage;
        private GarageItemDefinition item;
        private RectTransform rectTransform;
        private Vector2 startPosition;
        private Vector2 dragDistance;

        public void Initialize(
            FrontEndController owner,
            GarageItemDefinition definition
        )
        {
            garage = owner;
            item = definition;
            rectTransform = transform as RectTransform;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (dragDistance.sqrMagnitude < 100f)
            {
                garage?.MountGarageItem(item);
            }
            dragDistance = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = rectTransform.anchoredPosition;
            dragDistance = Vector2.zero;
            rectTransform.localScale = Vector3.one * 1.08f;
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.delta / Mathf.Max(0.01f, garage.UiScaleFactor);
            dragDistance += delta;
            rectTransform.anchoredPosition += delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localScale = Vector3.one;
            if (dragDistance.sqrMagnitude >= 100f)
            {
                garage?.MountGarageItem(item);
            }
        }
    }
}
