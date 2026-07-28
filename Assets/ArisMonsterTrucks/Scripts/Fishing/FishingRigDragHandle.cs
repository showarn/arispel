using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArisMonsterTrucks.Fishing
{
    public sealed class FishingRigDragHandle
        : MonoBehaviour,
            IBeginDragHandler,
            IDragHandler
    {
        private RectTransform coordinateSpace;
        private Action<Vector2> moved;

        public void Initialize(
            RectTransform space,
            Action<Vector2> onMoved
        )
        {
            coordinateSpace = space;
            moved = onMoved;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Move(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Move(eventData);
        }

        private void Move(PointerEventData eventData)
        {
            if (
                coordinateSpace == null
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    coordinateSpace,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                )
            )
            {
                return;
            }
            moved?.Invoke(localPoint);
        }
    }
}
