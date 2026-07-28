using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public enum GarageCategory
    {
        Body,
        Wheels,
        Decals,
        Accessories
    }

    public sealed class GarageItemDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public GarageCategory Category { get; }
        public string ResourcePath { get; }
        public int RequiredRating { get; }
        public int Price { get; }
        public string ColorHex { get; }

        public GarageItemDefinition(
            string id,
            string displayName,
            GarageCategory category,
            string resourcePath,
            int requiredRating,
            string colorHex = "#FFFFFF",
            int price = 0
        )
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            ResourcePath = resourcePath;
            RequiredRating = requiredRating;
            ColorHex = colorHex;
            Price = Mathf.Max(0, price);
        }
    }

    public static class TruckCustomization
    {
        private const string SelectionPrefix = "garage.selection.";
        private const string EquippedAccessoryPrefix = "garage.accessory.equipped.";
        private const string AccessoryMigrationKey = "garage.accessory.multiselect.v1";

        private static readonly GarageItemDefinition[] Catalog =
        {
            new("body_red", "RÖD", GarageCategory.Body, "Art/Truck/body_red", 0, "#FF5252"),
            new("body_orange", "ORANGE", GarageCategory.Body, "Art/Truck/body_orange", 0, "#FF9638"),
            new("body_yellow", "GUL", GarageCategory.Body, "Art/Truck/body_yellow", 0, "#FFE45C"),
            new("body_lime", "LIME", GarageCategory.Body, "Art/Truck/body_lime", 0, "#A8F05A"),
            new("body_green", "GRÖN", GarageCategory.Body, "Art/Truck/body_green", 0, "#55D879"),
            new("body_turquoise", "TURKOS", GarageCategory.Body, "Art/Truck/body_turquoise", 0, "#45DCCB"),
            new("body_sky", "LJUSBLÅ", GarageCategory.Body, "Art/Truck/body_sky", 0, "#64CCFF"),
            new("body_blue", "BLÅ", GarageCategory.Body, "Art/Truck/body_blue", 0, "#5590FF"),
            new("body_navy", "MARIN", GarageCategory.Body, "Art/Truck/body_navy", 0, "#4056B8"),
            new("body_purple", "LILA", GarageCategory.Body, "Art/Truck/body_purple", 0, "#9868E8"),
            new("body_pink", "ROSA", GarageCategory.Body, "Art/Truck/body_pink", 0, "#FF87C8"),
            new("body_magenta", "MAGENTA", GarageCategory.Body, "Art/Truck/body_magenta", 0, "#E853C6"),
            new("body_white", "VIT", GarageCategory.Body, "Art/Truck/body_white", 0, "#FFFFFF"),
            new("body_silver", "SILVER", GarageCategory.Body, "Art/Truck/body_silver", 0, "#B9C5D5"),
            new("body_black", "SVART", GarageCategory.Body, "Art/Truck/body_black", 0, "#48505C"),

            new("wheel_standard", "STANDARD", GarageCategory.Wheels, "Art/Truck/wheel_standard", 0),
            new("wheel_mud", "LERA", GarageCategory.Wheels, "Art/Truck/wheel_mud", 0, "#FFFFFF", 500),
            new("wheel_ice", "IS", GarageCategory.Wheels, "Art/Truck/wheel_ice", 0, "#FFFFFF", 1000),
            new("wheel_glow", "NEON", GarageCategory.Wheels, "Art/Truck/wheel_glow", 0, "#FFFFFF", 1500),

            new("decal_none", "INGEN", GarageCategory.Decals, "", 0),
            new("decal_flame", "FLAMMA", GarageCategory.Decals, "Art/Parts/decal_flame", 0, "#FFFFFF", 500),
            new("decal_lightning", "BLIXT", GarageCategory.Decals, "Art/Parts/decal_lightning", 0, "#FFFFFF", 500),
            new("decal_skull", "DÖSKALLE", GarageCategory.Decals, "Art/Parts/decal_skull", 0, "#FFFFFF", 500),

            new("accessory_none", "INGEN", GarageCategory.Accessories, "", 0),
            new("accessory_exhaust", "AVGAS", GarageCategory.Accessories, "Art/Parts/exhaust", 0, "#FFFFFF", 2000),
            new("accessory_lights", "LJUSRAMP", GarageCategory.Accessories, "Art/Parts/roof_light_bar", 0, "#FFFFFF", 2500)
        };
        private const string OwnershipPrefix = "garage.owned.";
        private const string PurchaseMigrationKey = "garage.purchases.v1";

        public static IReadOnlyList<GarageItemDefinition> GetItems(GarageCategory category)
        {
            List<GarageItemDefinition> result = new();
            foreach (GarageItemDefinition item in Catalog)
            {
                if (item.Category == category)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static GarageItemDefinition GetSelected(GarageCategory category)
        {
            EnsurePurchaseMigration();
            if (category == GarageCategory.Accessories)
            {
                IReadOnlyList<GarageItemDefinition> equipped = GetEquippedAccessories();
                return equipped.Count > 0 ? equipped[0] : Find("accessory_none");
            }

            string defaultId = category switch
            {
                GarageCategory.Body => "body_blue",
                GarageCategory.Wheels => "wheel_standard",
                GarageCategory.Decals => "decal_none",
                _ => "accessory_none"
            };
            string selectedId = PlayerPrefs.GetString(SelectionPrefix + category, defaultId);
            GarageItemDefinition selected = Find(selectedId) ?? Find(defaultId);
            if (!IsUnlocked(selected) || !IsOwned(selected))
            {
                selected = Find(defaultId);
            }
            return selected;
        }

        public static bool TrySelect(GarageItemDefinition item)
        {
            if (item == null || !IsUnlocked(item))
            {
                return false;
            }
            EnsurePurchaseMigration();
            if (!IsOwned(item))
            {
                if (!CoinWallet.TrySpend(item.Price))
                {
                    return false;
                }
                PlayerPrefs.SetInt(OwnershipPrefix + item.Id, 1);
            }

            if (item.Category == GarageCategory.Accessories)
            {
                EnsureAccessoryMigration();
                if (item.Id == "accessory_none")
                {
                    PlayerPrefs.DeleteKey(
                        EquippedAccessoryPrefix + "accessory_exhaust"
                    );
                    PlayerPrefs.DeleteKey(
                        EquippedAccessoryPrefix + "accessory_lights"
                    );
                }
                else
                {
                    string key = EquippedAccessoryPrefix + item.Id;
                    bool equipped = PlayerPrefs.GetInt(key, 0) == 1;
                    PlayerPrefs.SetInt(key, equipped ? 0 : 1);
                }
            }
            else
            {
                string selectedId = PlayerPrefs.GetString(
                    SelectionPrefix + item.Category,
                    ""
                );
                PlayerPrefs.SetString(
                    SelectionPrefix + item.Category,
                    item.Category == GarageCategory.Decals
                        && item.Id != "decal_none"
                        && selectedId == item.Id
                            ? "decal_none"
                            : item.Id
                );
            }
            PlayerPrefs.Save();
            return true;
        }

        public static IReadOnlyList<GarageItemDefinition> GetEquippedAccessories()
        {
            EnsureAccessoryMigration();
            EnsurePurchaseMigration();
            List<GarageItemDefinition> result = new();
            foreach (GarageItemDefinition item in Catalog)
            {
                if (
                    item.Category == GarageCategory.Accessories
                    && item.Id != "accessory_none"
                    && IsUnlocked(item)
                    && IsOwned(item)
                    && PlayerPrefs.GetInt(EquippedAccessoryPrefix + item.Id, 0) == 1
                )
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static bool IsSelected(GarageItemDefinition item)
        {
            if (item == null)
            {
                return false;
            }
            if (item.Category != GarageCategory.Accessories)
            {
                return GetSelected(item.Category).Id == item.Id;
            }

            IReadOnlyList<GarageItemDefinition> equipped = GetEquippedAccessories();
            if (item.Id == "accessory_none")
            {
                return equipped.Count == 0;
            }
            foreach (GarageItemDefinition selected in equipped)
            {
                if (selected.Id == item.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsUnlocked(GarageItemDefinition item)
        {
            return item != null
                && LevelProgression.GetBestLevelOneRating() >= item.RequiredRating;
        }

        public static bool IsOwned(GarageItemDefinition item)
        {
            if (item == null || item.Price <= 0)
            {
                return true;
            }

            EnsurePurchaseMigration();
            return PlayerPrefs.GetInt(OwnershipPrefix + item.Id, 0) == 1;
        }

        public static Color SelectedBodyColor()
        {
            return Color.white;
        }

        public static void ResetForFreshProfile()
        {
            foreach (GarageCategory category in System.Enum.GetValues(typeof(GarageCategory)))
            {
                PlayerPrefs.DeleteKey(SelectionPrefix + category);
            }
            foreach (GarageItemDefinition item in Catalog)
            {
                PlayerPrefs.DeleteKey(OwnershipPrefix + item.Id);
                PlayerPrefs.DeleteKey(EquippedAccessoryPrefix + item.Id);
            }
            PlayerPrefs.DeleteKey(AccessoryMigrationKey);
            PlayerPrefs.DeleteKey(PurchaseMigrationKey);
            PlayerPrefs.Save();
        }

        private static GarageItemDefinition Find(string id)
        {
            foreach (GarageItemDefinition item in Catalog)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        private static void EnsureAccessoryMigration()
        {
            if (PlayerPrefs.GetInt(AccessoryMigrationKey, 0) == 1)
            {
                return;
            }

            string oldId = PlayerPrefs.GetString(
                SelectionPrefix + GarageCategory.Accessories,
                "accessory_none"
            );
            if (oldId == "accessory_exhaust" || oldId == "accessory_lights")
            {
                PlayerPrefs.SetInt(EquippedAccessoryPrefix + oldId, 1);
            }
            PlayerPrefs.SetInt(AccessoryMigrationKey, 1);
            PlayerPrefs.Save();
        }

        private static void EnsurePurchaseMigration()
        {
            if (PlayerPrefs.GetInt(PurchaseMigrationKey, 0) == 1)
            {
                return;
            }

            string selectedDecal = PlayerPrefs.GetString(
                SelectionPrefix + GarageCategory.Decals,
                "decal_none"
            );
            if (selectedDecal != "decal_none")
            {
                PlayerPrefs.SetInt(OwnershipPrefix + selectedDecal, 1);
            }

            EnsureAccessoryMigration();
            foreach (GarageItemDefinition item in Catalog)
            {
                if (
                    item.Category == GarageCategory.Accessories
                    && PlayerPrefs.GetInt(EquippedAccessoryPrefix + item.Id, 0) == 1
                )
                {
                    PlayerPrefs.SetInt(OwnershipPrefix + item.Id, 1);
                }
            }

            PlayerPrefs.SetInt(PurchaseMigrationKey, 1);
            PlayerPrefs.Save();
        }
    }
}
