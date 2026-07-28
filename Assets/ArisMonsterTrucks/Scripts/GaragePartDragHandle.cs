using UnityEngine;
using UnityEngine.EventSystems;

namespace ArisMonsterTrucks
{
    public sealed class GaragePartDragHandle : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        private FrontEndController editor;
        private TruckLayoutPart part;
        private string itemId;

        public void Initialize(
            FrontEndController owner,
            TruckLayoutPart layoutPart,
            string layoutItemId = null
        )
        {
            editor = owner;
            part = layoutPart;
            itemId = layoutItemId;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            editor?.SelectLayoutPart(part, itemId);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (editor == null)
            {
                return;
            }

            float scaleFactor = editor.UiScaleFactor;
            editor.DragLayoutPart(
                part,
                itemId,
                eventData.delta / Mathf.Max(0.01f, scaleFactor)
            );
        }
    }
}
