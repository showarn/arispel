using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArisMonsterTrucks.Fishing
{
    public sealed class FishingSwipeHandler :
        MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler
    {
        private Vector2 start;
        private Action<int> swiped;

        public void Initialize(Action<int> onSwiped)
        {
            swiped = onSwiped;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            start = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float delta = eventData.position.x - start.x;
            if (Mathf.Abs(delta) < 65f)
            {
                return;
            }
            swiped?.Invoke(delta < 0f ? 1 : -1);
        }
    }
}
