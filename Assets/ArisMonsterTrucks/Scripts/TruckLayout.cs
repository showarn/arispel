using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public enum TruckLayoutPart
    {
        Body,
        Chassis,
        RearWheel,
        FrontWheel,
        Decal,
        Accessory,
        RearSuspension,
        FrontSuspension
    }

    [Serializable]
    public sealed class TruckPartLayout
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public float rotation;

        public TruckPartLayout(float x, float y, float width, float height, float rotation = 0f)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.rotation = rotation;
        }

        public TruckPartLayout Copy()
        {
            return new TruckPartLayout(x, y, width, height, rotation);
        }
    }

    [Serializable]
    public sealed class TruckItemLayoutEntry
    {
        public string key;
        public TruckPartLayout layout;

        public TruckItemLayoutEntry(string key, TruckPartLayout layout)
        {
            this.key = key;
            this.layout = layout;
        }
    }

    [Serializable]
    public sealed class TruckLayoutData
    {
        public int version;
        public TruckPartLayout body;
        public TruckPartLayout chassis;
        public TruckPartLayout rearWheel;
        public TruckPartLayout frontWheel;
        public TruckPartLayout decal;
        public TruckPartLayout accessory;
        public TruckPartLayout rearSuspension;
        public TruckPartLayout frontSuspension;
        public List<TruckItemLayoutEntry> itemLayouts;

        public TruckPartLayout Get(TruckLayoutPart part)
        {
            return part switch
            {
                TruckLayoutPart.Body => body,
                TruckLayoutPart.Chassis => chassis,
                TruckLayoutPart.RearWheel => rearWheel,
                TruckLayoutPart.FrontWheel => frontWheel,
                TruckLayoutPart.Decal => decal,
                TruckLayoutPart.Accessory => accessory,
                TruckLayoutPart.RearSuspension => rearSuspension,
                _ => frontSuspension
            };
        }

        public void Set(TruckLayoutPart part, TruckPartLayout value)
        {
            switch (part)
            {
                case TruckLayoutPart.Body:
                    body = value;
                    break;
                case TruckLayoutPart.Chassis:
                    chassis = value;
                    break;
                case TruckLayoutPart.RearWheel:
                    rearWheel = value;
                    break;
                case TruckLayoutPart.FrontWheel:
                    frontWheel = value;
                    break;
                case TruckLayoutPart.Decal:
                    decal = value;
                    break;
                case TruckLayoutPart.Accessory:
                    accessory = value;
                    break;
                case TruckLayoutPart.RearSuspension:
                    rearSuspension = value;
                    break;
                default:
                    frontSuspension = value;
                    break;
            }
        }
    }

    public static class TruckLayout
    {
        private const string SaveKey = "garage.layout.v1";
        private const string ResourcePath = "Config/truck_layout_defaults";
        private const int CurrentVersion = 4;
        public const float PreviewUnitsPerWorldUnit = 125f;
        public const float PreviewWorldOriginY = -0.8f;

        private static TruckLayoutData current;

        public static TruckLayoutData Current => current ??= Load();

        public static TruckLayoutData CreateDefault()
        {
            return new TruckLayoutData
            {
                version = CurrentVersion,
                body = new TruckPartLayout(0f, 5f, 680f, 315f),
                chassis = new TruckPartLayout(-3f, -142f, 455f, 114f),
                rearWheel = new TruckPartLayout(-185f, -174f, 300f, 300f),
                frontWheel = new TruckPartLayout(
                    180f,
                    -174f,
                    292.52099609375f,
                    292.52099609375f
                ),
                decal = new TruckPartLayout(
                    43.5f,
                    -44f,
                    243.23915100097657f,
                    137.0081787109375f
                ),
                accessory = new TruckPartLayout(
                    25f,
                    118f,
                    134.49951171875f,
                    61.220462799072269f
                ),
                rearSuspension = new TruckPartLayout(-182f, -92f, 90f, 180f, -10f),
                frontSuspension = new TruckPartLayout(180f, -92f, 90f, 180f, 10f),
                itemLayouts = new List<TruckItemLayoutEntry>
                {
                    new(
                        "body/body_red",
                        new TruckPartLayout(0f, 5f, 680f, 315f)
                    ),
                    new(
                        "rear-wheel/wheel_ice",
                        new TruckPartLayout(-185f, -174f, 300f, 300f)
                    ),
                    new(
                        "front-wheel/wheel_ice",
                        new TruckPartLayout(
                            180f,
                            -174f,
                            292.52099609375f,
                            292.52099609375f
                        )
                    ),
                    new(
                        "decal/decal_skull",
                        new TruckPartLayout(
                            43.5f,
                            -44f,
                            243.23915100097657f,
                            137.0081787109375f
                        )
                    ),
                    new(
                        "accessory/accessory_exhaust",
                        new TruckPartLayout(
                            170f,
                            118f,
                            302.9954528808594f,
                            137.91517639160157f
                        )
                    ),
                    new(
                        "accessory/accessory_lights",
                        new TruckPartLayout(
                            25f,
                            118f,
                            128.12200927734376f,
                            58.31760025024414f
                        )
                    )
                }
            };
        }

        public static TruckPartLayout Get(TruckLayoutPart part, string itemId = null)
        {
            TruckLayoutData data = Current;
            string itemKey = GetSelectedItemKey(part, itemId);
            if (string.IsNullOrEmpty(itemKey))
            {
                return data.Get(part);
            }

            data.itemLayouts ??= new List<TruckItemLayoutEntry>();
            foreach (TruckItemLayoutEntry entry in data.itemLayouts)
            {
                if (entry != null && entry.key == itemKey && entry.layout != null)
                {
                    return entry.layout;
                }
            }

            TruckPartLayout created = CreateDefault().Get(part).Copy();
            data.itemLayouts.Add(new TruckItemLayoutEntry(itemKey, created));
            return created;
        }

        public static void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
        }

        public static bool SaveAsProjectDefaults(out string path)
        {
            path = ResolveProjectDefaultsPath();
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(Current, true));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Kunde inte spara global billayout: " + exception.Message);
                return false;
            }
        }

        public static void ResetPart(TruckLayoutPart part, string itemId = null)
        {
            TruckPartLayout defaults = CreateDefault().Get(part);
            TruckPartLayout currentPart = Get(part, itemId);
            currentPart.x = defaults.x;
            currentPart.y = defaults.y;
            currentPart.width = defaults.width;
            currentPart.height = defaults.height;
            currentPart.rotation = defaults.rotation;
            Save();
        }

        private static TruckLayoutData Load()
        {
            TruckLayoutData defaults = LoadProjectDefaults() ?? CreateDefault();
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrWhiteSpace(json))
            {
                return defaults;
            }

            try
            {
                TruckLayoutData loaded = JsonUtility.FromJson<TruckLayoutData>(json);
                if (loaded == null)
                {
                    return defaults;
                }
                if (
                    loaded.body == null
                    || loaded.chassis == null
                    || loaded.rearWheel == null
                    || loaded.frontWheel == null
                    || loaded.decal == null
                    || loaded.accessory == null
                )
                {
                    return defaults;
                }
                if (loaded.version < 2)
                {
                    // Version 1 saknade fjäderfält. JsonUtility gav dem nollvärden i
                    // stället för null, så båda måste uttryckligen migreras.
                    loaded.rearSuspension = defaults.rearSuspension.Copy();
                    loaded.frontSuspension = defaults.frontSuspension.Copy();
                    loaded.version = 2;
                    PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(loaded));
                    PlayerPrefs.Save();
                }
                else
                {
                    loaded.rearSuspension ??= defaults.rearSuspension.Copy();
                    loaded.frontSuspension ??= defaults.frontSuspension.Copy();
                }
                loaded.itemLayouts ??= new List<TruckItemLayoutEntry>();
                if (loaded.version < 3)
                {
                    AddOrReplace(
                        loaded,
                        "body/" + TruckCustomization.GetSelected(GarageCategory.Body).Id,
                        loaded.body
                    );
                    string wheelId =
                        TruckCustomization.GetSelected(GarageCategory.Wheels).Id;
                    AddOrReplace(loaded, "rear-wheel/" + wheelId, loaded.rearWheel);
                    AddOrReplace(loaded, "front-wheel/" + wheelId, loaded.frontWheel);
                    AddOrReplace(
                        loaded,
                        "decal/" + TruckCustomization.GetSelected(GarageCategory.Decals).Id,
                        loaded.decal
                    );
                    // Den gamla kategoritransformen kom från det senast justerade
                    // avgasröret. Övriga tillbehör börjar på sina egna standardvärden.
                    AddOrReplace(
                        loaded,
                        "accessory/accessory_exhaust",
                        loaded.accessory
                    );
                    loaded.version = CurrentVersion;
                    PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(loaded));
                    PlayerPrefs.Save();
                }
                if (loaded.version < 4)
                {
                    // Version 4 är den godkända, visuellt inställda layouten.
                    // Kopiera in de kodade värdena så att äldre PlayerPrefs inte
                    // kan flytta tillbaka delarna efter en ny build.
                    loaded.body = defaults.body.Copy();
                    loaded.chassis = defaults.chassis.Copy();
                    loaded.rearWheel = defaults.rearWheel.Copy();
                    loaded.frontWheel = defaults.frontWheel.Copy();
                    loaded.decal = defaults.decal.Copy();
                    loaded.accessory = defaults.accessory.Copy();
                    loaded.rearSuspension = defaults.rearSuspension.Copy();
                    loaded.frontSuspension = defaults.frontSuspension.Copy();
                    foreach (TruckItemLayoutEntry entry in defaults.itemLayouts)
                    {
                        AddOrReplace(loaded, entry.key, entry.layout);
                    }
                    loaded.version = CurrentVersion;
                    PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(loaded));
                    PlayerPrefs.Save();
                }
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Kunde inte läsa sparad billayout: " + exception.Message);
                return defaults;
            }
        }

        private static string GetSelectedItemKey(TruckLayoutPart part, string itemId)
        {
            return part switch
            {
                TruckLayoutPart.Decal =>
                    "decal/" + TruckCustomization.GetSelected(GarageCategory.Decals).Id,
                TruckLayoutPart.Accessory =>
                    "accessory/"
                    + (
                        string.IsNullOrEmpty(itemId)
                            ? TruckCustomization.GetSelected(GarageCategory.Accessories).Id
                            : itemId
                    ),
                _ => null
            };
        }

        private static void AddOrReplace(
            TruckLayoutData data,
            string key,
            TruckPartLayout value
        )
        {
            if (value == null)
            {
                return;
            }

            foreach (TruckItemLayoutEntry entry in data.itemLayouts)
            {
                if (entry != null && entry.key == key)
                {
                    entry.layout = value.Copy();
                    return;
                }
            }
            data.itemLayouts.Add(new TruckItemLayoutEntry(key, value.Copy()));
        }

        private static TruckLayoutData LoadProjectDefaults()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<TruckLayoutData>(asset.text);
            }
            catch (Exception exception)
            {
                Debug.LogError("Ogiltig global billayout: " + exception.Message);
                return null;
            }
        }

        private static string ResolveProjectDefaultsPath()
        {
#if UNITY_EDITOR
            return Path.Combine(
                Application.dataPath,
                "Resources",
                "Config",
                "truck_layout_defaults.json"
            );
#elif DEVELOPMENT_BUILD
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "..")
            );
            string assetsPath = Path.Combine(projectRoot, "Assets");
            if (!Directory.Exists(assetsPath))
            {
                return null;
            }
            return Path.Combine(
                assetsPath,
                "Resources",
                "Config",
                "truck_layout_defaults.json"
            );
#else
            return null;
#endif
        }
    }
}
