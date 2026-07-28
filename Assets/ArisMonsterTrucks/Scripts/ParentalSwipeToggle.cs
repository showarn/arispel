using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class ParentalSwipeToggle
        : MonoBehaviour,
            IPointerClickHandler,
            IBeginDragHandler,
            IDragHandler,
            IEndDragHandler
    {
        private RectTransform track;
        private RectTransform handle;
        private Image trackImage;
        private Text noLabel;
        private Text yesLabel;
        private Action<bool> changed;
        private bool isOn;
        private bool dragging;

        public bool IsOn => isOn;

        public void Initialize(
            RectTransform trackRect,
            RectTransform handleRect,
            Image background,
            Text noText,
            Text yesText,
            bool initialValue,
            Action<bool> onChanged
        )
        {
            track = trackRect;
            handle = handleRect;
            trackImage = background;
            noLabel = noText;
            yesLabel = yesText;
            changed = onChanged;
            SetValue(initialValue, false);
        }

        public void SetValue(bool value, bool notify = true)
        {
            isOn = value;
            UpdateVisual();
            if (notify)
            {
                changed?.Invoke(isOn);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!dragging)
            {
                SetValue(!isOn);
            }
            dragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
            changed?.Invoke(isOn);
            dragging = false;
        }

        private void UpdateFromPointer(PointerEventData eventData)
        {
            if (
                track == null
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    track,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 local
                )
            )
            {
                return;
            }

            isOn = local.x >= 0f;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (track == null || handle == null)
            {
                return;
            }

            float travel = Mathf.Max(
                0f,
                (track.rect.width - handle.rect.width) * 0.5f - 8f
            );
            handle.anchoredPosition = new Vector2(isOn ? travel : -travel, 0f);
            trackImage.color = isOn
                ? RuntimeArt.Hex("#4FC66A")
                : RuntimeArt.Hex("#818795");
            yesLabel.color = isOn
                ? RuntimeArt.Hex("#1E6E34")
                : new Color(1f, 1f, 1f, 0.88f);
            noLabel.color = isOn
                ? new Color(1f, 1f, 1f, 0.88f)
                : RuntimeArt.Hex("#40245F");
        }
    }
}
