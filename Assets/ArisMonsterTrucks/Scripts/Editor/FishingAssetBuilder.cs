#if UNITY_EDITOR
using System.IO;
using ArisMonsterTrucks.Fishing;
using UnityEditor;
using UnityEngine;

namespace ArisMonsterTrucks.Editor
{
    public static class FishingAssetBuilder
    {
        private const string DefinitionFolder =
            "Assets/Resources/Config/Fishing";
        private const string FishArtFolder =
            "Assets/Resources/Art/Fishing/Fish";

        private readonly struct DefinitionSeed
        {
            public readonly string Id;
            public readonly string Name;
            public readonly FishRarity Rarity;
            public readonly float Weight;
            public readonly float MinimumLength;
            public readonly float MaximumLength;
            public readonly float SwimSpeed;
            public readonly string Fact;
            public readonly string Color;

            public DefinitionSeed(
                string id,
                string name,
                FishRarity rarity,
                float weight,
                float minimumLength,
                float maximumLength,
                float swimSpeed,
                string fact,
                string color
            )
            {
                Id = id;
                Name = name;
                Rarity = rarity;
                Weight = weight;
                MinimumLength = minimumLength;
                MaximumLength = maximumLength;
                SwimSpeed = swimSpeed;
                Fact = fact;
                Color = color;
            }
        }

        private static readonly DefinitionSeed[] Seeds =
        {
            new(
                "blue-fin",
                "Blåfenan",
                FishRarity.Common,
                34f,
                12f,
                26f,
                1.05f,
                "Blåfenan tycker om lugna simturer.",
                "#38A9F4"
            ),
            new(
                "sun-fish",
                "Solfisken",
                FishRarity.Common,
                32f,
                11f,
                25f,
                0.95f,
                "Solfisken glittrar som en liten sol.",
                "#FFD338"
            ),
            new(
                "stripe-fish",
                "Randfisken",
                FishRarity.Common,
                30f,
                13f,
                29f,
                1.12f,
                "Randfisken har tre fina ränder.",
                "#FF782D"
            ),
            new(
                "seaweed-fish",
                "Sjögräsfisken",
                FishRarity.Uncommon,
                15f,
                16f,
                34f,
                0.82f,
                "Sjögräsfisken gömmer sig bland gröna blad.",
                "#72C83E"
            ),
            new(
                "star-tail",
                "Stjärtfisken",
                FishRarity.Uncommon,
                13f,
                15f,
                32f,
                1.18f,
                "Stjärtfiskens stjärt ser ut som en stjärna.",
                "#9C68E7"
            ),
            new(
                "golden-fish",
                "Guldfisken",
                FishRarity.Uncommon,
                4f,
                18f,
                38f,
                0.9f,
                "Guldfisken lyser varmt under vattnet.",
                "#F3B620"
            ),
            new("pink-fish", "Rosa fisken", FishRarity.Common, 24f, 10f, 24f, 1.02f, "Rosa fisken älskar små bubblor.", "#F48DB5"),
            new("silver-fish", "Silverfisken", FishRarity.Common, 23f, 12f, 27f, 1.14f, "Silverfisken blänker när den svänger.", "#B9C7D5"),
            new("aqua-fish", "Turkosfisken", FishRarity.Common, 22f, 11f, 25f, 1.08f, "Turkosfisken simmar gärna i ringar.", "#40D5D2"),
            new("peach-fish", "Persikofisken", FishRarity.Common, 21f, 12f, 28f, 0.96f, "Persikofisken är mjuk i färgen.", "#FFAA72"),
            new("brown-fish", "Kastanjefisken", FishRarity.Common, 20f, 14f, 30f, 0.9f, "Kastanjefisken vilar gärna vid stenar.", "#A86D48"),
            new("bubble-fish", "Bubbelfisken", FishRarity.Uncommon, 16f, 14f, 31f, 1.06f, "Bubbelfisken gör pärlande bubblor.", "#7ACFF4"),
            new("leaf-fish", "Lövfisken", FishRarity.Uncommon, 15f, 15f, 33f, 0.86f, "Lövfiskens fenor liknar gröna blad.", "#79C839"),
            new("coral-fish", "Korallfisken", FishRarity.Common, 14f, 16f, 34f, 0.98f, "Korallfisken tycker om färgglada gömställen.", "#F47D87"),
            new("lantern-fish", "Lyktfisken", FishRarity.Common, 13f, 13f, 29f, 0.92f, "Lyktfisken har en liten vänlig lykta.", "#FFD331"),
            new("pearl-fish", "Pärlfisken", FishRarity.Uncommon, 12f, 15f, 32f, 0.88f, "Pärlfisken skimrar i regnbågens färger.", "#EDE9EE"),
            new("butterfly-fish", "Fjärilsfisken", FishRarity.Rare, 10f, 17f, 35f, 1.16f, "Fjärilsfisken har fenor som vingar.", "#35BFD1"),
            new("tiger-fish", "Tigerfisken", FishRarity.Rare, 9.5f, 18f, 38f, 1.2f, "Tigerfisken visar stolt sina ränder.", "#F58B20"),
            new("panda-fish", "Pandafisken", FishRarity.Common, 9f, 17f, 36f, 0.94f, "Pandafisken är svart, vit och nyfiken.", "#E7E5DD"),
            new("heart-fish", "Hjärtfisken", FishRarity.Common, 8.5f, 16f, 34f, 1.02f, "Hjärtfisken har en hjärtformad stjärt.", "#F1538B"),
            new("flower-fish", "Blomfisken", FishRarity.Uncommon, 8f, 18f, 37f, 0.96f, "Blomfiskens fenor ser ut som kronblad.", "#FFB22F"),
            new("moon-fish", "Månfisken", FishRarity.Uncommon, 7f, 20f, 41f, 0.9f, "Månfisken glittrar under månen.", "#285BD4"),
            new("aurora-fish", "Norrskensfisken", FishRarity.Rare, 6.8f, 21f, 43f, 1.04f, "Norrskensfisken bär himlens färger.", "#42D6CA"),
            new("ruby-fish", "Rubinfenan", FishRarity.Epic, 6.6f, 19f, 40f, 1.08f, "Rubinfenans fjäll liknar röda ädelstenar.", "#D9273C"),
            new("sapphire-fish", "Safirfisken", FishRarity.Common, 6.4f, 20f, 42f, 1.12f, "Safirfisken lyser djupt blå.", "#166EE8"),
            new("emerald-fish", "Smaragdfisken", FishRarity.Uncommon, 6.2f, 20f, 42f, 1.0f, "Smaragdfisken glimmar grönt.", "#1CC958"),
            new("lightning-fish", "Blixtfisken", FishRarity.Rare, 6f, 18f, 39f, 1.32f, "Blixtfisken är snabb men alltid snäll.", "#FFD51A"),
            new("cloud-fish", "Molnfisken", FishRarity.Epic, 5.8f, 22f, 45f, 0.78f, "Molnfisken svävar mjukt genom vattnet.", "#A9DDF6"),
            new("candy-fish", "Godisfisken", FishRarity.Epic, 5.6f, 17f, 38f, 1.06f, "Godisfisken har många glada färger.", "#F58FAE"),
            new("robot-fish", "Robotfisken", FishRarity.Common, 5.4f, 21f, 44f, 1.14f, "Robotfisken säger blipp när den vänder.", "#6DA8CF"),
            new("dino-fish", "Dinofisken", FishRarity.Uncommon, 5.2f, 23f, 48f, 0.92f, "Dinofisken har små mjuka taggar.", "#58B943"),
            new("rainbow-dragon-fish", "Regnbågsdraken", FishRarity.Legendary, 4f, 28f, 58f, 1.12f, "Regnbågsdraken är sjöns färggladaste hemlighet.", "#F26B42"),
            new("crystal-fish", "Kristallfisken", FishRarity.Rare, 3.8f, 25f, 54f, 0.88f, "Kristallfisken glittrar som is.", "#92E8FF"),
            new("nebula-fish", "Nebulosafisken", FishRarity.Epic, 3.6f, 27f, 57f, 0.94f, "Nebulosafisken bär små stjärnor på ryggen.", "#6947D8"),
            new("phoenix-fish", "Fenixfisken", FishRarity.Epic, 3.4f, 26f, 56f, 1.22f, "Fenixfisken lyser varmt som en soluppgång.", "#F26B18"),
            new("crown-fish", "Kronfisken", FishRarity.Legendary, 3.2f, 30f, 62f, 0.84f, "Kronfisken är Diamantdjupets kungliga vän.", "#D8A719")
        };

        [MenuItem("Aris Monstertrucks/Fiske/Skapa fiskdata")]
        public static void EnsureFishingAssets()
        {
            Directory.CreateDirectory(DefinitionFolder);
            ConfigureTexture(
                "Assets/Resources/Art/Fishing/fishing_background_rigged.png",
                false,
                2048
            );
            string[] locationTextures =
            {
                "lily_cove",
                "coral_coast",
                "aurora_lake",
                "star_lagoon",
                "diamond_depths"
            };
            for (int index = 0; index < locationTextures.Length; index++)
            {
                ConfigureTexture(
                    "Assets/Resources/Art/Fishing/Locations/"
                        + locationTextures[index]
                        + ".png",
                    false,
                    2048
                );
            }
            for (int index = 0; index < Seeds.Length; index++)
            {
                DefinitionSeed seed = Seeds[index];
                string texturePath = FishArtFolder + "/" + seed.Id + ".png";
                ConfigureTexture(texturePath, true, 512);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                if (sprite == null)
                {
                    Debug.LogError("Fiskgrafik saknas: " + texturePath);
                    continue;
                }

                string assetPath =
                    DefinitionFolder + "/" + seed.Id + ".asset";
                FishDefinition definition =
                    AssetDatabase.LoadAssetAtPath<FishDefinition>(assetPath);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<FishDefinition>();
                    AssetDatabase.CreateAsset(definition, assetPath);
                }
                definition.Configure(
                    seed.Id,
                    seed.Name,
                    index,
                    sprite,
                    seed.Rarity,
                    seed.Weight,
                    seed.MinimumLength,
                    seed.MaximumLength,
                    seed.SwimSpeed,
                    seed.Fact,
                    RuntimeArt.Hex(seed.Color)
                );
                EditorUtility.SetDirty(definition);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            FishCatalog.ClearCache();
            Debug.Log("FISHING_ASSETS_READY: " + Seeds.Length + " fiskar.");
        }

        private static void ConfigureTexture(
            string assetPath,
            bool sprite,
            int maximumSize
        )
        {
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport
            );
            TextureImporter importer =
                AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("Kunde inte importera textur: " + assetPath);
                return;
            }

            importer.textureType = sprite
                ? TextureImporterType.Sprite
                : TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = sprite;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = maximumSize;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }
}
#endif
