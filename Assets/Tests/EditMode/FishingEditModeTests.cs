using System;
using System.Collections.Generic;
using ArisMonsterTrucks.Fishing;
using NUnit.Framework;
using UnityEngine;

namespace ArisMonsterTrucks.Fishing.Tests
{
    public sealed class FishingEditModeTests
    {
        private sealed class MemoryStore : IKeyValueStore
        {
            private readonly Dictionary<string, string> values = new();

            public string GetString(string key, string defaultValue)
            {
                return values.TryGetValue(key, out string value)
                    ? value
                    : defaultValue;
            }

            public void SetString(string key, string value)
            {
                values[key] = value;
            }

            public void DeleteKey(string key)
            {
                values.Remove(key);
            }

            public void Save()
            {
            }
        }

        private IReadOnlyList<FishDefinition> definitions;

        [SetUp]
        public void SetUp()
        {
            FishCatalog.ClearCache();
            definitions = FishCatalog.Load();
        }

        [Test]
        public void AllFishHaveUniqueStableIds()
        {
            HashSet<string> ids = new();
            Assert.AreEqual(36, definitions.Count);
            for (int index = 0; index < definitions.Count; index++)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(definitions[index].StableId));
                Assert.IsTrue(ids.Add(definitions[index].StableId));
            }
        }

        [Test]
        public void AllDefinitionsHaveSpriteAndName()
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                Assert.IsNotNull(definitions[index].Sprite);
                Assert.IsFalse(string.IsNullOrWhiteSpace(definitions[index].DisplayName));
            }
        }

        [Test]
        public void AllSelectionWeightsAndLengthRangesAreValid()
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                FishDefinition fish = definitions[index];
                Assert.Greater(fish.SelectionWeight, 0f);
                Assert.Greater(fish.MinimumLengthCentimeters, 0f);
                Assert.GreaterOrEqual(
                    fish.MaximumLengthCentimeters,
                    fish.MinimumLengthCentimeters
                );
            }
        }

        [Test]
        public void WeightedSelectionCanChooseEveryRarity()
        {
            HashSet<FishRarity> found = new();
            FishSelectionService selection = new(
                definitions,
                new SeededRandomProvider(12345)
            );
            for (int index = 0; index < 5000; index++)
            {
                found.Add(selection.Select().Rarity);
            }
            CollectionAssert.AreEquivalent(
                new[]
                {
                    FishRarity.Common,
                    FishRarity.Uncommon,
                    FishRarity.Rare,
                    FishRarity.Epic,
                    FishRarity.Legendary
                },
                found
            );
        }

        [Test]
        public void FishingLocationsUnlockRaritiesStepByStep()
        {
            IReadOnlyList<FishingLocationDefinition> locations =
                FishingLocationCatalog.All;
            Assert.AreEqual(6, locations.Count);
            Assert.AreEqual(1, locations[0].MaximumStars);
            Assert.AreEqual(2, locations[1].MaximumStars);
            Assert.AreEqual(3, locations[2].MaximumStars);
            Assert.AreEqual(4, locations[3].MaximumStars);
            Assert.AreEqual(5, locations[4].MaximumStars);
            Assert.AreEqual(5, locations[5].MaximumStars);
            Assert.AreEqual(0f, locations[0].WeightFor(FishRarity.Legendary));
            Assert.AreEqual(0f, locations[3].WeightFor(FishRarity.Legendary));
            Assert.AreEqual(2f, locations[4].WeightFor(FishRarity.Legendary));
            Assert.AreEqual(2f, locations[5].WeightFor(FishRarity.Legendary));

            HashSet<int> assignedFish = new();
            for (int locationIndex = 0; locationIndex < locations.Count; locationIndex++)
            {
                FishingLocationDefinition location = locations[locationIndex];
                Assert.AreEqual(6, location.FishCount);
                int actualMaximumStars = 1;
                HashSet<FishRarity> presentRarities = new();
                for (int index = 0; index < location.FishIndices.Count; index++)
                {
                    int fishIndex = location.FishIndices[index];
                    Assert.IsTrue(assignedFish.Add(fishIndex));
                    presentRarities.Add(definitions[fishIndex].Rarity);
                    actualMaximumStars = Mathf.Max(
                        actualMaximumStars,
                        (int)definitions[fishIndex].Rarity + 1
                    );
                    Assert.LessOrEqual(
                        (int)definitions[fishIndex].Rarity + 1,
                        location.MaximumStars
                    );
                }
                Assert.AreEqual(location.MaximumStars, actualMaximumStars);
                for (
                    int rarityIndex = 0;
                    rarityIndex < location.MaximumStars;
                    rarityIndex++
                )
                {
                    Assert.IsTrue(
                        presentRarities.Contains((FishRarity)rarityIndex),
                        $"Bana {locationIndex + 1} saknar fisk med {rarityIndex + 1} stjärnor."
                    );
                }
            }
            Assert.AreEqual(36, assignedFish.Count);
            for (int index = 0; index < locations[0].FishIndices.Count; index++)
            {
                Assert.AreEqual(
                    FishRarity.Common,
                    definitions[locations[0].FishIndices[index]].Rarity
                );
            }
        }

        [Test]
        public void EveryLocationSelectsOnlyItsOwnSixFish()
        {
            for (
                int locationIndex = 0;
                locationIndex < FishingLocationCatalog.All.Count;
                locationIndex++
            )
            {
                FishingLocationDefinition location =
                    FishingLocationCatalog.All[locationIndex];
                LocationFishSelectionService selector = new(
                    definitions,
                    new SeededRandomProvider(7654 + locationIndex)
                );
                HashSet<string> expected = new();
                HashSet<string> found = new();
                for (int index = 0; index < location.FishIndices.Count; index++)
                {
                    int fishIndex = location.FishIndices[index];
                    expected.Add(definitions[fishIndex].StableId);
                }
                for (int cast = 0; cast < 3000; cast++)
                {
                    FishDefinition selected = selector.Select(location);
                    Assert.Contains(selected.StableId, new List<string>(expected));
                    found.Add(selected.StableId);
                }
                CollectionAssert.AreEquivalent(expected, found);
            }
        }

        [Test]
        public void EveryFirstLevelFishHasAReasonableCatchChance()
        {
            FishingLocationDefinition location =
                FishingLocationCatalog.All[0];
            LocationFishSelectionService selector = new(
                definitions,
                new SeededRandomProvider(77123)
            );
            Dictionary<string, int> catches = new();
            for (int cast = 0; cast < 10000; cast++)
            {
                string fishId = selector.Select(location).StableId;
                catches.TryGetValue(fishId, out int count);
                catches[fishId] = count + 1;
            }
            Assert.AreEqual(6, catches.Count);
            foreach (int count in catches.Values)
            {
                Assert.Greater(count, 1000);
            }
        }

        [Test]
        public void FourthLocationUsesItsConfiguredStarDistribution()
        {
            const int casts = 100000;
            FishingLocationDefinition location =
                FishingLocationCatalog.All[3];
            LocationFishSelectionService selector = new(
                definitions,
                new SeededRandomProvider(78124)
            );
            int[] catchesByRarity = new int[5];

            for (int cast = 0; cast < casts; cast++)
            {
                catchesByRarity[(int)selector.Select(location).Rarity]++;
            }

            Assert.That(catchesByRarity[0] / (float)casts, Is.InRange(0.65f, 0.69f));
            Assert.That(catchesByRarity[1] / (float)casts, Is.InRange(0.19f, 0.23f));
            Assert.That(catchesByRarity[2] / (float)casts, Is.InRange(0.09f, 0.11f));
            Assert.That(catchesByRarity[3] / (float)casts, Is.InRange(0.015f, 0.025f));
            Assert.AreEqual(0, catchesByRarity[4]);
        }

        [Test]
        public void NextLocationUnlocksOnlyAfterAllSixFishAreCaught()
        {
            MemoryStore store = new();
            FishCollectionService collection = new(
                new FishingSaveService(store)
            );
            Assert.IsTrue(
                FishingLocationProgression.IsUnlocked(
                    0,
                    definitions,
                    collection
                )
            );
            Assert.IsFalse(
                FishingLocationProgression.IsUnlocked(
                    1,
                    definitions,
                    collection
                )
            );

            FishingLocationDefinition firstLocation =
                FishingLocationCatalog.All[0];
            for (int index = 0; index < firstLocation.FishCount - 1; index++)
            {
                int fishIndex = firstLocation.FishIndices[index];
                collection.RecordCatch(
                    definitions[fishIndex].StableId,
                    20f,
                    DateTime.UtcNow
                );
            }
            Assert.IsFalse(
                FishingLocationProgression.IsUnlocked(
                    1,
                    definitions,
                    collection
                )
            );

            int lastFishIndex =
                firstLocation.FishIndices[firstLocation.FishCount - 1];
            collection.RecordCatch(
                definitions[lastFishIndex].StableId,
                20f,
                DateTime.UtcNow
            );
            Assert.IsTrue(
                FishingLocationProgression.IsComplete(
                    0,
                    definitions,
                    collection
                )
            );
            Assert.IsTrue(
                FishingLocationProgression.IsUnlocked(
                    1,
                    definitions,
                    collection
                )
            );
        }

        [Test]
        public void FixedSeedProducesReproducibleSelection()
        {
            FishSelectionService first = new(
                definitions,
                new SeededRandomProvider(9917)
            );
            FishSelectionService second = new(
                definitions,
                new SeededRandomProvider(9917)
            );
            for (int index = 0; index < 40; index++)
            {
                Assert.AreEqual(first.Select().StableId, second.Select().StableId);
            }
        }

        [Test]
        public void SaveCanSerializeAndRestore()
        {
            MemoryStore store = new();
            FishingSaveService save = new(store);
            FishCollectionService collection = new(save);
            collection.RecordCatch("blue-fin", 21.5f, DateTime.UtcNow);

            FishCollectionService restored = new(new FishingSaveService(store));
            FishCatchRecord record = restored.Get("blue-fin");
            Assert.IsNotNull(record);
            Assert.AreEqual(1, record.caughtCount);
            Assert.AreEqual(21.5f, record.largestLengthCentimeters, 0.001f);
            Assert.AreEqual(1, restored.Inventory().Count);
            Assert.AreEqual(
                21.5f,
                restored.Inventory()[0].Specimen.lengthCentimeters,
                0.001f
            );
        }

        [Test]
        public void SellingSpecimenRemovesInventoryButKeepsDiscovery()
        {
            MemoryStore store = new();
            FishCollectionService collection = new(
                new FishingSaveService(store)
            );
            collection.RecordCatch("blue-fin", 18f, DateTime.UtcNow);
            collection.RecordCatch("blue-fin", 24f, DateTime.UtcNow);
            string specimenId = collection.Inventory()[0].Specimen.specimenId;

            Assert.IsTrue(
                collection.TryRemoveSpecimen(
                    specimenId,
                    out string fishId,
                    out float length
                )
            );
            Assert.AreEqual("blue-fin", fishId);
            Assert.AreEqual(18f, length, 0.001f);
            Assert.AreEqual(1, collection.Inventory().Count);
            Assert.IsTrue(collection.IsDiscovered("blue-fin"));
            Assert.AreEqual(2, collection.Get("blue-fin").caughtCount);
            Assert.AreEqual(
                24f,
                collection.Get("blue-fin").largestLengthCentimeters,
                0.001f
            );
        }

        [Test]
        public void LegendaryFishPriceGrowsFromFiveToTwentyThousand()
        {
            FishDefinition legendary = null;
            for (int index = 0; index < definitions.Count; index++)
            {
                if (definitions[index].Rarity == FishRarity.Legendary)
                {
                    legendary = definitions[index];
                    break;
                }
            }
            Assert.IsNotNull(legendary);
            int minimum = FishSalePricing.Calculate(
                legendary,
                legendary.MinimumLengthCentimeters
            );
            int middle = FishSalePricing.Calculate(
                legendary,
                Mathf.Lerp(
                    legendary.MinimumLengthCentimeters,
                    legendary.MaximumLengthCentimeters,
                    0.5f
                )
            );
            int maximum = FishSalePricing.Calculate(
                legendary,
                legendary.MaximumLengthCentimeters
            );
            Assert.AreEqual(5000, minimum);
            Assert.Greater(middle, minimum);
            Assert.Less(middle, maximum);
            Assert.AreEqual(20000, maximum);
        }

        [Test]
        public void RegularFishPricesStayFarBelowRodPrice()
        {
            FishDefinition common = null;
            FishDefinition epic = null;
            for (int index = 0; index < definitions.Count; index++)
            {
                common ??= definitions[index].Rarity == FishRarity.Common
                    ? definitions[index]
                    : null;
                epic ??= definitions[index].Rarity == FishRarity.Epic
                    ? definitions[index]
                    : null;
            }
            Assert.IsNotNull(common);
            Assert.IsNotNull(epic);
            Assert.AreEqual(
                50,
                FishSalePricing.Calculate(
                    common,
                    common.MaximumLengthCentimeters
                )
            );
            Assert.AreEqual(
                1000,
                FishSalePricing.Calculate(
                    epic,
                    epic.MaximumLengthCentimeters
                )
            );
            Assert.Greater(
                FishingRodCollection.All[1].Price,
                FishSalePricing.Calculate(
                    epic,
                    epic.MaximumLengthCentimeters
                )
            );
        }

        [Test]
        public void RodCatalogContainsAllRequestedRareBonuses()
        {
            Assert.AreEqual(8, FishingRodCollection.All.Count);
            float[] expected =
            {
                0f, 0.03f, 0.05f, 0.06f, 0.08f, 0.10f, 0.12f, 0.15f
            };
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.AreEqual(
                    expected[index],
                    FishingRodCollection.All[index].RareChanceBonus,
                    0.0001f
                );
            }
        }

        [Test]
        public void LureCatalogContainsTwentyBalancedUniqueLures()
        {
            Assert.AreEqual(20, FishingLureCollection.All.Count);
            HashSet<string> ids = new();
            int previousPrice = 0;
            for (int index = 0; index < FishingLureCollection.All.Count; index++)
            {
                FishingLureDefinition lure =
                    FishingLureCollection.All[index];
                Assert.IsTrue(ids.Add(lure.Id));
                Assert.Greater(lure.Price, previousPrice);
                Assert.GreaterOrEqual(lure.RareChanceBonus, 0.01f);
                Assert.LessOrEqual(lure.RareChanceBonus, 0.03f);
                previousPrice = lure.Price;
            }
        }

        [Test]
        public void WormPackMatchesRequestedAmountPriceAndBonus()
        {
            Assert.AreEqual(25, FishingBaitInventory.WormsPerPack);
            Assert.AreEqual(500, FishingBaitInventory.PackPrice);
            Assert.AreEqual(
                0.03f,
                FishingBaitInventory.RareChanceBonus,
                0.0001f
            );
        }

        [Test]
        public void StarRodAddsThreePercentagePointsToRareFishChance()
        {
            const int casts = 100000;
            LocationFishSelectionService regularSelector = new(
                definitions,
                new SeededRandomProvider(44661)
            );
            LocationFishSelectionService starSelector = new(
                definitions,
                new SeededRandomProvider(90217)
            );
            FishingLocationDefinition location =
                FishingLocationCatalog.All[2];
            int regularRare = 0;
            int starRare = 0;
            for (int cast = 0; cast < casts; cast++)
            {
                if (regularSelector.Select(location).Rarity >= FishRarity.Rare)
                {
                    regularRare++;
                }
                if (
                    starSelector.Select(location, 0.03f).Rarity
                    >= FishRarity.Rare
                )
                {
                    starRare++;
                }
            }
            float increase = (starRare - regularRare) / (float)casts;
            Assert.That(increase, Is.InRange(0.025f, 0.035f));
        }

        [Test]
        public void UnknownFishInSaveDoesNotCrash()
        {
            MemoryStore store = new();
            store.SetString(
                FishingSaveService.SaveKey,
                "{\"version\":1,\"fish\":[{\"fishId\":\"future-fish\",\"caughtCount\":2,\"largestLengthCentimeters\":44.0}]}"
            );
            FishCollectionService collection = new(
                new FishingSaveService(store)
            );
            Assert.AreEqual(2, collection.Get("future-fish").caughtCount);
        }

        [Test]
        public void MissingOrCorruptFishDataUsesSafeDefaults()
        {
            MemoryStore emptyStore = new();
            FishSaveData empty = new FishingSaveService(emptyStore).Load();
            Assert.IsNotNull(empty);
            Assert.IsNotNull(empty.fish);

            MemoryStore corruptStore = new();
            corruptStore.SetString(FishingSaveService.SaveKey, "{not-json");
            FishSaveData corrupt = new FishingSaveService(corruptStore).Load();
            Assert.IsNotNull(corrupt);
            Assert.IsNotNull(corrupt.fish);
        }

        [Test]
        public void LargestLengthOnlyChangesForLargerCatch()
        {
            MemoryStore store = new();
            FishCollectionService collection = new(
                new FishingSaveService(store)
            );
            collection.RecordCatch("sun-fish", 25f, DateTime.UtcNow);
            collection.RecordCatch("sun-fish", 18f, DateTime.UtcNow);
            Assert.AreEqual(
                25f,
                collection.Get("sun-fish").largestLengthCentimeters,
                0.001f
            );
        }

        [Test]
        public void CatchCountIncreasesCorrectly()
        {
            MemoryStore store = new();
            FishCollectionService collection = new(
                new FishingSaveService(store)
            );
            collection.RecordCatch("stripe-fish", 20f, DateTime.UtcNow);
            collection.RecordCatch("stripe-fish", 22f, DateTime.UtcNow);
            collection.RecordCatch("stripe-fish", 21f, DateTime.UtcNow);
            Assert.AreEqual(3, collection.Get("stripe-fish").caughtCount);
        }

        [Test]
        public void StateMachineRejectsInvalidTransitions()
        {
            FishingStateMachine machine = new();
            Assert.IsFalse(machine.TryTransition(FishingState.FishBiting));
            Assert.IsTrue(machine.TryTransition(FishingState.Casting));
            Assert.IsFalse(machine.TryTransition(FishingState.CatchReveal));
        }
    }
}
