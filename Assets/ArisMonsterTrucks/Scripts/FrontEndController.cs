using System.Collections.Generic;
using ArisMonsterTrucks.Fishing;
using ArisMonsterTrucks.Stories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class FrontEndController : MonoBehaviour
    {
        private GameObject host;
        private Camera gameCamera;
        private GameObject loginRoot;
        private GameObject dashboardRoot;
        private GameObject storyHubRoot;
        private ParentalGateController parentalGate;
        private GameObject menuRoot;
        private GameObject levelSelectRoot;
        private GameObject garageRoot;
        private Canvas canvas;
        private RectTransform frontEndSafeRoot;
        private GameObject inventoryRoot;
        private GameObject layoutEditorRoot;
        private Text layoutToggleLabel;
        private Text layoutEditorStatus;
        private InputField usernameInput;
        private Text loginStatus;
        private Text dashboardGreeting;
        private Button monsterTruckGameButton;
        private Button puzzleGameButton;
        private Button memoryGameButton;
        private Button fishingGameButton;
        private Button storyCategoryButton;
        private RectTransform dashboardCardContent;
        private LevelCarouselSnap dashboardCarouselSnap;
        private Button previousDashboardPageButton;
        private Button nextDashboardPageButton;
        private int dashboardPageCount = 1;
        private GameObject monsterTruckParentLock;
        private GameObject puzzleParentLock;
        private GameObject memoryParentLock;
        private GameObject fishingParentLock;
        private GameObject storyParentLock;
        private RectTransform itemRow;
        private RectTransform levelCardContent;
        private ScrollRect levelCardScroll;
        private LevelCarouselSnap levelCarouselSnap;
        private Button previousLevelPageButton;
        private Button nextLevelPageButton;
        private Text levelPageText;
        private PuzzleGameController puzzleController;
        private MemoryGameController memoryController;
        private FishingGameController fishingController;
        private StorybookController storybookController;
        private Text garageStatus;
        private Image previewBody;
        private Image previewChassis;
        private Image previewRearSuspension;
        private Image previewFrontSuspension;
        private Image previewRearWheel;
        private Image previewFrontWheel;
        private Image previewDecal;
        private readonly Dictionary<string, Image> previewAccessories = new();
        private readonly Dictionary<GarageCategory, Button> categoryButtons = new();
        private readonly Dictionary<int, GameObject> levelLockOverlays = new();
        private Button partsTab;
        private Button paintTab;
        private Button stylingTab;
        private Button levelOneButton;
        private Button levelTwoButton;
        private Button levelThreeButton;
        private Button levelFourButton;
        private Button levelFiveButton;
        private Button levelSixButton;
        private Button levelSevenButton;
        private Button levelEightButton;
        private Button levelNineButton;
        private Button levelTenButton;
        private Button levelElevenButton;
        private Button levelTwelveButton;
        private Text levelOneAction;
        private Text levelTwoAction;
        private Text levelThreeAction;
        private Text levelFourAction;
        private Text levelFiveAction;
        private Text levelSixAction;
        private Text levelSevenAction;
        private Text levelEightAction;
        private Text levelNineAction;
        private Text levelTenAction;
        private Text levelElevenAction;
        private Text levelTwelveAction;
        private Text menuStatus;
        private Text menuCoinText;
        private Text levelCoinText;
        private Text garageCoinText;
        private int selectedLevel = 1;
        private bool showingPaint;
        private TruckLayoutPart selectedLayoutPart = TruckLayoutPart.Body;
        private string selectedLayoutItemId;
        private bool layoutEditorOpen;
        private GarageCategory activeCategory;
        private Font font;

        public float UiScaleFactor => canvas == null ? 1f : canvas.scaleFactor;

        public static void Create(GameObject hostObject, Camera camera)
        {
            GameObject root = new("Startskärm och garage");
            FrontEndController controller = root.AddComponent<FrontEndController>();
            controller.host = hostObject;
            controller.gameCamera = camera;
            controller.Build();
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selectedLevel = LevelProgression.GetSelectedLevel();
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject safeObject = new(
                "Gemensam säker meny-yta",
                typeof(RectTransform)
            );
            safeObject.transform.SetParent(transform, false);
            frontEndSafeRoot = safeObject.GetComponent<RectTransform>();
            Stretch(frontEndSafeRoot);
            safeObject.AddComponent<SafeAreaFitter>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventObject = new("Menyns pekskärmssystem");
                eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
            }

            BuildLogin();
            BuildDashboard();
            BuildStoryHub();
            BuildMainMenu();
            BuildLevelSelect();
            BuildGarage();
            puzzleController = PuzzleGameController.Create(
                transform,
                font,
                ShowDashboard
            );
            memoryController = MemoryGameController.Create(
                transform,
                font,
                ShowDashboard
            );
            fishingController = FishingGameController.Create(
                transform,
                font,
                ShowDashboard
            );
            storybookController = StorybookController.Create(
                transform,
                font,
                ShowStoryHub
            );
            parentalGate = ParentalGateController.Create(
                frontEndSafeRoot,
                font,
                OnInitialParentalSetupCompleted,
                OnParentalSettingsChanged
            );
            bool previewParentSettings = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisParentPreviewSettings"
            );
            bool previewParentPassword = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisParentPreviewPassword"
            );
            if (previewParentPassword)
            {
                parentalGate.ShowPasswordSetupPreview();
                return;
            }
            if (previewParentSettings)
            {
                parentalGate.ShowInitialSettingsPreview();
                return;
            }
            if (!ParentalControls.IsConfigured)
            {
                parentalGate.ShowInitialSetup();
                return;
            }
            bool previewParentDashboard = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisParentPreviewDashboard"
            );
            bool previewNewPuzzle = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisParentPreviewPuzzle"
            );
            bool openPuzzle = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisPuzzle"
            );
            bool openPuzzlePlay = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisPuzzlePlay"
            );
            bool openLevelSelect = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisLevelSelect"
            ) || PlayerProfile.ConsumeLevelSelectRequest();
            bool openMemory = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisMemory"
            );
            bool openFishing = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisFishing"
            );
            bool openStory = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisStory"
            );
            bool previewDashboardPageTwo = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisDashboardPage2"
            );
            bool startSelectedRace = PlayerProfile.ConsumeRaceStartRequest();
            if (previewParentDashboard)
            {
                ShowDashboard();
            }
            else if (previewNewPuzzle)
            {
                ShowPuzzleHub();
                puzzleController.StartPuzzle(1);
            }
            else if (
                startSelectedRace
                && ParentalControls.IsEnabled(ParentalGame.MonsterTrucks)
            )
            {
                selectedLevel = LevelProgression.GetSelectedLevel();
                StartRace();
            }
            else if (openLevelSelect)
            {
                ShowLevelSelect();
            }
            else if (openMemory)
            {
                ShowMemory();
            }
            else if (openFishing)
            {
                ShowFishing();
            }
            else if (openStory)
            {
                const string storyIdPrefix = "-arisStoryId=";
                string storyIdArgument = System.Array.Find(
                    System.Environment.GetCommandLineArgs(),
                    argument => argument.StartsWith(
                        storyIdPrefix,
                        System.StringComparison.Ordinal
                    )
                );
                ShowStory(
                    string.IsNullOrEmpty(storyIdArgument)
                        ? "lilla-lumi"
                        : storyIdArgument.Substring(storyIdPrefix.Length)
                );
            }
            else if (openPuzzlePlay)
            {
                ShowPuzzleHub();
                puzzleController.StartPuzzle();
            }
            else if (openPuzzle)
            {
                ShowPuzzleHub();
            }
            else
            {
                if (string.IsNullOrEmpty(PlayerProfile.Username))
                {
                    ShowLogin();
                }
                else
                {
                    ShowDashboard();
                    if (previewDashboardPageTwo)
                    {
                        dashboardCarouselSnap?.GoToPage(1);
                    }
                }
            }
        }

        private void BuildLogin()
        {
            loginRoot = new GameObject("Användarnamn", typeof(RectTransform));
            loginRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(loginRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Bakgrund",
                loginRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);

            Image shade = CreateImage("Mjuk toning", loginRoot.transform, null);
            shade.color = new Color(0.08f, 0.04f, 0.2f, 0.46f);
            Stretch(shade.rectTransform);

            Image panel = CreatePanel(
                "Spelarprofil",
                loginRoot.transform,
                Vector2.zero,
                new Vector2(920f, 610f),
                RuntimeArt.Hex("#FFF3AD")
            );

            Text title = CreateText(
                "Välkommen",
                panel.transform,
                "VÄLKOMMEN!",
                76,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 205f), new Vector2(760f, 100f));

            Text prompt = CreateText(
                "Fråga",
                panel.transform,
                "VAD HETER DU?",
                38,
                RuntimeArt.Hex("#6B4589")
            );
            SetRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 105f), new Vector2(700f, 65f));

            Image inputPanel = CreatePanel(
                "Namnfält",
                panel.transform,
                new Vector2(0f, 5f),
                new Vector2(650f, 100f),
                Color.white
            );
            usernameInput = inputPanel.gameObject.AddComponent<InputField>();
            usernameInput.targetGraphic = inputPanel;
            usernameInput.characterLimit = 18;
            usernameInput.lineType = InputField.LineType.SingleLine;

            Text inputText = CreateText(
                "Skrivet namn",
                inputPanel.transform,
                "",
                42,
                RuntimeArt.Hex("#40245F")
            );
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.fontStyle = FontStyle.Bold;
            inputText.supportRichText = false;
            SetRect(inputText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            inputText.rectTransform.offsetMin = new Vector2(35f, 8f);
            inputText.rectTransform.offsetMax = new Vector2(-35f, -8f);
            usernameInput.textComponent = inputText;

            Text placeholder = CreateText(
                "Platshållare",
                inputPanel.transform,
                "Skriv ditt användarnamn",
                34,
                RuntimeArt.Hex("#A79BAE")
            );
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.fontStyle = FontStyle.Italic;
            SetRect(placeholder.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            placeholder.rectTransform.offsetMin = new Vector2(35f, 8f);
            placeholder.rectTransform.offsetMax = new Vector2(-35f, -8f);
            usernameInput.placeholder = placeholder;

            Button continueButton = CreateButton(
                panel.transform,
                "FORTSÄTT",
                new Vector2(0f, -125f),
                new Vector2(500f, 115f),
                RuntimeArt.Hex("#FF6B35"),
                46
            );
            continueButton.onClick.AddListener(SubmitUsername);
            usernameInput.onEndEdit.AddListener(value =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitUsername();
                }
            });

            loginStatus = CreateText(
                "Namnstatus",
                panel.transform,
                "",
                28,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(loginStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -225f), new Vector2(700f, 55f));
        }

        private void BuildDashboard()
        {
            dashboardRoot = new GameObject("Speldashboard", typeof(RectTransform));
            dashboardRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(dashboardRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Dashboardbakgrund",
                dashboardRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            Image shade = CreateImage("Dashboardtoning", dashboardRoot.transform, null);
            shade.color = new Color(0.08f, 0.04f, 0.2f, 0.42f);
            Stretch(shade.rectTransform);

            Text title = CreateText(
                "Dashboardtitel",
                dashboardRoot.transform,
                "VÄLJ SPEL",
                72,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(900f, 110f));
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            dashboardGreeting = CreateText(
                "Spelarhälsning",
                dashboardRoot.transform,
                "",
                34,
                Color.white
            );
            SetRect(dashboardGreeting.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -155f), new Vector2(1000f, 60f));
            AddOutline(dashboardGreeting, RuntimeArt.Hex("#40245F"), 3f);

            IReadOnlyList<StoryDefinition> stories = StoryCatalog.All;
            int validStoryCount = 0;
            for (int index = 0; index < stories.Count; index++)
            {
                if (stories[index] != null && stories[index].IsValid)
                {
                    validStoryCount++;
                }
            }
            dashboardPageCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    (4f + (validStoryCount > 0 ? 1f : 0f)) / 4f
                )
            );
            BuildDashboardCardScroller(dashboardPageCount);

            monsterTruckGameButton = CreateButton(
                dashboardCardContent.transform,
                "",
                new Vector2(270f, 0f),
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#2A66DB")
            );
            PlaceDashboardCard(monsterTruckGameButton, new Vector2(270f, 0f));
            Image truckImage = CreateImage(
                "Monstertruckbild",
                monsterTruckGameButton.transform,
                RuntimeArt.LoadSprite("Art/UI/Tracks/rainbow_track_card")
            );
            truckImage.type = Image.Type.Simple;
            truckImage.preserveAspect = false;
            SetRect(truckImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(380f, 280f));
            Text truckTitle = CreateText("Spelnamn", monsterTruckGameButton.transform, "MONSTERTRUCKS", 34, Color.white);
            SetRect(truckTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(390f, 80f));
            AddOutline(truckTitle, RuntimeArt.Hex("#40245F"), 3f);
            Image truckPlayButton = CreateCardPlayButton(monsterTruckGameButton.transform);
            Text truckAction = CreateText("Spela", monsterTruckGameButton.transform, "SPELA", 34, Color.white);
            SetRect(truckAction.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -215f), new Vector2(420f, 60f));
            monsterTruckGameButton.onClick.AddListener(
                () => ActivateDashboardCard(ShowLevelSelect)
            );
            monsterTruckParentLock = CreateParentLockOverlay(
                monsterTruckGameButton.transform
            );

            puzzleGameButton = CreateButton(
                dashboardCardContent.transform,
                "",
                new Vector2(730f, 0f),
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#8B56D9")
            );
            PlaceDashboardCard(puzzleGameButton, new Vector2(730f, 0f));
            Image puzzleImage = CreateImage(
                "Pusselbild",
                puzzleGameButton.transform,
                RuntimeArt.LoadSprite("Art/Puzzles/skogsvanner_puzzle")
            );
            puzzleImage.type = Image.Type.Simple;
            puzzleImage.preserveAspect = false;
            SetRect(puzzleImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(380f, 280f));
            Text puzzleTitle = CreateText("Spelnamn", puzzleGameButton.transform, "PUSSEL", 46, Color.white);
            SetRect(puzzleTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(390f, 80f));
            AddOutline(puzzleTitle, RuntimeArt.Hex("#40245F"), 3f);
            Image puzzlePlayButton = CreateCardPlayButton(puzzleGameButton.transform);
            Text puzzleStatus = CreateText("Spela pussel", puzzleGameButton.transform, "SPELA", 34, Color.white);
            SetRect(puzzleStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -215f), new Vector2(500f, 65f));
            puzzleGameButton.onClick.AddListener(
                () => ActivateDashboardCard(ShowPuzzleHub)
            );
            puzzleParentLock = CreateParentLockOverlay(puzzleGameButton.transform);

            memoryGameButton = CreateButton(
                dashboardCardContent.transform,
                "",
                new Vector2(1190f, 0f),
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#E05A9D")
            );
            PlaceDashboardCard(memoryGameButton, new Vector2(1190f, 0f));
            Image memoryImage = CreateImage(
                "Memorybild",
                memoryGameButton.transform,
                RuntimeArt.LoadSprite("Art/Puzzles/korall_puzzle")
            );
            memoryImage.type = Image.Type.Simple;
            memoryImage.preserveAspect = false;
            SetRect(memoryImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(380f, 280f));
            Text memoryTitle = CreateText("Spelnamn", memoryGameButton.transform, "MEMORY", 46, Color.white);
            SetRect(memoryTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(390f, 80f));
            AddOutline(memoryTitle, RuntimeArt.Hex("#40245F"), 3f);
            Image memoryPlayButton = CreateCardPlayButton(memoryGameButton.transform);
            Text memoryStatus = CreateText("Spela memory", memoryGameButton.transform, "SPELA", 34, Color.white);
            SetRect(memoryStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -215f), new Vector2(440f, 65f));
            memoryGameButton.onClick.AddListener(
                () => ActivateDashboardCard(ShowMemory)
            );
            memoryParentLock = CreateParentLockOverlay(memoryGameButton.transform);

            fishingGameButton = CreateButton(
                dashboardCardContent.transform,
                "",
                new Vector2(1650f, 0f),
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#28A9C7")
            );
            PlaceDashboardCard(fishingGameButton, new Vector2(1650f, 0f));
            Image fishingImage = CreateImage(
                "Fiskebild",
                fishingGameButton.transform,
                RuntimeArt.LoadSprite("Art/Fishing/fishing_background")
            );
            fishingImage.type = Image.Type.Simple;
            fishingImage.preserveAspect = false;
            SetRect(fishingImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(380f, 280f));
            Text fishingTitle = CreateText("Spelnamn", fishingGameButton.transform, "FISKE", 46, Color.white);
            SetRect(fishingTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(390f, 80f));
            AddOutline(fishingTitle, RuntimeArt.Hex("#40245F"), 3f);
            Image fishingPlayButton = CreateCardPlayButton(fishingGameButton.transform);
            Text fishingStatus = CreateText("Spela fiske", fishingGameButton.transform, "SPELA", 34, Color.white);
            SetRect(fishingStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -215f), new Vector2(390f, 65f));
            fishingGameButton.onClick.AddListener(
                () => ActivateDashboardCard(ShowFishing)
            );
            fishingParentLock = CreateParentLockOverlay(fishingGameButton.transform);

            CreateStoryDashboardCategory(stories);

            GameObject navigationObject = new(
                "Nedre sidnavigation",
                typeof(RectTransform)
            );
            navigationObject.transform.SetParent(dashboardRoot.transform, false);
            SetRect(
                navigationObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 72f),
                new Vector2(270f, 104f)
            );

            previousDashboardPageButton = CreateButton(
                navigationObject.transform,
                "‹",
                new Vector2(-62f, 0f),
                new Vector2(90f, 100f),
                RuntimeArt.Hex("#7A5AA6"),
                60
            );
            previousDashboardPageButton.onClick.AddListener(
                () => dashboardCarouselSnap?.GoToPage(
                    dashboardCarouselSnap.CurrentPage - 1
                )
            );
            nextDashboardPageButton = CreateButton(
                navigationObject.transform,
                "›",
                new Vector2(62f, 0f),
                new Vector2(90f, 100f),
                RuntimeArt.Hex("#7A5AA6"),
                60
            );
            nextDashboardPageButton.onClick.AddListener(
                () => dashboardCarouselSnap?.GoToPage(
                    dashboardCarouselSnap.CurrentPage + 1
                )
            );
            UpdateDashboardPageControls(0);

            Button parents = CreateButton(
                dashboardRoot.transform,
                "FÖRÄLDRAR",
                Vector2.zero,
                new Vector2(300f, 76f),
                RuntimeArt.Hex("#66507F"),
                27
            );
            SetRect(
                parents.image.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-175f, -58f),
                new Vector2(300f, 76f)
            );
            parents.onClick.AddListener(() => parentalGate?.ShowUnlock());
        }

        private void BuildDashboardCardScroller(int pages)
        {
            GameObject scrollObject = new(
                "Horisontell startsidesvep",
                typeof(RectTransform)
            );
            scrollObject.transform.SetParent(dashboardRoot.transform, false);
            RectTransform scrollRectTransform =
                scrollObject.GetComponent<RectTransform>();
            SetRect(
                scrollRectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f),
                new Vector2(1920f, 620f)
            );

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.inertia = false;
            scroll.elasticity = 0.12f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 55f;

            GameObject viewport = new(
                "Klippfönster",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask)
            );
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Innehållskort", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            dashboardCardContent = content.GetComponent<RectTransform>();
            dashboardCardContent.anchorMin = new Vector2(0f, 0.5f);
            dashboardCardContent.anchorMax = new Vector2(0f, 0.5f);
            dashboardCardContent.pivot = new Vector2(0f, 0.5f);
            dashboardCardContent.anchoredPosition = Vector2.zero;
            dashboardCardContent.sizeDelta = new Vector2(
                1920f * Mathf.Max(1, pages),
                610f
            );

            scroll.viewport = viewportRect;
            scroll.content = dashboardCardContent;
            dashboardCarouselSnap =
                scrollObject.AddComponent<LevelCarouselSnap>();
            dashboardCarouselSnap.Initialize(
                scroll,
                pages,
                UpdateDashboardPageControls
            );
        }

        private static void PlaceDashboardCard(Button card, Vector2 position)
        {
            RectTransform rect = card.image.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchoredPosition = position;
        }

        private void CreateStoryDashboardCategory(
            IReadOnlyList<StoryDefinition> stories
        )
        {
            List<StoryDefinition> validStories = new();
            for (int index = 0; index < stories.Count; index++)
            {
                StoryDefinition story = stories[index];
                if (story != null && story.IsValid)
                {
                    validStories.Add(story);
                }
            }

            if (validStories.Count == 0)
            {
                return;
            }

            Vector2 position = new(1920f + 270f, 0f);
            storyCategoryButton = CreateButton(
                dashboardCardContent.transform,
                "",
                position,
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#4967A6")
            );
            Button card = storyCategoryButton;
            card.gameObject.name = "Sagokategori startsida";
            PlaceDashboardCard(card, position);

            Image preview = CreateImage(
                "Lilla Lumi-förhandsvisning",
                card.transform,
                validStories[0].Cover
            );
            preview.type = Image.Type.Simple;
            preview.preserveAspect = false;
            SetRect(
                preview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 90f),
                new Vector2(380f, 280f)
            );

            Text storyTitle = CreateText(
                "Kategorinamn",
                card.transform,
                "SAGOR",
                46,
                Color.white
            );
            SetRect(
                storyTitle.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -130f),
                new Vector2(390f, 80f)
            );
            AddOutline(storyTitle, RuntimeArt.Hex("#40245F"), 3f);
            CreateCardPlayButton(card.transform);
            Text action = CreateText(
                "Öppna sagor",
                card.transform,
                "ÖPPNA",
                34,
                Color.white
            );
            SetRect(
                action.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -215f),
                new Vector2(390f, 65f)
            );
            card.onClick.AddListener(
                () => ActivateDashboardCard(ShowStoryHub)
            );
            storyParentLock = CreateParentLockOverlay(card.transform);
        }

        private void BuildStoryHub()
        {
            storyHubRoot = new GameObject(
                "Berättelser",
                typeof(RectTransform)
            );
            storyHubRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(storyHubRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Berättelsebakgrund",
                storyHubRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);

            Image shade = CreateImage(
                "Berättelsetoning",
                storyHubRoot.transform,
                null
            );
            shade.color = new Color(0.08f, 0.04f, 0.2f, 0.5f);
            Stretch(shade.rectTransform);

            Button back = CreateButton(
                storyHubRoot.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                74
            );
            back.onClick.AddListener(ShowDashboard);

            Text title = CreateText(
                "Berättelsetitel",
                storyHubRoot.transform,
                "SAGOR",
                72,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -90f),
                new Vector2(1000f, 110f)
            );
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            IReadOnlyList<StoryDefinition> stories = StoryCatalog.All;
            List<StoryDefinition> validStories = new();
            for (int index = 0; index < stories.Count; index++)
            {
                if (stories[index] != null && stories[index].IsValid)
                {
                    validStories.Add(stories[index]);
                }
            }

            const float spacing = 460f;
            float firstX = -(validStories.Count - 1) * spacing * 0.5f;
            for (int index = 0; index < validStories.Count; index++)
            {
                CreateStoryHubCard(
                    validStories[index],
                    new Vector2(firstX + index * spacing, -40f)
                );
            }

            storyHubRoot.SetActive(false);
        }

        private void CreateStoryHubCard(
            StoryDefinition definition,
            Vector2 position
        )
        {
            Button card = CreateButton(
                storyHubRoot.transform,
                "",
                position,
                new Vector2(420f, 590f),
                RuntimeArt.Hex("#4967A6")
            );
            card.gameObject.name = "Berättelsekort " + definition.StoryId;

            Image preview = CreateImage(
                "Sagoillustration",
                card.transform,
                definition.Cover
            );
            preview.type = Image.Type.Simple;
            preview.preserveAspect = false;
            SetRect(
                preview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 90f),
                new Vector2(380f, 280f)
            );

            Text storyTitle = CreateText(
                "Sagonamn",
                card.transform,
                definition.Title.ToUpperInvariant(),
                definition.Title.Length > 16 ? 36 : 42,
                Color.white
            );
            storyTitle.lineSpacing = 0.9f;
            SetRect(
                storyTitle.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -130f),
                new Vector2(390f, 100f)
            );
            AddOutline(storyTitle, RuntimeArt.Hex("#40245F"), 3f);

            CreateCardPlayButton(card.transform);
            Text action = CreateText(
                "Läs saga",
                card.transform,
                "LÄS",
                34,
                Color.white
            );
            SetRect(
                action.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -215f),
                new Vector2(390f, 65f)
            );

            string storyId = definition.StoryId;
            card.onClick.AddListener(() => ShowStory(storyId));
        }

        private void ActivateDashboardCard(System.Action action)
        {
            if (
                dashboardCarouselSnap != null
                && !dashboardCarouselSnap.CanActivateContent
            )
            {
                return;
            }
            action?.Invoke();
        }

        private void UpdateDashboardPageControls(int page)
        {
            if (previousDashboardPageButton != null)
            {
                previousDashboardPageButton.interactable = page > 0;
            }
            if (nextDashboardPageButton != null)
            {
                nextDashboardPageButton.interactable =
                    page < dashboardPageCount - 1;
            }
        }

        private GameObject CreateParentLockOverlay(Transform parent)
        {
            Image overlay = CreateImage("Låst av förälder", parent, null);
            overlay.color = new Color(0.13f, 0.14f, 0.17f, 0.88f);
            overlay.raycastTarget = false;
            Stretch(overlay.rectTransform);
            Text lockText = CreateText(
                "Föräldralås",
                overlay.transform,
                "LÅST\nAV FÖRÄLDER",
                38,
                Color.white
            );
            Stretch(lockText.rectTransform);
            AddOutline(lockText, Color.black, 3f);
            return overlay.gameObject;
        }

        private Image CreateCardPlayButton(Transform parent)
        {
            Image button = CreateImage(
                "Spelaknapp",
                parent,
                RuntimeArt.RoundedRectangleSprite(
                    "DashboardPlayButton",
                    RuntimeArt.Hex("#B93B18"),
                    RuntimeArt.Hex("#FF6B35"),
                    300,
                    76,
                    26,
                    7
                )
            );
            button.raycastTarget = false;
            SetRect(
                button.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -215f),
                new Vector2(300f, 76f)
            );
            return button;
        }

        private void BuildMainMenu()
        {
            menuRoot = new GameObject("Startskärm", typeof(RectTransform));
            menuRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(menuRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Bakgrund",
                menuRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);

            Image shade = CreateImage("Läsbar toning", menuRoot.transform, null);
            shade.color = new Color(0.12f, 0.05f, 0.25f, 0.32f);
            Stretch(shade.rectTransform);

            Button back = CreateButton(
                menuRoot.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                74
            );
            back.onClick.AddListener(ShowDashboard);

            Image titlePanel = CreatePanel(
                "Titel",
                menuRoot.transform,
                new Vector2(0f, 230f),
                new Vector2(1050f, 230f),
                RuntimeArt.Hex("#FFF3AD")
            );
            Text title = CreateText(
                "Speltitel",
                titlePanel.transform,
                "ARIS MONSTERTRUCKS",
                92,
                RuntimeArt.Hex("#4A266C")
            );
            Stretch(title.rectTransform);

            Button play = CreateButton(
                menuRoot.transform,
                "SPELA",
                new Vector2(0f, -65f),
                new Vector2(560f, 155f),
                RuntimeArt.Hex("#FF6B35")
            );
            play.onClick.AddListener(ShowLevelSelect);

            Button garage = CreateButton(
                menuRoot.transform,
                "VERKSTAD",
                new Vector2(0f, -255f),
                new Vector2(560f, 135f),
                RuntimeArt.Hex("#50C9F5")
            );
            garage.onClick.AddListener(ShowGarage);

            Text hint = CreateText(
                "Starttips",
                menuRoot.transform,
                "VÄLJ EN BANA ELLER BYGG OM DIN TRUCK",
                30,
                Color.white
            );
            SetRect(
                hint.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 62f),
                new Vector2(980f, 55f)
            );
            AddOutline(hint, RuntimeArt.Hex("#40245F"), 3f);

            menuCoinText = CreateCoinBalance(
                menuRoot.transform,
                new Vector2(790f, 455f)
            );
            RefreshLevelButtons();
        }

        private void BuildLevelSelect()
        {
            levelSelectRoot = new GameObject("Banväljare", typeof(RectTransform));
            levelSelectRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(levelSelectRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Banväljarbakgrund",
                levelSelectRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);

            Image shade = CreateImage("Mörk menytoning", levelSelectRoot.transform, null);
            shade.color = new Color(0.08f, 0.05f, 0.18f, 0.54f);
            Stretch(shade.rectTransform);

            Button back = CreateButton(
                levelSelectRoot.transform,
                "←",
                Vector2.zero,
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                74
            );
            back.onClick.AddListener(ShowDashboard);
            SetRect(
                back.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(85f, -58f),
                new Vector2(150f, 90f)
            );

            Button workshop = CreateButton(
                levelSelectRoot.transform,
                "VERKSTAD",
                Vector2.zero,
                new Vector2(260f, 86f),
                RuntimeArt.Hex("#50C9F5"),
                34
            );
            workshop.onClick.AddListener(ShowGarage);
            SetRect(
                workshop.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(300f, -58f),
                new Vector2(260f, 86f)
            );

            Text title = CreateText(
                "Banväljartitel",
                levelSelectRoot.transform,
                "VÄLJ BANA",
                60,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -58f),
                new Vector2(900f, 105f)
            );
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            BuildLevelCardScroller();

            levelOneButton = CreateLevelCard(
                1,
                "REGNBÅGSLOOPEN",
                "KULLAR • LOOP • BALLONGER",
                "Art/UI/Tracks/rainbow_track_card",
                new Vector2(350f, 0f),
                RuntimeArt.Hex("#5A8CE8"),
                out levelOneAction
            );
            levelTwoButton = CreateLevelCard(
                2,
                "DIRTBANAN",
                "PLATÅER • HOPP • JORD",
                "Art/UI/Tracks/dirt_track_card",
                new Vector2(960f, 0f),
                RuntimeArt.Hex("#D07A32"),
                out levelTwoAction
            );
            levelThreeButton = CreateLevelCard(
                3,
                "BERGSKLÄTTRING",
                "STEN • BERG • BRANTA BACKAR",
                "Art/UI/Tracks/mountain_track_card",
                new Vector2(1570f, 0f),
                RuntimeArt.Hex("#667C8A"),
                out levelThreeAction
            );
            levelFourButton = CreateLevelCard(
                4,
                "ISBANAN",
                "SNÖ • IS • FRUSNA BACKAR",
                "Art/UI/Tracks/ice_track_card",
                new Vector2(2270f, 0f),
                RuntimeArt.Hex("#54AEDD"),
                out levelFourAction
            );
            levelFiveButton = CreateLevelCard(
                5,
                "LAVABANAN",
                "VULKANER • LAVA • BASALT",
                "Art/UI/Tracks/lava_track_card",
                new Vector2(2880f, 0f),
                RuntimeArt.Hex("#B84428"),
                out levelFiveAction
            );
            levelSixButton = CreateLevelCard(
                6,
                "SPÖKBANAN",
                "SKELETT • SPÖKEN • MÅNSKEN",
                "Art/UI/Tracks/haunted_track_card",
                new Vector2(3490f, 0f),
                RuntimeArt.Hex("#5C4598"),
                out levelSixAction
            );
            levelSevenButton = CreateLevelCard(
                7,
                "DJUNGELBANAN",
                "DJUR • LIANER • VATTENFALL",
                "Art/UI/Tracks/jungle_track_card",
                new Vector2(4190f, 0f),
                RuntimeArt.Hex("#218B4B"),
                out levelSevenAction
            );
            levelEightButton = CreateLevelCard(
                8,
                "AFRIKABANAN",
                "SAVANN • DJUR • AKACIATRÄD",
                "Art/UI/Tracks/africa_track_card",
                new Vector2(4800f, 0f),
                RuntimeArt.Hex("#C77B24"),
                out levelEightAction
            );
            levelNineButton = CreateLevelCard(
                9,
                "ÖKENBANAN",
                "SAND • ORMAR • OASER",
                "Art/UI/Tracks/desert_track_card",
                new Vector2(5410f, 0f),
                RuntimeArt.Hex("#D06B2A"),
                out levelNineAction
            );
            levelTenButton = CreateLevelCard(
                10,
                "VATTENBANAN",
                "PLASK • RUTCHKANOR • VÅGOR",
                "Art/UI/Tracks/waterpark_track_card",
                new Vector2(6110f, 0f),
                RuntimeArt.Hex("#168CCB"),
                out levelTenAction
            );
            levelElevenButton = CreateLevelCard(
                11,
                "RYMDBANAN",
                "KRATRAR • KRISTALLER • PLANETER",
                "Art/UI/Tracks/space_track_card",
                new Vector2(6720f, 0f),
                RuntimeArt.Hex("#7047C8"),
                out levelElevenAction
            );
            levelTwelveButton = CreateLevelCard(
                12,
                "GODISBANAN",
                "POLKAGRISAR • KAKOR • GELÉ",
                "Art/UI/Tracks/candy_track_card",
                new Vector2(7330f, 0f),
                RuntimeArt.Hex("#E45A9B"),
                out levelTwelveAction
            );

            previousLevelPageButton = CreateButton(
                levelSelectRoot.transform,
                "‹",
                new Vector2(-895f, -38f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousLevelPageButton.onClick.AddListener(
                () => ChangeLevelPage(-1)
            );
            nextLevelPageButton = CreateButton(
                levelSelectRoot.transform,
                "›",
                new Vector2(895f, -38f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextLevelPageButton.onClick.AddListener(
                () => ChangeLevelPage(1)
            );
            levelPageText = CreateText(
                "Bansida",
                levelSelectRoot.transform,
                "",
                27,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                levelPageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -480f),
                new Vector2(320f, 50f)
            );

            menuStatus = CreateText(
                "Banstatus",
                levelSelectRoot.transform,
                "",
                27,
                Color.white
            );
            SetRect(
                menuStatus.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 92f),
                new Vector2(1500f, 48f)
            );
            AddOutline(menuStatus, RuntimeArt.Hex("#40245F"), 3f);

            levelCoinText = CreateCoinBalance(
                levelSelectRoot.transform,
                Vector2.zero
            );
            SetRect(
                levelCoinText.transform.parent.GetComponent<RectTransform>(),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-160f, -58f),
                new Vector2(260f, 82f)
            );
            levelSelectRoot.SetActive(false);
        }

        private void BuildLevelCardScroller()
        {
            GameObject scrollObject = new("Horisontell bansvep", typeof(RectTransform));
            scrollObject.transform.SetParent(levelSelectRoot.transform, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            SetRect(
                scrollRectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -38f),
                new Vector2(1920f, 720f)
            );

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.inertia = false;
            scroll.elasticity = 0.12f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 55f;

            GameObject viewport = new(
                "Klippfönster",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask)
            );
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Bankort", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            levelCardContent = content.GetComponent<RectTransform>();
            levelCardContent.anchorMin = new Vector2(0f, 0.5f);
            levelCardContent.anchorMax = new Vector2(0f, 0.5f);
            levelCardContent.pivot = new Vector2(0f, 0.5f);
            levelCardContent.anchoredPosition = Vector2.zero;
            levelCardContent.sizeDelta = new Vector2(7680f, 690f);

            scroll.viewport = viewportRect;
            scroll.content = levelCardContent;
            levelCardScroll = scroll;
            levelCarouselSnap = scrollObject.AddComponent<LevelCarouselSnap>();
            levelCarouselSnap.Initialize(scroll, 4, UpdateLevelPageControls);
        }

        private Button CreateLevelCard(
            int level,
            string title,
            string subtitle,
            string imagePath,
            Vector2 position,
            Color fill,
            out Text actionLabel
        )
        {
            Button card = CreateButton(
                levelCardContent.transform,
                "",
                position,
                new Vector2(510f, 700f),
                RuntimeArt.Hex("#2A66DB"),
                34
            );
            card.gameObject.name = "Bankort " + level;
            card.image.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            card.image.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            card.image.rectTransform.anchoredPosition = position;

            Image preview = CreateImage(
                "Banbild",
                card.transform,
                RuntimeArt.LoadSprite(imagePath)
            );
            preview.type = Image.Type.Simple;
            preview.preserveAspect = true;
            SetRect(
                preview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 145f),
                new Vector2(465f, 315f)
            );

            Text number = CreateText(
                "Bannummer",
                card.transform,
                "BANA " + level,
                26,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                number.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -180f),
                new Vector2(440f, 42f)
            );

            Text name = CreateText("Bannamn", card.transform, title, 38, Color.white);
            SetRect(
                name.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -70f),
                new Vector2(460f, 75f)
            );
            AddOutline(name, RuntimeArt.Hex("#40245F"), 2f);

            Text details = CreateText(
                "Bandetaljer",
                card.transform,
                subtitle,
                21,
                RuntimeArt.Hex("#FFF1C7")
            );
            SetRect(
                details.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -145f),
                new Vector2(455f, 45f)
            );

            actionLabel = CreateText(
                "Kortstatus",
                card.transform,
                "SPELA",
                34,
                Color.white
            );
            Image playButton = CreateImage(
                "Spelaknapp",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "TrackPlayButton",
                    RuntimeArt.Hex("#B93B18"),
                    RuntimeArt.Hex("#FF6B35"),
                    330,
                    82,
                    28,
                    7
                )
            );
            playButton.raycastTarget = false;
            SetRect(
                playButton.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -255f),
                new Vector2(330f, 82f)
            );
            actionLabel.transform.SetAsLastSibling();
            SetRect(
                actionLabel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -255f),
                new Vector2(420f, 75f)
            );
            AddOutline(actionLabel, RuntimeArt.Hex("#40245F"), 3f);
            levelLockOverlays[level] = CreateLevelLockOverlay(card.transform, level);
            card.onClick.AddListener(() => SelectLevelAndStart(level));
            return card;
        }

        private GameObject CreateLevelLockOverlay(Transform parent, int level)
        {
            Image greyOut = CreateImage(
                "Tydligt låst bana " + level,
                parent,
                null
            );
            greyOut.color = new Color(0.16f, 0.17f, 0.19f, 0.78f);
            greyOut.raycastTarget = false;
            Stretch(greyOut.rectTransform);
            Image lockBody = CreateImage(
                "Lås bana " + level,
                greyOut.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "LevelLock_" + level,
                    RuntimeArt.Hex("#454545"),
                    RuntimeArt.Hex("#777777"),
                    230,
                    160,
                    32,
                    8
                )
            );
            SetRect(
                lockBody.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f),
                new Vector2(230f, 160f)
            );
            lockBody.raycastTarget = false;
            Image lockIcon = CreateImage(
                "Tydlig hänglåssymbol",
                lockBody.transform,
                RuntimeArt.PadlockSprite()
            );
            lockIcon.preserveAspect = true;
            lockIcon.raycastTarget = false;
            SetRect(
                lockIcon.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 25f),
                new Vector2(92f, 92f)
            );
            Text lockLabel = CreateText(
                "Låstmarkering",
                lockBody.transform,
                "LÅST",
                30,
                Color.white
            );
            SetRect(
                lockLabel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -52f),
                new Vector2(190f, 45f)
            );
            AddOutline(lockLabel, RuntimeArt.Hex("#40245F"), 2f);
            greyOut.gameObject.SetActive(false);
            return greyOut.gameObject;
        }

        private void BuildGarage()
        {
            garageRoot = new GameObject("Garage", typeof(RectTransform));
            garageRoot.transform.SetParent(frontEndSafeRoot, false);
            Stretch(garageRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Verkstadsbakgrund",
                garageRoot.transform,
                RuntimeArt.LoadSprite("Art/Garage/workshop_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);

            Button back = CreateButton(
                garageRoot.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                74
            );
            back.onClick.AddListener(ShowLevelSelect);

            Button play = CreateButton(
                garageRoot.transform,
                "SPELA",
                new Vector2(800f, 470f),
                new Vector2(260f, 90f),
                RuntimeArt.Hex("#FF6B35")
            );
            play.onClick.AddListener(StartRace);

            garageCoinText = CreateCoinBalance(
                garageRoot.transform,
                new Vector2(-770f, 365f)
            );

            Text title = CreateText(
                "Garagetitel",
                garageRoot.transform,
                "BYGG DIN BIL!",
                62,
                Color.white
            );
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(900f, 100f));
            AddOutline(title, RuntimeArt.Hex("#24304D"), 5f);

            BuildGarageTruckPreview();
            BuildGarageInventory();
            BuildLayoutEditor();
            garageRoot.SetActive(false);
        }

        private void BuildGarageTruckPreview()
        {
            GameObject preview = new("Livebil");
            preview.transform.SetParent(garageRoot.transform, false);
            RectTransform previewRect = preview.AddComponent<RectTransform>();
            SetRect(previewRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 105f), new Vector2(900f, 440f));

            previewRearSuspension = CreateImage("Bakfjäder", preview.transform, RuntimeArt.LoadSprite("Art/Truck/suspension_spring"));
            previewRearSuspension.type = Image.Type.Simple;
            previewRearSuspension.preserveAspect = true;
            SetRect(previewRearSuspension.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-192f, -92f), new Vector2(90f, 180f));
            previewRearSuspension.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -10f);

            previewFrontSuspension = CreateImage("Framfjäder", preview.transform, RuntimeArt.LoadSprite("Art/Truck/suspension_spring"));
            previewFrontSuspension.type = Image.Type.Simple;
            previewFrontSuspension.preserveAspect = true;
            SetRect(previewFrontSuspension.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(190f, -92f), new Vector2(90f, 180f));
            previewFrontSuspension.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 10f);

            previewChassis = CreateImage("Chassi", preview.transform, RuntimeArt.LoadSprite("Art/Truck/chassis"));
            previewChassis.type = Image.Type.Simple;
            previewChassis.preserveAspect = true;
            SetRect(previewChassis.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, -142f), new Vector2(455f, 114f));

            previewBody = CreateImage("Kaross", preview.transform, RuntimeArt.LoadSprite("Art/Truck/body_plain"));
            previewBody.type = Image.Type.Simple;
            previewBody.preserveAspect = true;
            SetRect(previewBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(680f, 315f));

            previewDecal = CreateImage("Dekal", preview.transform, null);
            previewDecal.type = Image.Type.Simple;
            previewDecal.preserveAspect = true;
            SetRect(previewDecal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(48f, -28f), new Vector2(245f, 138f));

            foreach (
                GarageItemDefinition item in TruckCustomization.GetItems(
                    GarageCategory.Accessories
                )
            )
            {
                if (string.IsNullOrEmpty(item.ResourcePath))
                {
                    continue;
                }
                Image accessoryImage = CreateImage(
                    item.DisplayName,
                    preview.transform,
                    RuntimeArt.LoadSprite(item.ResourcePath)
                );
                accessoryImage.type = Image.Type.Simple;
                accessoryImage.preserveAspect = true;
                previewAccessories[item.Id] = accessoryImage;
                AddDragHandle(
                    accessoryImage,
                    TruckLayoutPart.Accessory,
                    item.Id
                );
            }

            previewRearWheel = CreateImage("Bakhjul", preview.transform, null);
            previewRearWheel.type = Image.Type.Simple;
            previewRearWheel.preserveAspect = true;
            SetRect(previewRearWheel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -174f), new Vector2(300f, 300f));

            previewFrontWheel = CreateImage("Framhjul", preview.transform, null);
            previewFrontWheel.type = Image.Type.Simple;
            previewFrontWheel.preserveAspect = true;
            SetRect(previewFrontWheel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(195f, -174f), new Vector2(300f, 300f));

            AddDragHandle(previewBody, TruckLayoutPart.Body);
            AddDragHandle(previewChassis, TruckLayoutPart.Chassis);
            AddDragHandle(previewRearWheel, TruckLayoutPart.RearWheel);
            AddDragHandle(previewFrontWheel, TruckLayoutPart.FrontWheel);
            AddDragHandle(previewDecal, TruckLayoutPart.Decal);
            AddDragHandle(previewRearSuspension, TruckLayoutPart.RearSuspension);
            AddDragHandle(previewFrontSuspension, TruckLayoutPart.FrontSuspension);
        }

        private void BuildGarageInventory()
        {
            Image inventory = CreatePanel(
                "Dellista",
                garageRoot.transform,
                new Vector2(0f, -385f),
                new Vector2(1660f, 300f),
                RuntimeArt.Hex("#FFF1C7")
            );
            inventoryRoot = inventory.gameObject;

            partsTab = CreateButton(
                inventory.transform,
                "FÄRG",
                new Vector2(-500f, 100f),
                new Vector2(390f, 68f),
                RuntimeArt.Hex("#5A8CE8"),
                30
            );
            partsTab.onClick.AddListener(() => ShowCategory(GarageCategory.Body));
            paintTab = CreateButton(
                inventory.transform,
                "DÄCK",
                new Vector2(0f, 100f),
                new Vector2(390f, 68f),
                RuntimeArt.Hex("#FFB83D"),
                30
            );
            paintTab.onClick.AddListener(() => ShowCategory(GarageCategory.Wheels));
            stylingTab = CreateButton(
                inventory.transform,
                "STYLING",
                new Vector2(500f, 100f),
                new Vector2(390f, 68f),
                RuntimeArt.Hex("#5A8CE8"),
                30
            );
            stylingTab.onClick.AddListener(() => ShowCategory(GarageCategory.Decals));

            GameObject row = new("Delkort");
            row.transform.SetParent(inventory.transform, false);
            itemRow = row.AddComponent<RectTransform>();
            SetRect(itemRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -43f), new Vector2(1540f, 170f));

            garageStatus = CreateText(
                "Status",
                garageRoot.transform,
                "",
                30,
                Color.white
            );
            SetRect(garageStatus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(930f, 55f));
            AddOutline(garageStatus, RuntimeArt.Hex("#24304D"), 3f);
        }

        private void BuildLayoutEditor()
        {
            Button toggle = CreateButton(
                garageRoot.transform,
                "JUSTERA DELAR",
                new Vector2(-650f, 470f),
                new Vector2(260f, 82f),
                RuntimeArt.Hex("#50C9F5"),
                30
            );
            layoutToggleLabel = toggle.GetComponentInChildren<Text>();
            toggle.onClick.AddListener(ToggleLayoutEditor);
            toggle.gameObject.SetActive(Debug.isDebugBuild);

            Image panel = CreatePanel(
                "Layouteditor",
                garageRoot.transform,
                new Vector2(0f, -385f),
                new Vector2(1660f, 290f),
                new Color(0.08f, 0.14f, 0.25f, 0.97f)
            );
            layoutEditorRoot = panel.gameObject;

            string[] names =
            {
                "KAROSS", "CHASSI", "BAKHJUL", "FRAMHJUL",
                "DEKAL", "AVGAS", "LJUSRAMP", "BAKFJÄDER", "FRAMFJÄDER"
            };
            TruckLayoutPart[] parts =
            {
                TruckLayoutPart.Body,
                TruckLayoutPart.Chassis,
                TruckLayoutPart.RearWheel,
                TruckLayoutPart.FrontWheel,
                TruckLayoutPart.Decal,
                TruckLayoutPart.Accessory,
                TruckLayoutPart.Accessory,
                TruckLayoutPart.RearSuspension,
                TruckLayoutPart.FrontSuspension
            };
            string[] itemIds =
            {
                null, null, null, null, null,
                "accessory_exhaust", "accessory_lights", null, null
            };
            for (int i = 0; i < names.Length; i++)
            {
                TruckLayoutPart part = parts[i];
                string itemId = itemIds[i];
                Button partButton = CreateButton(
                    panel.transform,
                    names[i],
                    new Vector2(-704f + i * 176f, 86f),
                    new Vector2(162f, 58f),
                    RuntimeArt.Hex("#5A8CE8"),
                    19
                );
                partButton.onClick.AddListener(
                    () => SelectLayoutPart(part, itemId)
                );
            }

            CreateEditorCommand(panel.transform, "←", -665f, () => NudgeSelected(new Vector2(-5f, 0f)));
            CreateEditorCommand(panel.transform, "→", -520f, () => NudgeSelected(new Vector2(5f, 0f)));
            CreateEditorCommand(panel.transform, "↑", -375f, () => NudgeSelected(new Vector2(0f, 5f)));
            CreateEditorCommand(panel.transform, "↓", -230f, () => NudgeSelected(new Vector2(0f, -5f)));
            CreateEditorCommand(panel.transform, "–", -65f, () => ScaleSelected(0.94f));
            CreateEditorCommand(panel.transform, "+", 80f, () => ScaleSelected(1.06f));
            CreateEditorCommand(panel.transform, "↶", 245f, () => RotateSelected(5f));
            CreateEditorCommand(panel.transform, "↷", 390f, () => RotateSelected(-5f));
            CreateEditorCommand(panel.transform, "ÅTERSTÄLL", 585f, ResetSelected, 220f, 23);

            layoutEditorStatus = CreateText(
                "Layoutstatus",
                panel.transform,
                "",
                24,
                Color.white
            );
            SetRect(
                layoutEditorStatus.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -108f),
                new Vector2(1450f, 45f)
            );
            AddOutline(layoutEditorStatus, RuntimeArt.Hex("#24304D"), 2f);
            layoutEditorRoot.SetActive(false);
        }

        private void CreateEditorCommand(
            Transform parent,
            string label,
            float x,
            UnityEngine.Events.UnityAction action,
            float width = 120f,
            int fontSize = 36
        )
        {
            Button button = CreateButton(
                parent,
                label,
                new Vector2(x, -15f),
                new Vector2(width, 68f),
                RuntimeArt.Hex("#FFF1A8"),
                fontSize
            );
            button.onClick.AddListener(action);
        }

        private void ShowLogin()
        {
            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(true);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            usernameInput.text = "";
            loginStatus.text = "";
            usernameInput.ActivateInputField();
        }

        private void OnInitialParentalSetupCompleted()
        {
            RefreshDashboardPermissions();
            if (string.IsNullOrEmpty(PlayerProfile.Username))
            {
                ShowLogin();
            }
            else
            {
                ShowDashboard();
            }
        }

        private void OnParentalSettingsChanged()
        {
            RefreshDashboardPermissions();
            ShowDashboard();
        }

        private void RefreshDashboardPermissions()
        {
            SetGamePermission(
                monsterTruckGameButton,
                monsterTruckParentLock,
                ParentalGame.MonsterTrucks
            );
            SetGamePermission(
                puzzleGameButton,
                puzzleParentLock,
                ParentalGame.Puzzle
            );
            SetGamePermission(
                memoryGameButton,
                memoryParentLock,
                ParentalGame.Memory
            );
            SetGamePermission(
                fishingGameButton,
                fishingParentLock,
                ParentalGame.Fishing
            );
            SetGamePermission(
                storyCategoryButton,
                storyParentLock,
                ParentalGame.Stories
            );
        }

        private static void SetGamePermission(
            Button button,
            GameObject lockOverlay,
            ParentalGame game
        )
        {
            bool enabled = ParentalControls.IsEnabled(game);
            if (button != null)
            {
                button.interactable = enabled;
            }
            lockOverlay?.SetActive(!enabled);
        }

        private void SubmitUsername()
        {
            string username = usernameInput.text.Trim();
            if (username.Length < 2)
            {
                loginStatus.text = "SKRIV MINST TVÅ TECKEN";
                usernameInput.ActivateInputField();
                return;
            }

            PlayerProfile.SaveUsername(username);
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(true);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            string username = PlayerProfile.Username;
            dashboardGreeting.text = string.IsNullOrEmpty(username)
                ? "VÄLKOMMEN!"
                : "HEJ " + username.ToUpperInvariant() + "!";
            RefreshDashboardPermissions();
            dashboardCarouselSnap?.GoToPage(0);
        }

        private void ShowMenu()
        {
            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(true);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            selectedLevel = LevelProgression.GetSelectedLevel();
            RefreshLevelButtons();
            RefreshWallet();
        }

        private void ShowLevelSelect()
        {
            if (!ParentalControls.IsEnabled(ParentalGame.MonsterTrucks))
            {
                ShowDashboard();
                return;
            }
            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(true);
            garageRoot.SetActive(false);
            selectedLevel = LevelProgression.GetSelectedLevel();
            menuStatus.text = "";
            if (levelCarouselSnap != null)
            {
                levelCarouselSnap.GoToPage((selectedLevel - 1) / 3);
            }
            RefreshLevelButtons();
            RefreshWallet();
        }

        private void ShowGarage()
        {
            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(true);
            activeCategory = GarageCategory.Wheels;
            RefreshWallet();
            RefreshPreview();
            ShowCategory(activeCategory);
        }

        private void ShowPuzzleHub()
        {
            if (!ParentalControls.IsEnabled(ParentalGame.Puzzle))
            {
                ShowDashboard();
                return;
            }
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            puzzleController?.ShowHub();
        }

        private void ShowMemory()
        {
            if (!ParentalControls.IsEnabled(ParentalGame.Memory))
            {
                ShowDashboard();
                return;
            }
            puzzleController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            memoryController?.Show();
        }

        private void ShowFishing()
        {
            if (!ParentalControls.IsEnabled(ParentalGame.Fishing))
            {
                ShowDashboard();
                return;
            }
            puzzleController?.Hide();
            memoryController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            fishingController?.Show();
        }

        private void ShowStory(string storyId)
        {
            if (!ParentalControls.IsEnabled(ParentalGame.Stories))
            {
                ShowDashboard();
                return;
            }

            StoryDefinition definition = StoryCatalog.Get(storyId);
            if (definition == null || !definition.IsValid)
            {
                Debug.LogError("Kunde inte öppna sagan: " + storyId);
                ShowDashboard();
                return;
            }

            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            storybookController?.Show(definition);
            const string storyPagePrefix = "-arisStoryPage=";
            string storyPageArgument = System.Array.Find(
                System.Environment.GetCommandLineArgs(),
                argument => argument.StartsWith(
                    storyPagePrefix,
                    System.StringComparison.Ordinal
                )
            );
            if (
                !string.IsNullOrEmpty(storyPageArgument)
                && int.TryParse(
                    storyPageArgument.Substring(storyPagePrefix.Length),
                    out int previewPage
                )
            )
            {
                storybookController?.SeekToPage(previewPage, false);
            }
            if (
                System.Array.Exists(
                    System.Environment.GetCommandLineArgs(),
                    argument => argument == "-arisStoryEnd"
                )
            )
            {
                storybookController?.ShowCompletionPreview();
            }
        }

        private void ShowStoryHub()
        {
            if (!ParentalControls.IsEnabled(ParentalGame.Stories))
            {
                ShowDashboard();
                return;
            }

            puzzleController?.Hide();
            memoryController?.Hide();
            fishingController?.Hide();
            storybookController?.Hide();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(true);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
        }

        private void ShowCategory(GarageCategory category)
        {
            activeCategory = category;
            showingPaint = category == GarageCategory.Body;
            partsTab.image.color =
                category == GarageCategory.Body
                    ? RuntimeArt.Hex("#FFB83D")
                    : RuntimeArt.Hex("#5A8CE8");
            paintTab.image.color =
                category == GarageCategory.Wheels
                    ? RuntimeArt.Hex("#FFB83D")
                    : RuntimeArt.Hex("#5A8CE8");
            stylingTab.image.color =
                category == GarageCategory.Decals
                    ? RuntimeArt.Hex("#FFB83D")
                    : RuntimeArt.Hex("#5A8CE8");
            for (int i = itemRow.childCount - 1; i >= 0; i--)
            {
                Destroy(itemRow.GetChild(i).gameObject);
            }

            List<GarageItemDefinition> items = new();
            if (category == GarageCategory.Body)
            {
                items.AddRange(TruckCustomization.GetItems(GarageCategory.Body));
            }
            else if (category == GarageCategory.Wheels)
            {
                items.AddRange(TruckCustomization.GetItems(GarageCategory.Wheels));
            }
            else
            {
                items.AddRange(TruckCustomization.GetItems(GarageCategory.Decals));
                items.AddRange(TruckCustomization.GetItems(GarageCategory.Accessories));
                items.RemoveAll(item => string.IsNullOrEmpty(item.ResourcePath));
            }

            for (int i = 0; i < items.Count; i++)
            {
                GarageItemDefinition item = items[i];
                bool unlocked = TruckCustomization.IsUnlocked(item);
                bool owned = TruckCustomization.IsOwned(item);
                bool selected = TruckCustomization.IsSelected(item);
                int columns = showingPaint ? 8 : items.Count;
                int column = i % columns;
                int row = i / columns;
                int itemsInRow = Mathf.Min(columns, items.Count - row * columns);
                float x = showingPaint
                    ? -(itemsInRow - 1) * 95f + column * 190f
                    : (i - (items.Count - 1) * 0.5f)
                        * (category == GarageCategory.Wheels ? 340f : 270f);
                float y = showingPaint ? 40f - row * 88f : 0f;
                Vector2 cardSize = showingPaint
                    ? new Vector2(168f, 75f)
                    : (
                        category == GarageCategory.Wheels
                            ? new Vector2(310f, 145f)
                            : new Vector2(245f, 140f)
                    );
                Button card = CreateButton(
                    itemRow,
                    "",
                    new Vector2(x, y),
                    cardSize,
                    unlocked
                        ? (
                            selected
                                ? RuntimeArt.Hex("#9AF0B1")
                                : owned
                                    ? RuntimeArt.Hex("#FFF1A8")
                                    : RuntimeArt.Hex("#FFE08A")
                        )
                        : RuntimeArt.Hex("#A9A1B2"),
                    28
                );
                GarageItemDragHandle dragHandle =
                    card.gameObject.AddComponent<GarageItemDragHandle>();
                dragHandle.Initialize(this, item);

                if (showingPaint)
                {
                    Image swatch = CreateImage(
                        "Färgprov",
                        card.transform,
                        RuntimeArt.CircleSprite(
                            "Paint_" + item.Id,
                            RuntimeArt.Hex("#40245F"),
                            RuntimeArt.Hex(item.ColorHex),
                            Color.white,
                            96
                        )
                    );
                    SetRect(
                        swatch.rectTransform,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        Vector2.zero,
                        new Vector2(58f, 58f)
                    );
                }
                else if (!string.IsNullOrEmpty(item.ResourcePath))
                {
                    Image thumbnail = CreateImage(
                        "Bild",
                        card.transform,
                        RuntimeArt.LoadSprite(item.ResourcePath)
                    );
                    thumbnail.type = Image.Type.Simple;
                    thumbnail.preserveAspect = true;
                    thumbnail.color = RuntimeArt.Hex(item.ColorHex);
                    SetRect(
                        thumbnail.rectTransform,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        Vector2.zero,
                        category == GarageCategory.Wheels
                            ? new Vector2(220f, 125f)
                            : new Vector2(165f, 105f)
                    );
                }

                if (selected)
                {
                    Text check = CreateText(
                        "Vald",
                        card.transform,
                        "✓",
                        42,
                        RuntimeArt.Hex("#176A3A")
                    );
                    SetRect(
                        check.rectTransform,
                        new Vector2(1f, 1f),
                        new Vector2(1f, 1f),
                        new Vector2(-18f, -17f),
                        new Vector2(38f, 38f)
                    );
                }

                if (!unlocked)
                {
                    CreateLockBadge(card.transform);
                    Text requirement = CreateText(
                        "Krav",
                        card.transform,
                        item.RequiredRating + " PLUPPAR",
                        21,
                        RuntimeArt.Hex("#5E4770")
                    );
                    SetRect(requirement.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58f, -35f), new Vector2(165f, 40f));
                }
                else if (!owned && item.Price > 0)
                {
                    Image priceCoin = CreateImage(
                        "Myntpris",
                        card.transform,
                        RuntimeArt.GoldCoinSprite()
                    );
                    priceCoin.preserveAspect = true;
                    SetRect(
                        priceCoin.rectTransform,
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f),
                        new Vector2(24f, 23f),
                        new Vector2(34f, 34f)
                    );
                    Text price = CreateText(
                        "Pris",
                        card.transform,
                        item.Price.ToString(),
                        24,
                        RuntimeArt.Hex("#4A266C")
                    );
                    SetRect(
                        price.rectTransform,
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f),
                        new Vector2(75f, 23f),
                        new Vector2(90f, 38f)
                    );
                }
            }

            garageStatus.text =
                showingPaint
                    ? "VÄLJ EN FÄRG"
                    : "TRYCK ELLER DRA BILDEN TILL BILEN";
        }

        private void CreateLockBadge(Transform parent)
        {
            Image shackle = CreateImage(
                "Låsbåge",
                parent,
                RuntimeArt.CircleSprite(
                    "GarageLockRing",
                    RuntimeArt.Hex("#40245F"),
                    RuntimeArt.Hex("#A9A1B2"),
                    RuntimeArt.Hex("#D8D1DF"),
                    64
                )
            );
            SetRect(
                shackle.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-80f, 22f),
                new Vector2(60f, 60f)
            );

            Image lockBody = CreateImage(
                "Låskropp",
                parent,
                RuntimeArt.RoundedRectangleSprite(
                    "GarageLockBody",
                    RuntimeArt.Hex("#40245F"),
                    RuntimeArt.Hex("#FFD84A"),
                    76,
                    58,
                    14,
                    6
                )
            );
            SetRect(
                lockBody.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-80f, -2f),
                new Vector2(76f, 58f)
            );
        }

        private void SelectItem(GarageItemDefinition item)
        {
            bool wasOwned = TruckCustomization.IsOwned(item);
            if (!TruckCustomization.TrySelect(item))
            {
                garageStatus.text = !TruckCustomization.IsUnlocked(item)
                    ? "LÅST – FÅ " + item.RequiredRating + " PLUPPAR PÅ BANAN"
                    : "DU BEHÖVER " + item.Price + " MYNT";
                return;
            }

            garageStatus.text =
                !wasOwned && item.Price > 0
                    ? item.DisplayName + " KÖPT!"
                    : item.Category == GarageCategory.Accessories
                    ? (
                        TruckCustomization.IsSelected(item)
                            ? item.DisplayName + " MONTERAD!"
                            : item.DisplayName + " BORTTAGEN!"
                    )
                    : item.DisplayName + " VALD!";
            RefreshWallet();
            RefreshPreview();
            ShowCategory(activeCategory);
        }

        public void MountGarageItem(GarageItemDefinition item)
        {
            SelectItem(item);
        }

        private void RefreshPreview()
        {
            GarageItemDefinition body = TruckCustomization.GetSelected(GarageCategory.Body);
            GarageItemDefinition wheels = TruckCustomization.GetSelected(GarageCategory.Wheels);
            GarageItemDefinition decal = TruckCustomization.GetSelected(GarageCategory.Decals);
            previewBody.sprite = RuntimeArt.LoadSprite(body.ResourcePath);
            previewBody.color = Color.white;
            previewRearWheel.sprite = RuntimeArt.LoadSprite(wheels.ResourcePath);
            previewFrontWheel.sprite = RuntimeArt.LoadSprite(wheels.ResourcePath);
            previewDecal.sprite = string.IsNullOrEmpty(decal.ResourcePath)
                ? null
                : RuntimeArt.LoadSprite(decal.ResourcePath);
            previewDecal.enabled = previewDecal.sprite != null;

            foreach (
                GarageItemDefinition accessory in TruckCustomization.GetItems(
                    GarageCategory.Accessories
                )
            )
            {
                if (
                    string.IsNullOrEmpty(accessory.ResourcePath)
                    || !previewAccessories.TryGetValue(accessory.Id, out Image image)
                )
                {
                    continue;
                }
                image.sprite = RuntimeArt.LoadSprite(accessory.ResourcePath);
                image.enabled =
                    layoutEditorOpen || TruckCustomization.IsSelected(accessory);
            }
            ApplySavedLayoutToPreview();
            ApplyPreviewLayerOrder();
        }

        private void ApplyPreviewLayerOrder()
        {
            Transform[] ordered =
                new[]
                {
                    previewChassis.transform,
                    previewRearSuspension.transform,
                    previewFrontSuspension.transform,
                    previewAccessories["accessory_exhaust"].transform,
                    previewBody.transform,
                    previewDecal.transform,
                    previewAccessories["accessory_lights"].transform,
                    previewRearWheel.transform,
                    previewFrontWheel.transform
                };

            for (int i = 0; i < ordered.Length; i++)
            {
                ordered[i].SetSiblingIndex(i);
            }
        }

        private void AddDragHandle(
            Image image,
            TruckLayoutPart part,
            string itemId = null
        )
        {
            GaragePartDragHandle handle = image.gameObject.AddComponent<GaragePartDragHandle>();
            handle.Initialize(this, part, itemId);
        }

        private void ToggleLayoutEditor()
        {
            layoutEditorOpen = !layoutEditorOpen;
            inventoryRoot.SetActive(!layoutEditorOpen);
            layoutEditorRoot.SetActive(layoutEditorOpen);
            layoutToggleLabel.text = layoutEditorOpen ? "KLAR" : "JUSTERA DELAR";
            if (layoutEditorOpen)
            {
                SelectLayoutPart(selectedLayoutPart, selectedLayoutItemId);
                RefreshPreview();
            }
            else
            {
                TruckLayout.Save();
                bool savedGlobally = TruckLayout.SaveAsProjectDefaults(out _);
                garageStatus.text = savedGlobally
                    ? "GLOBAL STANDARDLAYOUT SPARAD!"
                    : "BILLAYOUT SPARAD LOKALT!";
                RefreshPreview();
            }
        }

        public void SelectLayoutPart(TruckLayoutPart part, string itemId = null)
        {
            if (!layoutEditorOpen)
            {
                return;
            }

            selectedLayoutPart = part;
            selectedLayoutItemId = itemId;
            UpdateLayoutStatus("DRA DELEN DIREKT ELLER ANVÄND KNAPPARNA");
        }

        public void DragLayoutPart(
            TruckLayoutPart part,
            string itemId,
            Vector2 delta
        )
        {
            if (!layoutEditorOpen)
            {
                return;
            }

            selectedLayoutPart = part;
            selectedLayoutItemId = itemId;
            TruckPartLayout value = TruckLayout.Get(part, itemId);
            value.x += delta.x;
            value.y += delta.y;
            CommitLayoutChange("POSITION SPARAD");
        }

        private void NudgeSelected(Vector2 delta)
        {
            TruckPartLayout value = TruckLayout.Get(
                selectedLayoutPart,
                selectedLayoutItemId
            );
            value.x += delta.x;
            value.y += delta.y;
            CommitLayoutChange("POSITION SPARAD");
        }

        private void ScaleSelected(float multiplier)
        {
            TruckPartLayout value = TruckLayout.Get(
                selectedLayoutPart,
                selectedLayoutItemId
            );
            value.width = Mathf.Clamp(value.width * multiplier, 35f, 1000f);
            value.height = Mathf.Clamp(value.height * multiplier, 35f, 1000f);
            CommitLayoutChange("STORLEK SPARAD");
        }

        private void RotateSelected(float degrees)
        {
            TruckPartLayout value = TruckLayout.Get(
                selectedLayoutPart,
                selectedLayoutItemId
            );
            value.rotation = Mathf.Repeat(value.rotation + degrees + 180f, 360f) - 180f;
            CommitLayoutChange("ROTATION SPARAD");
        }

        private void ResetSelected()
        {
            TruckLayout.ResetPart(selectedLayoutPart, selectedLayoutItemId);
            ApplySavedLayoutToPreview();
            bool savedGlobally = TruckLayout.SaveAsProjectDefaults(out _);
            UpdateLayoutStatus(
                savedGlobally
                    ? "DELEN ÅTERSTÄLLD • GLOBALT SPARAD"
                    : "DELEN ÅTERSTÄLLD • ENDAST LOKALT SPARAD"
            );
        }

        private void CommitLayoutChange(string message)
        {
            TruckLayout.Save();
            ApplySavedLayoutToPreview();
            bool savedGlobally = TruckLayout.SaveAsProjectDefaults(out string path);
            UpdateLayoutStatus(
                savedGlobally
                    ? message + " • GLOBALT SPARAD"
                    : message + " • ENDAST LOKALT SPARAD"
            );
        }

        private void UpdateLayoutStatus(string message)
        {
            if (layoutEditorStatus == null)
            {
                return;
            }

            TruckPartLayout value = TruckLayout.Get(
                selectedLayoutPart,
                selectedLayoutItemId
            );
            layoutEditorStatus.text =
                selectedLayoutPart
                + "  X "
                + Mathf.RoundToInt(value.x)
                + "  Y "
                + Mathf.RoundToInt(value.y)
                + "  STORLEK "
                + Mathf.RoundToInt(value.width)
                + "  VINKEL "
                + Mathf.RoundToInt(value.rotation)
                + "°  •  "
                + message;
        }

        private void ApplySavedLayoutToPreview()
        {
            TruckLayoutData defaults = TruckLayout.CreateDefault();
            ApplyPreviewRect(previewBody, TruckLayout.Get(TruckLayoutPart.Body));
            ApplyPreviewRect(previewChassis, TruckLayout.Get(TruckLayoutPart.Chassis));
            ApplyPreviewRect(previewRearWheel, TruckLayout.Get(TruckLayoutPart.RearWheel));
            ApplyPreviewRect(previewFrontWheel, TruckLayout.Get(TruckLayoutPart.FrontWheel));
            ApplyPreviewRect(previewDecal, TruckLayout.Get(TruckLayoutPart.Decal));
            ApplyPreviewRect(
                previewRearSuspension,
                TruckLayout.Get(TruckLayoutPart.RearSuspension)
            );
            ApplyPreviewRect(
                previewFrontSuspension,
                TruckLayout.Get(TruckLayoutPart.FrontSuspension)
            );

            TruckPartLayout accessoryDefault = defaults.accessory;
            foreach (KeyValuePair<string, Image> entry in previewAccessories)
            {
                GarageAccessoryMount mount = GarageAccessoryMounts.Get(entry.Key);
                TruckPartLayout accessory = TruckLayout.Get(
                    TruckLayoutPart.Accessory,
                    entry.Key
                );
                float accessoryScale =
                    accessory.width / Mathf.Max(1f, accessoryDefault.width);
                SetRect(
                    entry.Value.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    mount.PreviewPosition
                        + new Vector2(
                            accessory.x - accessoryDefault.x,
                            accessory.y - accessoryDefault.y
                        ),
                    mount.PreviewSize * accessoryScale
                );
                entry.Value.rectTransform.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    accessory.rotation
                );
                entry.Value.rectTransform.localScale = new Vector3(
                    mount.MirrorHorizontally ? -1f : 1f,
                    1f,
                    1f
                );
            }
        }

        private static void ApplyPreviewRect(Image image, TruckPartLayout value)
        {
            SetRect(
                image.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(value.x, value.y),
                new Vector2(value.width, value.height)
            );
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, value.rotation);
        }

        private void StartRace()
        {
            TruckLayout.Save();
            loginRoot.SetActive(false);
            dashboardRoot.SetActive(false);
            storyHubRoot.SetActive(false);
            menuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
            garageRoot.SetActive(false);
            gameObject.SetActive(false);
            RaceDirector director = host.GetComponent<RaceDirector>();
            if (director == null)
            {
                director = host.AddComponent<RaceDirector>();
                director.BuildGame(gameCamera, selectedLevel);
            }
        }

        private void SelectLevelAndStart(int levelNumber)
        {
            if (!LevelProgression.TrySelectLevel(levelNumber))
            {
                menuStatus.text =
                    "KLARA BANA "
                    + (levelNumber - 1)
                    + " FÖR ATT LÅSA UPP BANA "
                    + levelNumber
                    + "!";
                return;
            }

            selectedLevel = levelNumber;
            menuStatus.text = levelNumber switch
            {
                2 => "DIRTBANAN STARTAR!",
                3 => "BERGSKLÄTTRINGEN STARTAR!",
                4 => "ISBANAN STARTAR!",
                5 => "LAVABANAN STARTAR!",
                6 => "SPÖKBANAN STARTAR!",
                7 => "DJUNGELBANAN STARTAR!",
                8 => "AFRIKABANAN STARTAR!",
                9 => "ÖKENBANAN STARTAR!",
                10 => "VATTENBANAN STARTAR!",
                11 => "RYMDBANAN STARTAR!",
                12 => "GODISBANAN STARTAR!",
                _ => "REGNBÅGSLOOPEN STARTAR!"
            };
            RefreshLevelButtons();
            StartRace();
        }

        private void ChangeLevelPage(int direction)
        {
            if (levelCarouselSnap == null)
            {
                return;
            }
            levelCarouselSnap.GoToPage(levelCarouselSnap.CurrentPage + direction);
        }

        private void UpdateLevelPageControls(int page)
        {
            const int pageCount = 4;
            if (previousLevelPageButton != null)
            {
                previousLevelPageButton.interactable = page > 0;
            }
            if (nextLevelPageButton != null)
            {
                nextLevelPageButton.interactable = page < pageCount - 1;
            }
            if (levelPageText != null)
            {
                levelPageText.text = "";
            }
        }

        private void RefreshLevelButtons()
        {
            if (
                levelOneButton == null
                || levelTwoButton == null
                || levelThreeButton == null
                || levelFourButton == null
                || levelFiveButton == null
                || levelSixButton == null
                || levelSevenButton == null
                || levelEightButton == null
                || levelNineButton == null
                || levelTenButton == null
                || levelElevenButton == null
                || levelTwelveButton == null
            )
            {
                return;
            }

            bool levelTwoUnlocked = LevelProgression.IsLevelTwoUnlocked();
            bool levelThreeUnlocked = LevelProgression.IsLevelThreeUnlocked();
            bool levelFourUnlocked = LevelProgression.IsLevelUnlocked(4);
            bool levelFiveUnlocked = LevelProgression.IsLevelUnlocked(5);
            bool levelSixUnlocked = LevelProgression.IsLevelUnlocked(6);
            bool levelSevenUnlocked = LevelProgression.IsLevelUnlocked(7);
            bool levelEightUnlocked = LevelProgression.IsLevelUnlocked(8);
            bool levelNineUnlocked = LevelProgression.IsLevelUnlocked(9);
            bool levelTenUnlocked = LevelProgression.IsLevelUnlocked(10);
            bool levelElevenUnlocked = LevelProgression.IsLevelUnlocked(11);
            bool levelTwelveUnlocked = LevelProgression.IsLevelUnlocked(12);
            levelOneButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelTwoButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelThreeButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelFourButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelFiveButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelSixButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelSevenButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelEightButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelNineButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelTenButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelElevenButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelTwelveButton.image.color = RuntimeArt.Hex("#2A66DB");
            levelOneAction.text = "SPELA";
            levelTwoAction.text = levelTwoUnlocked
                ? "SPELA"
                : "LÅST";
            levelThreeAction.text = levelThreeUnlocked
                ? "SPELA"
                : "LÅST";
            levelFourAction.text = levelFourUnlocked
                ? "SPELA"
                : "LÅST";
            levelFiveAction.text = levelFiveUnlocked
                ? "SPELA"
                : "LÅST";
            levelSixAction.text = levelSixUnlocked
                ? "SPELA"
                : "LÅST";
            levelSevenAction.text = levelSevenUnlocked
                ? "SPELA"
                : "LÅST";
            levelEightAction.text = levelEightUnlocked
                ? "SPELA"
                : "LÅST";
            levelNineAction.text = levelNineUnlocked
                ? "SPELA"
                : "LÅST";
            levelTenAction.text = levelTenUnlocked
                ? "SPELA"
                : "LÅST";
            levelElevenAction.text = levelElevenUnlocked
                ? "SPELA"
                : "LÅST";
            levelTwelveAction.text = levelTwelveUnlocked
                ? "SPELA"
                : "LÅST";
            levelLockOverlays[1].SetActive(false);
            levelLockOverlays[2].SetActive(!levelTwoUnlocked);
            levelLockOverlays[3].SetActive(!levelThreeUnlocked);
            levelLockOverlays[4].SetActive(!levelFourUnlocked);
            levelLockOverlays[5].SetActive(!levelFiveUnlocked);
            levelLockOverlays[6].SetActive(!levelSixUnlocked);
            levelLockOverlays[7].SetActive(!levelSevenUnlocked);
            levelLockOverlays[8].SetActive(!levelEightUnlocked);
            levelLockOverlays[9].SetActive(!levelNineUnlocked);
            levelLockOverlays[10].SetActive(!levelTenUnlocked);
            levelLockOverlays[11].SetActive(!levelElevenUnlocked);
            levelLockOverlays[12].SetActive(!levelTwelveUnlocked);
        }

        private Text CreateCoinBalance(Transform parent, Vector2 position)
        {
            Image panel = CreatePanel(
                "Myntsaldo",
                parent,
                position,
                new Vector2(260f, 82f),
                RuntimeArt.Hex("#FFF1A8")
            );
            Image coin = CreateImage(
                "Mynt",
                panel.transform,
                RuntimeArt.GoldCoinSprite()
            );
            coin.preserveAspect = true;
            SetRect(
                coin.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-78f, 0f),
                new Vector2(58f, 58f)
            );
            Text value = CreateText(
                "Saldo",
                panel.transform,
                CoinWallet.Balance.ToString(),
                40,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(
                value.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(38f, 0f),
                new Vector2(150f, 58f)
            );
            return value;
        }

        private void RefreshWallet()
        {
            string balance = CoinWallet.Balance.ToString();
            if (menuCoinText != null)
            {
                menuCoinText.text = balance;
            }
            if (levelCoinText != null)
            {
                levelCoinText.text = balance;
            }
            if (garageCoinText != null)
            {
                garageCoinText.text = balance;
            }
        }

        private Image CreatePanel(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color fill
        )
        {
            Image panel = CreateImage(
                name,
                parent,
                RuntimeArt.RoundedRectangleSprite(
                    "FrontendPanel_" + name,
                    RuntimeArt.Hex("#40245F"),
                    fill,
                    Mathf.RoundToInt(size.x),
                    Mathf.RoundToInt(size.y),
                    42,
                    8
                )
            );
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            return panel;
        }

        private Button CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color fill,
            int fontSize = 52
        )
        {
            GameObject buttonObject = new(label + "-knapp");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "FrontendButton_" + label + fill,
                RuntimeArt.Hex("#40245F"),
                fill,
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y),
                38,
                8
            );
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            if (!string.IsNullOrEmpty(label))
            {
                Text text = CreateText("Text", buttonObject.transform, label, fontSize, Color.white);
                Stretch(text.rectTransform);
                AddOutline(text, RuntimeArt.Hex("#40245F"), 3f);
            }
            return button;
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
            if (
                name.IndexOf(
                    "bakgrund",
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0
                || name.IndexOf(
                    "toning",
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                imageObject.AddComponent<SafeAreaFullBleed>();
            }
            return image;
        }

        private static void AddOutline(Text text, Color color, float distance)
        {
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
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

    public sealed class LevelCarouselSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private const float SwipeThreshold = 70f;

        private ScrollRect scroll;
        private int pageCount = 1;
        private System.Action<int> onPageChanged;
        private Vector2 dragStart;
        private float suppressActivationUntil;

        public int CurrentPage { get; private set; }
        public bool CanActivateContent =>
            Time.unscaledTime >= suppressActivationUntil;

        public void Initialize(
            ScrollRect target,
            int pages,
            System.Action<int> pageChanged
        )
        {
            scroll = target;
            pageCount = Mathf.Max(1, pages);
            onPageChanged = pageChanged;
            GoToPage(0);
        }

        public void GoToPage(int page)
        {
            CurrentPage = Mathf.Clamp(page, 0, pageCount - 1);
            if (scroll != null)
            {
                scroll.StopMovement();
                scroll.horizontalNormalizedPosition = pageCount <= 1
                    ? 0f
                    : CurrentPage / (pageCount - 1f);
            }
            onPageChanged?.Invoke(CurrentPage);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStart = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (scroll == null)
            {
                return;
            }

            float horizontalDistance = eventData.position.x - dragStart.x;
            if (Mathf.Abs(horizontalDistance) >= 8f)
            {
                suppressActivationUntil = Time.unscaledTime + 0.18f;
            }
            if (Mathf.Abs(horizontalDistance) >= SwipeThreshold)
            {
                GoToPage(CurrentPage + (horizontalDistance < 0f ? 1 : -1));
                return;
            }

            float lastPage = pageCount - 1f;
            int page = lastPage <= 0f
                ? 0
                : Mathf.RoundToInt(
                    scroll.horizontalNormalizedPosition * lastPage
                );
            GoToPage(page);
        }
    }
}
