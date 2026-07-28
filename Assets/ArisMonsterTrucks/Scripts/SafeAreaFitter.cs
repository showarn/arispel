using System;
using System.Globalization;
using UnityEngine;

namespace ArisMonsterTrucks
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private static bool previewAreaRead;
        private static Rect? previewArea;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            Rect desiredArea = ActiveSafeArea();
            if (
                desiredArea != lastSafeArea
                || Screen.width != lastScreenSize.x
                || Screen.height != lastScreenSize.y
            )
            {
                Apply(desiredArea, new Vector2Int(Screen.width, Screen.height));
            }
        }

        private void Apply()
        {
            Apply(
                ActiveSafeArea(),
                new Vector2Int(Screen.width, Screen.height)
            );
        }

        public static Rect ActiveSafeArea()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!previewAreaRead)
            {
                previewAreaRead = true;
                const string prefix = "-arisSafeArea=";
                string argument = Array.Find(
                    Environment.GetCommandLineArgs(),
                    value => value.StartsWith(
                        prefix,
                        StringComparison.Ordinal
                    )
                );
                if (!string.IsNullOrEmpty(argument))
                {
                    string[] values = argument
                        .Substring(prefix.Length)
                        .Split(',');
                    if (
                        values.Length == 4
                        && float.TryParse(
                            values[0],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float x
                        )
                        && float.TryParse(
                            values[1],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float y
                        )
                        && float.TryParse(
                            values[2],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float width
                        )
                        && float.TryParse(
                            values[3],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float height
                        )
                    )
                    {
                        previewArea = new Rect(x, y, width, height);
                    }
                }
            }
            if (previewArea.HasValue)
            {
                return previewArea.Value;
            }
#endif
            return Screen.safeArea;
        }

        public void ApplyForTesting(Rect safeArea, Vector2Int screenSize)
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }
            Apply(safeArea, screenSize);
        }

        public static void CalculateAnchors(
            Rect safeArea,
            Vector2Int screenSize,
            out Vector2 minimum,
            out Vector2 maximum
        )
        {
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                minimum = Vector2.zero;
                maximum = Vector2.one;
                return;
            }

            float xMin = Mathf.Clamp(safeArea.xMin, 0f, screenSize.x);
            float yMin = Mathf.Clamp(safeArea.yMin, 0f, screenSize.y);
            float xMax = Mathf.Clamp(safeArea.xMax, xMin, screenSize.x);
            float yMax = Mathf.Clamp(safeArea.yMax, yMin, screenSize.y);
            minimum = new Vector2(xMin / screenSize.x, yMin / screenSize.y);
            maximum = new Vector2(xMax / screenSize.x, yMax / screenSize.y);
        }

        private void Apply(Rect safeArea, Vector2Int screenSize)
        {
            lastSafeArea = safeArea;
            lastScreenSize = screenSize;
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                return;
            }

            CalculateAnchors(
                safeArea,
                screenSize,
                out Vector2 minimum,
                out Vector2 maximum
            );
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
