using UnityEngine;

namespace ArisMonsterTrucks.Fishing
{
    public static class FishSalePricing
    {
        private static readonly int[] MinimumPrices =
        {
            10, 25, 75, 250, 5000
        };

        private static readonly int[] MaximumPrices =
        {
            50, 100, 300, 1000, 20000
        };

        public static int Calculate(
            FishDefinition definition,
            float lengthCentimeters
        )
        {
            if (definition == null)
            {
                return 0;
            }

            int rarityIndex = Mathf.Clamp(
                (int)definition.Rarity,
                0,
                MinimumPrices.Length - 1
            );
            float lengthProgress = Mathf.InverseLerp(
                definition.MinimumLengthCentimeters,
                definition.MaximumLengthCentimeters,
                Mathf.Clamp(
                    lengthCentimeters,
                    definition.MinimumLengthCentimeters,
                    definition.MaximumLengthCentimeters
                )
            );
            int price = Mathf.RoundToInt(
                Mathf.Lerp(
                    MinimumPrices[rarityIndex],
                    MaximumPrices[rarityIndex],
                    lengthProgress
                ) / 5f
            ) * 5;
            return Mathf.Clamp(
                price,
                MinimumPrices[rarityIndex],
                MaximumPrices[rarityIndex]
            );
        }
    }

    public sealed class FishingRodDefinition
    {
        public FishingRodDefinition(
            string id,
            string displayName,
            int price,
            float rareChanceBonus,
            string borderHex,
            string shaftHex,
            string handleHex,
            string accentHex,
            string symbol
        )
        {
            Id = id;
            DisplayName = displayName;
            Price = Mathf.Max(0, price);
            RareChanceBonus = Mathf.Clamp01(rareChanceBonus);
            BorderHex = borderHex;
            ShaftHex = shaftHex;
            HandleHex = handleHex;
            AccentHex = accentHex;
            Symbol = symbol;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int Price { get; }
        public float RareChanceBonus { get; }
        public string BorderHex { get; }
        public string ShaftHex { get; }
        public string HandleHex { get; }
        public string AccentHex { get; }
        public string Symbol { get; }
    }

    public static class FishingRodCollection
    {
        private const string SelectedRodKey = "fishing.rod.v1.selected";
        private const string StarRodOwnedKey = "fishing.rod.v1.starOwned";
        private const string OwnedRodPrefix = "fishing.rod.v2.owned.";
        private const string ClassicRodId = "classic";
        private const string StarRodId = "star";

        private static readonly FishingRodDefinition[] Rods =
        {
            new(
                "classic", "KLASSISKT SPÖ", 0, 0f,
                "#4E2B18", "#9A5A25", "#E0B15B", "#FFD84D", ""
            ),
            new(
                "star", "STJÄRNSPÖ", 2500, 0.03f,
                "#39216A", "#55DDF2", "#FF71C8", "#FFF3AD", "★"
            ),
            new(
                "ocean", "HAVSSPÖ", 5000, 0.05f,
                "#123F67", "#36D9EF", "#2586D9", "#B8FAFF", "≈"
            ),
            new(
                "fire", "ELDSPÖ", 8000, 0.06f,
                "#651B12", "#FF6438", "#FFB12F", "#FFF080", "◆"
            ),
            new(
                "galaxy", "GALAXSPÖ", 15000, 0.08f,
                "#271254", "#8C5BFF", "#E85AD7", "#74E8FF", "✦"
            ),
            new(
                "royal", "KUNGASPÖ", 30000, 0.10f,
                "#5B3A00", "#FFD84D", "#7C3FC9", "#FFFFFF", "♛"
            ),
            new(
                "aurora", "NORRSKENSSPÖ", 45000, 0.12f,
                "#123D46", "#62F2C4", "#5F7BFF", "#FFF59A", "✧"
            ),
            new(
                "diamond", "DIAMANTSPÖ", 70000, 0.15f,
                "#24304D", "#B9F2FF", "#6677CC", "#FFFFFF", "◇"
            )
        };

        public static System.Collections.Generic.IReadOnlyList<FishingRodDefinition>
            All => Rods;

        public static FishingRodDefinition Selected
        {
            get
            {
                string selectedId = PlayerPrefs.GetString(
                    SelectedRodKey,
                    ClassicRodId
                );
                for (int index = 0; index < Rods.Length; index++)
                {
                    if (
                        Rods[index].Id == selectedId
                        && IsOwned(Rods[index])
                    )
                    {
                        return Rods[index];
                    }
                }
                return Rods[0];
            }
        }

        public static float RareChanceBonus => Selected.RareChanceBonus;

        public static bool IsOwned(FishingRodDefinition rod)
        {
            if (rod == null)
            {
                return false;
            }
            if (rod.Id == ClassicRodId)
            {
                return true;
            }
            if (
                rod.Id == StarRodId
                && PlayerPrefs.GetInt(StarRodOwnedKey, 0) == 1
            )
            {
                return true;
            }
            return PlayerPrefs.GetInt(OwnedRodPrefix + rod.Id, 0) == 1;
        }

        public static bool IsSelected(FishingRodDefinition rod)
        {
            return rod != null && Selected.Id == rod.Id;
        }

        public static bool TryBuyOrSelect(FishingRodDefinition rod)
        {
            if (rod == null)
            {
                return false;
            }
            if (!IsOwned(rod))
            {
                if (!CoinWallet.TrySpend(rod.Price))
                {
                    return false;
                }
                PlayerPrefs.SetInt(OwnedRodPrefix + rod.Id, 1);
                if (rod.Id == StarRodId)
                {
                    PlayerPrefs.SetInt(StarRodOwnedKey, 1);
                }
            }
            PlayerPrefs.SetString(SelectedRodKey, rod.Id);
            PlayerPrefs.Save();
            return true;
        }
    }

    public sealed class FishingLureDefinition
    {
        public FishingLureDefinition(
            string id,
            string displayName,
            int price,
            float rareChanceBonus,
            string borderHex,
            string bodyHex,
            string accentHex,
            string symbol,
            int style
        )
        {
            Id = id;
            DisplayName = displayName;
            Price = Mathf.Max(0, price);
            RareChanceBonus = Mathf.Clamp01(rareChanceBonus);
            BorderHex = borderHex;
            BodyHex = bodyHex;
            AccentHex = accentHex;
            Symbol = symbol;
            Style = Mathf.Clamp(style, 0, 3);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int Price { get; }
        public float RareChanceBonus { get; }
        public string BorderHex { get; }
        public string BodyHex { get; }
        public string AccentHex { get; }
        public string Symbol { get; }
        public int Style { get; }
    }

    public static class FishingLureCollection
    {
        private const string SelectedLureKey = "fishing.lure.v1.selected";
        private const string OwnedLurePrefix = "fishing.lure.v1.owned.";

        private static readonly FishingLureDefinition[] Lures =
        {
            new("copper-spoon", "KOPPARSKED", 300, 0.01f, "#5B2D12", "#D87932", "#FFE09A", "●", 0),
            new("red-spinner", "RÖD SPINNARE", 450, 0.01f, "#651B12", "#F04432", "#FFD84D", "✦", 1),
            new("yellow-jig", "GUL JIGG", 600, 0.01f, "#6A4A00", "#FFD43B", "#FF7A35", "◆", 2),
            new("silver-arrow", "SILVERPIL", 800, 0.01f, "#394B61", "#D8E8F2", "#65BFFF", "›", 3),
            new("frog-lure", "GRODDRAG", 1000, 0.01f, "#245522", "#65C832", "#FFF080", "●", 2),
            new("blue-minnow", "BLÅ MINNOW", 1250, 0.02f, "#123F67", "#39BCEB", "#EAF8FF", "≈", 3),
            new("rainbow-spoon", "REGNBÅGSSKED", 1500, 0.02f, "#5C2A83", "#E85A96", "#FFD43B", "✦", 0),
            new("fire-spinner", "ELDSPINNARE", 1800, 0.02f, "#651B12", "#FF6438", "#FFF080", "◆", 1),
            new("pearl-jig", "PÄRLEMORJIGG", 2200, 0.02f, "#514B6A", "#F5E8FF", "#74E8FF", "●", 2),
            new("night-minnow", "NATTFISK", 2600, 0.02f, "#151B38", "#334A91", "#9DF7FF", "≈", 3),
            new("coral-spoon", "KORALLSKED", 3000, 0.02f, "#7B3045", "#FF7994", "#FFF3AD", "✦", 0),
            new("emerald-spinner", "SMARAGDSPINNARE", 3500, 0.02f, "#104D3A", "#27C58B", "#D5FF72", "◆", 1),
            new("ice-jig", "ISJIGG", 4000, 0.02f, "#214E72", "#8DE7FF", "#FFFFFF", "●", 2),
            new("sun-minnow", "SOLMINNOW", 4600, 0.02f, "#7A4100", "#FFB12F", "#FFF080", "≈", 3),
            new("moon-spoon", "MÅNSKED", 5200, 0.02f, "#272856", "#8F91D8", "#FFFFFF", "☾", 0),
            new("aurora-spinner", "NORRSKENSSPINNARE", 6000, 0.03f, "#123D46", "#62F2C4", "#8C5BFF", "✦", 1),
            new("ruby-jig", "RUBINJIGG", 7000, 0.03f, "#64152D", "#E3355B", "#FFD0D8", "◆", 2),
            new("galaxy-minnow", "GALAXMINNOW", 8200, 0.03f, "#271254", "#8C5BFF", "#74E8FF", "✧", 3),
            new("gold-spoon", "GULDSKED", 9800, 0.03f, "#6A4300", "#FFD84D", "#FFFFFF", "♛", 0),
            new("diamond-spinner", "DIAMANTSPINNARE", 12000, 0.03f, "#24304D", "#B9F2FF", "#FFFFFF", "◇", 1)
        };

        public static System.Collections.Generic.IReadOnlyList<FishingLureDefinition>
            All => Lures;

        public static FishingLureDefinition Selected
        {
            get
            {
                string selectedId = PlayerPrefs.GetString(SelectedLureKey, "");
                for (int index = 0; index < Lures.Length; index++)
                {
                    if (
                        Lures[index].Id == selectedId
                        && IsOwned(Lures[index])
                    )
                    {
                        return Lures[index];
                    }
                }
                return null;
            }
        }

        public static float RareChanceBonus =>
            Selected == null ? 0f : Selected.RareChanceBonus;

        public static bool IsOwned(FishingLureDefinition lure)
        {
            return lure != null
                && PlayerPrefs.GetInt(OwnedLurePrefix + lure.Id, 0) == 1;
        }

        public static bool IsSelected(FishingLureDefinition lure)
        {
            return lure != null && Selected?.Id == lure.Id;
        }

        public static bool TryBuyOrSelect(FishingLureDefinition lure)
        {
            if (lure == null)
            {
                return false;
            }
            if (!IsOwned(lure))
            {
                if (!CoinWallet.TrySpend(lure.Price))
                {
                    return false;
                }
                PlayerPrefs.SetInt(OwnedLurePrefix + lure.Id, 1);
            }
            PlayerPrefs.SetString(SelectedLureKey, lure.Id);
            PlayerPrefs.Save();
            return true;
        }

        public static void SelectFloat()
        {
            PlayerPrefs.DeleteKey(SelectedLureKey);
            PlayerPrefs.Save();
        }
    }

    public static class FishingBaitInventory
    {
        public const int PackPrice = 500;
        public const int WormsPerPack = 25;
        public const float RareChanceBonus = 0.03f;

        private const string WormCountKey = "fishing.bait.v1.worms";

        public static int WormCount =>
            Mathf.Max(0, PlayerPrefs.GetInt(WormCountKey, 0));

        public static bool TryBuyPack()
        {
            if (!CoinWallet.TrySpend(PackPrice))
            {
                return false;
            }
            PlayerPrefs.SetInt(
                WormCountKey,
                WormCount + WormsPerPack
            );
            PlayerPrefs.Save();
            return true;
        }

        public static bool TryConsumeWorm()
        {
            int current = WormCount;
            if (current <= 0)
            {
                return false;
            }
            PlayerPrefs.SetInt(WormCountKey, current - 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}
