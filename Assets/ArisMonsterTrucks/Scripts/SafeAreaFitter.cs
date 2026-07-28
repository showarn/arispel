using UnityEngine;

namespace ArisMonsterTrucks
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (
                Screen.safeArea != lastSafeArea
                || Screen.width != lastScreenSize.x
                || Screen.height != lastScreenSize.y
            )
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            rect.anchorMin = new Vector2(
                safeArea.xMin / Screen.width,
                safeArea.yMin / Screen.height
            );
            rect.anchorMax = new Vector2(
                safeArea.xMax / Screen.width,
                safeArea.yMax / Screen.height
            );
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
