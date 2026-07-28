using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Fishing
{
    public interface IRandomProvider
    {
        float Range(float minimumInclusive, float maximumExclusive);
    }

    public sealed class SeededRandomProvider : IRandomProvider
    {
        private readonly System.Random random;

        public SeededRandomProvider(int seed)
        {
            random = new System.Random(seed);
        }

        public float Range(float minimumInclusive, float maximumExclusive)
        {
            if (maximumExclusive <= minimumInclusive)
            {
                return minimumInclusive;
            }
            return minimumInclusive
                + (float)random.NextDouble()
                * (maximumExclusive - minimumInclusive);
        }
    }

    public sealed class FishSelectionService
    {
        private readonly IReadOnlyList<FishDefinition> definitions;
        private readonly IRandomProvider random;

        public FishSelectionService(
            IReadOnlyList<FishDefinition> fishDefinitions,
            IRandomProvider randomProvider
        )
        {
            definitions = fishDefinitions
                ?? throw new ArgumentNullException(nameof(fishDefinitions));
            random = randomProvider
                ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        public FishDefinition Select()
        {
            float totalWeight = 0f;
            for (int index = 0; index < definitions.Count; index++)
            {
                if (definitions[index] != null)
                {
                    totalWeight += Mathf.Max(0f, definitions[index].SelectionWeight);
                }
            }

            if (totalWeight <= 0f)
            {
                return definitions.Count > 0 ? definitions[0] : null;
            }

            float roll = random.Range(0f, totalWeight);
            for (int index = 0; index < definitions.Count; index++)
            {
                FishDefinition definition = definitions[index];
                if (definition == null)
                {
                    continue;
                }

                roll -= Mathf.Max(0f, definition.SelectionWeight);
                if (roll <= 0f)
                {
                    return definition;
                }
            }
            return definitions[definitions.Count - 1];
        }

        public float SelectLength(FishDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }
            return random.Range(
                definition.MinimumLengthCentimeters,
                definition.MaximumLengthCentimeters
            );
        }
    }

    public interface IKeyValueStore
    {
        string GetString(string key, string defaultValue);
        void SetString(string key, string value);
        void DeleteKey(string key);
        void Save();
    }

    public sealed class PlayerPrefsStore : IKeyValueStore
    {
        public string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public sealed class FishSpecimenRecord
    {
        public string specimenId;
        public float lengthCentimeters;
        public string caughtUtc;
    }

    [Serializable]
    public sealed class FishCatchRecord
    {
        public string fishId;
        public int caughtCount;
        public float largestLengthCentimeters;
        public string lastCaughtUtc;
        public List<FishSpecimenRecord> specimens = new();
    }

    [Serializable]
    public sealed class FishSaveData
    {
        public int version = 2;
        public List<FishCatchRecord> fish = new();
    }

    public sealed class FishingSaveService
    {
        public const string SaveKey = "fishing.collection.v1";

        private readonly IKeyValueStore store;

        public FishingSaveService(IKeyValueStore keyValueStore)
        {
            store = keyValueStore
                ?? throw new ArgumentNullException(nameof(keyValueStore));
        }

        public FishSaveData Load()
        {
            string json = store.GetString(SaveKey, "");
            if (string.IsNullOrWhiteSpace(json))
            {
                return new FishSaveData();
            }

            try
            {
                FishSaveData data = JsonUtility.FromJson<FishSaveData>(json);
                if (data == null)
                {
                    return new FishSaveData();
                }
                data.version = Mathf.Max(2, data.version);
                data.fish ??= new List<FishCatchRecord>();
                data.fish.RemoveAll(record => record == null);
                for (int index = 0; index < data.fish.Count; index++)
                {
                    FishCatchRecord record = data.fish[index];
                    record.fishId ??= "";
                    record.caughtCount = Mathf.Max(0, record.caughtCount);
                    record.largestLengthCentimeters =
                        Mathf.Max(0f, record.largestLengthCentimeters);
                    record.lastCaughtUtc ??= "";
                    record.specimens ??= new List<FishSpecimenRecord>();
                    record.specimens.RemoveAll(specimen => specimen == null);
                    for (
                        int specimenIndex = 0;
                        specimenIndex < record.specimens.Count;
                        specimenIndex++
                    )
                    {
                        FishSpecimenRecord specimen =
                            record.specimens[specimenIndex];
                        specimen.specimenId ??= "";
                        specimen.lengthCentimeters = Mathf.Max(
                            0f,
                            specimen.lengthCentimeters
                        );
                        specimen.caughtUtc ??= "";
                    }
                }
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Fiskbokens sparning kunde inte läsas. Säkra standardvärden används. "
                    + exception.Message
                );
                return new FishSaveData();
            }
        }

        public void Save(FishSaveData data)
        {
            FishSaveData safeData = data ?? new FishSaveData();
            safeData.version = 2;
            safeData.fish ??= new List<FishCatchRecord>();
            store.SetString(SaveKey, JsonUtility.ToJson(safeData));
            store.Save();
        }

        public void Reset()
        {
            store.DeleteKey(SaveKey);
            store.Save();
        }
    }

    public sealed class FishCollectionService
    {
        private readonly FishingSaveService saveService;
        private readonly FishSaveData data;

        public FishCollectionService(FishingSaveService service)
        {
            saveService = service ?? throw new ArgumentNullException(nameof(service));
            data = saveService.Load();
        }

        public int DiscoveredCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < data.fish.Count; index++)
                {
                    if (data.fish[index].caughtCount > 0)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public FishCatchRecord Get(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return null;
            }
            for (int index = 0; index < data.fish.Count; index++)
            {
                if (string.Equals(
                    data.fish[index].fishId,
                    stableId,
                    StringComparison.Ordinal
                ))
                {
                    return data.fish[index];
                }
            }
            return null;
        }

        public bool IsDiscovered(string stableId)
        {
            FishCatchRecord record = Get(stableId);
            return record != null && record.caughtCount > 0;
        }

        public bool RecordCatch(
            string stableId,
            float lengthCentimeters,
            DateTime caughtUtc
        )
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return false;
            }

            FishCatchRecord record = Get(stableId);
            bool isNew = record == null || record.caughtCount <= 0;
            if (record == null)
            {
                record = new FishCatchRecord { fishId = stableId };
                data.fish.Add(record);
            }

            record.caughtCount = Mathf.Max(0, record.caughtCount) + 1;
            record.largestLengthCentimeters = Mathf.Max(
                record.largestLengthCentimeters,
                Mathf.Max(0f, lengthCentimeters)
            );
            record.lastCaughtUtc = caughtUtc.ToUniversalTime().ToString("O");
            record.specimens ??= new List<FishSpecimenRecord>();
            record.specimens.Add(
                new FishSpecimenRecord
                {
                    specimenId = Guid.NewGuid().ToString("N"),
                    lengthCentimeters = Mathf.Max(0f, lengthCentimeters),
                    caughtUtc = caughtUtc.ToUniversalTime().ToString("O")
                }
            );
            saveService.Save(data);
            return isNew;
        }

        public List<(string FishId, FishSpecimenRecord Specimen)> Inventory()
        {
            List<(string FishId, FishSpecimenRecord Specimen)> result = new();
            for (int recordIndex = 0; recordIndex < data.fish.Count; recordIndex++)
            {
                FishCatchRecord record = data.fish[recordIndex];
                if (record?.specimens == null)
                {
                    continue;
                }
                for (
                    int specimenIndex = 0;
                    specimenIndex < record.specimens.Count;
                    specimenIndex++
                )
                {
                    result.Add((record.fishId, record.specimens[specimenIndex]));
                }
            }
            return result;
        }

        public bool TryRemoveSpecimen(
            string specimenId,
            out string fishId,
            out float lengthCentimeters
        )
        {
            fishId = "";
            lengthCentimeters = 0f;
            if (string.IsNullOrWhiteSpace(specimenId))
            {
                return false;
            }

            for (int recordIndex = 0; recordIndex < data.fish.Count; recordIndex++)
            {
                FishCatchRecord record = data.fish[recordIndex];
                if (record?.specimens == null)
                {
                    continue;
                }
                for (
                    int specimenIndex = 0;
                    specimenIndex < record.specimens.Count;
                    specimenIndex++
                )
                {
                    FishSpecimenRecord specimen = record.specimens[specimenIndex];
                    if (
                        !string.Equals(
                            specimen.specimenId,
                            specimenId,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }

                    fishId = record.fishId;
                    lengthCentimeters = specimen.lengthCentimeters;
                    record.specimens.RemoveAt(specimenIndex);
                    saveService.Save(data);
                    return true;
                }
            }
            return false;
        }
    }
}
