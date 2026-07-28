using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Fishing
{
    public enum FishRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    [CreateAssetMenu(
        fileName = "FishDefinition",
        menuName = "Aris Familjespel/Fiske/Fiskdefinition"
    )]
    public sealed class FishDefinition : ScriptableObject
    {
        [SerializeField] private string stableId;
        [SerializeField] private string displayName;
        [SerializeField] private int displayOrder;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Sprite catchSprite;
        [SerializeField] private FishRarity rarity;
        [SerializeField] private float selectionWeight = 1f;
        [SerializeField] private float minimumLengthCentimeters = 10f;
        [SerializeField] private float maximumLengthCentimeters = 25f;
        [SerializeField] private float swimSpeed = 1f;
        [SerializeField] private AudioClip fishSound;
        [SerializeField] private string shortFact;
        [SerializeField] private Color uiColor = Color.white;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public int DisplayOrder => displayOrder;
        public Sprite Sprite => sprite;
        public Sprite CatchSprite => catchSprite != null ? catchSprite : sprite;
        public FishRarity Rarity => rarity;
        public float SelectionWeight => selectionWeight;
        public float MinimumLengthCentimeters => minimumLengthCentimeters;
        public float MaximumLengthCentimeters => maximumLengthCentimeters;
        public float SwimSpeed => swimSpeed;
        public AudioClip FishSound => fishSound;
        public string ShortFact => shortFact;
        public Color UiColor => uiColor;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string name,
            int order,
            Sprite regularSprite,
            FishRarity fishRarity,
            float weight,
            float minimumLength,
            float maximumLength,
            float speed,
            string fact,
            Color color
        )
        {
            stableId = id;
            displayName = name;
            displayOrder = order;
            sprite = regularSprite;
            catchSprite = regularSprite;
            rarity = fishRarity;
            selectionWeight = weight;
            minimumLengthCentimeters = minimumLength;
            maximumLengthCentimeters = maximumLength;
            swimSpeed = speed;
            shortFact = fact;
            uiColor = color;
        }
#endif
    }

    public static class FishCatalog
    {
        private const string ResourceFolder = "Config/Fishing";
        private static IReadOnlyList<FishDefinition> cached;

        public static IReadOnlyList<FishDefinition> Load()
        {
            if (cached != null)
            {
                return cached;
            }

            FishDefinition[] definitions =
                Resources.LoadAll<FishDefinition>(ResourceFolder);
            System.Array.Sort(
                definitions,
                (left, right) => left.DisplayOrder.CompareTo(right.DisplayOrder)
            );
            cached = definitions;
            return cached;
        }

        public static void ClearCache()
        {
            cached = null;
        }
    }
}
