using System;
using System.Collections.Generic;
using System.IO;
using ArisMonsterTrucks.Stories;
using UnityEditor;
using UnityEngine;

namespace ArisMonsterTrucks.EditorTools
{
    public static class StoryAssetBuilder
    {
        private const string StoryRoot =
            "Assets/Resources/Art/Stories/LillaLumi";
        private const string AudioPath =
            "Assets/Resources/Audio/Stories/lilla_lumi.mp3";
        private const string AssetPath =
            "Assets/Resources/Stories/lilla-lumi.asset";

        private static readonly float[] Timeline =
        {
            0f,
            4.04f,
            22.50f,
            43.81f,
            60.49f,
            79.23f,
            95.61f,
            111.07f,
            128.51f,
            143.61f,
            152.23f,
            176.07f,
            198.59f,
            210.25f,
            232.17f,
            240.51f,
            259.63f,
            287.138f
        };

        private static readonly string[] Texts =
        {
            "Lilla Lumi och månens glittrande stig",
            "Det var en gång en liten vit säl som hette Lumi.\n\n"
                + "Lumi bodde på en mjuk klippa nära havet tillsammans med sin mamma. "
                + "På dagarna brukade hon simma, leka med bubblor och försöka fånga sitt eget blanka svansplask.\n\n"
                + "Men allra mest tyckte Lumi om kvällen.",
            "När solen sjönk ner bakom havet blev himlen rosa, lila och till sist alldeles mörkblå. "
                + "Då tändes den stora, runda månen ovanför vattnet.\n\n"
                + "En kväll låg Lumi på sin klippa och tittade ut över havet.\n\n"
                + "Månen lyste så starkt att den skapade en lång, glittrande stig över vågorna.",
            "– Mamma, vart leder månens stig? frågade Lumi.\n\n"
                + "Mamma Säl tittade ut över vattnet och log.\n\n"
                + "– Det vet bara månen, svarade hon. Men du får simma en liten bit och se efter. "
                + "Jag finns här och väntar på dig.",
            "Lumi gled försiktigt ner i det ljumma vattnet.\n\n"
                + "Plopp.\n\nHavet gungade mjukt omkring henne.\n\n"
                + "Upp och ner.\n\nUpp och ner.\n\n"
                + "Lumi började simma längs den glittrande stigen. "
                + "Månskenet dansade framför nosen som hundratals små silverstjärnor.",
            "Efter en stund hörde hon ett försiktigt ljud.\n\n"
                + "– Blubb, blubb.\n\n"
                + "Bakom en sten satt en liten blå fisk.\n\n"
                + "– Varför är du vaken? frågade Lumi.\n\n"
                + "– Jag hittar inte hem, sa fisken. Allt ser annorlunda ut i mörkret.",
            "– Du kan simma med mig, sa Lumi. Månens stig kanske visar vägen.\n\n"
                + "Fisken simmade bredvid henne.\n\n"
                + "Tillsammans följde de ljuset över det lugna havet.\n\n"
                + "Upp och ner.\n\nUpp och ner.",
            "Snart mötte de en gammal havssköldpadda som vilade på en bädd av mjukt sjögräs.\n\n"
                + "– God kväll, små vänner, sa sköldpaddan långsamt. Vart är ni på väg?\n\n"
                + "– Vi följer månens stig, sa Lumi. Och den lilla fisken försöker hitta hem.",
            "Sköldpaddan tittade upp mot månen.\n\n"
                + "– Då ska ni följa det mjukaste ljuset, sa hon. "
                + "Månen lyser alltid lite extra över platser där någon väntar.\n\n"
                + "Lumi och fisken tackade sköldpaddan och simmade vidare.",
            "Nu blev vågorna ännu lugnare.\n\n"
                + "Havet lät:\n\nSchhh.\n\nSchhh.\n\nSchhh.\n\n"
                + "Som om det viskade att allting var tryggt.",
            "Plötsligt pekade den lilla fisken med sin fena.\n\n"
                + "– Titta!\n\n"
                + "Framför dem lyste en liten korallgrotta i månens sken. "
                + "I öppningen simmade flera fiskar fram och tillbaka.\n\n"
                + "– Där är du ju! ropade fiskarna glatt.\n\n"
                + "Den lilla blå fisken simmade snabbt fram till sin familj.\n\n"
                + "– Tack, Lumi! sa den. Nu är jag hemma.",
            "Lumi vinkade med sin fena och fortsatte en liten bit längs månens stig.\n\n"
                + "Längre ut på havet flöt en liten båt. Seglet var nedfällt och en lykta lyste varmt i fören. "
                + "Båten gungade långsamt på vågorna.\n\n"
                + "Upp och ner.\n\nUpp och ner.\n\n"
                + "På båtens kant satt en liten måsfågel och gäspade.",
            "– Vet du vart månens stig leder? frågade Lumi.\n\n"
                + "Måsen gäspade ännu en gång.\n\n"
                + "– Kanske leder den inte bort någonstans, sa hon. Kanske leder den hem.",
            "Lumi stannade och tittade tillbaka.\n\n"
                + "Långt borta kunde hon se sin klippa. Där satt mamma Säl och väntade. "
                + "Månen lyste över henne och gjorde hennes päls alldeles silvervit.\n\n"
                + "Då förstod Lumi.\n\n"
                + "Månens stig hade hjälpt den lilla fisken att hitta hem.\n\n"
                + "Och nu visade den även vägen hem för Lumi.",
            "Hon började simma tillbaka.\n\n"
                + "Havet bar henne försiktigt framåt.\n\n"
                + "Schhh.\n\nSchhh.\n\nSchhh.",
            "När Lumi kom fram till klippan hjälpte mamma henne upp ur vattnet.\n\n"
                + "– Fick du veta vart månens stig leder? frågade mamma.\n\n"
                + "Lumi kröp nära intill hennes varma sida.\n\n"
                + "– Ja, sa hon sömnigt. Den leder till någon som väntar på en.\n\n"
                + "Mamma Säl lade sin fena omkring henne.",
            "Ovanför dem lyste månen.\n\n"
                + "Under dem gungade havet sakta.\n\n"
                + "Upp och ner.\n\nUpp och ner.\n\n"
                + "Lumis ögon blev tyngre och tyngre.\n\n"
                + "Hon tänkte på den lilla fisken, den gamla sköldpaddan och båten som vilade på vågorna.\n\n"
                + "Sedan somnade hon tryggt bredvid sin mamma medan havet fortsatte att viska:\n\n"
                + "Schhh.\nSchhh.\nSov så gott."
        };

        private static readonly StoryAnimationType[] Animations =
        {
            StoryAnimationType.MoonlightGlow,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.MoonlightGlow,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.SoftBob,
            StoryAnimationType.SoftBob,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SoftBob,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.MoonlightGlow,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SoftBob,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SoftBob,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.MoonlightGlow
        };

        [MenuItem("Aris/Stories/Build Lilla Lumi")]
        public static void BuildLillaLumi()
        {
            ValidateSourceFiles();
            ConfigureImageImporter(StoryRoot + "/cover.png");
            for (int page = 1; page <= 16; page++)
            {
                ConfigureImageImporter(
                    StoryRoot + $"/page_{page:00}.png"
                );
            }
            ConfigureAudioImporter();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Sprite cover = AssetDatabase.LoadAssetAtPath<Sprite>(
                StoryRoot + "/cover.png"
            );
            AudioClip narration = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioPath
            );
            List<StoryPage> pages = new();
            for (int index = 0; index < Texts.Length; index++)
            {
                string illustrationPath =
                    index == 0
                        ? StoryRoot + "/cover.png"
                        : StoryRoot + $"/page_{index:00}.png";
                Sprite illustration = AssetDatabase.LoadAssetAtPath<Sprite>(
                    illustrationPath
                );
                StoryPage page = new();
                Vector2 parallax =
                    index % 2 == 0
                        ? new Vector2(1f, 0.2f)
                        : new Vector2(-0.7f, 0.25f);
                page.Configure(
                    illustration,
                    Texts[index],
                    Timeline[index],
                    Timeline[index + 1],
                    Animations[index],
                    parallax,
                    index % 3 == 0 ? 1.035f : 1.022f,
                    index == 0
                );
                pages.Add(page);
            }

            Directory.CreateDirectory("Assets/Resources/Stories");
            StoryDefinition definition =
                AssetDatabase.LoadAssetAtPath<StoryDefinition>(AssetPath);
            if (definition == null)
            {
                definition =
                    ScriptableObject.CreateInstance<StoryDefinition>();
                AssetDatabase.CreateAsset(definition, AssetPath);
            }
            definition.Configure(
                "lilla-lumi",
                "Lilla Lumi",
                cover,
                narration,
                pages
            );
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            StoryCatalog.ClearCache();
            Debug.Log(
                $"Lilla Lumi byggd: {pages.Count} tidslinjeposter, "
                    + $"{narration.length:F3} sekunder."
            );
        }

        public static void ValidateLillaLumi()
        {
            StoryDefinition definition =
                AssetDatabase.LoadAssetAtPath<StoryDefinition>(AssetPath);
            if (definition == null || !definition.IsValid)
            {
                throw new InvalidOperationException(
                    "Lilla Lumi StoryDefinition saknas eller är ogiltig."
                );
            }
            if (definition.Pages.Count != 17)
            {
                throw new InvalidOperationException(
                    "Lilla Lumi ska ha omslag och 16 berättelsesidor."
                );
            }
            if (Mathf.Abs(definition.Narration.length - 287.138f) > 0.25f)
            {
                throw new InvalidOperationException(
                    "Berättarljudets längd avviker från ljudanalysen."
                );
            }
            for (int index = 0; index < definition.Pages.Count; index++)
            {
                StoryPage page = definition.Pages[index];
                if (
                    page.Illustration == null
                    || string.IsNullOrWhiteSpace(page.Text)
                    || page.EndTime <= page.StartTime
                )
                {
                    throw new InvalidOperationException(
                        "Ogiltig sagosida vid index " + index + "."
                    );
                }
                if (
                    index > 0
                    && Mathf.Abs(
                        page.StartTime
                            - definition.Pages[index - 1].EndTime
                    ) > 0.01f
                )
                {
                    throw new InvalidOperationException(
                        "Tidslinjen har ett glapp vid sida " + index + "."
                    );
                }
            }
            Debug.Log(
                "Lilla Lumi validerad: alla illustrationer, texter och "
                    + "tidsintervall är kompletta."
            );
        }

        private static void ValidateSourceFiles()
        {
            List<string> missing = new();
            AddIfMissing(StoryRoot + "/cover.png", missing);
            for (int page = 1; page <= 16; page++)
            {
                AddIfMissing(StoryRoot + $"/page_{page:00}.png", missing);
            }
            AddIfMissing(AudioPath, missing);
            if (missing.Count > 0)
            {
                throw new FileNotFoundException(
                    "Sagobygget saknar filer:\n" + string.Join("\n", missing)
                );
            }
        }

        private static void AddIfMissing(
            string path,
            ICollection<string> missing
        )
        {
            if (!File.Exists(path))
            {
                missing.Add(path);
            }
        }

        internal static void ConfigureImageImporter(string path)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport
            );
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    "Kunde inte konfigurera bildimport: " + path
                );
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static void ConfigureAudioImporter()
        {
            ConfigureAudioImporter(AudioPath);
        }

        internal static void ConfigureAudioImporter(string path)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport
            );
            AudioImporter importer =
                AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    "Kunde inte konfigurera berättarljudet."
                );
            }
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.sampleRateSetting =
                AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = false;
            importer.loadInBackground = false;
            importer.ambisonic = false;
            importer.SaveAndReimport();
        }
    }
}
