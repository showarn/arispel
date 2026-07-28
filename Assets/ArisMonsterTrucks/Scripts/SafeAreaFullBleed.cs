using UnityEngine;

namespace ArisMonsterTrucks
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFullBleed : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastArea;
        private Vector2Int lastScreen;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            Rect area = SafeAreaFitter.ActiveSafeArea();
            Vector2Int screen = new(Screen.width, Screen.height);
            if (area != lastArea || screen != lastScreen)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            Rect area = SafeAreaFitter.ActiveSafeArea();
            Vector2Int screen = new(Screen.width, Screen.height);
            lastArea = area;
            lastScreen = screen;
            SafeAreaFitter.CalculateAnchors(
                area,
                screen,
                out Vector2 minimum,
                out Vector2 maximum
            );
            Vector2 span = maximum - minimum;
            if (span.x <= 0f || span.y <= 0f)
            {
                return;
            }

            rect.anchorMin = new Vector2(
                -minimum.x / span.x,
                -minimum.y / span.y
            );
            rect.anchorMax = new Vector2(
                (1f - minimum.x) / span.x,
                (1f - minimum.y) / span.y
            );
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
