using System;
using System.Collections.Generic;
using System.IO;
using ArisMonsterTrucks.Stories;
using UnityEditor;
using UnityEngine;

namespace ArisMonsterTrucks.EditorTools
{
    public static class AriSisterStoryAssetBuilder
    {
        private const string StoryRoot =
            "Assets/Resources/Art/Stories/AriOchLillasystern";
        private const string AudioPath =
            "Assets/Resources/Audio/Stories/ari_och_lillasystern.mp3";
        private const string AssetPath =
            "Assets/Resources/Stories/ari-och-lillasystern.asset";

        private static readonly float[] Timeline =
        {
            0f,
            3.34f,
            25.12f,
            48.18f,
            60.50f,
            78.22f,
            89.56f,
            103.90f,
            120.22f,
            133.14f,
            147.06f,
            160.66f,
            177.38f,
            190.30f,
            202.74f,
            215.32f,
            225.10f,
            232.98f,
            255.98f,
            267.32f,
            294.79184f
        };

        private static readonly string[] Texts =
        {
            "Ari och lillasystern som tog väldigt lång tid på sig",
            "En dag var Ari hemma i radhuset med mamma och pappa. "
                + "Han hade parkerat sina leksaksbilar i en lång kö över "
                + "vardagsrumsgolvet när mamma log på ett hemligt sätt.\n\n"
                + "– Ari, vi har något viktigt att berätta, sa hon.\n\n"
                + "Ari stannade mitt i en stor bilkrasch.\n\n"
                + "– Har vi köpt en grävmaskin?\n\n"
                + "Pappa skrattade.\n\n"
                + "– Nej, du ska få en lillasyster.",
            "Ari tittade på mamma, sedan på pappa och till sist på mammas "
                + "mage.\n\n"
                + "– En riktig lillasyster?\n\n"
                + "– En alldeles riktig. När hon har vuxit färdigt ska hon "
                + "flytta in här i radhuset med oss.\n\n"
                + "Ari blev så glad att han sprang två varv runt "
                + "vardagsrummet. På det tredje snubblade han på en kudde "
                + "och landade med rumpan först.\n\n"
                + "– När kommer hon?\n\n"
                + "– När hon är färdig i magen, svarade mamma.",
            "Från den dagen väntade Ari. Varje morgon frågade han:\n\n"
                + "– Kommer lillasyster i dag?\n\n"
                + "När det ringde på dörren ropade han:\n\n"
                + "– Nu är det hon!\n\n"
                + "Men det var bara grannen som ville låna en skruvmejsel.",
            "Ari gjorde plats åt lillasyster bredvid sin kudde. Han lade dit "
                + "en filt och en röd leksaksbil.\n\n"
                + "– Hon får låna den röda. Men inte den blå. Den är väldigt "
                + "snabb.\n\n"
                + "Dagar blev till veckor och veckor till månader. Ingen "
                + "lillasyster kom. I stället blev mammas mage större och "
                + "rundare.",
            "Ari brukade lägga örat mot magen. En dag kom en liten buff från "
                + "insidan.\n\n"
                + "– Hon sparkade mig!\n\n"
                + "– Hon hälsade nog, sa mamma.\n\n"
                + "– Hon kunde ha vinkat i stället, tyckte Ari.",
            "Ju större magen blev, desto tröttare blev mamma. Ibland somnade "
                + "hon nästan i soffan. Då lade Ari en filt över henne och "
                + "placerade sin mjuka kanin bredvid.\n\n"
                + "– Du får låna den tills du är färdigsovd, viskade han.",
            "En kväll kom mormor till radhuset med en stor väska, en kudde "
                + "och väldigt många saker som hon kanske kunde behöva.\n\n"
                + "– Ska du flytta in också? frågade Ari.\n\n"
                + "– Nej, jag sover bara här i natt. I morgon åker du och jag "
                + "till mitt röda hus.",
            "Nästa morgon kramade Ari mamma, sedan pappa och sedan mamma en "
                + "gång till, bara för säkerhets skull. På vägen tittade han "
                + "ut genom bilfönstret. Han saknade redan mamma, pappa och "
                + "radhuset.",
            "Den första kvällen hos mormor kändes det röda huset stort och "
                + "tyst.\n\n"
                + "– Jag saknar mamma och pappa, sa Ari.\n\n"
                + "Mormor höll om honom.\n\n"
                + "– De saknar dig också. Snart ses ni igen. Och kanske "
                + "följer någon väldigt liten med hem.",
            "Ari undrade om lillasyster hade hår, om hon kunde prata och om "
                + "hon visste hur länge han hade väntat. Till slut somnade "
                + "han.\n\n"
                + "Mormor smög ut och hann dricka en hel kopp te medan den "
                + "fortfarande var varm. Det hade nästan aldrig hänt förut.",
            "Nästa dag kom en flicka som hette Sara.\n\n"
                + "– Vill du hoppa studsmatta?\n\n"
                + "Det ville Ari. De hoppade som grodor, som kängurur och "
                + "till sist som två väldigt vingliga popcorn. Sedan byggde "
                + "de en koja, jagade såpbubblor och åt mellanmål i "
                + "trädgården.",
            "Ari hade så roligt att mamma och pappa nästan blev bortglömda."
                + "\n\nNästan.\n\n"
                + "För när kvällen kom blev allting tyst igen.\n\n"
                + "– Jag saknar dem nu, sa Ari och drog täcket till näsan.",
            "Mormor berättade samma saga två gånger, hämtade vatten tre "
                + "gånger och letade efter kaninen som hela tiden låg under "
                + "Aris arm. Till slut somnade han, och mormor fick lite "
                + "lugn och ro igen.",
            "Dagarna gick. Ari och mormor ritade, åt frukost i pyjamas och "
                + "vinkade åt traktorer som körde förbi. Ibland lekte Ari "
                + "med Sara. Ibland frågade han:\n\n"
                + "– Är lillasyster färdig snart?",
            "Hemma vid radhuset väntade grannbarnen.\n\n"
                + "– När kommer Ari hem? frågade de varje dag.\n\n"
                + "De tittade mot vägen och lyssnade efter bilen.",
            "En morgon sa mormor:\n\n"
                + "– Ari, i dag åker vi hem!\n\n"
                + "Ari sprang mot bilen så fort att mormor nästan glömde "
                + "hans ena sko.",
            "När bilen svängde in vid radhuset stod grannbarnen utanför och "
                + "vinkade. Ari hoppade ur bilen, men då hörde han ett litet "
                + "pip inifrån huset.\n\n"
                + "Han öppnade dörren försiktigt.\n\n"
                + "Mamma satt i soffan. Pappa satt bredvid. I mammas famn låg "
                + "ett litet paket insvept i en mjuk filt.\n\n"
                + "Paketet rörde sig.\n\n"
                + "Ari gick närmare.\n\n"
                + "Där var hon. Hans lillasyster.",
            "Hon var mycket mindre än han hade föreställt sig. Hon hade små "
                + "händer, en liten näsa och pyttesmå fingrar.\n\n"
                + "– Är det där hela lillasystern?\n\n"
                + "– Ja, sa mamma. Det är hela lillasystern.",
            "Ari satte sig bredvid mamma och sträckte försiktigt fram ett "
                + "finger. Lillasyster grep tag i det och höll fast.\n\n"
                + "– Där är du ju, viskade Ari. Du tog väldigt lång tid på "
                + "dig.\n\n"
                + "Sedan hämtade han den röda leksaksbilen och lade den "
                + "bredvid hennes filt. Den blå behöll han själv. Den var ju "
                + "väldigt snabb.\n\n"
                + "Nu var mamma, pappa, Ari och lillasyster äntligen hemma "
                + "tillsammans i radhuset.\n\n"
                + "Och Ari hade fått ett nytt och mycket viktigt jobb.\n\n"
                + "Han var storebror."
        };

        private static readonly StoryAnimationType[] Animations =
        {
            StoryAnimationType.SlowZoom,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.SoftBob,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.MoonlightGlow,
            StoryAnimationType.SoftBob,
            StoryAnimationType.SoftBob,
            StoryAnimationType.MoonlightGlow,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.GentleDrift,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.SlowZoom,
            StoryAnimationType.SlowZoom
        };

        [MenuItem("Aris/Stories/Build Ari och lillasystern")]
        public static void BuildAriAndSister()
        {
            ValidateSourceFiles();
            StoryAssetBuilder.ConfigureImageImporter(
                StoryRoot + "/cover.png"
            );
            for (int page = 1; page <= 19; page++)
            {
                StoryAssetBuilder.ConfigureImageImporter(
                    StoryRoot + $"/page_{page:00}.png"
                );
            }
            StoryAssetBuilder.ConfigureAudioImporter(AudioPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Sprite cover = AssetDatabase.LoadAssetAtPath<Sprite>(
                StoryRoot + "/cover.png"
            );
            AudioClip narration = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioPath
            );
            if (narration == null)
            {
                throw new InvalidOperationException(
                    "Kunde inte importera berättarljudet för Ari."
                );
            }

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
                        ? new Vector2(0.75f, 0.2f)
                        : new Vector2(-0.65f, 0.22f);
                float endTime =
                    index == Texts.Length - 1
                        ? narration.length
                        : Timeline[index + 1];
                page.Configure(
                    illustration,
                    Texts[index],
                    Timeline[index],
                    endTime,
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
                "ari-och-lillasystern",
                "Ari och lillasystern",
                cover,
                narration,
                pages,
                1,
                "Ari är storebror."
            );
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            StoryCatalog.ClearCache();
            Debug.Log(
                $"Ari och lillasystern byggd: {pages.Count} "
                    + $"tidslinjeposter, {narration.length:F3} sekunder."
            );
        }

        [MenuItem("Aris/Stories/Validate Ari och lillasystern")]
        public static void ValidateAriAndSister()
        {
            StoryDefinition definition =
                AssetDatabase.LoadAssetAtPath<StoryDefinition>(AssetPath);
            if (definition == null || !definition.IsValid)
            {
                throw new InvalidOperationException(
                    "Ari StoryDefinition saknas eller är ogiltig."
                );
            }
            if (definition.Pages.Count != 20)
            {
                throw new InvalidOperationException(
                    "Ari-sagan ska ha omslag och 19 berättelsesidor."
                );
            }
            if (Mathf.Abs(definition.Narration.length - 294.79184f) > 0.25f)
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
                        "Ogiltig Ari-sida vid index " + index + "."
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
                        "Ari-tidslinjen har ett glapp vid sida "
                            + index
                            + "."
                    );
                }
            }
            if (Mathf.Abs(definition.Pages[0].StartTime) > 0.001f)
            {
                throw new InvalidOperationException(
                    "Ari-tidslinjen måste börja vid 0."
                );
            }
            if (
                Mathf.Abs(
                    definition.Pages[definition.Pages.Count - 1].EndTime
                        - definition.Narration.length
                ) > 0.01f
            )
            {
                throw new InvalidOperationException(
                    "Ari-tidslinjens slut matchar inte AudioClip."
                );
            }
            Debug.Log(
                "Ari och lillasystern validerad: alla illustrationer, "
                    + "texter och tidsintervall är kompletta."
            );
        }

        private static void ValidateSourceFiles()
        {
            List<string> missing = new();
            AddIfMissing(StoryRoot + "/cover.png", missing);
            for (int page = 1; page <= 19; page++)
            {
                AddIfMissing(StoryRoot + $"/page_{page:00}.png", missing);
            }
            AddIfMissing(AudioPath, missing);
            if (missing.Count > 0)
            {
                throw new FileNotFoundException(
                    "Ari-sagobygget saknar filer:\n"
                        + string.Join("\n", missing)
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
    }
}
