using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Fishing
{
    public sealed class FishingLocationDefinition
    {
        private readonly float[] rarityWeights;

        public FishingLocationDefinition(
            string stableId,
            string displayName,
            string backgroundResourcePath,
            int[] fishIndices,
            params float[] weights
        )
        {
            StableId = stableId;
            DisplayName = displayName;
            BackgroundResourcePath = backgroundResourcePath;
            FishIndices = fishIndices ?? Array.Empty<int>();
            rarityWeights = new float[5];
            for (int index = 0; index < rarityWeights.Length; index++)
            {
                rarityWeights[index] =
                    weights != null && index < weights.Length
                        ? Mathf.Max(0f, weights[index])
                        : 0f;
            }
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public string BackgroundResourcePath { get; }
        public IReadOnlyList<int> FishIndices { get; }
        public int FishCount => FishIndices.Count;
        public int MaximumStars
        {
            get
            {
                for (int index = rarityWeights.Length - 1; index >= 0; index--)
                {
                    if (rarityWeights[index] > 0f)
                    {
                        return index + 1;
                    }
                }
                return 1;
            }
        }

        public float WeightFor(FishRarity rarity)
        {
            int index = Mathf.Clamp((int)rarity, 0, rarityWeights.Length - 1);
            return rarityWeights[index];
        }
    }

    public static class FishingLocationCatalog
    {
        private static readonly IReadOnlyList<FishingLocationDefinition> AllLocations =
            new[]
            {
                new FishingLocationDefinition(
                    "forest-pond",
                    "SKOGSTJÄRNEN",
                    "Art/Fishing/fishing_background_rigged",
                    new[] { 0, 1, 2, 6, 7, 8 },
                    100f, 0f, 0f, 0f, 0f
                ),
                new FishingLocationDefinition(
                    "lily-cove",
                    "NÄCKROSVIKEN",
                    "Art/Fishing/Locations/lily_cove",
                    new[] { 9, 10, 3, 4, 11, 12 },
                    88f, 12f, 0f, 0f, 0f
                ),
                new FishingLocationDefinition(
                    "coral-coast",
                    "KORALLKUSTEN",
                    "Art/Fishing/Locations/coral_coast",
                    new[] { 13, 14, 15, 5, 16, 17 },
                    76f, 20f, 4f, 0f, 0f
                ),
                new FishingLocationDefinition(
                    "aurora-lake",
                    "NORRSKENSSJÖN",
                    "Art/Fishing/Locations/aurora_lake",
                    new[] { 18, 19, 20, 21, 22, 23 },
                    67f, 21f, 10f, 2f, 0f
                ),
                new FishingLocationDefinition(
                    "star-lagoon",
                    "STJÄRNLAGUNEN",
                    "Art/Fishing/Locations/star_lagoon",
                    new[] { 24, 25, 26, 27, 28, 31 },
                    63f, 20f, 10f, 5f, 2f
                ),
                new FishingLocationDefinition(
                    "diamond-depths",
                    "DIAMANTDJUPET",
                    "Art/Fishing/Locations/diamond_depths",
                    new[] { 29, 30, 32, 33, 34, 35 },
                    55f, 22f, 13f, 8f, 2f
                )
            };

        public static IReadOnlyList<FishingLocationDefinition> All => AllLocations;
    }

    public static class FishingLocationProgression
    {
        public const int FishPerLocation = 6;

        public static int CaughtFishCount(
            FishingLocationDefinition location,
            IReadOnlyList<FishDefinition> definitions,
            FishCollectionService collection
        )
        {
            if (location == null || definitions == null || collection == null)
            {
                return 0;
            }

            int caught = 0;
            for (int index = 0; index < location.FishIndices.Count; index++)
            {
                int fishIndex = location.FishIndices[index];
                if (fishIndex < 0 || fishIndex >= definitions.Count)
                {
                    continue;
                }
                FishDefinition fish = definitions[fishIndex];
                if (fish != null && collection.IsDiscovered(fish.StableId))
                {
                    caught++;
                }
            }
            return caught;
        }

        public static bool IsComplete(
            int locationIndex,
            IReadOnlyList<FishDefinition> definitions,
            FishCollectionService collection
        )
        {
            IReadOnlyList<FishingLocationDefinition> locations =
                FishingLocationCatalog.All;
            if (locationIndex < 0 || locationIndex >= locations.Count)
            {
                return false;
            }

            FishingLocationDefinition location = locations[locationIndex];
            int availableFish = 0;
            if (definitions != null)
            {
                for (int index = 0; index < location.FishIndices.Count; index++)
                {
                    int fishIndex = location.FishIndices[index];
                    if (fishIndex >= 0 && fishIndex < definitions.Count)
                    {
                        availableFish++;
                    }
                }
            }
            return availableFish == location.FishCount
                && CaughtFishCount(location, definitions, collection)
                    == location.FishCount;
        }

        public static bool IsUnlocked(
            int locationIndex,
            IReadOnlyList<FishDefinition> definitions,
            FishCollectionService collection
        )
        {
            if (locationIndex == 0)
            {
                return true;
            }
            return locationIndex > 0
                && locationIndex < FishingLocationCatalog.All.Count
                && IsComplete(locationIndex - 1, definitions, collection);
        }
    }

    public sealed class LocationFishSelectionService
    {
        private readonly IReadOnlyList<FishDefinition> definitions;
        private readonly IRandomProvider random;

        public LocationFishSelectionService(
            IReadOnlyList<FishDefinition> fishDefinitions,
            IRandomProvider randomProvider
        )
        {
            definitions = fishDefinitions
                ?? throw new ArgumentNullException(nameof(fishDefinitions));
            random = randomProvider
                ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        public FishDefinition Select(
            FishingLocationDefinition location,
            float rareChanceBonus = 0f
        )
        {
            if (location == null)
            {
                return SelectFallback();
            }

            float fishTotal = 0f;
            float rareTotal = 0f;
            float regularTotal = 0f;
            float[] raritySelectionTotals = new float[5];
            for (int index = 0; index < location.FishIndices.Count; index++)
            {
                int fishIndex = location.FishIndices[index];
                if (fishIndex < 0 || fishIndex >= definitions.Count)
                {
                    continue;
                }
                FishDefinition fish = definitions[fishIndex];
                if (fish != null)
                {
                    int rarityIndex = Mathf.Clamp(
                        (int)fish.Rarity,
                        0,
                        raritySelectionTotals.Length - 1
                    );
                    raritySelectionTotals[rarityIndex] += Mathf.Max(
                        0.01f,
                        fish.SelectionWeight
                    );
                }
            }

            for (int rarityIndex = 0; rarityIndex < raritySelectionTotals.Length; rarityIndex++)
            {
                if (raritySelectionTotals[rarityIndex] <= 0f)
                {
                    continue;
                }

                float rarityWeight = location.WeightFor(
                    (FishRarity)rarityIndex
                );
                fishTotal += rarityWeight;
                if ((FishRarity)rarityIndex >= FishRarity.Rare)
                {
                    rareTotal += rarityWeight;
                }
                else
                {
                    regularTotal += rarityWeight;
                }
            }
            if (fishTotal <= 0f)
            {
                return SelectFallback();
            }

            bool? selectRare = null;
            if (
                rareChanceBonus > 0f
                && rareTotal > 0f
                && regularTotal > 0f
            )
            {
                float baseRareChance = rareTotal / fishTotal;
                float boostedRareChance = Mathf.Clamp01(
                    baseRareChance + Mathf.Clamp01(rareChanceBonus)
                );
                selectRare = random.Range(0f, 1f) < boostedRareChance;
            }

            float selectedTotal = selectRare.HasValue
                ? selectRare.Value ? rareTotal : regularTotal
                : fishTotal;
            float fishRoll = random.Range(0f, selectedTotal);
            FishDefinition lastMatch = null;
            for (int index = 0; index < location.FishIndices.Count; index++)
            {
                int fishIndex = location.FishIndices[index];
                if (fishIndex < 0 || fishIndex >= definitions.Count)
                {
                    continue;
                }
                FishDefinition fish = definitions[fishIndex];
                int rarityIndex = fish != null
                    ? Mathf.Clamp(
                        (int)fish.Rarity,
                        0,
                        raritySelectionTotals.Length - 1
                    )
                    : 0;
                if (
                    fish == null
                    || raritySelectionTotals[rarityIndex] <= 0f
                    || location.WeightFor(fish.Rarity) <= 0f
                    || (
                        selectRare.HasValue
                        && (fish.Rarity >= FishRarity.Rare)
                            != selectRare.Value
                    )
                )
                {
                    continue;
                }
                lastMatch = fish;
                fishRoll -=
                    location.WeightFor(fish.Rarity)
                    * Mathf.Max(0.01f, fish.SelectionWeight)
                    / raritySelectionTotals[rarityIndex];
                if (fishRoll <= 0f)
                {
                    return fish;
                }
            }
            return lastMatch ?? SelectFallback();
        }

        private FishDefinition SelectFallback()
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                if (definitions[index] != null)
                {
                    return definitions[index];
                }
            }
            return null;
        }
    }
}
