using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArisMonsterTrucks
{
    public sealed class SwipePageHandler :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private const float SwipeThreshold = 70f;

        private Action<int> onSwipe;
        private Vector2 dragStart;

        public void Initialize(Action<int> swipeAction)
        {
            onSwipe = swipeAction;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStart = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Unity kräver en aktiv dragmottagare för att skicka begin/end-drag
            // när svepet startar på ett vanligt kort eller på bakgrunden.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float horizontalDistance = eventData.position.x - dragStart.x;
            if (Mathf.Abs(horizontalDistance) < SwipeThreshold)
            {
                return;
            }

            onSwipe?.Invoke(horizontalDistance < 0f ? 1 : -1);
        }
    }
}
