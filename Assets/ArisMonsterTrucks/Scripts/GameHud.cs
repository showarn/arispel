using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class GameHud : MonoBehaviour
    {
        private RaceDirector director;
        private Text coinText;
        private Text countdownText;
        private RectTransform coinPanel;
        private RectTransform playerMarker;
        private RectTransform npcMarker;
        private RectTransform progressFill;
        private Image gasImage;
        private Vector2 gasRestPosition;
        private GameObject hintPanel;
        private GameObject finishPanel;
        private Text finishText;
        private Text resultCoinText;
        private Text resultStarsText;
        private Text totalStarsText;
        private Image finishTrackPreview;
        private Button finishActionButton;
        private Text finishActionButtonText;
        private Text unlockText;
        private RectTransform playerResultTruck;
        private RectTransform npcResultTruck;
        private GameObject playerWinnerLabel;
        private GameObject npcWinnerLabel;
        private Font font;
        private RectTransform safeRoot;

        public static GameHud Create(RaceDirector raceDirector)
        {
            GameObject root = new("Spelgränssnitt");
            GameHud hud = root.AddComponent<GameHud>();
            hud.director = raceDirector;
            hud.Build();
            return hud;
        }

        public void SetCoins(int value)
        {
            coinText.text = value.ToString();
        }

        public void SetGasPressed(bool pressed)
        {
            if (gasImage == null)
            {
                return;
            }
            gasImage.color = pressed ? RuntimeArt.Hex("#FFF0A8") : Color.white;
            gasImage.rectTransform.localScale = pressed
                ? new Vector3(0.96f, 0.88f, 1f)
                : Vector3.one;
            gasImage.rectTransform.anchoredPosition = gasRestPosition
                + (pressed ? new Vector2(0f, -22f) : Vector2.zero);
            if (hintPanel != null)
            {
                hintPanel.SetActive(!pressed);
            }
        }

        public void SetProgress(float playerX, float npcX)
        {
            float playerT = Mathf.InverseLerp(-6f, ColorTrackBuilder.FinishX, playerX);
            float npcT = Mathf.InverseLerp(-9f, ColorTrackBuilder.FinishX, npcX);
            float playerAnchor = Mathf.Lerp(0.08f, 0.86f, playerT);
            float npcAnchor = Mathf.Lerp(0.08f, 0.86f, npcT);
            playerMarker.anchorMin = playerMarker.anchorMax =
                new Vector2(playerAnchor, 0.3f);
            npcMarker.anchorMin = npcMarker.anchorMax =
                new Vector2(npcAnchor, 0.7f);
            progressFill.anchorMax = new Vector2(playerAnchor, 0.5f);
        }

        public void ShowCountdown(string value)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = value;
            countdownText.rectTransform.localScale = Vector3.one * 1.2f;
        }

        public void HideCountdown()
        {
            countdownText.gameObject.SetActive(false);
        }

        public void PulseCoinCounter()
        {
            StopCoroutine(nameof(PulseCoinRoutine));
            StartCoroutine(nameof(PulseCoinRoutine));
        }

        public void ShowFinish(int coins, bool npcWasFirst, int rating, bool unlockedNext)
        {
            finishPanel.SetActive(true);
            finishText.text = npcWasFirst
                ? "NÄSTAN!"
                : "VINST!";
            resultCoinText.text = "+" + coins + " MYNT";
            resultStarsText.text = "+" + rating + (
                rating == 1 ? " STJÄRNA" : " STJÄRNOR"
            );
            totalStarsText.text = "★ " + LevelProgression.TotalStars;
            finishActionButton.onClick.RemoveAllListeners();
            if (npcWasFirst)
            {
                finishActionButtonText.text = "FÖRSÖK IGEN";
                finishActionButton.onClick.AddListener(director.RestartRace);
            }
            else if (director.HasNextLevel)
            {
                finishActionButtonText.text = "NÄSTA BANA";
                finishActionButton.onClick.AddListener(director.StartNextLevel);
            }
            else
            {
                finishActionButtonText.text = "FORTSÄTT";
                finishActionButton.onClick.AddListener(director.ExitToLevelSelect);
            }
            if (!npcWasFirst)
            {
                StartCoroutine(FireworkRoutine(coins));
            }
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject safeObject = new("Säker spel-HUD", typeof(RectTransform));
            safeObject.transform.SetParent(transform, false);
            safeRoot = safeObject.GetComponent<RectTransform>();
            Stretch(safeRoot);
            safeObject.AddComponent<SafeAreaFitter>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventObject = new("Pekskärmssystem");
                eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
            }

            BuildPlayerCard();
            BuildProgressBar();
            BuildMenuButton();
            BuildGasButton();
            BuildCountdown();
            BuildFinishPanel();
            BuildHint();
        }

        private void BuildMenuButton()
        {
            Button menu = CreateButton(safeRoot, "MENY", Vector2.zero);
            menu.image.sprite = RuntimeArt.RoundedRectangleSprite(
                "RaceMenuButton",
                RuntimeArt.Hex("#40245F"),
                RuntimeArt.Hex("#7A5AA6"),
                220,
                92,
                32,
                7
            );
            SetRect(
                menu.image.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-130f, -64f),
                new Vector2(220f, 92f)
            );
            Text label = menu.GetComponentInChildren<Text>();
            label.text = "MENY  ≡";
            label.fontSize = 34;
            menu.onClick.AddListener(director.ExitToMenu);
        }

        private void BuildCoinCounter()
        {
            Image panel = CreateImage(
                "Myntpanel",
                safeRoot,
                RuntimeArt.RoundedRectangleSprite(
                    "CoinPanel",
                    RuntimeArt.Hex("#6E3B15"),
                    RuntimeArt.Hex("#FFF3B0"),
                    260,
                    92,
                    30,
                    7
                )
            );
            coinPanel = panel.rectTransform;
            SetRect(
                coinPanel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(145f, -64f),
                new Vector2(260f, 92f)
            );

            Image coin = CreateImage("Myntsymbol", panel.transform, RuntimeArt.GoldCoinSprite());
            SetRect(
                coin.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(49f, 0f),
                new Vector2(64f, 64f)
            );

            Text star = CreateText("Stjärna", coin.transform, "★", 39, RuntimeArt.Hex("#FFF7BA"));
            Stretch(star.rectTransform);

            coinText = CreateText("Myntantal", panel.transform, "0", 48, RuntimeArt.Hex("#4A266C"));
            coinText.alignment = TextAnchor.MiddleCenter;
            SetRect(
                coinText.rectTransform,
                new Vector2(0.68f, 0.5f),
                new Vector2(0.68f, 0.5f),
                Vector2.zero,
                new Vector2(140f, 74f)
            );
        }

        private void BuildPlayerCard()
        {
            Image panel = CreateImage(
                "Spelarprofil",
                safeRoot,
                RuntimeArt.RoundedRectangleSprite(
                    "RacePlayerPanel",
                    RuntimeArt.Hex("#B77A16"),
                    RuntimeArt.Hex("#FFF0A2"),
                    420,
                    92,
                    30,
                    7
                )
            );
            SetRect(
                panel.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(230f, -64f),
                new Vector2(420f, 92f)
            );
            coinPanel = panel.rectTransform;

            Image avatarFrame = CreateImage(
                "Profilbildsram",
                panel.transform,
                RuntimeArt.CircleSprite(
                    "RaceProfileFrame",
                    RuntimeArt.Hex("#B77A16"),
                    Color.white,
                    Color.white,
                    96
                )
            );
            SetRect(
                avatarFrame.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(51f, 0f),
                new Vector2(72f, 72f)
            );
            Image avatar = CreateImage(
                "Profilbild",
                avatarFrame.transform,
                RuntimeArt.LoadSprite("Art/Fishing/Character/head_idle")
            );
            avatar.preserveAspect = true;
            SetRect(
                avatar.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -2f),
                new Vector2(62f, 62f)
            );

            string username = string.IsNullOrEmpty(PlayerProfile.Username)
                ? "DU"
                : PlayerProfile.Username;
            Text name = CreateText(
                "Spelarnamn",
                panel.transform,
                username,
                30,
                RuntimeArt.Hex("#4A266C")
            );
            name.alignment = TextAnchor.MiddleLeft;
            SetRect(
                name.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(230f, 20f),
                new Vector2(270f, 40f)
            );

            Image profileCoin = CreateImage(
                "Profilmynt",
                panel.transform,
                RuntimeArt.GoldCoinSprite()
            );
            profileCoin.preserveAspect = true;
            SetRect(
                profileCoin.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(125f, -23f),
                new Vector2(34f, 34f)
            );
            coinText = CreateText(
                "Insamlade mynt",
                panel.transform,
                "0",
                22,
                RuntimeArt.Hex("#4A266C")
            );
            coinText.alignment = TextAnchor.MiddleLeft;
            SetRect(
                coinText.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(190f, -23f),
                new Vector2(100f, 34f)
            );

            totalStarsText = CreateText(
                "Samlade stjärnor",
                panel.transform,
                "★ " + LevelProgression.TotalStars,
                23,
                RuntimeArt.Hex("#9A5200")
            );
            totalStarsText.alignment = TextAnchor.MiddleLeft;
            SetRect(
                totalStarsText.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(330f, -22f),
                new Vector2(150f, 36f)
            );
        }

        private void BuildProgressBar()
        {
            Image panel = CreateImage(
                "Tävlingsmätare",
                safeRoot,
                RuntimeArt.RoundedRectangleSprite(
                    "ProgressPanel",
                    RuntimeArt.Hex("#432A67"),
                    new Color(1f, 1f, 1f, 0.9f),
                    620,
                    92,
                    30,
                    7
                )
            );
            SetRect(
                panel.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(90f, -64f),
                new Vector2(620f, 92f)
            );

            Image line = CreateImage("Bana", panel.transform, null);
            line.color = RuntimeArt.Hex("#E8E2CF");
            SetRect(
                line.rectTransform,
                new Vector2(0.08f, 0.5f),
                new Vector2(0.86f, 0.5f),
                Vector2.zero,
                new Vector2(0f, 18f)
            );
            Image fill = CreateImage("Din progress", panel.transform, null);
            fill.color = RuntimeArt.Hex("#76D94D");
            progressFill = fill.rectTransform;
            SetRect(
                progressFill,
                new Vector2(0.08f, 0.5f),
                new Vector2(0.08f, 0.5f),
                Vector2.zero,
                new Vector2(0f, 18f)
            );

            playerMarker = CreateMarker(panel.transform, "DU", RuntimeArt.Hex("#FFD83D"), 0.08f, 0.3f);
            npcMarker = CreateMarker(panel.transform, "K", RuntimeArt.Hex("#F58BFF"), 0.08f, 0.7f);

            Text finishFlag = CreateText("Målflagga", panel.transform, "MÅL", 28, RuntimeArt.Hex("#4A266C"));
            SetRect(finishFlag.rectTransform, new Vector2(0.93f, 0.5f), new Vector2(0.93f, 0.5f), Vector2.zero, new Vector2(80f, 62f));
        }

        private RectTransform CreateMarker(Transform parent, string label, Color color, float start, float height)
        {
            Image marker = CreateImage(
                "Markör " + label,
                parent,
                RuntimeArt.CircleSprite("Marker_" + label, RuntimeArt.Hex("#3E245F"), color, Color.white)
            );
            RectTransform rect = marker.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(start, height);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(70f, 70f);

            Text text = CreateText("Text", marker.transform, label, label == "DU" ? 25 : 30, RuntimeArt.Hex("#432A67"));
            Stretch(text.rectTransform);
            return rect;
        }

        private void BuildGasButton()
        {
            GameObject buttonObject = new("Stor gaspedal");
            buttonObject.transform.SetParent(safeRoot, false);
            gasImage = buttonObject.AddComponent<Image>();
            gasImage.sprite = RuntimeArt.CircleSprite(
                "GasPedalRing",
                RuntimeArt.Hex("#B63C25"),
                RuntimeArt.Hex("#FF7A3D"),
                RuntimeArt.Hex("#FFF09A"),
                256
            );
            gasImage.preserveAspect = true;
            SetRect(
                gasImage.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-190f, 205f),
                new Vector2(310f, 310f)
            );
            gasRestPosition = gasImage.rectTransform.anchoredPosition;

            Image pedal = CreateImage(
                "Gaspedal",
                buttonObject.transform,
                RuntimeArt.LoadSprite("Art/UI/gas_pedal")
            );
            pedal.type = Image.Type.Simple;
            pedal.preserveAspect = true;
            SetRect(
                pedal.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 32f),
                new Vector2(125f, 188f)
            );

            Text gasLabel = CreateText(
                "GAS-text",
                buttonObject.transform,
                "GAS",
                44,
                Color.white
            );
            SetRect(
                gasLabel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -104f),
                new Vector2(180f, 58f)
            );

            HoldGasButton hold = buttonObject.AddComponent<HoldGasButton>();
            hold.Initialize(director);
        }

        private void BuildCountdown()
        {
            countdownText = CreateText("Nedräkning", safeRoot, "3", 210, Color.white);
            countdownText.alignment = TextAnchor.MiddleCenter;
            countdownText.fontStyle = FontStyle.Bold;
            countdownText.horizontalOverflow = HorizontalWrapMode.Overflow;
            countdownText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(
                countdownText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 90f),
                new Vector2(720f, 280f)
            );
            Outline outline = countdownText.gameObject.AddComponent<Outline>();
            outline.effectColor = RuntimeArt.Hex("#4A266C");
            outline.effectDistance = new Vector2(8f, -8f);
        }

        private void BuildHint()
        {
            Image hint = CreateImage(
                "Hjälptext",
                safeRoot,
                RuntimeArt.RoundedRectangleSprite(
                    "HintPanel",
                    new Color(0.2f, 0.1f, 0.35f, 0.8f),
                    new Color(0.2f, 0.1f, 0.35f, 0.72f),
                    590,
                    96,
                    30,
                    2
                )
            );
            hintPanel = hint.gameObject;
            SetRect(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(340f, 90f), new Vector2(590f, 96f));
            Text text = CreateText("Text", hint.transform, "HÅLL IN GASEN!", 42, Color.white);
            Stretch(text.rectTransform);
        }

        private void BuildFinishPanel()
        {
            Image shade = CreateImage("Målruta", safeRoot, null);
            shade.color = new Color(0.18f, 0.08f, 0.3f, 0.82f);
            Stretch(shade.rectTransform);
            finishPanel = shade.gameObject;

            Image card = CreateImage(
                "Resultatkort",
                shade.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "ModularFinishCard",
                    RuntimeArt.Hex("#4A266C"),
                    RuntimeArt.Hex("#FFF7D6"),
                    1220,
                    800,
                    58,
                    12
                )
            );
            card.type = Image.Type.Sliced;
            card.preserveAspect = false;
            SetRect(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1220f, 800f));

            Image titleBanner = CreateImage(
                "Titelband",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "FinishTitleBanner",
                    RuntimeArt.Hex("#4A266C"),
                    RuntimeArt.Hex("#FFF0A6"),
                    760,
                    116,
                    40,
                    9
                )
            );
            SetRect(titleBanner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), new Vector2(760f, 116f));

            finishText = CreateText("Resultat", card.transform, "DU VANN!", 78, RuntimeArt.Hex("#4A266C"));
            finishText.resizeTextForBestFit = true;
            finishText.resizeTextMinSize = 42;
            finishText.resizeTextMaxSize = 78;
            SetRect(finishText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 270f), new Vector2(900f, 115f));

            Image previewFrame = CreateImage(
                "Banförhandsvisningsram",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "FinishPreviewFrame",
                    RuntimeArt.Hex("#4A266C"),
                    Color.white,
                    620,
                    430,
                    34,
                    9
                )
            );
            SetRect(
                previewFrame.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-245f, -25f),
                new Vector2(620f, 430f)
            );
            finishTrackPreview = CreateImage(
                "Färdig bana",
                previewFrame.transform,
                RuntimeArt.LoadSprite(TrackCardPath(director.LevelNumber))
            );
            finishTrackPreview.type = Image.Type.Simple;
            finishTrackPreview.preserveAspect = false;
            SetRect(
                finishTrackPreview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(588f, 392f)
            );

            playerResultTruck = CreateResultTruck(
                card.transform,
                true,
                new Vector2(-245f, 35f),
                out playerWinnerLabel
            );
            npcResultTruck = CreateResultTruck(
                card.transform,
                false,
                new Vector2(245f, 35f),
                out npcWinnerLabel
            );

            Image coinResultPanel = CreateImage(
                "Myntresultat",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "CoinResultPanel",
                    RuntimeArt.Hex("#6E3B15"),
                    RuntimeArt.Hex("#FFF3B0"),
                    320,
                    105,
                    34,
                    7
                )
            );
            SetRect(coinResultPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 30f), new Vector2(360f, 125f));

            Image resultCoinIcon = CreateImage(
                "Myntbild",
                coinResultPanel.transform,
                RuntimeArt.GoldCoinSprite()
            );
            resultCoinIcon.type = Image.Type.Simple;
            resultCoinIcon.preserveAspect = true;
            SetRect(resultCoinIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-88f, 0f), new Vector2(72f, 72f));

            resultCoinText = CreateText("Insamlade mynt", coinResultPanel.transform, "+0 MYNT", 46, RuntimeArt.Hex("#4A266C"));
            SetRect(resultCoinText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 0f), new Vector2(205f, 82f));

            Image starsPanel = CreateImage(
                "Stjärnresultat",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "StarResultPanel",
                    RuntimeArt.Hex("#4A266C"),
                    RuntimeArt.Hex("#FFF7D6"),
                    360,
                    105,
                    34,
                    7
                )
            );
            SetRect(
                starsPanel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, -105f),
                new Vector2(360f, 105f)
            );
            resultStarsText = CreateText(
                "Intjänade stjärnor",
                starsPanel.transform,
                "+1 STJÄRNA",
                35,
                RuntimeArt.Hex("#D17A00")
            );
            Stretch(resultStarsText.rectTransform);

            unlockText = CreateText("Upplåsning", card.transform, "NY BANA UPPLÅST!", 30, RuntimeArt.Hex("#5C2A83"));
            Image unlockPanel = CreateImage(
                "Upplåsningsfält",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "UnlockResultPanel",
                    RuntimeArt.Hex("#5C2A83"),
                    RuntimeArt.Hex("#F3D8FF"),
                    620,
                    58,
                    24,
                    6
                )
            );
            SetRect(unlockPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -268f), new Vector2(620f, 58f));
            unlockText.transform.SetParent(unlockPanel.transform, false);
            Stretch(unlockText.rectTransform);

            Button levels = CreateButton(card.transform, "BANVÄLJARE", new Vector2(-225f, -305f));
            SetRect(levels.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-225f, -305f), new Vector2(410f, 105f));
            levels.GetComponentInChildren<Text>().fontSize = 36;
            levels.onClick.AddListener(director.ExitToLevelSelect);

            finishActionButton = CreateButton(card.transform, "NÄSTA BANA", new Vector2(225f, -305f));
            SetRect(finishActionButton.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(225f, -305f), new Vector2(410f, 105f));
            finishActionButtonText = finishActionButton.GetComponentInChildren<Text>();
            finishActionButtonText.fontSize = 36;
            playerResultTruck.gameObject.SetActive(false);
            npcResultTruck.gameObject.SetActive(false);
            unlockPanel.gameObject.SetActive(false);
            finishPanel.SetActive(false);
        }

        private static string TrackCardPath(int levelNumber)
        {
            return levelNumber switch
            {
                2 => "Art/UI/Tracks/dirt_track_card",
                3 => "Art/UI/Tracks/mountain_track_card",
                4 => "Art/UI/Tracks/ice_track_card",
                5 => "Art/UI/Tracks/lava_track_card",
                6 => "Art/UI/Tracks/haunted_track_card",
                7 => "Art/UI/Tracks/jungle_track_card",
                8 => "Art/UI/Tracks/africa_track_card",
                9 => "Art/UI/Tracks/desert_track_card",
                10 => "Art/UI/Tracks/waterpark_track_card",
                11 => "Art/UI/Tracks/space_track_card",
                12 => "Art/UI/Tracks/candy_track_card",
                _ => "Art/UI/Tracks/rainbow_track_card"
            };
        }

        private RectTransform CreateResultTruck(
            Transform parent,
            bool isPlayer,
            Vector2 position,
            out GameObject winnerLabel
        )
        {
            GameObject group = new(isPlayer ? "Resultat DU" : "Resultat KOMPIS");
            group.transform.SetParent(parent, false);
            RectTransform groupRect = group.AddComponent<RectTransform>();
            SetRect(groupRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(390f, 245f));

            Image glow = group.AddComponent<Image>();
            glow.sprite = RuntimeArt.RoundedRectangleSprite(
                isPlayer ? "PlayerResultGlow" : "NpcResultGlow",
                RuntimeArt.Hex("#6A347F"),
                isPlayer ? RuntimeArt.Hex("#FFF0A0") : RuntimeArt.Hex("#F5D2FF"),
                390,
                245,
                45,
                9
            );

            Image rearSpring = CreateImage("Bakfjäder", group.transform, RuntimeArt.LoadSprite("Art/Truck/suspension_spring"));
            rearSpring.type = Image.Type.Simple;
            rearSpring.preserveAspect = true;
            SetRect(rearSpring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-72f, -34f), new Vector2(42f, 88f));

            Image frontSpring = CreateImage("Framfjäder", group.transform, RuntimeArt.LoadSprite("Art/Truck/suspension_spring"));
            frontSpring.type = Image.Type.Simple;
            frontSpring.preserveAspect = true;
            SetRect(frontSpring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(72f, -34f), new Vector2(42f, 88f));

            Image body = CreateImage(
                "Kaross",
                group.transform,
                RuntimeArt.LoadSprite(
                    isPlayer
                        ? TruckCustomization.GetSelected(GarageCategory.Body).ResourcePath
                        : "Art/Truck/body_plain"
                )
            );
            body.type = Image.Type.Simple;
            body.preserveAspect = true;
            if (isPlayer)
            {
                body.color = TruckCustomization.SelectedBodyColor();
            }
            else
            {
                body.color = new Color(0.95f, 0.68f, 1f);
            }
            SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(300f, 139f));

            Sprite wheelSprite = RuntimeArt.LoadSprite(
                isPlayer
                    ? TruckCustomization.GetSelected(GarageCategory.Wheels).ResourcePath
                    : "Art/Truck/wheel_glow"
            );
            Image rearWheel = CreateImage("Bakhjul", group.transform, wheelSprite);
            rearWheel.type = Image.Type.Simple;
            rearWheel.preserveAspect = true;
            SetRect(rearWheel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-72f, -87f), new Vector2(180f, 180f));

            Image frontWheel = CreateImage("Framhjul", group.transform, wheelSprite);
            frontWheel.type = Image.Type.Simple;
            frontWheel.preserveAspect = true;
            SetRect(frontWheel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(72f, -87f), new Vector2(180f, 180f));

            if (isPlayer)
            {
                AddSelectedResultParts(group.transform);
            }

            Text name = CreateText(
                "Namn",
                group.transform,
                isPlayer && !string.IsNullOrEmpty(PlayerProfile.Username)
                    ? PlayerProfile.Username.ToUpperInvariant()
                    : isPlayer ? "DU" : "KOMPIS",
                38,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(name.rectTransform, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), Vector2.zero, new Vector2(260f, 58f));

            Text winner = CreateText(
                "Vinnarmärke",
                group.transform,
                "★ VINNARE ★",
                34,
                RuntimeArt.Hex("#D96B00")
            );
            SetRect(winner.rectTransform, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(330f, 60f));
            winnerLabel = winner.gameObject;
            return groupRect;
        }

        private static void AddSelectedResultParts(Transform parent)
        {
            GarageItemDefinition decal = TruckCustomization.GetSelected(GarageCategory.Decals);
            if (!string.IsNullOrEmpty(decal.ResourcePath))
            {
                Image decalImage = CreateImage(
                    "Monterad dekal",
                    parent,
                    RuntimeArt.LoadSprite(decal.ResourcePath)
                );
                decalImage.type = Image.Type.Simple;
                decalImage.preserveAspect = true;
                SetRect(
                    decalImage.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-5f, 8f),
                    new Vector2(96f, 55f)
                );
            }

            foreach (
                GarageItemDefinition accessory in TruckCustomization.GetEquippedAccessories()
            )
            {
                AddResultAccessory(parent, accessory);
            }
        }

        private static void AddResultAccessory(
            Transform parent,
            GarageItemDefinition accessory
        )
        {
            GarageAccessoryMount mount = GarageAccessoryMounts.Get(accessory.Id);
            const float resultScale = 0.44f;
            TruckPartLayout defaults = TruckLayout.CreateDefault().accessory;
            TruckPartLayout layout = TruckLayout.Get(
                TruckLayoutPart.Accessory,
                accessory.Id
            );
            float sizeScale = layout.width / Mathf.Max(1f, defaults.width);
            Image accessoryImage = CreateImage(
                "Monterat tillbehör",
                parent,
                RuntimeArt.LoadSprite(accessory.ResourcePath)
            );
            accessoryImage.type = Image.Type.Simple;
            accessoryImage.preserveAspect = true;
            SetRect(
                accessoryImage.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                (
                    mount.PreviewPosition
                    + new Vector2(layout.x - defaults.x, layout.y - defaults.y)
                ) * resultScale + new Vector2(0f, 5f),
                mount.PreviewSize * sizeScale * resultScale
            );
            accessoryImage.rectTransform.localRotation = Quaternion.Euler(
                0f,
                0f,
                layout.rotation
            );
            if (mount.MirrorHorizontally)
            {
                accessoryImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
            if (mount.BehindBody)
            {
                accessoryImage.transform.SetSiblingIndex(3);
            }
        }

        private static string BuildRatingText(int rating)
        {
            rating = Mathf.Clamp(rating, 1, 3);
            return rating switch
            {
                3 => "● ● ●",
                2 => "● ● ○",
                _ => "● ○ ○"
            };
        }

        private IEnumerator FireworkRoutine(int coins)
        {
            yield return null;
            Color[] colors =
            {
                RuntimeArt.Hex("#FF4F87"),
                RuntimeArt.Hex("#FFD43B"),
                RuntimeArt.Hex("#45E07A"),
                RuntimeArt.Hex("#55C8FF"),
                RuntimeArt.Hex("#C070FF")
            };

            Vector2[] origins =
            {
                new(-430f, 160f),
                new(420f, 180f),
                new(0f, 260f),
                new(-260f, -40f),
                new(280f, -30f)
            };

            for (int burst = 0; burst < origins.Length; burst++)
            {
                for (int i = 0; i < 14; i++)
                {
                    Text star = CreateText(
                        "Fyverkeristjärna",
                        finishPanel.transform,
                        "★",
                        Random.Range(34, 64),
                        colors[(i + burst) % colors.Length]
                    );
                    SetRect(
                        star.rectTransform,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        origins[burst],
                        new Vector2(80f, 80f)
                    );

                    float angle = Mathf.PI * 2f * i / 14f + Random.Range(-0.12f, 0.12f);
                    float speed = Random.Range(170f, 380f);
                    UiFireworkStar motion = star.gameObject.AddComponent<UiFireworkStar>();
                    motion.Initialize(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed);
                }
                yield return new WaitForSeconds(0.24f);
            }

        }

        private Button CreateButton(Transform parent, string label, Vector2 position)
        {
            GameObject buttonObject = new(label);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "RestartButton",
                RuntimeArt.Hex("#B43A25"),
                RuntimeArt.Hex("#FF6B35"),
                480,
                150,
                46,
                10
            );
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(480f, 150f));

            Text text = CreateText("Text", buttonObject.transform, label, 56, Color.white);
            Stretch(text.rectTransform);
            return button;
        }

        private IEnumerator PulseCoinRoutine()
        {
            coinPanel.localScale = Vector3.one * 1.16f;
            yield return new WaitForSeconds(0.12f);
            coinPanel.localScale = Vector3.one;
        }

        private Text CreateText(string name, Transform parent, string value, int size, Color color)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            GameObject imageObject = new(name);
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == null ? Image.Type.Simple : Image.Type.Sliced;
            return image;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size
        )
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public sealed class UiFireworkStar : MonoBehaviour
    {
        private RectTransform rect;
        private Text text;
        private Vector2 velocity;
        private float lifetime;

        public void Initialize(Vector2 initialVelocity)
        {
            rect = GetComponent<RectTransform>();
            text = GetComponent<Text>();
            velocity = initialVelocity;
            lifetime = 1.65f;
            rect.localScale = Vector3.one * 0.2f;
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            lifetime -= delta;
            rect.anchoredPosition += velocity * delta;
            velocity *= Mathf.Pow(0.17f, delta);
            velocity += Vector2.down * 75f * delta;
            rect.Rotate(0f, 0f, 190f * delta);
            rect.localScale = Vector3.one * Mathf.Clamp01((1.65f - lifetime) * 7f);

            Color color = text.color;
            color.a = Mathf.Clamp01(lifetime / 0.55f);
            text.color = color;

            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public sealed class HoldGasButton :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {
        private RaceDirector director;

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            director.SetGasHeld(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            director.SetGasHeld(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            director.SetGasHeld(false);
        }

        private void OnDisable()
        {
            director?.SetGasHeld(false);
        }
    }

}
