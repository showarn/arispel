using UnityEngine;

namespace ArisMonsterTrucks
{
    public readonly struct GarageAccessoryMount
    {
        public Vector2 PreviewPosition { get; }
        public Vector2 PreviewSize { get; }
        public float PixelsPerUnit { get; }
        public int SortingOrder { get; }
        public float RuntimeDepth { get; }
        public bool MirrorHorizontally { get; }
        public bool BehindBody { get; }

        public GarageAccessoryMount(
            Vector2 previewPosition,
            Vector2 previewSize,
            float pixelsPerUnit,
            int sortingOrder,
            float runtimeDepth,
            bool mirrorHorizontally,
            bool behindBody
        )
        {
            PreviewPosition = previewPosition;
            PreviewSize = previewSize;
            PixelsPerUnit = pixelsPerUnit;
            SortingOrder = sortingOrder;
            RuntimeDepth = runtimeDepth;
            MirrorHorizontally = mirrorHorizontally;
            BehindBody = behindBody;
        }
    }

    public static class GarageAccessoryMounts
    {
        public static GarageAccessoryMount Get(string accessoryId)
        {
            return accessoryId switch
            {
                "accessory_exhaust" => new GarageAccessoryMount(
                    new Vector2(-134f, 82f),
                    new Vector2(110f, 96f),
                    160f,
                    19,
                    -0.05f,
                    false,
                    true
                ),
                _ => new GarageAccessoryMount(
                    new Vector2(35f, 123f),
                    new Vector2(145f, 66f),
                    160f,
                    23,
                    -0.05f,
                    false,
                    false
                )
            };
        }
    }
}
