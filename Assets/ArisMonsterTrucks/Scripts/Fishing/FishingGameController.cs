using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Fishing
{
    public sealed class FishingGameController : MonoBehaviour
    {
        private const float MinimumBiteDelay = 1.5f;
        private const float MaximumBiteDelay = 4f;
        private const float BiteWindow = 3f;

        private sealed class FishBookCard
        {
            public Image Picture;
            public Text Question;
            public Text Name;
            public Text Details;
            public Image RarityBand;
        }

        private sealed class FishShopCard
        {
            public GameObject Root;
            public Image Picture;
            public Text Name;
            public Text Details;
            public Text Price;
            public Button SellOneButton;
            public Button SellAllButton;
            public string FishId;
        }

        private sealed class FishShopStack
        {
            public string FishId;
            public readonly List<FishSpecimenRecord> Specimens = new();
        }

        [Serializable]
        private sealed class FishingRigLayout
        {
            public Vector2 rodPosition;
            public float rodAngle;
            public float rodLength;
            public Vector2 fishingShoulder;
            public float gripAlongRod;
            public float gripNormalOffset;
            public float fishingHandOverlap;
            public Vector2 restingShoulder;
            public Vector2 restingHand;
            public float restingHandOverlap;
            public Vector2 bobberWaterPosition;

            public static FishingRigLayout Default()
            {
                return new FishingRigLayout
                {
                    rodPosition = new Vector2(-395.82175f, 9.19091f),
                    rodAngle = 28f,
                    rodLength = 530f,
                    fishingShoulder = new Vector2(-491.4072f, -16.54363f),
                    gripAlongRod = -14f,
                    gripNormalOffset = 9.54816f,
                    fishingHandOverlap = 12f,
                    restingShoulder = new Vector2(-648.2653f, -12.86727f),
                    restingHand = new Vector2(-568.61084f, -65.56181f),
                    restingHandOverlap = 8f,
                    bobberWaterPosition = new Vector2(385f, -145f)
                };
            }
        }

        private readonly FishingStateMachine stateMachine = new();
        private readonly List<Image> bubbles = new();
        private readonly List<Vector2> bubbleVelocities = new();
        private readonly List<FishingConfettiParticle> confetti = new();
        private readonly List<FishBookCard> fishBookCards = new();
        private readonly List<FishShopCard> fishShopCards = new();
        private readonly List<FishShopStack> fishShopInventory = new();
        private readonly List<GameObject> rodShopCards = new();
        private readonly List<Text> rodShopActionTexts = new();
        private readonly List<GameObject> lureShopCards = new();
        private readonly List<Text> lureShopActionTexts = new();
        private readonly List<Image> specialRodBands = new();
        private readonly List<Image> locationCompleteFishImages = new();
        private readonly List<RectTransform> activeLureVariants = new();
        private readonly List<SwimmingFishView> swimmers = new();
        private readonly List<GameObject> locationCards = new();
        private readonly List<Button> locationButtons = new();
        private readonly List<GameObject> locationLocks = new();
        private readonly List<Text> locationActionTexts = new();
        private readonly List<Text> locationProgressTexts = new();

        private GameObject sceneRoot;
        private GameObject fishBookPanel;
        private GameObject fishShopPanel;
        private GameObject fishShopInventoryRoot;
        private GameObject fishingRodShopRoot;
        private GameObject baitShopRoot;
        private GameObject lureShopRoot;
        private GameObject locationPanel;
        private GameObject gameplayTopHud;
        private GameObject mainControlRoot;
        private GameObject catchPopup;
        private GameObject locationCompletePopup;
        private GameObject hintPanel;
        private RectTransform safeArea;
        private RectTransform waterArea;
        private RectTransform rod;
        private Image rodImage;
        private Image classicRodHandle;
        private GameObject specialRodDecoration;
        private Image specialRodReel;
        private Image specialRodReelHub;
        private Text specialRodSymbol;
        private RectTransform fishingLine;
        private RectTransform bobber;
        private RectTransform activeLure;
        private RectTransform catchFish;
        private RectTransform characterBody;
        private RectTransform characterHead;
        private RectTransform frontArm;
        private RectTransform restingArm;
        private Image characterHeadImage;
        private Sprite headIdleSprite;
        private Sprite headBlinkSprite;
        private Sprite headHappySprite;
        private Sprite headReelSprite;
        private Image mainButtonImage;
        private Image backgroundImage;
        private Button previousLocationPageButton;
        private Button nextLocationPageButton;
        private Button mainButton;
        private Text mainButtonText;
        private Text progressText;
        private Text soundButtonText;
        private Text hintText;
        private Text catchRibbonText;
        private Text catchNameText;
        private Text catchRarityText;
        private Text catchLengthText;
        private Text fishBookPageText;
        private Text fishShopPageText;
        private Text fishShopCoinText;
        private Text fishShopStatusText;
        private Text fishShopEmptyText;
        private Text rodShopPageText;
        private Text baitCountText;
        private Text baitActionText;
        private Text lureShopPageText;
        private Text floatActionText;
        private Text wormCountText;
        private Text catchValueText;
        private Text locationCompleteTitleText;
        private Text locationCompleteSubtitleText;
        private Text locationCompleteNextText;
        private Button previousFishShopPageButton;
        private Button nextFishShopPageButton;
        private Button previousRodShopPageButton;
        private Button nextRodShopPageButton;
        private Button previousLureShopPageButton;
        private Button nextLureShopPageButton;
        private Text rigEditorValues;
        private Image catchPicture;
        private Image splash;
        private readonly List<Text> waterRings = new();

        private Font font;
        private Action onBack;
        private AudioSource audioSource;
        private AudioSource introAudioSource;
        private IReadOnlyList<FishDefinition> definitions;
        private FishSelectionService selection;
        private LocationFishSelectionService locationSelection;
        private FishCollectionService collection;
        private IRandomProvider random;
        private FishDefinition selectedFish;
        private float selectedLength;
        private Coroutine flowRoutine;
        private Vector2 bobberRestPosition;
        private Vector2 bobberWaterPosition = new(385f, -145f);
        private FishingRigLayout rigLayout;
        private GameObject rigEditorPanel;
        private RectTransform rodBaseHandle;
        private RectTransform fishingShoulderHandle;
        private RectTransform fishingGripHandle;
        private RectTransform restingShoulderHandle;
        private RectTransform restingHandHandle;
        private RectTransform waterHandle;
        private bool hasCastOnce;
        private int fishBookPage;
        private int fishShopPage;
        private int fishShopTab;
        private int rodShopPage;
        private int lureShopPage;
        private bool currentCastUsesWorm;
        private bool tackleUnderwater;
        private bool pendingLocationCompletion;
        private int locationIndex;
        private int locationPage;
        private float bubbleSpawnClock;
        private float catchBounceClock;
        private float nextBlinkAt;
        private float blinkStartedAt = -1f;
        private bool selectorPointerDown;
        private Vector2 selectorPointerStart;

        public FishingState CurrentState => stateMachine.Current;
        public string MainButtonLabel => mainButtonText == null ? "" : mainButtonText.text;

        public static FishingGameController Create(
            Transform parent,
            Font uiFont,
            Action returnAction
        )
        {
            GameObject host = new("Fiskespelet", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            Stretch(host.GetComponent<RectTransform>());
            FishingGameController controller =
                host.AddComponent<FishingGameController>();
            controller.font = uiFont;
            controller.onBack = returnAction;
            controller.Build();
            controller.Hide();
            return controller;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            sceneRoot.SetActive(true);
            fishBookPanel.SetActive(false);
            fishShopPanel.SetActive(false);
            catchPopup.SetActive(false);
            locationCompletePopup.SetActive(false);
            hintPanel.SetActive(false);
            selectedFish = null;
            pendingLocationCompletion = false;
            if (HasCommandLineFlag("-arisFishingLurePreview"))
            {
                FishingLureCollection.TryBuyOrSelect(
                    FishingLureCollection.All[0]
                );
            }
            ResetVisuals();
            stateMachine.Reset();
            RefreshProgress();
            RefreshWormCount();
            RefreshFishBook();
            if (
                HasCommandLineFlag("-arisFishingAutoTest")
                || HasCommandLineFlag("-arisFishingShop")
                || HasCommandLineFlag("-arisFishingRodShop")
                || HasCommandLineFlag("-arisFishingRodLastPage")
                || HasCommandLineFlag("-arisFishingLureShop")
                || HasCommandLineFlag("-arisFishingLureSecondPage")
                || HasCommandLineFlag("-arisFishingLurePreview")
                || !string.IsNullOrEmpty(PreviewPathFromCommandLine())
            )
            {
                ApplyLocation();
            }
            else
            {
                ShowLocationSelector();
            }
            if (
                HasCommandLineFlag("-arisFishingShop")
                || HasCommandLineFlag("-arisFishingRodShop")
                || HasCommandLineFlag("-arisFishingRodLastPage")
                || HasCommandLineFlag("-arisFishingLureShop")
                || HasCommandLineFlag("-arisFishingLureSecondPage")
            )
            {
                OpenFishShop();
                if (
                    HasCommandLineFlag("-arisFishingRodShop")
                    || HasCommandLineFlag("-arisFishingRodLastPage")
                )
                {
                    if (HasCommandLineFlag("-arisFishingRodLastPage"))
                    {
                        rodShopPage = Mathf.Max(
                            0,
                            Mathf.CeilToInt(
                                FishingRodCollection.All.Count / 3f
                            ) - 1
                        );
                    }
                    ShowFishShopTab(1);
                }
                else if (
                    HasCommandLineFlag("-arisFishingLureShop")
                    || HasCommandLineFlag("-arisFishingLureSecondPage")
                )
                {
                    if (HasCommandLineFlag("-arisFishingLureSecondPage"))
                    {
                        lureShopPage = 1;
                    }
                    ShowFishShopTab(3);
                }
            }
            SetRigEditorVisible(HasCommandLineFlag("-arisFishingRigEditor"));
            if (
                AppPreferences.SoundEnabled
                && introAudioSource != null
                && introAudioSource.clip != null
            )
            {
                introAudioSource.Stop();
                introAudioSource.Play();
            }
            string previewPath = PreviewPathFromCommandLine();
            if (!string.IsNullOrEmpty(previewPath))
            {
                StartCoroutine(CapturePreview(previewPath));
            }
            if (HasCommandLineFlag("-arisFishingAutoTest"))
            {
                StartCoroutine(AutoVerifyRoutine());
            }
        }

        public void Hide()
        {
            StopAllCoroutines();
            flowRoutine = null;
            if (introAudioSource != null)
            {
                introAudioSource.Stop();
            }
            if (gameObject.activeSelf)
            {
                stateMachine.Reset();
            }
            gameObject.SetActive(false);
        }

        public void PressPrimaryButton()
        {
            switch (stateMachine.Current)
            {
                case FishingState.Idle:
                    BeginCast();
                    break;
                case FishingState.WaitingForBite:
                    ShowEarlyPressHint();
                    break;
                case FishingState.FishBiting:
                    BeginReel();
                    break;
            }
        }

        private void Build()
        {
            rigLayout = LoadRigLayout();
            bobberWaterPosition = rigLayout.bobberWaterPosition;
            definitions = FishCatalog.Load();
            random = CreateRandomProvider();
            selection = new FishSelectionService(definitions, random);
            locationSelection = new LocationFishSelectionService(definitions, random);
            collection = new FishCollectionService(
                new FishingSaveService(new PlayerPrefsStore())
            );

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            introAudioSource = gameObject.AddComponent<AudioSource>();
            introAudioSource.playOnAwake = false;
            introAudioSource.loop = false;
            introAudioSource.spatialBlend = 0f;
            introAudioSource.volume = 0.9f;
            introAudioSource.clip = Resources.Load<AudioClip>(
                "Audio/Fishing/arisfiske"
            );

            sceneRoot = new GameObject("Fiskescen", typeof(RectTransform));
            sceneRoot.transform.SetParent(transform, false);
            Stretch(sceneRoot.GetComponent<RectTransform>());

            backgroundImage = CreateImage(
                "Fiskebakgrund",
                sceneRoot.transform,
                RuntimeArt.LoadSprite("Art/Fishing/fishing_background_rigged")
            );
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = false;
            Stretch(backgroundImage.rectTransform);

            GameObject waterObject = new("Vattenområde", typeof(RectTransform));
            waterObject.transform.SetParent(sceneRoot.transform, false);
            waterArea = waterObject.GetComponent<RectTransform>();
            Stretch(waterArea);

            BuildSwimmingFish();
            BuildCharacterRig();
            BuildFishingRig();

            GameObject safeObject = new("Safe area", typeof(RectTransform));
            safeObject.transform.SetParent(sceneRoot.transform, false);
            safeArea = safeObject.GetComponent<RectTransform>();
            Stretch(safeArea);
            safeObject.AddComponent<SafeAreaFitter>();

            BuildTopControls();
            BuildMainControl();
            BuildHint();
            BuildFishBook();
            BuildFishShop();
            BuildCatchPopup();
            BuildLocationCompletePopup();
            BuildEffectPools();
            BuildRigEditor();
            BuildLocationSelector();

            stateMachine.Changed += HandleStateChanged;
            HandleStateChanged(FishingState.Idle, FishingState.Idle);
        }

        private static IRandomProvider CreateRandomProvider()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                const string prefix = "-arisFishingSeed=";
                if (
                    args[index].StartsWith(prefix, StringComparison.Ordinal)
                    && int.TryParse(args[index].Substring(prefix.Length), out int seed)
                )
                {
                    return new SeededRandomProvider(seed);
                }
            }
            return new SeededRandomProvider(Environment.TickCount);
        }

        private static string PreviewPathFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            const string prefix = "-arisFishingPreviewPath=";
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index].StartsWith(prefix, StringComparison.Ordinal))
                {
                    return args[index].Substring(prefix.Length);
                }
            }
            return "";
        }

        private static string FlowPreviewPathFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            const string prefix = "-arisFishingFlowPreviewPath=";
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index].StartsWith(prefix, StringComparison.Ordinal))
                {
                    return args[index].Substring(prefix.Length);
                }
            }
            return "";
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (string.Equals(args[index], flag, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerator CapturePreview(string path)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.4f);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(0.5f);
            Debug.Log("FISHING_PREVIEW_SAVED: " + path);
        }

        private IEnumerator AutoVerifyRoutine()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            PressPrimaryButton();
            float deadline = Time.realtimeSinceStartup + 2f;
            while (
                stateMachine.Current != FishingState.WaitingForBite
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            if (stateMachine.Current != FishingState.WaitingForBite)
            {
                Debug.LogError("ARIS_FISHING_AUTOTEST: Casting/Waiting failed.");
                yield break;
            }

            string flowPreviewPath = FlowPreviewPathFromCommandLine();
            if (!string.IsNullOrEmpty(flowPreviewPath))
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(flowPreviewPath);
                yield return new WaitForSecondsRealtime(0.6f);
                Debug.Log("FISHING_FLOW_PREVIEW_SAVED: " + flowPreviewPath);
            }

            PressPrimaryButton();
            deadline = Time.realtimeSinceStartup + MaximumBiteDelay + 1f;
            while (
                stateMachine.Current != FishingState.FishBiting
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            if (stateMachine.Current != FishingState.FishBiting)
            {
                Debug.LogError("ARIS_FISHING_AUTOTEST: Bite failed.");
                yield break;
            }

            PressPrimaryButton();
            deadline = Time.realtimeSinceStartup + 2f;
            while (
                stateMachine.Current != FishingState.CatchReveal
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            if (
                stateMachine.Current != FishingState.CatchReveal
                || selectedFish == null
                || !collection.IsDiscovered(selectedFish.StableId)
            )
            {
                Debug.LogError("ARIS_FISHING_AUTOTEST: Catch/save failed.");
                yield break;
            }

            ContinueAfterCatch();
            deadline = Time.realtimeSinceStartup + 1.5f;
            while (
                stateMachine.Current != FishingState.Idle
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            OpenFishBook();
            yield return new WaitForSecondsRealtime(0.2f);
            bool bookOpened = fishBookPanel.activeSelf;
            CloseFishBook();
            if (stateMachine.Current == FishingState.Idle && bookOpened)
            {
                Debug.Log(
                    "ARIS_FISHING_AUTOTEST: PASS cast wait early-press bite reel reveal save book return."
                );
            }
            else
            {
                Debug.LogError("ARIS_FISHING_AUTOTEST: Return/book failed.");
            }
        }

        private void BuildSwimmingFish()
        {
            if (definitions.Count == 0)
            {
                Debug.LogError("Fiskespelet saknar FishDefinition-assets.");
                return;
            }

            for (int index = 0; index < 6; index++)
            {
                FishDefinition definition = definitions[index % definitions.Count];
                Image fish = CreateImage(
                    "Bakgrundsfisk " + (index + 1),
                    waterArea,
                    definition.Sprite
                );
                fish.type = Image.Type.Simple;
                fish.preserveAspect = true;
                fish.color = new Color(1f, 1f, 1f, 0.68f);
                SetRect(
                    fish.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(190f, 135f)
                );
                SwimmingFishView swimmer =
                    fish.gameObject.AddComponent<SwimmingFishView>();
                swimmer.Initialize(
                    waterArea,
                    definition.Sprite,
                    definition.SwimSpeed,
                    -760f + index * 300f,
                    -245f - index % 3 * 90f,
                    index % 2 == 0
                );
                swimmers.Add(swimmer);
            }
        }

        private void BuildFishingRig()
        {
            rodImage = CreateImage(
                "Fiskespö",
                sceneRoot.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "FishingRod",
                    RuntimeArt.Hex("#4E2B18"),
                    RuntimeArt.Hex("#9A5A25"),
                    560,
                    25,
                    12,
                    5
                )
            );
            rod = rodImage.rectTransform;
            SetRect(
                rod,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                rigLayout.rodPosition,
                new Vector2(rigLayout.rodLength, 25f)
            );
            rod.pivot = new Vector2(0f, 0.5f);
            rod.localRotation = Quaternion.Euler(0f, 0f, rigLayout.rodAngle);
            bobberRestPosition = RodTipPosition() + new Vector2(0f, -125f);

            classicRodHandle = CreateImage(
                "Spöhandtag",
                rod,
                RuntimeArt.RoundedRectangleSprite(
                    "FishingRodHandle",
                    RuntimeArt.Hex("#3A251A"),
                    RuntimeArt.Hex("#E0B15B"),
                    120,
                    38,
                    16,
                    4
                )
            );
            SetRect(
                classicRodHandle.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(40f, 0f),
                new Vector2(120f, 38f)
            );
            BuildAlternativeRodDesign();
            ApplySelectedRodDesign();

            Image line = CreateImage(
                "Fiskelina",
                sceneRoot.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "FishingLine",
                    RuntimeArt.Hex("#5B93B2"),
                    new Color(1f, 1f, 1f, 0.96f),
                    600,
                    7,
                    3,
                    1
                )
            );
            fishingLine = line.rectTransform;

            Image bobberImage = CreateImage(
                "Flöte",
                sceneRoot.transform,
                RuntimeArt.CircleSprite(
                    "FishingBobber",
                    RuntimeArt.Hex("#8E281E"),
                    RuntimeArt.Hex("#F04432"),
                    Color.white,
                    128
                )
            );
            bobber = bobberImage.rectTransform;
            SetRect(
                bobber,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                bobberRestPosition,
                new Vector2(65f, 65f)
            );

            Image cap = CreateImage(
                "Flötestopp",
                bobber,
                RuntimeArt.CircleSprite(
                    "FishingBobberCap",
                    RuntimeArt.Hex("#D8D8D8"),
                    Color.white,
                    Color.white,
                    64
                )
            );
            SetRect(
                cap.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 18f),
                new Vector2(40f, 28f)
            );

            for (
                int index = 0;
                index < FishingLureCollection.All.Count;
                index++
            )
            {
                RectTransform lureVariant = CreateLureVisual(
                    sceneRoot.transform,
                    FishingLureCollection.All[index],
                    new Vector2(135f, 82f)
                );
                lureVariant.gameObject.name =
                    "Aktivt drag " + FishingLureCollection.All[index].Id;
                lureVariant.gameObject.SetActive(false);
                activeLureVariants.Add(lureVariant);
            }
            activeLure = activeLureVariants[0];

            catchFish = CreateImage(
                "Fångstfisk",
                sceneRoot.transform,
                null
            ).rectTransform;
            SetRect(
                catchFish,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                bobberWaterPosition,
                new Vector2(280f, 210f)
            );
            catchFish.GetComponent<Image>().preserveAspect = true;
            catchFish.gameObject.SetActive(false);

            splash = CreateImage(
                "Vattenstänk",
                sceneRoot.transform,
                RuntimeArt.CircleSprite(
                    "FishingSplash",
                    RuntimeArt.Hex("#B9F2FF"),
                    RuntimeArt.Hex("#E8FCFF"),
                    Color.white,
                    128
                )
            );
            SetRect(
                splash.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                bobberWaterPosition + new Vector2(0f, -20f),
                new Vector2(150f, 42f)
            );
            splash.gameObject.SetActive(false);

            for (int index = 0; index < 3; index++)
            {
                Text ring = CreateText(
                    "Vattenring " + (index + 1),
                    waterArea,
                    "○",
                    110,
                    new Color(0.86f, 0.98f, 1f, 0.9f)
                );
                SetRect(
                    ring.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    bobberWaterPosition + new Vector2(0f, -12f),
                    new Vector2(180f, 80f)
                );
                ring.gameObject.SetActive(false);
                waterRings.Add(ring);
            }
            BuildCharacterArms();
            ApplyActiveLureDesign();
            RefreshTackleVisual();
            UpdateFishingLine();
        }

        private void BuildAlternativeRodDesign()
        {
            specialRodDecoration = new GameObject(
                "Specialspödetaljer",
                typeof(RectTransform)
            );
            specialRodDecoration.transform.SetParent(rod, false);
            Stretch(specialRodDecoration.GetComponent<RectTransform>());

            specialRodReel = CreateImage(
                "Specialrulle",
                specialRodDecoration.transform,
                RuntimeArt.CircleSprite(
                    "SpecialFishingReel",
                    RuntimeArt.Hex("#39216A"),
                    RuntimeArt.Hex("#55DDF2"),
                    RuntimeArt.Hex("#FFF3AD"),
                    128
                )
            );
            SetRect(
                specialRodReel.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(118f, -48f),
                new Vector2(78f, 78f)
            );

            specialRodReelHub = CreateImage(
                "Rullnav",
                specialRodReel.transform,
                RuntimeArt.CircleSprite(
                    "SpecialFishingReelHub",
                    RuntimeArt.Hex("#6B3BA8"),
                    RuntimeArt.Hex("#FFD84D"),
                    Color.white,
                    64
                )
            );
            SetRect(
                specialRodReelHub.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(34f, 34f)
            );

            for (int index = 0; index < 4; index++)
            {
                Image band = CreateImage(
                    "Spöband " + (index + 1),
                    specialRodDecoration.transform,
                    RuntimeArt.RoundedRectangleSprite(
                        "SpecialRodBand_" + index,
                        RuntimeArt.Hex("#6B3BA8"),
                        index % 2 == 0
                            ? RuntimeArt.Hex("#FFD84D")
                            : RuntimeArt.Hex("#FF71C8"),
                        22,
                        42,
                        8,
                        3
                    )
                );
                SetRect(
                    band.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(205f + index * 82f, 0f),
                    new Vector2(18f, 39f)
                );
                specialRodBands.Add(band);
            }

            specialRodSymbol = CreateText(
                "Spösymbol",
                specialRodDecoration.transform,
                "★",
                42,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                specialRodSymbol.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(rigLayout.rodLength - 48f, 0f),
                new Vector2(65f, 65f)
            );
            AddOutline(specialRodSymbol, RuntimeArt.Hex("#6B3BA8"), 2f);
        }

        private void ApplySelectedRodDesign()
        {
            FishingRodDefinition selected = FishingRodCollection.Selected;
            bool specialSelected = selected.RareChanceBonus > 0f;
            rodImage.sprite = RuntimeArt.RoundedRectangleSprite(
                "SelectedFishingRod_" + selected.Id,
                RuntimeArt.Hex(selected.BorderHex),
                RuntimeArt.Hex(selected.ShaftHex),
                560,
                25,
                12,
                5
            );
            classicRodHandle.sprite = RuntimeArt.RoundedRectangleSprite(
                "FishingRodHandle_" + selected.Id,
                RuntimeArt.Hex(selected.BorderHex),
                RuntimeArt.Hex(selected.HandleHex),
                120,
                38,
                16,
                4
            );
            specialRodDecoration.SetActive(specialSelected);
            if (!specialSelected)
            {
                return;
            }
            specialRodReel.sprite = RuntimeArt.CircleSprite(
                "FishingReel_" + selected.Id,
                RuntimeArt.Hex(selected.BorderHex),
                RuntimeArt.Hex(selected.ShaftHex),
                RuntimeArt.Hex(selected.AccentHex),
                128
            );
            specialRodReelHub.sprite = RuntimeArt.CircleSprite(
                "FishingReelHub_" + selected.Id,
                RuntimeArt.Hex(selected.BorderHex),
                RuntimeArt.Hex(selected.HandleHex),
                Color.white,
                64
            );
            for (int index = 0; index < specialRodBands.Count; index++)
            {
                specialRodBands[index].sprite =
                    RuntimeArt.RoundedRectangleSprite(
                        "FishingRodBand_" + selected.Id + "_" + index,
                        RuntimeArt.Hex(selected.BorderHex),
                        index % 2 == 0
                            ? RuntimeArt.Hex(selected.AccentHex)
                            : RuntimeArt.Hex(selected.HandleHex),
                        22,
                        42,
                        8,
                        3
                    );
            }
            specialRodSymbol.text = selected.Symbol;
            specialRodSymbol.color = RuntimeArt.Hex(selected.AccentHex);
        }

        private void BuildCharacterRig()
        {
            Image bodyImage = CreateImage(
                "Riggad bäverkropp",
                sceneRoot.transform,
                RuntimeArt.LoadSprite("Art/Fishing/Character/body")
            );
            bodyImage.preserveAspect = true;
            characterBody = bodyImage.rectTransform;
            SetRect(
                characterBody,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-575f, -72f),
                new Vector2(455f, 258f)
            );

            headIdleSprite = RuntimeArt.LoadSprite(
                "Art/Fishing/Character/head_idle"
            );
            headBlinkSprite = RuntimeArt.LoadSprite(
                "Art/Fishing/Character/head_blink"
            );
            headHappySprite = RuntimeArt.LoadSprite(
                "Art/Fishing/Character/head_happy"
            );
            headReelSprite = RuntimeArt.LoadSprite(
                "Art/Fishing/Character/head_reel"
            );
            characterHeadImage = CreateImage(
                "Riggat bäverhuvud",
                sceneRoot.transform,
                headIdleSprite
            );
            characterHeadImage.preserveAspect = true;
            characterHead = characterHeadImage.rectTransform;
            SetRect(
                characterHead,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-585f, 142f),
                new Vector2(365f, 310f)
            );
            nextBlinkAt = Time.unscaledTime + 1.5f;
        }

        private void BuildCharacterArms()
        {
            Image restingArmImage = CreateImage(
                "Vilande bäverarm",
                sceneRoot.transform,
                RuntimeArt.LoadSprite(
                    "Art/Fishing/Character/resting_arm",
                    100f,
                    new Vector2(0f, 0.5f)
                )
            );
            restingArmImage.preserveAspect = false;
            restingArm = restingArmImage.rectTransform;
            SetRect(
                restingArm,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                rigLayout.restingShoulder,
                new Vector2(108f, 48f)
            );
            restingArm.pivot = new Vector2(0f, 0.5f);

            Image frontArmImage = CreateImage(
                "Riggad fiskearm",
                sceneRoot.transform,
                RuntimeArt.LoadSprite(
                    "Art/Fishing/Character/holding_arm",
                    100f,
                    new Vector2(0f, 0.5f)
                )
            );
            frontArmImage.preserveAspect = false;
            frontArm = frontArmImage.rectTransform;
            SetRect(
                frontArm,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                rigLayout.fishingShoulder,
                new Vector2(98f, 62f)
            );
            frontArm.pivot = new Vector2(0f, 0.5f);
            UpdateCharacterAnimation();
        }

        private FishingRigLayout LoadRigLayout()
        {
            FishingRigLayout layout = FishingRigLayout.Default();
            if (!HasCommandLineFlag("-arisFishingRigEditor"))
            {
                return layout;
            }

            string path = RigLayoutPath();
            try
            {
                if (File.Exists(path))
                {
                    FishingRigLayout loaded = JsonUtility.FromJson<FishingRigLayout>(
                        File.ReadAllText(path)
                    );
                    if (loaded != null && loaded.rodLength > 100f)
                    {
                        return loaded;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Kunde inte läsa fiskeriggen, använder standardvärden: "
                        + exception.Message
                );
            }
            return layout;
        }

        private static string RigLayoutPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            const string prefix = "-arisFishingRigOutputPath=";
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index].StartsWith(prefix, StringComparison.Ordinal))
                {
                    return args[index].Substring(prefix.Length);
                }
            }
            return Path.Combine(
                Application.persistentDataPath,
                "fishing_rig_layout.json"
            );
        }

        private void BuildRigEditor()
        {
            Image panel = CreatePanel(
                "Fiskerigg-editor",
                sceneRoot.transform,
                new Vector2(730f, 0f),
                new Vector2(430f, 850f),
                new Color(0.08f, 0.12f, 0.2f, 0.93f)
            );
            rigEditorPanel = panel.gameObject;

            Text title = CreateText(
                "Editorrubrik",
                panel.transform,
                "FISKERIGG-EDITOR",
                30,
                Color.white
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 360f),
                new Vector2(390f, 60f)
            );

            rigEditorValues = CreateText(
                "Rigg-värden",
                panel.transform,
                "",
                19,
                Color.white
            );
            rigEditorValues.alignment = TextAnchor.UpperLeft;
            SetRect(
                rigEditorValues.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 190f),
                new Vector2(370f, 250f)
            );

            Button angleMinus = CreateButton(
                panel.transform,
                "VINKEL −",
                new Vector2(-105f, 48f),
                new Vector2(185f, 65f),
                RuntimeArt.Hex("#4378C8"),
                20
            );
            angleMinus.onClick.AddListener(() => AdjustRodAngle(-1f));
            Button anglePlus = CreateButton(
                panel.transform,
                "VINKEL +",
                new Vector2(105f, 48f),
                new Vector2(185f, 65f),
                RuntimeArt.Hex("#4378C8"),
                20
            );
            anglePlus.onClick.AddListener(() => AdjustRodAngle(1f));

            Button lengthMinus = CreateButton(
                panel.transform,
                "KORTARE",
                new Vector2(-105f, -32f),
                new Vector2(185f, 65f),
                RuntimeArt.Hex("#7B58C7"),
                20
            );
            lengthMinus.onClick.AddListener(() => AdjustRodLength(-10f));
            Button lengthPlus = CreateButton(
                panel.transform,
                "LÄNGRE",
                new Vector2(105f, -32f),
                new Vector2(185f, 65f),
                RuntimeArt.Hex("#7B58C7"),
                20
            );
            lengthPlus.onClick.AddListener(() => AdjustRodLength(10f));

            Button save = CreateButton(
                panel.transform,
                "SPARA JSON",
                new Vector2(0f, -135f),
                new Vector2(390f, 78f),
                RuntimeArt.Hex("#2EAE62"),
                24
            );
            save.onClick.AddListener(SaveRigLayout);
            Button reset = CreateButton(
                panel.transform,
                "ÅTERSTÄLL",
                new Vector2(0f, -230f),
                new Vector2(390f, 70f),
                RuntimeArt.Hex("#D17B36"),
                22
            );
            reset.onClick.AddListener(ResetRigEditor);

            Text help = CreateText(
                "Editorhjälp",
                panel.transform,
                "Dra de färgade punkterna.\nSpara när allt sitter rätt.",
                20,
                RuntimeArt.Hex("#FFF3C4")
            );
            SetRect(
                help.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -335f),
                new Vector2(390f, 90f)
            );

            rodBaseHandle = CreateRigHandle(
                "SPÖ",
                RuntimeArt.Hex("#FF8B31"),
                position =>
                {
                    rigLayout.rodPosition = position;
                    ApplyRigEditorLayout();
                }
            );
            fishingShoulderHandle = CreateRigHandle(
                "AXEL",
                RuntimeArt.Hex("#FFD43B"),
                position =>
                {
                    rigLayout.fishingShoulder = position;
                    ApplyRigEditorLayout();
                }
            );
            fishingGripHandle = CreateRigHandle(
                "HAND",
                RuntimeArt.Hex("#58E183"),
                position =>
                {
                    SetGripFromWorldPosition(position);
                    ApplyRigEditorLayout();
                }
            );
            restingShoulderHandle = CreateRigHandle(
                "V-AXEL",
                RuntimeArt.Hex("#49C7F2"),
                position =>
                {
                    rigLayout.restingShoulder = position;
                    ApplyRigEditorLayout();
                }
            );
            restingHandHandle = CreateRigHandle(
                "V-HAND",
                RuntimeArt.Hex("#B087FF"),
                position =>
                {
                    rigLayout.restingHand = position;
                    ApplyRigEditorLayout();
                }
            );
            waterHandle = CreateRigHandle(
                "VATTEN",
                RuntimeArt.Hex("#24D8E8"),
                position =>
                {
                    rigLayout.bobberWaterPosition = position;
                    ApplyRigEditorLayout();
                }
            );

            rigEditorPanel.SetActive(false);
            SetRigHandlesVisible(false);
        }

        private RectTransform CreateRigHandle(
            string label,
            Color color,
            Action<Vector2> moved
        )
        {
            Image handleImage = CreateImage(
                label + "-handtag",
                sceneRoot.transform,
                RuntimeArt.CircleSprite(
                    "FishingRigHandle_" + label,
                    Color.white,
                    color,
                    Color.white,
                    96
                )
            );
            handleImage.raycastTarget = true;
            SetRect(
                handleImage.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(30f, 30f)
            );
            FishingRigDragHandle drag =
                handleImage.gameObject.AddComponent<FishingRigDragHandle>();
            drag.Initialize(sceneRoot.GetComponent<RectTransform>(), moved);
            return handleImage.rectTransform;
        }

        private void SetRigEditorVisible(bool visible)
        {
            if (rigEditorPanel == null)
            {
                return;
            }
            rigEditorPanel.SetActive(visible);
            SetRigHandlesVisible(visible);
            if (visible)
            {
                mainButton.interactable = false;
                ApplyRigEditorLayout();
            }
        }

        private void SetRigHandlesVisible(bool visible)
        {
            RectTransform[] handles =
            {
                rodBaseHandle,
                fishingShoulderHandle,
                fishingGripHandle,
                restingShoulderHandle,
                restingHandHandle,
                waterHandle
            };
            for (int index = 0; index < handles.Length; index++)
            {
                if (handles[index] != null)
                {
                    handles[index].gameObject.SetActive(visible);
                }
            }
        }

        private void ApplyRigEditorLayout()
        {
            bobberWaterPosition = rigLayout.bobberWaterPosition;
            rod.anchoredPosition = rigLayout.rodPosition;
            rod.sizeDelta = new Vector2(rigLayout.rodLength, rod.sizeDelta.y);
            rod.localRotation = Quaternion.Euler(
                0f,
                0f,
                rigLayout.rodAngle
            );
            bobberRestPosition = RodTipPosition() + new Vector2(0f, -125f);
            bobber.anchoredPosition = bobberRestPosition;
            UpdateCharacterAnimation();
            UpdateFishingLine();

            rodBaseHandle.anchoredPosition = rigLayout.rodPosition;
            fishingShoulderHandle.anchoredPosition =
                rigLayout.fishingShoulder;
            fishingGripHandle.anchoredPosition = FishingGripPosition();
            restingShoulderHandle.anchoredPosition =
                rigLayout.restingShoulder;
            restingHandHandle.anchoredPosition = rigLayout.restingHand;
            waterHandle.anchoredPosition = rigLayout.bobberWaterPosition;

            if (rigEditorValues != null)
            {
                rigEditorValues.text =
                    "Orange: spöbas\n"
                    + "Gul: fiskearmens axel\n"
                    + "Grön: hand på spö\n"
                    + "Blå/lila: vilarm\n"
                    + "Turkos: flötets vattenpunkt\n\n"
                    + $"Spö: {rigLayout.rodPosition.x:0}, {rigLayout.rodPosition.y:0}\n"
                    + $"Vinkel: {rigLayout.rodAngle:0.0}°  Längd: {rigLayout.rodLength:0}\n"
                    + $"Grepp: {rigLayout.gripAlongRod:0}, {rigLayout.gripNormalOffset:0}";
            }
        }

        private Vector2 FishingGripPosition()
        {
            float angle = rigLayout.rodAngle * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 normal = new(-direction.y, direction.x);
            return rigLayout.rodPosition
                + direction * rigLayout.gripAlongRod
                + normal * rigLayout.gripNormalOffset;
        }

        private void SetGripFromWorldPosition(Vector2 position)
        {
            float angle = rigLayout.rodAngle * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 normal = new(-direction.y, direction.x);
            Vector2 delta = position - rigLayout.rodPosition;
            rigLayout.gripAlongRod = Vector2.Dot(delta, direction);
            rigLayout.gripNormalOffset = Vector2.Dot(delta, normal);
        }

        private void AdjustRodAngle(float delta)
        {
            rigLayout.rodAngle = Mathf.Clamp(
                rigLayout.rodAngle + delta,
                -15f,
                70f
            );
            ApplyRigEditorLayout();
        }

        private void AdjustRodLength(float delta)
        {
            rigLayout.rodLength = Mathf.Clamp(
                rigLayout.rodLength + delta,
                320f,
                800f
            );
            ApplyRigEditorLayout();
        }

        private void ResetRigEditor()
        {
            rigLayout = FishingRigLayout.Default();
            ApplyRigEditorLayout();
        }

        private void SaveRigLayout()
        {
            string path = RigLayoutPath();
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(
                    path,
                    JsonUtility.ToJson(rigLayout, true)
                );
                rigEditorValues.text =
                    "SPARAD!\n\n" + path + "\n\n" + rigEditorValues.text;
                Debug.Log("FISHING_RIG_SAVED: " + path);
            }
            catch (Exception exception)
            {
                rigEditorValues.text =
                    "KUNDE INTE SPARA:\n" + exception.Message;
                Debug.LogError(
                    "Kunde inte spara fiskeriggen: " + exception
                );
            }
        }

        private void BuildTopControls()
        {
            gameplayTopHud = new GameObject(
                "Gemensam övre fiske-HUD",
                typeof(RectTransform)
            );
            gameplayTopHud.transform.SetParent(safeArea, false);
            SetRect(
                gameplayTopHud.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -62f),
                new Vector2(1740f, 96f)
            );
            Transform topHud = gameplayTopHud.transform;

            Button back = CreateButton(
                topHud,
                "←",
                new Vector2(-775f, 0f),
                new Vector2(120f, 88f),
                RuntimeArt.Hex("#7A5AA6"),
                58
            );
            back.onClick.AddListener(ExitFishing);

            Button book = CreateButton(
                topHud,
                "FISKBOK",
                new Vector2(-610f, 0f),
                new Vector2(190f, 88f),
                RuntimeArt.Hex("#5A8CE8"),
                30
            );
            book.onClick.AddListener(OpenFishBook);

            Button shop = CreateButton(
                topHud,
                "BUTIK",
                new Vector2(-415f, 0f),
                new Vector2(170f, 88f),
                RuntimeArt.Hex("#E85A96"),
                30
            );
            shop.onClick.AddListener(OpenFishShop);

            Image progress = CreatePanel(
                "Upptäckta fiskar",
                topHud,
                new Vector2(-180f, 0f),
                new Vector2(260f, 82f),
                RuntimeArt.Hex("#FFF3C4")
            );
            progressText = CreateText(
                "Fiskframsteg",
                progress.transform,
                "FISKAR 0 / 6",
                29,
                RuntimeArt.Hex("#4A3424")
            );
            Stretch(progressText.rectTransform);

            Image wormIcon = CreateImage(
                "Maskikon liten",
                topHud,
                RuntimeArt.LoadSprite("Art/Fishing/UI/worm_bait")
            );
            wormIcon.preserveAspect = true;
            SetRect(
                wormIcon.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(5f, 0f),
                new Vector2(68f, 68f)
            );
            wormCountText = CreateText(
                "Maskantal",
                topHud,
                "× 0",
                35,
                Color.white
            );
            SetRect(
                wormCountText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(85f, 0f),
                new Vector2(105f, 70f)
            );
            AddOutline(wormCountText, RuntimeArt.Hex("#4A3424"), 4f);

            Button sound = CreateButton(
                topHud,
                "",
                new Vector2(680f, 0f),
                new Vector2(190f, 88f),
                RuntimeArt.Hex("#F4B928"),
                32
            );
            soundButtonText = CreateText(
                "Ljudstatus",
                sound.transform,
                AppPreferences.SoundEnabled ? "LJUD PÅ" : "LJUD AV",
                27,
                Color.white
            );
            Stretch(soundButtonText.rectTransform);
            AddOutline(soundButtonText, RuntimeArt.Hex("#4A3424"), 3f);
            sound.onClick.AddListener(ToggleSound);
            RefreshWormCount();
        }

        private void BuildMainControl()
        {
            mainControlRoot = new GameObject(
                "Stor kontrolltouchyta",
                typeof(RectTransform)
            );
            mainControlRoot.transform.SetParent(safeArea, false);
            SetRect(
                mainControlRoot.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 90f),
                new Vector2(820f, 145f)
            );

            mainButton = CreateButton(
                mainControlRoot.transform,
                "",
                Vector2.zero,
                new Vector2(700f, 110f),
                RuntimeArt.Hex("#65C832"),
                58
            );
            mainButtonImage = mainButton.image;
            mainButtonText = CreateText(
                "Huvudknappstext",
                mainButton.transform,
                "KASTA",
                64,
                Color.white
            );
            Stretch(mainButtonText.rectTransform);
            AddOutline(mainButtonText, RuntimeArt.Hex("#285B17"), 4f);
            mainButton.onClick.AddListener(PressPrimaryButton);
        }

        private void BuildHint()
        {
            hintPanel = CreatePanel(
                "Vänligt tips",
                safeArea,
                new Vector2(0f, -260f),
                new Vector2(780f, 90f),
                RuntimeArt.Hex("#FFF3C4")
            ).gameObject;
            hintText = CreateText(
                "Tipstext",
                hintPanel.transform,
                "",
                31,
                RuntimeArt.Hex("#4A3424")
            );
            Stretch(hintText.rectTransform);
            hintPanel.SetActive(false);
        }

        private void BuildFishBook()
        {
            Image shade = CreateImage("Fiskbokstoning", safeArea, null);
            shade.color = new Color(0.04f, 0.12f, 0.24f, 0.9f);
            Stretch(shade.rectTransform);
            fishBookPanel = shade.gameObject;

            Image book = CreatePanel(
                "Fiskbok",
                shade.transform,
                Vector2.zero,
                new Vector2(1260f, 800f),
                RuntimeArt.Hex("#F7E4B2")
            );
            Text title = CreateText(
                "Fiskbokstitel",
                book.transform,
                "MIN FISKBOK",
                58,
                RuntimeArt.Hex("#4A3424")
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -55f),
                new Vector2(620f, 76f)
            );

            Button close = CreateButton(
                book.transform,
                "←",
                new Vector2(-535f, 330f),
                new Vector2(115f, 76f),
                RuntimeArt.Hex("#F4B928"),
                50
            );
            close.onClick.AddListener(CloseFishBook);

            fishBookPageText = CreateText(
                "Fiskbokssida",
                book.transform,
                "SIDA 1 / 6",
                25,
                RuntimeArt.Hex("#4A3424")
            );
            SetRect(
                fishBookPageText.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-155f, -58f),
                new Vector2(240f, 55f)
            );

            Button previous = CreateButton(
                book.transform,
                "‹",
                new Vector2(-570f, -15f),
                new Vector2(76f, 145f),
                RuntimeArt.Hex("#8B56D9"),
                68
            );
            previous.onClick.AddListener(() => ChangeFishBookPage(-1));
            Button next = CreateButton(
                book.transform,
                "›",
                new Vector2(570f, -15f),
                new Vector2(76f, 145f),
                RuntimeArt.Hex("#8B56D9"),
                68
            );
            next.onClick.AddListener(() => ChangeFishBookPage(1));

            float[] xs = { -375f, 0f, 375f };
            float[] ys = { 145f, -180f };
            for (int index = 0; index < 6; index++)
            {
                Image card = CreatePanel(
                    "Fiskkort " + (index + 1),
                    book.transform,
                    new Vector2(xs[index % 3], ys[index / 3]),
                    new Vector2(315f, 270f),
                    RuntimeArt.Hex("#FFF8E8")
                );
                Image picture = CreateImage("Fiskbild", card.transform, null);
                picture.type = Image.Type.Simple;
                picture.preserveAspect = true;
                SetRect(
                    picture.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 55f),
                    new Vector2(205f, 140f)
                );
                Text question = CreateText(
                    "Frågetecken",
                    card.transform,
                    "?",
                    80,
                    Color.white
                );
                SetRect(
                    question.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 55f),
                    new Vector2(160f, 130f)
                );
                Text name = CreateText(
                    "Fisknamn",
                    card.transform,
                    "???",
                    27,
                    RuntimeArt.Hex("#4A3424")
                );
                SetRect(
                    name.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -49f),
                    new Vector2(285f, 42f)
                );
                Text details = CreateText(
                    "Fiskdetaljer",
                    card.transform,
                    "HITTA FISKEN",
                    18,
                    RuntimeArt.Hex("#6B5B4D")
                );
                SetRect(
                    details.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -94f),
                    new Vector2(285f, 48f)
                );
                Image rarityBand = CreateImage(
                    "Raritet",
                    card.transform,
                    RuntimeArt.RoundedRectangleSprite(
                        "FishingRarityBand",
                        Color.clear,
                        RuntimeArt.Hex("#86C8FF"),
                        250,
                        18,
                        9,
                        0
                    )
                );
                SetRect(
                    rarityBand.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -123f),
                    new Vector2(210f, 15f)
                );
                fishBookCards.Add(
                    new FishBookCard
                    {
                        Picture = picture,
                        Question = question,
                        Name = name,
                        Details = details,
                        RarityBand = rarityBand
                    }
                );
            }
            fishBookPanel.SetActive(false);
        }

        private void BuildFishShop()
        {
            Image background = CreateImage(
                "Fiskebutik",
                safeArea,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            fishShopPanel = background.gameObject;
            SwipePageHandler shopSwipe =
                fishShopPanel.AddComponent<SwipePageHandler>();
            shopSwipe.Initialize(HandleFishShopSwipe);

            Image shade = CreateImage("Butikstoning", background.transform, null);
            shade.color = new Color(0.08f, 0.04f, 0.2f, 0.55f);
            Stretch(shade.rectTransform);

            Button close = CreateButton(
                background.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            close.onClick.AddListener(CloseFishShop);

            Text title = CreateText(
                "Butikstitel",
                background.transform,
                "FISKEBUTIK",
                64,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -62f),
                new Vector2(700f, 105f)
            );
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            Image wallet = CreatePanel(
                "Butiksmynt",
                background.transform,
                new Vector2(790f, 470f),
                new Vector2(285f, 86f),
                RuntimeArt.Hex("#FFF3AD")
            );
            Image coin = CreateImage(
                "Myntsymbol",
                wallet.transform,
                RuntimeArt.GoldCoinSprite()
            );
            coin.preserveAspect = true;
            SetRect(
                coin.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-91f, 0f),
                new Vector2(62f, 62f)
            );
            fishShopCoinText = CreateText(
                "Myntsaldo",
                wallet.transform,
                "0",
                36,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(
                fishShopCoinText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(38f, 0f),
                new Vector2(150f, 70f)
            );

            Button fishTab = CreateButton(
                background.transform,
                "SÄLJ FISK",
                new Vector2(-570f, 390f),
                new Vector2(285f, 72f),
                RuntimeArt.Hex("#E85A96"),
                25
            );
            fishTab.onClick.AddListener(() => ShowFishShopTab(0));
            Button rodTab = CreateButton(
                background.transform,
                "FISKESPÖN",
                new Vector2(-190f, 390f),
                new Vector2(285f, 72f),
                RuntimeArt.Hex("#5A8CE8"),
                25
            );
            rodTab.onClick.AddListener(() => ShowFishShopTab(1));
            Button baitTab = CreateButton(
                background.transform,
                "BETE",
                new Vector2(190f, 390f),
                new Vector2(285f, 72f),
                RuntimeArt.Hex("#65C832"),
                25
            );
            baitTab.onClick.AddListener(() => ShowFishShopTab(2));
            Button lureTab = CreateButton(
                background.transform,
                "FISKEDRAG",
                new Vector2(570f, 390f),
                new Vector2(285f, 72f),
                RuntimeArt.Hex("#F4B928"),
                25
            );
            lureTab.onClick.AddListener(() => ShowFishShopTab(3));

            fishShopInventoryRoot = new GameObject(
                "Säljbara fiskar",
                typeof(RectTransform)
            );
            fishShopInventoryRoot.transform.SetParent(background.transform, false);
            Stretch(fishShopInventoryRoot.GetComponent<RectTransform>());

            for (int slot = 0; slot < 3; slot++)
            {
                int selectedSlot = slot;
                fishShopCards.Add(
                    CreateFishShopCard(
                        fishShopInventoryRoot.transform,
                        new Vector2(-600f + slot * 600f, -35f),
                        () => SellFishFromShop(selectedSlot, false),
                        () => SellFishFromShop(selectedSlot, true)
                    )
                );
            }

            fishShopEmptyText = CreateText(
                "Tom fiskelåda",
                fishShopInventoryRoot.transform,
                "FISKELÅDAN ÄR TOM\nFÅNGA EN FISK OCH KOM TILLBAKA!",
                42,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                fishShopEmptyText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(1000f, 180f)
            );
            AddOutline(fishShopEmptyText, RuntimeArt.Hex("#40245F"), 4f);

            previousFishShopPageButton = CreateButton(
                fishShopInventoryRoot.transform,
                "‹",
                new Vector2(-895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousFishShopPageButton.onClick.AddListener(
                () => ChangeFishShopPage(-1)
            );
            nextFishShopPageButton = CreateButton(
                fishShopInventoryRoot.transform,
                "›",
                new Vector2(895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextFishShopPageButton.onClick.AddListener(
                () => ChangeFishShopPage(1)
            );
            fishShopPageText = CreateText(
                "Butikssida",
                fishShopInventoryRoot.transform,
                "",
                25,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                fishShopPageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -470f),
                new Vector2(300f, 45f)
            );

            fishingRodShopRoot = new GameObject(
                "Fiskespöbutik",
                typeof(RectTransform)
            );
            fishingRodShopRoot.transform.SetParent(background.transform, false);
            Stretch(fishingRodShopRoot.GetComponent<RectTransform>());
            for (int index = 0; index < FishingRodCollection.All.Count; index++)
            {
                int selectedRod = index;
                CreateFishingRodShopCard(
                    fishingRodShopRoot.transform,
                    new Vector2(-600f + index % 3 * 600f, -35f),
                    FishingRodCollection.All[index],
                    out Text actionText,
                    () => BuyOrSelectRod(selectedRod)
                );
                rodShopActionTexts.Add(actionText);
            }
            previousRodShopPageButton = CreateButton(
                fishingRodShopRoot.transform,
                "‹",
                new Vector2(-895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousRodShopPageButton.onClick.AddListener(
                () => ChangeRodShopPage(-1)
            );
            nextRodShopPageButton = CreateButton(
                fishingRodShopRoot.transform,
                "›",
                new Vector2(895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextRodShopPageButton.onClick.AddListener(
                () => ChangeRodShopPage(1)
            );
            rodShopPageText = CreateText(
                "Spösida",
                fishingRodShopRoot.transform,
                "",
                25,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                rodShopPageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -470f),
                new Vector2(300f, 45f)
            );

            BuildBaitShop(background.transform);
            BuildLureShop(background.transform);

            fishShopStatusText = CreateText(
                "Butiksstatus",
                background.transform,
                "",
                28,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                fishShopStatusText.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 45f),
                new Vector2(1300f, 50f)
            );
            AddOutline(fishShopStatusText, RuntimeArt.Hex("#40245F"), 3f);

            fishShopPanel.SetActive(false);
        }

        private FishShopCard CreateFishShopCard(
            Transform parent,
            Vector2 position,
            UnityEngine.Events.UnityAction sellOneAction,
            UnityEngine.Events.UnityAction sellAllAction
        )
        {
            Image panel = CreatePanel(
                "Fiskkort",
                parent,
                position,
                new Vector2(510f, 700f),
                RuntimeArt.Hex("#2A66DB")
            );
            Image picture = CreateImage("Fiskbild", panel.transform, null);
            picture.type = Image.Type.Simple;
            picture.preserveAspect = true;
            SetRect(
                picture.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 150f),
                new Vector2(410f, 280f)
            );
            Text name = CreateText(
                "Fisknamn",
                panel.transform,
                "",
                38,
                Color.white
            );
            SetRect(
                name.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -45f),
                new Vector2(450f, 65f)
            );
            AddOutline(name, RuntimeArt.Hex("#40245F"), 3f);
            Text details = CreateText(
                "Fiskdetaljer",
                panel.transform,
                "",
                25,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                details.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -115f),
                new Vector2(450f, 55f)
            );
            Text price = CreateText(
                "Fiskpris",
                panel.transform,
                "",
                34,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                price.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -175f),
                new Vector2(430f, 55f)
            );
            Button sellOne = CreateButton(
                panel.transform,
                "SÄLJ 1",
                new Vector2(-112f, -270f),
                new Vector2(210f, 82f),
                RuntimeArt.Hex("#FF6B35"),
                28
            );
            Button sellAll = CreateButton(
                panel.transform,
                "SÄLJ ALLA",
                new Vector2(112f, -270f),
                new Vector2(210f, 82f),
                RuntimeArt.Hex("#E85A96"),
                27
            );
            sellOne.onClick.AddListener(sellOneAction);
            sellAll.onClick.AddListener(sellAllAction);
            return new FishShopCard
            {
                Root = panel.gameObject,
                Picture = picture,
                Name = name,
                Details = details,
                Price = price,
                SellOneButton = sellOne,
                SellAllButton = sellAll
            };
        }

        private void CreateFishingRodShopCard(
            Transform parent,
            Vector2 position,
            FishingRodDefinition definition,
            out Text actionText,
            UnityEngine.Events.UnityAction action
        )
        {
            bool specialDesign = definition.RareChanceBonus > 0f;
            Image panel = CreatePanel(
                definition.DisplayName,
                parent,
                position,
                new Vector2(510f, 700f),
                RuntimeArt.Hex("#2A66DB")
            );
            rodShopCards.Add(panel.gameObject);
            Image rodPreview = CreateImage(
                "Spöbild",
                panel.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "ShopRod_" + definition.Id,
                    RuntimeArt.Hex(definition.BorderHex),
                    RuntimeArt.Hex(definition.ShaftHex),
                    390,
                    25,
                    12,
                    5
                )
            );
            SetRect(
                rodPreview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 145f),
                new Vector2(390f, 25f)
            );
            rodPreview.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, 18f);

            Image previewHandle = CreateImage(
                "Spöhandtag",
                rodPreview.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "ShopRodHandle_" + definition.Id,
                    RuntimeArt.Hex(definition.BorderHex),
                    RuntimeArt.Hex(definition.HandleHex),
                    112,
                    38,
                    16,
                    4
                )
            );
            SetRect(
                previewHandle.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(55f, 0f),
                new Vector2(112f, 38f)
            );

            if (specialDesign)
            {
                Image previewReel = CreateImage(
                    "Specialrulle",
                    rodPreview.transform,
                    RuntimeArt.CircleSprite(
                        "ShopReel_" + definition.Id,
                        RuntimeArt.Hex(definition.BorderHex),
                        RuntimeArt.Hex(definition.ShaftHex),
                        RuntimeArt.Hex(definition.AccentHex),
                        96
                    )
                );
                SetRect(
                    previewReel.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(112f, -45f),
                    new Vector2(68f, 68f)
                );
                Image reelHub = CreateImage(
                    "Rullnav",
                    previewReel.transform,
                    RuntimeArt.CircleSprite(
                        "ShopReelHub_" + definition.Id,
                        RuntimeArt.Hex(definition.BorderHex),
                        RuntimeArt.Hex(definition.HandleHex),
                        Color.white,
                        64
                    )
                );
                SetRect(
                    reelHub.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(30f, 30f)
                );

                for (int bandIndex = 0; bandIndex < 4; bandIndex++)
                {
                    Image band = CreateImage(
                        "Spöband " + (bandIndex + 1),
                        rodPreview.transform,
                        RuntimeArt.RoundedRectangleSprite(
                            "ShopRodBand_" + definition.Id + "_" + bandIndex,
                            RuntimeArt.Hex(definition.BorderHex),
                            bandIndex % 2 == 0
                                ? RuntimeArt.Hex(definition.AccentHex)
                                : RuntimeArt.Hex(definition.HandleHex),
                            18,
                            36,
                            7,
                            3
                        )
                    );
                    SetRect(
                        band.rectTransform,
                        new Vector2(0f, 0.5f),
                        new Vector2(0f, 0.5f),
                        new Vector2(165f + bandIndex * 57f, 0f),
                        new Vector2(16f, 34f)
                    );
                }

                Text symbol = CreateText(
                    "Toppsymbol",
                    rodPreview.transform,
                    definition.Symbol,
                    40,
                    RuntimeArt.Hex(definition.AccentHex)
                );
                SetRect(
                    symbol.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(365f, 0f),
                    new Vector2(58f, 58f)
                );
                AddOutline(
                    symbol,
                    RuntimeArt.Hex(definition.BorderHex),
                    2f
                );
            }
            Text name = CreateText(
                "Spönamn",
                panel.transform,
                definition.DisplayName,
                38,
                Color.white
            );
            SetRect(
                name.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -45f),
                new Vector2(460f, 75f)
            );
            AddOutline(name, RuntimeArt.Hex("#40245F"), 3f);
            Text info = CreateText(
                "Spöinfo",
                panel.transform,
                specialDesign
                    ? "UNIK DESIGN • SAMMA PASSFORM\n+"
                        + Mathf.RoundToInt(definition.RareChanceBonus * 100f)
                        + "% SÄLLSYNTA FISKAR"
                    : "DITT PÅLITLIGA ORIGINALSPÖ\nALLTID TILLGÄNGLIGT",
                specialDesign ? 23 : 24,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                info.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -130f),
                new Vector2(450f, 100f)
            );
            Button select = CreateButton(
                panel.transform,
                "",
                new Vector2(0f, -270f),
                new Vector2(330f, 82f),
                RuntimeArt.Hex("#FF6B35"),
                34
            );
            actionText = CreateText(
                "Spöåtgärd",
                select.transform,
                "",
                31,
                Color.white
            );
            Stretch(actionText.rectTransform);
            AddOutline(actionText, RuntimeArt.Hex("#40245F"), 3f);
            select.onClick.AddListener(action);
        }

        private RectTransform CreateLureVisual(
            Transform parent,
            FishingLureDefinition definition,
            Vector2 size
        )
        {
            GameObject rootObject = new(
                "Fiskedrag " + definition.Id,
                typeof(RectTransform)
            );
            rootObject.transform.SetParent(parent, false);
            RectTransform root = rootObject.GetComponent<RectTransform>();
            SetRect(
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                size
            );
            root.localRotation = Quaternion.Euler(0f, 0f, 0f);
            switch (definition.Style)
            {
                case 0:
                    BuildSpoonLure(root, definition, size);
                    break;
                case 1:
                    BuildSpinnerLure(root, definition, size);
                    break;
                case 2:
                    BuildJigLure(root, definition, size);
                    break;
                default:
                    BuildMinnowLure(root, definition, size);
                    break;
            }
            return root;
        }

        private Image CreateLurePart(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 size,
            float rotation = 0f
        )
        {
            Image part = CreateImage(name, parent, sprite);
            SetRect(
                part.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size
            );
            part.rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, rotation);
            return part;
        }

        private Sprite LureBarSprite(
            string name,
            Color border,
            Color fill,
            Vector2 size
        )
        {
            return RuntimeArt.RoundedRectangleSprite(
                name,
                border,
                fill,
                Mathf.Max(8, Mathf.RoundToInt(size.x)),
                Mathf.Max(8, Mathf.RoundToInt(size.y)),
                Mathf.Max(3, Mathf.RoundToInt(size.y * 0.45f)),
                Mathf.Max(1, Mathf.RoundToInt(size.y * 0.12f))
            );
        }

        private void BuildSpoonLure(
            Transform root,
            FishingLureDefinition lure,
            Vector2 size
        )
        {
            Color border = RuntimeArt.Hex(lure.BorderHex);
            Color body = RuntimeArt.Hex(lure.BodyHex);
            Color accent = RuntimeArt.Hex(lure.AccentHex);
            CreateLurePart(
                root,
                "Skedkropp",
                RuntimeArt.CircleSprite(
                    "Spoon_" + lure.Id,
                    border,
                    body,
                    accent,
                    128
                ),
                new Vector2(-size.x * 0.08f, size.y * 0.02f),
                new Vector2(size.x * 0.5f, size.y * 0.78f),
                -12f
            );
            CreateLurePart(
                root,
                "Skedblänk",
                RuntimeArt.CircleSprite(
                    "SpoonShine_" + lure.Id,
                    accent,
                    Color.white,
                    Color.white,
                    64
                ),
                new Vector2(-size.x * 0.16f, size.y * 0.16f),
                Vector2.one * size.y * 0.17f
            );
            CreateLureConnector(
                root,
                lure,
                size,
                new Vector2(size.x * 0.25f, 0f),
                new Vector2(size.x * 0.18f, size.y * 0.08f)
            );
            CreateTrebleHook(
                root,
                size,
                new Vector2(size.x * 0.38f, -size.y * 0.22f)
            );
        }

        private void BuildSpinnerLure(
            Transform root,
            FishingLureDefinition lure,
            Vector2 size
        )
        {
            Color border = RuntimeArt.Hex(lure.BorderHex);
            Color body = RuntimeArt.Hex(lure.BodyHex);
            Color accent = RuntimeArt.Hex(lure.AccentHex);
            Vector2 wireSize = new(size.x * 0.66f, size.y * 0.065f);
            CreateLurePart(
                root,
                "Spinnartråd",
                LureBarSprite("SpinnerWire_" + lure.Id, border, Color.white, wireSize),
                Vector2.zero,
                wireSize
            );
            CreateLurePart(
                root,
                "Spinnarblad",
                RuntimeArt.CircleSprite(
                    "SpinnerBlade_" + lure.Id,
                    border,
                    body,
                    accent,
                    128
                ),
                new Vector2(-size.x * 0.22f, size.y * 0.08f),
                new Vector2(size.x * 0.3f, size.y * 0.55f),
                -28f
            );
            for (int index = 0; index < 3; index++)
            {
                CreateLurePart(
                    root,
                    "Spinnarpärla " + index,
                    RuntimeArt.CircleSprite(
                        "SpinnerBead_" + lure.Id + "_" + index,
                        border,
                        index % 2 == 0 ? accent : body,
                        Color.white,
                        64
                    ),
                    new Vector2(
                        size.x * (0.02f + index * 0.095f),
                        0f
                    ),
                    Vector2.one * size.y * 0.19f
                );
            }
            CreateLurePart(
                root,
                "Spinnarfjäder övre",
                LureBarSprite(
                    "SpinnerFeatherTop_" + lure.Id,
                    border,
                    body,
                    new Vector2(size.x * 0.22f, size.y * 0.12f)
                ),
                new Vector2(size.x * 0.31f, size.y * 0.1f),
                new Vector2(size.x * 0.22f, size.y * 0.12f),
                28f
            );
            CreateLurePart(
                root,
                "Spinnarfjäder undre",
                LureBarSprite(
                    "SpinnerFeatherBottom_" + lure.Id,
                    border,
                    accent,
                    new Vector2(size.x * 0.22f, size.y * 0.12f)
                ),
                new Vector2(size.x * 0.31f, -size.y * 0.1f),
                new Vector2(size.x * 0.22f, size.y * 0.12f),
                -28f
            );
            CreateTrebleHook(
                root,
                size,
                new Vector2(size.x * 0.42f, -size.y * 0.2f)
            );
        }

        private void BuildJigLure(
            Transform root,
            FishingLureDefinition lure,
            Vector2 size
        )
        {
            Color border = RuntimeArt.Hex(lure.BorderHex);
            Color body = RuntimeArt.Hex(lure.BodyHex);
            Color accent = RuntimeArt.Hex(lure.AccentHex);
            CreateLurePart(
                root,
                "Jigghuvud",
                RuntimeArt.CircleSprite(
                    "JigHead_" + lure.Id,
                    border,
                    body,
                    accent,
                    128
                ),
                new Vector2(-size.x * 0.25f, size.y * 0.03f),
                Vector2.one * size.y * 0.48f
            );
            Vector2 jigBodySize = new(size.x * 0.48f, size.y * 0.31f);
            CreateLurePart(
                root,
                "Jiggkropp",
                LureBarSprite(
                    "JigBody_" + lure.Id,
                    border,
                    body,
                    jigBodySize
                ),
                new Vector2(0f, 0f),
                jigBodySize
            );
            CreateLurePart(
                root,
                "Jiggöga",
                RuntimeArt.CircleSprite(
                    "JigEye_" + lure.Id,
                    border,
                    Color.white,
                    accent,
                    64
                ),
                new Vector2(-size.x * 0.3f, size.y * 0.08f),
                Vector2.one * size.y * 0.16f
            );
            Vector2 tailSize = new(size.x * 0.28f, size.y * 0.13f);
            CreateLurePart(
                root,
                "Jiggstjärt övre",
                LureBarSprite(
                    "JigTailTop_" + lure.Id,
                    border,
                    accent,
                    tailSize
                ),
                new Vector2(size.x * 0.3f, size.y * 0.1f),
                tailSize,
                35f
            );
            CreateLurePart(
                root,
                "Jiggstjärt undre",
                LureBarSprite(
                    "JigTailBottom_" + lure.Id,
                    border,
                    body,
                    tailSize
                ),
                new Vector2(size.x * 0.3f, -size.y * 0.1f),
                tailSize,
                -35f
            );
            CreateTrebleHook(
                root,
                size,
                new Vector2(size.x * 0.03f, -size.y * 0.28f)
            );
        }

        private void BuildMinnowLure(
            Transform root,
            FishingLureDefinition lure,
            Vector2 size
        )
        {
            Color border = RuntimeArt.Hex(lure.BorderHex);
            Color body = RuntimeArt.Hex(lure.BodyHex);
            Color accent = RuntimeArt.Hex(lure.AccentHex);
            CreateLurePart(
                root,
                "Wobblerkropp",
                RuntimeArt.CircleSprite(
                    "MinnowBody_" + lure.Id,
                    border,
                    body,
                    accent,
                    128
                ),
                new Vector2(-size.x * 0.05f, 0f),
                new Vector2(size.x * 0.6f, size.y * 0.5f)
            );
            CreateLurePart(
                root,
                "Wobbleröga",
                RuntimeArt.CircleSprite(
                    "MinnowEye_" + lure.Id,
                    border,
                    Color.white,
                    accent,
                    64
                ),
                new Vector2(-size.x * 0.25f, size.y * 0.08f),
                Vector2.one * size.y * 0.15f
            );
            for (int index = 0; index < 3; index++)
            {
                Vector2 stripeSize = new(size.x * 0.035f, size.y * 0.32f);
                CreateLurePart(
                    root,
                    "Wobblerrand " + index,
                    LureBarSprite(
                        "MinnowStripe_" + lure.Id + "_" + index,
                        accent,
                        accent,
                        stripeSize
                    ),
                    new Vector2(
                        size.x * (-0.04f + index * 0.09f),
                        0f
                    ),
                    stripeSize,
                    -12f
                );
            }
            Vector2 finSize = new(size.x * 0.2f, size.y * 0.12f);
            CreateLurePart(
                root,
                "Wobblerstjärt övre",
                LureBarSprite(
                    "MinnowTailTop_" + lure.Id,
                    border,
                    body,
                    finSize
                ),
                new Vector2(size.x * 0.3f, size.y * 0.1f),
                finSize,
                36f
            );
            CreateLurePart(
                root,
                "Wobblerstjärt undre",
                LureBarSprite(
                    "MinnowTailBottom_" + lure.Id,
                    border,
                    accent,
                    finSize
                ),
                new Vector2(size.x * 0.3f, -size.y * 0.1f),
                finSize,
                -36f
            );
            Vector2 lipSize = new(size.x * 0.18f, size.y * 0.09f);
            CreateLurePart(
                root,
                "Dyksked",
                LureBarSprite(
                    "MinnowLip_" + lure.Id,
                    border,
                    RuntimeArt.Hex("#B9F2FF"),
                    lipSize
                ),
                new Vector2(-size.x * 0.34f, -size.y * 0.17f),
                lipSize,
                -30f
            );
            CreateTrebleHook(
                root,
                size,
                new Vector2(0f, -size.y * 0.3f)
            );
        }

        private void CreateLureConnector(
            Transform root,
            FishingLureDefinition lure,
            Vector2 size,
            Vector2 position,
            Vector2 connectorSize
        )
        {
            Color metal = RuntimeArt.Hex("#D8E8F2");
            Color border = RuntimeArt.Hex("#394B61");
            CreateLurePart(
                root,
                "Lekande",
                LureBarSprite(
                    "LureConnector_" + lure.Id,
                    border,
                    metal,
                    connectorSize
                ),
                position,
                connectorSize
            );
            CreateLurePart(
                root,
                "Lekande ring",
                RuntimeArt.CircleSprite(
                    "LureRing_" + lure.Id,
                    border,
                    metal,
                    Color.white,
                    64
                ),
                position + new Vector2(connectorSize.x * 0.55f, 0f),
                Vector2.one * size.y * 0.18f
            );
        }

        private void CreateTrebleHook(
            Transform root,
            Vector2 size,
            Vector2 position
        )
        {
            Color metal = RuntimeArt.Hex("#D8E8F2");
            Vector2 stemSize = new(size.x * 0.035f, size.y * 0.3f);
            CreateLurePart(
                root,
                "Krokstam",
                LureBarSprite("HookStem", metal, metal, stemSize),
                position + new Vector2(0f, size.y * 0.08f),
                stemSize
            );
            for (int index = 0; index < 3; index++)
            {
                float rotation = -48f + index * 48f;
                Vector2 armSize = new(size.x * 0.16f, size.y * 0.055f);
                CreateLurePart(
                    root,
                    "Krokarm " + index,
                    LureBarSprite(
                        "HookArm_" + index,
                        metal,
                        metal,
                        armSize
                    ),
                    position + new Vector2(
                        (index - 1) * size.x * 0.045f,
                        -size.y * 0.09f
                    ),
                    armSize,
                    rotation
                );
            }
        }

        private void ApplyActiveLureDesign()
        {
            FishingLureDefinition selected = FishingLureCollection.Selected;
            for (int index = 0; index < activeLureVariants.Count; index++)
            {
                bool isSelected =
                    selected != null
                    && FishingLureCollection.All[index].Id == selected.Id;
                activeLureVariants[index].gameObject.SetActive(false);
                if (isSelected)
                {
                    activeLure = activeLureVariants[index];
                }
            }
        }

        private void BuildBaitShop(Transform parent)
        {
            baitShopRoot = new GameObject("Betesbutik", typeof(RectTransform));
            baitShopRoot.transform.SetParent(parent, false);
            Stretch(baitShopRoot.GetComponent<RectTransform>());
            Image panel = CreatePanel(
                "Maskpaket",
                baitShopRoot.transform,
                new Vector2(0f, -35f),
                new Vector2(760f, 700f),
                RuntimeArt.Hex("#2A66DB")
            );
            Image worm = CreateImage(
                "Maskikon",
                panel.transform,
                RuntimeArt.LoadSprite("Art/Fishing/UI/worm_bait")
            );
            worm.type = Image.Type.Simple;
            worm.preserveAspect = true;
            SetRect(
                worm.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 155f),
                new Vector2(330f, 260f)
            );
            Text title = CreateText(
                "Betesnamn",
                panel.transform,
                FishingBaitInventory.WormsPerPack + " MASKAR",
                48,
                Color.white
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -25f),
                new Vector2(650f, 75f)
            );
            AddOutline(title, RuntimeArt.Hex("#40245F"), 3f);
            Text info = CreateText(
                "Betesinfo",
                panel.transform,
                "+3% SÄLLSYNTA FISKAR\n1 MASK ANVÄNDS PER FÖRSÖK",
                28,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                info.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -115f),
                new Vector2(660f, 105f)
            );
            baitCountText = CreateText(
                "Masklager",
                panel.transform,
                "",
                31,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                baitCountText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -195f),
                new Vector2(620f, 55f)
            );
            Button buy = CreateButton(
                panel.transform,
                "",
                new Vector2(0f, -280f),
                new Vector2(430f, 82f),
                RuntimeArt.Hex("#65C832"),
                30
            );
            baitActionText = CreateText(
                "Köp bete",
                buy.transform,
                "",
                30,
                Color.white
            );
            Stretch(baitActionText.rectTransform);
            AddOutline(baitActionText, RuntimeArt.Hex("#40245F"), 3f);
            buy.onClick.AddListener(BuyWormPack);
        }

        private void BuildLureShop(Transform parent)
        {
            lureShopRoot = new GameObject(
                "Fiskedragsbutik",
                typeof(RectTransform)
            );
            lureShopRoot.transform.SetParent(parent, false);
            Stretch(lureShopRoot.GetComponent<RectTransform>());

            CreateLureShopCard(
                lureShopRoot.transform,
                Vector2.zero,
                null,
                out floatActionText,
                SelectFloat
            );
            for (int index = 0; index < FishingLureCollection.All.Count; index++)
            {
                int selectedLure = index;
                CreateLureShopCard(
                    lureShopRoot.transform,
                    Vector2.zero,
                    FishingLureCollection.All[index],
                    out Text actionText,
                    () => BuyOrSelectLure(selectedLure)
                );
                lureShopActionTexts.Add(actionText);
            }
            previousLureShopPageButton = CreateButton(
                lureShopRoot.transform,
                "‹",
                new Vector2(-895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousLureShopPageButton.onClick.AddListener(
                () => ChangeLureShopPage(-1)
            );
            nextLureShopPageButton = CreateButton(
                lureShopRoot.transform,
                "›",
                new Vector2(895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextLureShopPageButton.onClick.AddListener(
                () => ChangeLureShopPage(1)
            );
            lureShopPageText = CreateText(
                "Dragsida",
                lureShopRoot.transform,
                "",
                25,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                lureShopPageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -470f),
                new Vector2(300f, 45f)
            );
        }

        private void CreateLureShopCard(
            Transform parent,
            Vector2 position,
            FishingLureDefinition lure,
            out Text actionText,
            UnityEngine.Events.UnityAction action
        )
        {
            bool isFloat = lure == null;
            Image panel = CreatePanel(
                isFloat ? "KLASSISKT FLÖTE" : lure.DisplayName,
                parent,
                position,
                new Vector2(510f, 700f),
                RuntimeArt.Hex("#2A66DB")
            );
            lureShopCards.Add(panel.gameObject);
            if (isFloat)
            {
                Image preview = CreateImage(
                    "Flötesbild",
                    panel.transform,
                    RuntimeArt.CircleSprite(
                        "ShopFloat",
                        RuntimeArt.Hex("#8E281E"),
                        RuntimeArt.Hex("#F04432"),
                        Color.white,
                        128
                    )
                );
                SetRect(
                    preview.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 145f),
                    new Vector2(190f, 190f)
                );
            }
            else
            {
                RectTransform preview = CreateLureVisual(
                    panel.transform,
                    lure,
                    new Vector2(340f, 170f)
                );
                preview.anchoredPosition = new Vector2(0f, 145f);
            }
            Text name = CreateText(
                "Dragnamn",
                panel.transform,
                isFloat ? "KLASSISKT FLÖTE" : lure.DisplayName,
                36,
                Color.white
            );
            SetRect(
                name.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -45f),
                new Vector2(460f, 75f)
            );
            AddOutline(name, RuntimeArt.Hex("#40245F"), 3f);
            Text info = CreateText(
                "Draginfo",
                panel.transform,
                isFloat
                    ? "FISKA MED MASK OCH FLÖTE\nINGEN DRAGBONUS"
                    : "SYNS UNDER VATTNET\n+"
                        + Mathf.RoundToInt(lure.RareChanceBonus * 100f)
                        + "% SÄLLSYNTA FISKAR",
                24,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                info.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -135f),
                new Vector2(450f, 100f)
            );
            Button select = CreateButton(
                panel.transform,
                "",
                new Vector2(0f, -270f),
                new Vector2(330f, 82f),
                RuntimeArt.Hex("#FF6B35"),
                31
            );
            actionText = CreateText(
                "Dragåtgärd",
                select.transform,
                "",
                29,
                Color.white
            );
            Stretch(actionText.rectTransform);
            AddOutline(actionText, RuntimeArt.Hex("#40245F"), 3f);
            select.onClick.AddListener(action);
        }

        private void OpenFishShop()
        {
            if (stateMachine.Current != FishingState.Idle)
            {
                return;
            }
            fishShopTab = 0;
            fishShopPage = 0;
            fishShopStatusText.text = "";
            RefreshFishShop();
            fishShopPanel.transform.SetAsLastSibling();
            fishShopPanel.SetActive(true);
            mainButton.interactable = false;
            Play(FishingSound.Button, 0.8f);
        }

        private void CloseFishShop()
        {
            fishShopPanel.SetActive(false);
            mainButton.interactable = true;
            Play(FishingSound.Button, 0.7f);
        }

        private void ShowFishShopTab(int tab)
        {
            fishShopTab = Mathf.Clamp(tab, 0, 3);
            fishShopStatusText.text = "";
            RefreshFishShop();
            Play(FishingSound.Button, 0.7f);
        }

        private void HandleFishShopSwipe(int direction)
        {
            switch (fishShopTab)
            {
                case 0:
                    ChangeFishShopPage(direction);
                    break;
                case 1:
                    ChangeRodShopPage(direction);
                    break;
                case 2:
                    ShowFishShopTab(direction > 0 ? 3 : 1);
                    break;
                case 3:
                    ChangeLureShopPage(direction);
                    break;
            }
        }

        private void ChangeFishShopPage(int direction)
        {
            int pageCount = Mathf.Max(
                1,
                Mathf.CeilToInt(fishShopInventory.Count / 3f)
            );
            fishShopPage = Mathf.Clamp(
                fishShopPage + direction,
                0,
                pageCount - 1
            );
            RefreshFishShop();
            Play(FishingSound.Button, 0.65f);
        }

        private void ChangeRodShopPage(int direction)
        {
            int pageCount = Mathf.Max(
                1,
                Mathf.CeilToInt(FishingRodCollection.All.Count / 3f)
            );
            rodShopPage = Mathf.Clamp(
                rodShopPage + direction,
                0,
                pageCount - 1
            );
            RefreshFishShop();
            Play(FishingSound.Button, 0.65f);
        }

        private void ChangeLureShopPage(int direction)
        {
            int pageCount = Mathf.Max(
                1,
                Mathf.CeilToInt(lureShopCards.Count / 3f)
            );
            lureShopPage = Mathf.Clamp(
                lureShopPage + direction,
                0,
                pageCount - 1
            );
            RefreshFishShop();
            Play(FishingSound.Button, 0.65f);
        }

        private void RefreshFishShop()
        {
            fishShopInventoryRoot.SetActive(fishShopTab == 0);
            fishingRodShopRoot.SetActive(fishShopTab == 1);
            baitShopRoot.SetActive(fishShopTab == 2);
            lureShopRoot.SetActive(fishShopTab == 3);
            fishShopCoinText.text = CoinWallet.Balance.ToString();

            if (fishShopTab == 1)
            {
                int rodPageCount = Mathf.Max(
                    1,
                    Mathf.CeilToInt(FishingRodCollection.All.Count / 3f)
                );
                rodShopPage = Mathf.Clamp(
                    rodShopPage,
                    0,
                    rodPageCount - 1
                );
                for (int index = 0; index < rodShopCards.Count; index++)
                {
                    FishingRodDefinition rod = FishingRodCollection.All[index];
                    rodShopCards[index].SetActive(index / 3 == rodShopPage);
                    rodShopActionTexts[index].text =
                        FishingRodCollection.IsSelected(rod)
                            ? "VALT"
                            : FishingRodCollection.IsOwned(rod)
                                ? "VÄLJ"
                                : "KÖP " + rod.Price;
                }
                previousRodShopPageButton.interactable = rodShopPage > 0;
                nextRodShopPageButton.interactable =
                    rodShopPage < rodPageCount - 1;
                rodShopPageText.text =
                    "SIDA " + (rodShopPage + 1) + " / " + rodPageCount;
                return;
            }
            if (fishShopTab == 2)
            {
                baitCountText.text =
                    "DU HAR " + FishingBaitInventory.WormCount + " MASKAR";
                baitActionText.text =
                    "KÖP "
                    + FishingBaitInventory.WormsPerPack
                    + " • "
                    + FishingBaitInventory.PackPrice
                    + " MYNT";
                return;
            }
            if (fishShopTab == 3)
            {
                int lurePageCount = Mathf.Max(
                    1,
                    Mathf.CeilToInt(lureShopCards.Count / 3f)
                );
                lureShopPage = Mathf.Clamp(
                    lureShopPage,
                    0,
                    lurePageCount - 1
                );
                FishingLureDefinition selected =
                    FishingLureCollection.Selected;
                floatActionText.text = selected == null ? "VALT" : "VÄLJ";
                for (int index = 0; index < lureShopCards.Count; index++)
                {
                    lureShopCards[index].SetActive(
                        index / 3 == lureShopPage
                    );
                    lureShopCards[index]
                        .GetComponent<RectTransform>()
                        .anchoredPosition =
                        new Vector2(-600f + index % 3 * 600f, -35f);
                    if (index == 0)
                    {
                        continue;
                    }
                    FishingLureDefinition lure =
                        FishingLureCollection.All[index - 1];
                    lureShopActionTexts[index - 1].text =
                        FishingLureCollection.IsSelected(lure)
                            ? "VALT"
                            : FishingLureCollection.IsOwned(lure)
                                ? "VÄLJ"
                                : "KÖP " + lure.Price;
                }
                previousLureShopPageButton.interactable = lureShopPage > 0;
                nextLureShopPageButton.interactable =
                    lureShopPage < lurePageCount - 1;
                lureShopPageText.text =
                    "SIDA "
                    + (lureShopPage + 1)
                    + " / "
                    + lurePageCount;
                return;
            }

            fishShopInventory.Clear();
            List<(string FishId, FishSpecimenRecord Specimen)> inventory =
                collection.Inventory();
            for (int index = 0; index < inventory.Count; index++)
            {
                (string FishId, FishSpecimenRecord Specimen) entry =
                    inventory[index];
                FishShopStack stack = fishShopInventory.Find(
                    candidate => candidate.FishId == entry.FishId
                );
                if (stack == null)
                {
                    stack = new FishShopStack { FishId = entry.FishId };
                    fishShopInventory.Add(stack);
                }
                stack.Specimens.Add(entry.Specimen);
            }
            fishShopInventory.Sort(
                (left, right) => string.CompareOrdinal(
                    LatestCaughtUtc(right),
                    LatestCaughtUtc(left)
                )
            );
            int pageCount = Mathf.Max(
                1,
                Mathf.CeilToInt(fishShopInventory.Count / 3f)
            );
            fishShopPage = Mathf.Clamp(fishShopPage, 0, pageCount - 1);
            fishShopEmptyText.gameObject.SetActive(
                fishShopInventory.Count == 0
            );
            previousFishShopPageButton.interactable = fishShopPage > 0;
            nextFishShopPageButton.interactable =
                fishShopPage < pageCount - 1;
            fishShopPageText.text =
                fishShopInventory.Count == 0
                    ? ""
                    : "SIDA " + (fishShopPage + 1) + " / " + pageCount;

            for (int slot = 0; slot < fishShopCards.Count; slot++)
            {
                FishShopCard card = fishShopCards[slot];
                int inventoryIndex = fishShopPage * 3 + slot;
                if (inventoryIndex >= fishShopInventory.Count)
                {
                    card.Root.SetActive(false);
                    card.FishId = "";
                    continue;
                }

                FishShopStack stack = fishShopInventory[inventoryIndex];
                FishDefinition definition = FindFishDefinition(stack.FishId);
                if (definition == null)
                {
                    card.Root.SetActive(false);
                    card.FishId = "";
                    continue;
                }

                stack.Specimens.Sort(
                    (left, right) =>
                        left.lengthCentimeters.CompareTo(
                            right.lengthCentimeters
                        )
                );
                FishSpecimenRecord shortest = stack.Specimens[0];
                int onePrice = FishSalePricing.Calculate(
                    definition,
                    shortest.lengthCentimeters
                );
                int allPrice = 0;
                for (int index = 0; index < stack.Specimens.Count; index++)
                {
                    allPrice += FishSalePricing.Calculate(
                        definition,
                        stack.Specimens[index].lengthCentimeters
                    );
                }
                card.Root.SetActive(true);
                card.FishId = stack.FishId;
                card.Picture.sprite = definition.Sprite;
                card.Name.text =
                    definition.DisplayName
                    + "  ×"
                    + stack.Specimens.Count;
                card.Details.text =
                    RarityLabel(definition.Rarity)
                    + "  •  "
                    + shortest.lengthCentimeters.ToString("0.0")
                    + "–"
                    + stack.Specimens[stack.Specimens.Count - 1]
                        .lengthCentimeters.ToString("0.0")
                    + " CM";
                card.Price.text =
                    "1: " + onePrice + "  •  ALLA: " + allPrice;
            }
        }

        private static string LatestCaughtUtc(FishShopStack stack)
        {
            string latest = "";
            for (int index = 0; index < stack.Specimens.Count; index++)
            {
                if (
                    string.CompareOrdinal(
                        stack.Specimens[index].caughtUtc,
                        latest
                    ) > 0
                )
                {
                    latest = stack.Specimens[index].caughtUtc;
                }
            }
            return latest;
        }

        private void SellFishFromShop(int slot, bool sellAll)
        {
            if (slot < 0 || slot >= fishShopCards.Count)
            {
                return;
            }
            FishShopCard card = fishShopCards[slot];
            FishShopStack stack = fishShopInventory.Find(
                candidate => candidate.FishId == card.FishId
            );
            FishDefinition definition = FindFishDefinition(card.FishId);
            if (
                definition == null
                || stack == null
                || stack.Specimens.Count == 0
            )
            {
                return;
            }
            stack.Specimens.Sort(
                (left, right) =>
                    left.lengthCentimeters.CompareTo(right.lengthCentimeters)
            );
            int price = 0;
            int soldCount = sellAll ? stack.Specimens.Count : 1;
            for (int index = 0; index < soldCount; index++)
            {
                FishSpecimenRecord specimen = stack.Specimens[index];
                price += FishSalePricing.Calculate(
                    definition,
                    specimen.lengthCentimeters
                );
                if (
                    !collection.TryRemoveSpecimen(
                        specimen.specimenId,
                        out _,
                        out _
                    )
                )
                {
                    return;
                }
            }
            CoinWallet.Add(price);
            fishShopStatusText.text =
                (sellAll ? soldCount + " × " : "")
                + definition.DisplayName
                + " SÅLD"
                + (sellAll ? "A" : "")
                + " FÖR "
                + price
                + " MYNT!";
            RefreshFishShop();
            Play(FishingSound.Popup, 0.85f);
        }

        private FishDefinition FindFishDefinition(string stableId)
        {
            for (int index = 0; index < definitions.Count; index++)
            {
                FishDefinition definition = definitions[index];
                if (
                    definition != null
                    && string.Equals(
                        definition.StableId,
                        stableId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return definition;
                }
            }
            return null;
        }

        private void BuyOrSelectRod(int index)
        {
            if (index < 0 || index >= FishingRodCollection.All.Count)
            {
                return;
            }
            FishingRodDefinition rod = FishingRodCollection.All[index];
            bool alreadyOwned = FishingRodCollection.IsOwned(rod);
            if (!FishingRodCollection.TryBuyOrSelect(rod))
            {
                fishShopStatusText.text =
                    "DU BEHÖVER "
                    + rod.Price
                    + " MYNT FÖR "
                    + rod.DisplayName;
                Play(FishingSound.Early, 0.8f);
                return;
            }
            ApplySelectedRodDesign();
            RefreshWormCount();
            fishShopStatusText.text = alreadyOwned
                ? rod.DisplayName + " VALT!"
                : rod.DisplayName + " KÖPT OCH VALT!";
            RefreshFishShop();
            Play(FishingSound.Popup, 0.9f);
        }

        private void BuyWormPack()
        {
            if (!FishingBaitInventory.TryBuyPack())
            {
                fishShopStatusText.text =
                    "DU BEHÖVER "
                    + FishingBaitInventory.PackPrice
                    + " MYNT FÖR "
                    + FishingBaitInventory.WormsPerPack
                    + " MASKAR";
                Play(FishingSound.Early, 0.8f);
                return;
            }
            fishShopStatusText.text =
                FishingBaitInventory.WormsPerPack + " MASKAR KÖPTA!";
            RefreshWormCount();
            RefreshFishShop();
            Play(FishingSound.Popup, 0.9f);
        }

        private void BuyOrSelectLure(int index)
        {
            if (index < 0 || index >= FishingLureCollection.All.Count)
            {
                return;
            }
            FishingLureDefinition lure = FishingLureCollection.All[index];
            bool alreadyOwned = FishingLureCollection.IsOwned(lure);
            if (!FishingLureCollection.TryBuyOrSelect(lure))
            {
                fishShopStatusText.text =
                    "DU BEHÖVER "
                    + lure.Price
                    + " MYNT FÖR "
                    + lure.DisplayName;
                Play(FishingSound.Early, 0.8f);
                return;
            }
            ApplyActiveLureDesign();
            RefreshTackleVisual();
            RefreshWormCount();
            fishShopStatusText.text = alreadyOwned
                ? lure.DisplayName + " VALT!"
                : lure.DisplayName + " KÖPT OCH VALT!";
            RefreshFishShop();
            Play(FishingSound.Popup, 0.9f);
        }

        private void SelectFloat()
        {
            FishingLureCollection.SelectFloat();
            RefreshTackleVisual();
            RefreshWormCount();
            fishShopStatusText.text = "KLASSISKT FLÖTE VALT!";
            RefreshFishShop();
            Play(FishingSound.Button, 0.8f);
        }

        private void BuildLocationSelector()
        {
            locationPanel = new GameObject(
                "Fiskeplatsväljare",
                typeof(RectTransform)
            );
            locationPanel.transform.SetParent(safeArea, false);
            Stretch(locationPanel.GetComponent<RectTransform>());
            SwipePageHandler swipeHandler =
                locationPanel.AddComponent<SwipePageHandler>();
            swipeHandler.Initialize(ChangeLocationPage);

            Text heading = CreateText(
                "Välj fiskeplats",
                locationPanel.transform,
                "VÄLJ FISKEPLATS",
                64,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                heading.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -62f),
                new Vector2(900f, 105f)
            );
            AddOutline(heading, RuntimeArt.Hex("#40245F"), 5f);

            Button close = CreateButton(
                locationPanel.transform,
                "←",
                Vector2.zero,
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            close.onClick.AddListener(ExitFishing);
            SetRect(
                close.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(85f, -58f),
                new Vector2(150f, 90f)
            );

            float[] cardPositions = { -600f, 0f, 600f };
            for (
                int index = 0;
                index < FishingLocationCatalog.All.Count;
                index++
            )
            {
                int selectedLocation = index;
                FishingLocationDefinition location =
                    FishingLocationCatalog.All[index];
                Button card = CreateButton(
                    locationPanel.transform,
                    "",
                    new Vector2(cardPositions[index % 3], -35f),
                    new Vector2(510f, 700f),
                    RuntimeArt.Hex("#2A66DB"),
                    34
                );
                card.gameObject.name = "Fiskeplatskort " + (index + 1);

                Image preview = CreateImage(
                    "Platsbild",
                    card.transform,
                    RuntimeArt.LoadSprite(location.BackgroundResourcePath)
                );
                preview.type = Image.Type.Simple;
                preview.preserveAspect = false;
                SetRect(
                    preview.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 145f),
                    new Vector2(465f, 315f)
                );

                Text name = CreateText(
                    "Namn",
                    card.transform,
                    location.DisplayName,
                    38,
                    Color.white
                );
                SetRect(
                    name.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -70f),
                    new Vector2(460f, 75f)
                );
                AddOutline(name, RuntimeArt.Hex("#40245F"), 3f);

                Text progress = CreateText(
                    "Info",
                    card.transform,
                    "",
                    27,
                    RuntimeArt.Hex("#FFF3AD")
                );
                SetRect(
                    progress.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -145f),
                    new Vector2(450f, 55f)
                );

                Image actionButton = CreateImage(
                    "Spelaknapp",
                    card.transform,
                    RuntimeArt.RoundedRectangleSprite(
                        "FishingLocationPlayButton",
                        RuntimeArt.Hex("#B93B18"),
                        RuntimeArt.Hex("#FF6B35"),
                        330,
                        82,
                        28,
                        7
                    )
                );
                actionButton.raycastTarget = false;
                SetRect(
                    actionButton.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -255f),
                    new Vector2(330f, 82f)
                );

                Text action = CreateText(
                    "Åtgärd",
                    card.transform,
                    "SPELA",
                    34,
                    Color.white
                );
                SetRect(
                    action.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -255f),
                    new Vector2(430f, 70f)
                );
                AddOutline(action, RuntimeArt.Hex("#40245F"), 3f);

                card.onClick.AddListener(
                    () => SelectLocation(selectedLocation)
                );
                locationCards.Add(card.gameObject);
                locationButtons.Add(card);
                locationProgressTexts.Add(progress);
                locationActionTexts.Add(action);
                locationLocks.Add(
                    index == 0 ? null : CreateLocationLock(card.transform)
                );
            }

            previousLocationPageButton = CreateButton(
                locationPanel.transform,
                "‹",
                new Vector2(-895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousLocationPageButton.onClick.AddListener(
                () => ChangeLocationPage(-1)
            );
            nextLocationPageButton = CreateButton(
                locationPanel.transform,
                "›",
                new Vector2(895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextLocationPageButton.onClick.AddListener(
                () => ChangeLocationPage(1)
            );

            RefreshLocationCards();
            RefreshLocationPage();
            locationPanel.SetActive(false);
        }

        private void ShowLocationSelector()
        {
            stateMachine.Reset();
            ResetVisuals();
            locationPage = Mathf.Clamp(locationIndex / 3, 0, 1);
            RefreshLocationCards();
            RefreshLocationPage();
            gameplayTopHud.SetActive(false);
            mainControlRoot.SetActive(false);
            locationPanel.transform.SetAsLastSibling();
            locationPanel.SetActive(true);
            mainButton.interactable = false;
        }

        private void ChangeLocationPage(int direction)
        {
            int pageCount = Mathf.CeilToInt(
                FishingLocationCatalog.All.Count / 3f
            );
            locationPage = Mathf.Clamp(
                locationPage + direction,
                0,
                pageCount - 1
            );
            Play(FishingSound.Button, 0.7f);
            RefreshLocationPage();
        }

        private void RefreshLocationCards()
        {
            for (int index = 0; index < locationButtons.Count; index++)
            {
                FishingLocationDefinition location =
                    FishingLocationCatalog.All[index];
                bool unlocked = FishingLocationProgression.IsUnlocked(
                    index,
                    definitions,
                    collection
                );
                int caught = FishingLocationProgression.CaughtFishCount(
                    location,
                    definitions,
                    collection
                );
                locationButtons[index].interactable = unlocked;
                locationActionTexts[index].text = unlocked ? "SPELA" : "LÅST";
                locationProgressTexts[index].text =
                    "FISKAR "
                    + caught
                    + " / "
                    + location.FishCount;
                if (locationLocks[index] != null)
                {
                    locationLocks[index].SetActive(!unlocked);
                }
            }
        }

        private void RefreshLocationPage()
        {
            int pageCount = Mathf.CeilToInt(
                FishingLocationCatalog.All.Count / 3f
            );
            for (int index = 0; index < locationCards.Count; index++)
            {
                locationCards[index].SetActive(index / 3 == locationPage);
            }
            previousLocationPageButton.interactable = locationPage > 0;
            nextLocationPageButton.interactable =
                locationPage < pageCount - 1;
        }

        private GameObject CreateLocationLock(Transform parent)
        {
            Image greyOut = CreateImage("Tydligt låst", parent, null);
            greyOut.color = new Color(0.16f, 0.17f, 0.19f, 0.78f);
            greyOut.raycastTarget = false;
            Stretch(greyOut.rectTransform);
            Image lockBody = CreatePanel(
                "Lås",
                greyOut.transform,
                new Vector2(0f, 35f),
                new Vector2(230f, 160f),
                RuntimeArt.Hex("#777777")
            );
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
            return greyOut.gameObject;
        }

        private void SelectLocation(int selectedLocation)
        {
            if (
                !FishingLocationProgression.IsUnlocked(
                    selectedLocation,
                    definitions,
                    collection
                )
            )
            {
                return;
            }
            locationIndex = selectedLocation;
            Play(FishingSound.Button, 0.8f);
            locationPanel.SetActive(false);
            ApplyLocation();
        }

        private void ApplyLocation()
        {
            FishingLocationDefinition location =
                FishingLocationCatalog.All[locationIndex];
            backgroundImage.sprite =
                RuntimeArt.LoadSprite(location.BackgroundResourcePath);
            for (int index = 0; index < swimmers.Count; index++)
            {
                FishDefinition fish = locationSelection.Select(location);
                if (fish != null)
                {
                    swimmers[index].SetFish(fish.Sprite, fish.SwimSpeed);
                }
            }
            gameplayTopHud.SetActive(true);
            mainControlRoot.SetActive(true);
            mainButton.interactable = true;
            HandleStateChanged(stateMachine.Current, stateMachine.Current);
        }

        private void BuildCatchPopup()
        {
            Image shade = CreateImage("Fångsttoning", safeArea, null);
            shade.color = new Color(0.02f, 0.1f, 0.25f, 0.88f);
            Stretch(shade.rectTransform);
            catchPopup = shade.gameObject;

            Image panel = CreatePanel(
                "Fångstpopup",
                shade.transform,
                Vector2.zero,
                new Vector2(950f, 850f),
                RuntimeArt.Hex("#EAF8FF")
            );
            catchRibbonText = CreateText(
                "Fångstrubrik",
                panel.transform,
                "NY FISK!",
                62,
                Color.white
            );
            Image ribbon = CreatePanel(
                "Rubrikband",
                panel.transform,
                new Vector2(0f, 330f),
                new Vector2(650f, 105f),
                RuntimeArt.Hex("#2A66DB")
            );
            catchRibbonText.transform.SetParent(ribbon.transform, false);
            Stretch(catchRibbonText.rectTransform);
            AddOutline(catchRibbonText, RuntimeArt.Hex("#173B80"), 4f);

            catchPicture = CreateImage("Stor fisk", panel.transform, null);
            catchPicture.type = Image.Type.Simple;
            catchPicture.preserveAspect = true;
            SetRect(
                catchPicture.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 100f),
                new Vector2(540f, 360f)
            );

            catchNameText = CreateText(
                "Fångstnamn",
                panel.transform,
                "",
                48,
                RuntimeArt.Hex("#4A3424")
            );
            SetRect(
                catchNameText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -105f),
                new Vector2(760f, 70f)
            );
            catchRarityText = CreateText(
                "Fångstraritet",
                panel.transform,
                "",
                31,
                RuntimeArt.Hex("#7A45A5")
            );
            SetRect(
                catchRarityText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -165f),
                new Vector2(620f, 50f)
            );
            catchLengthText = CreateText(
                "Fångstlängd",
                panel.transform,
                "",
                28,
                RuntimeArt.Hex("#4A3424")
            );
            SetRect(
                catchLengthText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -215f),
                new Vector2(620f, 45f)
            );
            catchValueText = CreateText(
                "Fångstvärde",
                panel.transform,
                "",
                27,
                RuntimeArt.Hex("#C18400")
            );
            SetRect(
                catchValueText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -260f),
                new Vector2(620f, 42f)
            );

            Button continueButton = CreateButton(
                panel.transform,
                "BRA JOBBAT!",
                new Vector2(0f, -350f),
                new Vector2(560f, 110f),
                RuntimeArt.Hex("#65C832"),
                43
            );
            continueButton.onClick.AddListener(ContinueAfterCatch);
            catchPopup.SetActive(false);
        }

        private void BuildLocationCompletePopup()
        {
            Image shade = CreateImage("Fiskeplats klar toning", safeArea, null);
            shade.color = new Color(0.02f, 0.08f, 0.2f, 0.92f);
            Stretch(shade.rectTransform);
            locationCompletePopup = shade.gameObject;
            Image panel = CreatePanel(
                "Fiskeplats klar",
                shade.transform,
                Vector2.zero,
                new Vector2(1420f, 850f),
                RuntimeArt.Hex("#FFF3C4")
            );
            locationCompleteTitleText = CreateText(
                "Grattisrubrik",
                panel.transform,
                "GRATTIS!",
                68,
                RuntimeArt.Hex("#E85A96")
            );
            SetRect(
                locationCompleteTitleText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -62f),
                new Vector2(1100f, 90f)
            );
            AddOutline(
                locationCompleteTitleText,
                RuntimeArt.Hex("#40245F"),
                4f
            );
            locationCompleteSubtitleText = CreateText(
                "Klartext",
                panel.transform,
                "",
                34,
                RuntimeArt.Hex("#4A3424")
            );
            SetRect(
                locationCompleteSubtitleText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -140f),
                new Vector2(1250f, 65f)
            );

            float[] xs = { -480f, -288f, -96f, 96f, 288f, 480f };
            for (int index = 0; index < 6; index++)
            {
                Image fishCard = CreatePanel(
                    "Klar fisk " + (index + 1),
                    panel.transform,
                    new Vector2(xs[index], 55f),
                    new Vector2(170f, 260f),
                    RuntimeArt.Hex("#EAF8FF")
                );
                Image fishImage = CreateImage(
                    "Fiskbild",
                    fishCard.transform,
                    null
                );
                fishImage.preserveAspect = true;
                SetRect(
                    fishImage.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 18f),
                    new Vector2(150f, 180f)
                );
                locationCompleteFishImages.Add(fishImage);
                Text check = CreateText(
                    "Klar",
                    fishCard.transform,
                    "✓",
                    42,
                    RuntimeArt.Hex("#65C832")
                );
                SetRect(
                    check.rectTransform,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 31f),
                    new Vector2(120f, 55f)
                );
            }

            Button stay = CreateButton(
                panel.transform,
                "FORTSÄTT FISKA HÄR",
                new Vector2(-330f, -300f),
                new Vector2(570f, 105f),
                RuntimeArt.Hex("#5A8CE8"),
                32
            );
            stay.onClick.AddListener(ContinueFishingCurrentLocation);
            Button next = CreateButton(
                panel.transform,
                "",
                new Vector2(330f, -300f),
                new Vector2(570f, 105f),
                RuntimeArt.Hex("#65C832"),
                32
            );
            locationCompleteNextText = CreateText(
                "Nästa plats text",
                next.transform,
                "NÄSTA FISKEPLATS",
                32,
                Color.white
            );
            Stretch(locationCompleteNextText.rectTransform);
            AddOutline(
                locationCompleteNextText,
                RuntimeArt.Hex("#285B17"),
                3f
            );
            next.onClick.AddListener(ContinueToNextLocation);
            locationCompletePopup.SetActive(false);
        }

        private void BuildEffectPools()
        {
            Color[] colors =
            {
                RuntimeArt.Hex("#FFD43B"),
                RuntimeArt.Hex("#FF5E8E"),
                RuntimeArt.Hex("#55C8FF"),
                RuntimeArt.Hex("#61E08B"),
                RuntimeArt.Hex("#B76DFF")
            };
            for (int index = 0; index < 24; index++)
            {
                Text particle = CreateText(
                    "Återanvänd konfetti " + (index + 1),
                    safeArea,
                    index % 2 == 0 ? "★" : "●",
                    34 + index % 3 * 8,
                    colors[index % colors.Length]
                );
                SetRect(
                    particle.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(55f, 55f)
                );
                FishingConfettiParticle component =
                    particle.gameObject.AddComponent<FishingConfettiParticle>();
                particle.gameObject.SetActive(false);
                confetti.Add(component);
            }

            for (int index = 0; index < 14; index++)
            {
                Image bubble = CreateImage(
                    "Återanvänd bubbla " + (index + 1),
                    waterArea,
                    RuntimeArt.CircleSprite(
                        "FishingBubble",
                        RuntimeArt.Hex("#63D8FF"),
                        new Color(0.75f, 0.96f, 1f, 0.5f),
                        Color.white,
                        64
                    )
                );
                SetRect(
                    bubble.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    bobberWaterPosition,
                    new Vector2(28f, 28f)
                );
                bubble.gameObject.SetActive(false);
                bubbles.Add(bubble);
                bubbleVelocities.Add(Vector2.zero);
            }
        }

        private void BeginCast()
        {
            if (
                definitions.Count == 0
                || !stateMachine.TryTransition(FishingState.Casting)
            )
            {
                return;
            }
            currentCastUsesWorm =
                FishingLureCollection.Selected == null
                && FishingBaitInventory.WormCount > 0;
            hasCastOnce = true;
            Play(FishingSound.Button, 0.8f);
            StartFlow(CastRoutine());
        }

        private IEnumerator CastRoutine()
        {
            Play(FishingSound.Cast, 0.9f);
            tackleUnderwater = false;
            RefreshTackleVisual();
            fishingLine.gameObject.SetActive(true);
            Vector2 start = bobberRestPosition;
            const float duration = 0.95f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                Vector2 position = Vector2.Lerp(start, bobberWaterPosition, eased);
                position.y += Mathf.Sin(progress * Mathf.PI) * 260f;
                bobber.anchoredPosition = position;
                rod.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(
                        rigLayout.rodAngle,
                        8f,
                        Mathf.Sin(progress * Mathf.PI)
                    )
                );
                RefreshTackleVisual();
                UpdateFishingLine();
                yield return null;
            }
            bobber.anchoredPosition = bobberWaterPosition;
            tackleUnderwater = FishingLureCollection.Selected != null;
            RefreshTackleVisual();
            rod.localRotation = Quaternion.Euler(
                0f,
                0f,
                rigLayout.rodAngle
            );
            UpdateFishingLine();
            Play(FishingSound.Land, 0.85f);
            StartCoroutine(PlayWaterRings());

            selectedFish = locationSelection.Select(
                FishingLocationCatalog.All[locationIndex],
                FishingRodCollection.RareChanceBonus
                    + FishingLureCollection.RareChanceBonus
                    + (
                        currentCastUsesWorm
                            ? FishingBaitInventory.RareChanceBonus
                            : 0f
                    )
            );
            selectedLength = selection.SelectLength(selectedFish);
            if (!stateMachine.TryTransition(FishingState.WaitingForBite))
            {
                yield break;
            }
            float delay = random.Range(
                MinimumBiteDelay,
                MaximumBiteDelay
            );
            float waited = 0f;
            while (
                waited < delay
                && stateMachine.Current == FishingState.WaitingForBite
            )
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            if (stateMachine.Current != FishingState.WaitingForBite)
            {
                yield break;
            }

            stateMachine.TryTransition(FishingState.FishBiting);
            Play(FishingSound.Bite, 1f);
            AppPreferences.TryVibrate();
            bubbleSpawnClock = 0f;
            float biteTime = 0f;
            while (
                biteTime < BiteWindow
                && stateMachine.Current == FishingState.FishBiting
            )
            {
                biteTime += Time.unscaledDeltaTime;
                yield return null;
            }
            if (stateMachine.Current == FishingState.FishBiting)
            {
                ConsumeCastWorm();
                stateMachine.TryTransition(FishingState.ReturningToIdle);
                Play(FishingSound.SwimAway, 0.8f);
                ShowHint("FISKEN SIMMADE VIDARE – VI PROVAR IGEN!");
                yield return ReturnToIdleRoutine(1.25f);
            }
        }

        private void ShowEarlyPressHint()
        {
            Play(FishingSound.Early, 0.72f);
            StopCoroutine(nameof(HideHintRoutine));
            ShowHint("NÄSTAN – VÄNTA PÅ BUBBLORNA");
            StartCoroutine(nameof(HideHintRoutine));
        }

        private IEnumerator HideHintRoutine()
        {
            yield return new WaitForSecondsRealtime(1.4f);
            hintPanel.SetActive(false);
        }

        private void BeginReel()
        {
            if (!stateMachine.TryTransition(FishingState.ReelingIn))
            {
                return;
            }
            Play(FishingSound.Button, 0.75f);
            StartFlow(ReelRoutine());
        }

        private IEnumerator ReelRoutine()
        {
            Play(FishingSound.Reel, 0.9f);
            Image caughtImage = catchFish.GetComponent<Image>();
            caughtImage.sprite = selectedFish == null ? null : selectedFish.Sprite;
            catchFish.gameObject.SetActive(true);
            Vector2 fishStart = bobberWaterPosition + new Vector2(0f, -125f);
            Vector2 fishEnd = new(-405f, 35f);
            Vector2 bobberStart = bobber.anchoredPosition;
            tackleUnderwater = false;
            const float duration = 1.15f;
            float elapsed = 0f;
            splash.gameObject.SetActive(true);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                Vector2 fishPosition = Vector2.Lerp(fishStart, fishEnd, eased);
                fishPosition.y += Mathf.Sin(progress * Mathf.PI) * 170f;
                catchFish.anchoredPosition = fishPosition;
                catchFish.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(progress * Mathf.PI * 2f) * 12f
                );
                bobber.anchoredPosition = Vector2.Lerp(
                    bobberStart,
                    bobberRestPosition,
                    eased
                );
                RefreshTackleVisual();
                rod.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(18f, 35f, Mathf.Sin(progress * Mathf.PI))
                );
                UpdateFishingLine();
                yield return null;
            }
            splash.gameObject.SetActive(false);
            Play(FishingSound.Splash, 0.9f);
            Play(FishingSound.Catch, 0.95f);
            catchFish.gameObject.SetActive(false);
            bobber.gameObject.SetActive(false);
            activeLure.gameObject.SetActive(false);
            fishingLine.gameObject.SetActive(false);
            if (stateMachine.TryTransition(FishingState.CatchReveal))
            {
                ShowCatchPopup();
            }
        }

        private void ShowCatchPopup()
        {
            if (selectedFish == null)
            {
                stateMachine.TryTransition(FishingState.ReturningToIdle);
                StartFlow(ReturnToIdleRoutine(0.4f));
                return;
            }

            ConsumeCastWorm();
            FishingLocationDefinition currentLocation =
                FishingLocationCatalog.All[locationIndex];
            int caughtBefore = FishingLocationProgression.CaughtFishCount(
                currentLocation,
                definitions,
                collection
            );
            bool isNew = collection.RecordCatch(
                selectedFish.StableId,
                selectedLength,
                DateTime.UtcNow
            );
            int caughtAfter = FishingLocationProgression.CaughtFishCount(
                currentLocation,
                definitions,
                collection
            );
            pendingLocationCompletion =
                caughtBefore < currentLocation.FishCount
                && caughtAfter >= currentLocation.FishCount;
            catchRibbonText.text = isNew ? "NY FISK!" : "FISK FÅNGAD!";
            catchPicture.sprite = selectedFish.CatchSprite;
            catchNameText.text = selectedFish.DisplayName;
            catchRarityText.text = RarityLabel(selectedFish.Rarity);
            catchRarityText.color = RarityColor(selectedFish.Rarity);
            catchLengthText.text =
                Mathf.RoundToInt(selectedLength) + " CM  •  BRA JOBBAT!";
            int earnedStars = selectedFish.Rarity switch
            {
                FishRarity.Legendary => 4,
                FishRarity.Epic => 3,
                FishRarity.Rare => 2,
                _ => 1
            };
            GlobalStarWallet.Add(earnedStars);
            catchValueText.text =
                "BUTIKSVÄRDE "
                + FishSalePricing.Calculate(selectedFish, selectedLength)
                + " MYNT  •  +"
                + earnedStars
                + " ★";
            catchPopup.transform.SetAsLastSibling();
            catchPopup.SetActive(true);
            catchBounceClock = 0f;
            RefreshProgress();
            RefreshFishBook();
            LaunchConfetti();
            Play(FishingSound.Popup, 0.75f);
            Play(
                isNew
                    ? FishingSound.NewFish
                    : selectedFish.Rarity switch
                    {
                        FishRarity.Legendary => FishingSound.Rare,
                        FishRarity.Epic => FishingSound.Rare,
                        FishRarity.Rare => FishingSound.Rare,
                        FishRarity.Uncommon => FishingSound.Uncommon,
                        _ => FishingSound.Common
                    },
                0.95f
            );
            if (selectedFish.FishSound != null && AppPreferences.SoundEnabled)
            {
                audioSource.PlayOneShot(selectedFish.FishSound, 0.8f);
            }
            else if (AppPreferences.SoundEnabled)
            {
                audioSource.PlayOneShot(
                    FishingAudioLibrary.GetFishVoice(selectedFish.StableId),
                    0.72f
                );
            }
        }

        private void ContinueAfterCatch()
        {
            if (stateMachine.Current != FishingState.CatchReveal)
            {
                return;
            }
            Play(FishingSound.Button, 0.8f);
            catchPopup.SetActive(false);
            if (pendingLocationCompletion)
            {
                ShowLocationCompletePopup();
                return;
            }
            stateMachine.TryTransition(FishingState.ReturningToIdle);
            StartFlow(ReturnToIdleRoutine(0.5f));
        }

        private void ShowLocationCompletePopup()
        {
            FishingLocationDefinition location =
                FishingLocationCatalog.All[locationIndex];
            locationCompleteTitleText.text =
                "GRATTIS – " + location.DisplayName + " KLAR!";
            locationCompleteSubtitleText.text =
                "DU HAR FÅNGAT ALLA "
                + location.FishCount
                + " FISKAR I BANA "
                + (locationIndex + 1);
            for (
                int index = 0;
                index < locationCompleteFishImages.Count;
                index++
            )
            {
                int definitionIndex = location.FishIndices[index];
                locationCompleteFishImages[index].sprite =
                    definitions[definitionIndex].Sprite;
            }
            locationCompleteNextText.text =
                locationIndex < FishingLocationCatalog.All.Count - 1
                    ? "NÄSTA FISKEPLATS"
                    : "VÄLJ FISKEPLATS";
            locationCompletePopup.transform.SetAsLastSibling();
            locationCompletePopup.SetActive(true);
            LaunchConfetti();
            Play(FishingSound.NewFish, 1f);
        }

        private void ContinueFishingCurrentLocation()
        {
            pendingLocationCompletion = false;
            locationCompletePopup.SetActive(false);
            stateMachine.TryTransition(FishingState.ReturningToIdle);
            StartFlow(ReturnToIdleRoutine(0.35f));
            Play(FishingSound.Button, 0.8f);
        }

        private void ContinueToNextLocation()
        {
            pendingLocationCompletion = false;
            locationCompletePopup.SetActive(false);
            if (locationIndex < FishingLocationCatalog.All.Count - 1)
            {
                locationIndex++;
                ApplyLocation();
                stateMachine.TryTransition(FishingState.ReturningToIdle);
                StartFlow(ReturnToIdleRoutine(0.35f));
            }
            else
            {
                stateMachine.Reset();
                ResetVisuals();
                ShowLocationSelector();
            }
            Play(FishingSound.Button, 0.8f);
        }

        private IEnumerator ReturnToIdleRoutine(float delay)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            ResetVisuals();
            hintPanel.SetActive(false);
            selectedFish = null;
            stateMachine.TryTransition(FishingState.Idle);
        }

        private void StartFlow(IEnumerator routine)
        {
            if (flowRoutine != null)
            {
                StopCoroutine(flowRoutine);
            }
            flowRoutine = StartCoroutine(routine);
        }

        private void HandleStateChanged(FishingState previous, FishingState next)
        {
            if (mainButton == null)
            {
                return;
            }

            mainButton.transform.localScale = Vector3.one;
            switch (next)
            {
                case FishingState.Idle:
                    mainButton.interactable = true;
                    mainButtonText.text = hasCastOnce ? "KASTA IGEN" : "KASTA";
                    mainButtonImage.color = Color.white;
                    break;
                case FishingState.Casting:
                    mainButton.interactable = false;
                    mainButtonText.text = "KASTAR…";
                    break;
                case FishingState.WaitingForBite:
                    mainButton.interactable = true;
                    mainButtonText.text = "VÄNTA…";
                    mainButtonImage.color = RuntimeArt.Hex("#5A8CE8");
                    break;
                case FishingState.FishBiting:
                    mainButton.interactable = true;
                    mainButtonText.text = "DRA UPP!";
                    mainButtonImage.color = RuntimeArt.Hex("#FF8A2A");
                    break;
                case FishingState.ReelingIn:
                    mainButton.interactable = false;
                    mainButtonText.text = "DRAR UPP…";
                    break;
                case FishingState.CatchReveal:
                    mainButton.interactable = false;
                    break;
                case FishingState.ReturningToIdle:
                    mainButton.interactable = false;
                    mainButtonText.text = "KASTA IGEN";
                    break;
                case FishingState.Paused:
                    mainButton.interactable = false;
                    break;
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateSelectorSwipeInput();
            float time = Time.unscaledTime;
            if (stateMachine.Current == FishingState.WaitingForBite)
            {
                Vector2 position = bobberWaterPosition;
                position.y += Mathf.Sin(time * 2.2f) * 5f;
                bobber.anchoredPosition = position;
                RefreshTackleVisual();
                rod.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    rigLayout.rodAngle
                        + Mathf.Sin(time * 1.4f) * 1.5f
                );
                UpdateFishingLine();
            }
            else if (stateMachine.Current == FishingState.FishBiting)
            {
                Vector2 position = bobberWaterPosition;
                position.y += Mathf.Abs(Mathf.Sin(time * 9f)) * 22f - 24f;
                bobber.anchoredPosition = position;
                RefreshTackleVisual();
                rod.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    17f + Mathf.Sin(time * 7f) * 4f
                );
                float pulse = 1f + Mathf.Sin(time * 6f) * 0.045f;
                mainButton.transform.localScale = Vector3.one * pulse;
                UpdateFishingLine();
                UpdateBubbles();
            }

            UpdateActiveBubbles();
            UpdateCharacterAnimation();
            if (catchPopup != null && catchPopup.activeSelf)
            {
                catchBounceClock += Time.unscaledDeltaTime;
                float bounce = 1f
                    + Mathf.Abs(Mathf.Sin(catchBounceClock * 3.2f)) * 0.045f;
                catchPicture.rectTransform.localScale = Vector3.one * bounce;
            }
        }

        private void UpdateSelectorSwipeInput()
        {
            bool bookVisible = fishBookPanel != null && fishBookPanel.activeSelf;
            bool locationsVisible =
                locationPanel != null && locationPanel.activeSelf;
            if (locationsVisible)
            {
                selectorPointerDown = false;
                return;
            }
            if (!bookVisible)
            {
                selectorPointerDown = false;
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectorPointerDown = true;
                selectorPointerStart = Input.mousePosition;
            }
            if (!selectorPointerDown || !Input.GetMouseButtonUp(0))
            {
                return;
            }
            selectorPointerDown = false;
            float delta = ((Vector2)Input.mousePosition - selectorPointerStart).x;
            if (Mathf.Abs(delta) < 65f)
            {
                return;
            }
            int direction = delta < 0f ? 1 : -1;
            ChangeFishBookPage(direction);
        }

        private void UpdateBubbles()
        {
            bubbleSpawnClock -= Time.unscaledDeltaTime;
            if (bubbleSpawnClock > 0f)
            {
                return;
            }
            bubbleSpawnClock = 0.13f;
            for (int index = 0; index < bubbles.Count; index++)
            {
                if (bubbles[index].gameObject.activeSelf)
                {
                    continue;
                }
                bubbles[index].rectTransform.anchoredPosition =
                    bobberWaterPosition
                    + new Vector2(
                        UnityEngine.Random.Range(-70f, 70f),
                        UnityEngine.Random.Range(-50f, 10f)
                    );
                bubbleVelocities[index] = new Vector2(
                    UnityEngine.Random.Range(-18f, 18f),
                    UnityEngine.Random.Range(70f, 130f)
                );
                bubbles[index].color = Color.white;
                bubbles[index].gameObject.SetActive(true);
                if (index % 4 == 0)
                {
                    Play(FishingSound.Bubble, 0.28f);
                }
                break;
            }
        }

        private void UpdateActiveBubbles()
        {
            float delta = Time.unscaledDeltaTime;
            for (int index = 0; index < bubbles.Count; index++)
            {
                Image bubble = bubbles[index];
                if (!bubble.gameObject.activeSelf)
                {
                    continue;
                }
                bubble.rectTransform.anchoredPosition +=
                    bubbleVelocities[index] * delta;
                Color color = bubble.color;
                color.a -= delta * 0.85f;
                bubble.color = color;
                if (color.a <= 0f)
                {
                    bubble.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator PlayWaterRings()
        {
            if (AppPreferences.ReducedMotion)
            {
                yield break;
            }
            for (int index = 0; index < waterRings.Count; index++)
            {
                Text ring = waterRings[index];
                ring.gameObject.SetActive(true);
                ring.rectTransform.localScale = Vector3.one * 0.3f;
                Color color = ring.color;
                color.a = 0.9f;
                ring.color = color;
                float elapsed = 0f;
                while (elapsed < 0.55f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float progress = elapsed / 0.55f;
                    ring.rectTransform.localScale =
                        Vector3.one * Mathf.Lerp(0.3f, 1.5f, progress);
                    color.a = 0.9f * (1f - progress);
                    ring.color = color;
                    yield return null;
                }
                ring.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.08f);
            }
        }

        private void LaunchConfetti()
        {
            if (AppPreferences.ReducedMotion)
            {
                return;
            }
            Color[] colors =
            {
                RuntimeArt.Hex("#FFD43B"),
                RuntimeArt.Hex("#FF5E8E"),
                RuntimeArt.Hex("#55C8FF"),
                RuntimeArt.Hex("#61E08B"),
                RuntimeArt.Hex("#B76DFF")
            };
            for (int index = 0; index < confetti.Count; index++)
            {
                confetti[index].transform.SetAsLastSibling();
                confetti[index].Launch(
                    new Vector2(
                        UnityEngine.Random.Range(-320f, 320f),
                        UnityEngine.Random.Range(-40f, 210f)
                    ),
                    new Vector2(
                        UnityEngine.Random.Range(-260f, 260f),
                        UnityEngine.Random.Range(280f, 620f)
                    ),
                    colors[index % colors.Length]
                );
            }
        }

        private void OpenFishBook()
        {
            if (stateMachine.Current != FishingState.Idle)
            {
                return;
            }
            Play(FishingSound.FishBook, 0.82f);
            fishBookPage = Mathf.Clamp(
                fishBookPage,
                0,
                Mathf.Max(0, FishBookPageCount() - 1)
            );
            RefreshFishBook();
            fishBookPanel.transform.SetAsLastSibling();
            fishBookPanel.SetActive(true);
            mainButton.interactable = false;
        }

        private void CloseFishBook()
        {
            Play(FishingSound.Button, 0.75f);
            fishBookPanel.SetActive(false);
            mainButton.interactable = true;
        }

        private void RefreshFishBook()
        {
            int pageCount = FishBookPageCount();
            fishBookPage = Mathf.Clamp(fishBookPage, 0, pageCount - 1);
            fishBookPageText.text =
                "BANA " + (fishBookPage + 1) + " / " + pageCount;
            for (int index = 0; index < fishBookCards.Count; index++)
            {
                FishBookCard card = fishBookCards[index];
                FishingLocationDefinition location =
                    FishingLocationCatalog.All[fishBookPage];
                if (index >= location.FishIndices.Count)
                {
                    card.Picture.gameObject.SetActive(false);
                    card.Question.gameObject.SetActive(true);
                    card.Name.text = "???";
                    card.Details.text = "";
                    continue;
                }

                int definitionIndex = location.FishIndices[index];
                FishDefinition definition = definitions[definitionIndex];
                FishCatchRecord record = collection.Get(definition.StableId);
                bool discovered = record != null && record.caughtCount > 0;
                card.Picture.sprite = definition.Sprite;
                card.Picture.gameObject.SetActive(true);
                card.Picture.color = discovered
                    ? Color.white
                    : new Color(0.08f, 0.12f, 0.18f, 0.78f);
                card.Question.gameObject.SetActive(!discovered);
                card.Name.text = discovered ? definition.DisplayName : "???";
                card.Details.text = discovered
                    ? record.caughtCount
                        + " ST  •  BÄST "
                        + Mathf.RoundToInt(record.largestLengthCentimeters)
                        + " CM\n"
                        + RarityLabel(definition.Rarity)
                    : "HITTA FISKEN";
                card.RarityBand.color = discovered
                    ? RarityColor(definition.Rarity)
                    : RuntimeArt.Hex("#A8A8A8");
            }
        }

        private int FishBookPageCount()
        {
            return Mathf.Max(1, FishingLocationCatalog.All.Count);
        }

        private void ChangeFishBookPage(int direction)
        {
            if (fishBookPanel == null || !fishBookPanel.activeSelf)
            {
                return;
            }
            int pageCount = FishBookPageCount();
            fishBookPage = (fishBookPage + direction + pageCount) % pageCount;
            Play(FishingSound.Button, 0.65f);
            RefreshFishBook();
        }

        private void RefreshProgress()
        {
            progressText.text =
                "FISKAR "
                + Mathf.Clamp(collection.DiscoveredCount, 0, definitions.Count)
                + " / "
                + definitions.Count;
        }

        private void RefreshWormCount()
        {
            if (wormCountText != null)
            {
                int worms = FishingBaitInventory.WormCount;
                wormCountText.text = "× " + worms;
            }
        }

        private void ConsumeCastWorm()
        {
            if (!currentCastUsesWorm)
            {
                return;
            }
            currentCastUsesWorm = false;
            FishingBaitInventory.TryConsumeWorm();
            RefreshWormCount();
        }

        private void ToggleSound()
        {
            AppPreferences.SoundEnabled = !AppPreferences.SoundEnabled;
            soundButtonText.text =
                AppPreferences.SoundEnabled ? "LJUD PÅ" : "LJUD AV";
            if (AppPreferences.SoundEnabled)
            {
                Play(FishingSound.Button, 0.8f);
            }
        }

        private void ExitFishing()
        {
            Play(FishingSound.Button, 0.7f);
            Hide();
            onBack?.Invoke();
        }

        private void ShowHint(string message)
        {
            hintText.text = message;
            hintPanel.transform.SetAsLastSibling();
            hintPanel.SetActive(true);
        }

        private void ResetVisuals()
        {
            rod.localRotation = Quaternion.Euler(
                0f,
                0f,
                rigLayout.rodAngle
            );
            bobberRestPosition = RodTipPosition() + new Vector2(0f, -125f);
            bobber.anchoredPosition = bobberRestPosition;
            bobber.localRotation = Quaternion.identity;
            tackleUnderwater = false;
            ApplyActiveLureDesign();
            RefreshTackleVisual();
            fishingLine.gameObject.SetActive(true);
            catchFish.gameObject.SetActive(false);
            splash.gameObject.SetActive(false);
            catchPicture.rectTransform.localScale = Vector3.one;
            for (int index = 0; index < bubbles.Count; index++)
            {
                bubbles[index].gameObject.SetActive(false);
            }
            for (int index = 0; index < waterRings.Count; index++)
            {
                waterRings[index].gameObject.SetActive(false);
            }
            UpdateFishingLine();
        }

        private void RefreshTackleVisual()
        {
            if (bobber == null || activeLure == null)
            {
                return;
            }
            bool useLure = FishingLureCollection.Selected != null;
            bobber.gameObject.SetActive(!useLure);
            activeLure.gameObject.SetActive(useLure);
            if (useLure)
            {
                activeLure.anchoredPosition =
                    bobber.anchoredPosition
                    + (
                        tackleUnderwater
                            ? new Vector2(0f, -92f)
                            : Vector2.zero
                    );
            }
        }

        private void UpdateFishingLine()
        {
            Vector2 lineStart = RodTipPosition();
            Vector2 lineEnd =
                FishingLureCollection.Selected == null
                    ? bobber.anchoredPosition + new Vector2(0f, 18f)
                    : activeLure.anchoredPosition
                        + new Vector2(0f, activeLure.sizeDelta.y * 0.2f);
            Vector2 delta = lineEnd - lineStart;
            fishingLine.anchorMin = fishingLine.anchorMax = new Vector2(0.5f, 0.5f);
            fishingLine.pivot = new Vector2(0f, 0.5f);
            fishingLine.anchoredPosition = lineStart;
            fishingLine.sizeDelta = new Vector2(delta.magnitude, 7f);
            fishingLine.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
            );
        }

        private Vector2 RodTipPosition()
        {
            if (rod == null)
            {
                return Vector2.zero;
            }
            float angle = rod.localEulerAngles.z * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            return rod.anchoredPosition + direction * rod.sizeDelta.x;
        }

        private void UpdateCharacterAnimation()
        {
            if (
                rod == null
                || frontArm == null
                || restingArm == null
                || characterHeadImage == null
            )
            {
                return;
            }

            float angle = rod.localEulerAngles.z * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 normal = new(-direction.y, direction.x);
            float handMotion = AppPreferences.ReducedMotion
                ? 0f
                : Mathf.Sin(Time.unscaledTime * 2.4f) * 2f;

            PositionArm(
                frontArm,
                rigLayout.fishingShoulder,
                rod.anchoredPosition
                    + direction * rigLayout.gripAlongRod
                    + normal * (rigLayout.gripNormalOffset + handMotion),
                rigLayout.fishingHandOverlap
            );
            PositionArm(
                restingArm,
                rigLayout.restingShoulder,
                rigLayout.restingHand,
                rigLayout.restingHandOverlap
            );

            float now = Time.unscaledTime;
            bool blinking = false;
            if (!AppPreferences.ReducedMotion)
            {
                if (blinkStartedAt < 0f && now >= nextBlinkAt)
                {
                    blinkStartedAt = now;
                }
                if (blinkStartedAt >= 0f)
                {
                    float blinkProgress = (now - blinkStartedAt) / 0.24f;
                    if (blinkProgress >= 1f)
                    {
                        blinkStartedAt = -1f;
                        nextBlinkAt = now + UnityEngine.Random.Range(2.4f, 4.4f);
                    }
                    else
                    {
                        blinking = true;
                    }
                }
            }

            if (blinking)
            {
                characterHeadImage.sprite = headBlinkSprite;
            }
            else if (
                stateMachine.Current == FishingState.ReelingIn
                || stateMachine.Current == FishingState.FishBiting
            )
            {
                characterHeadImage.sprite = headReelSprite;
            }
            else if (
                stateMachine.Current == FishingState.CatchReveal
                || stateMachine.Current == FishingState.ReturningToIdle
            )
            {
                characterHeadImage.sprite = headHappySprite;
            }
            else
            {
                characterHeadImage.sprite = headIdleSprite;
            }

            float headBob = AppPreferences.ReducedMotion
                ? 0f
                : Mathf.Sin(now * 1.7f) * 2f;
            characterHead.anchoredPosition = new Vector2(-585f, 142f + headBob);
            characterHead.localRotation = Quaternion.Euler(
                0f,
                0f,
                AppPreferences.ReducedMotion
                    ? 0f
                    : Mathf.Sin(now * 1.15f) * 0.8f
            );
            float breathing = AppPreferences.ReducedMotion
                ? 1f
                : 1f + Mathf.Sin(now * 1.6f) * 0.008f;
            characterBody.localScale = new Vector3(1f, breathing, 1f);
        }

        private static void PositionArm(
            RectTransform arm,
            Vector2 shoulder,
            Vector2 hand,
            float handOverlap = 34f
        )
        {
            Vector2 delta = hand - shoulder;
            arm.anchoredPosition = shoulder;
            arm.sizeDelta = new Vector2(
                Mathf.Max(90f, delta.magnitude + handOverlap),
                arm.sizeDelta.y
            );
            arm.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
            );
        }

        private void Play(FishingSound sound, float volume)
        {
            if (!AppPreferences.SoundEnabled)
            {
                return;
            }
            audioSource.PlayOneShot(FishingAudioLibrary.Get(sound), volume);
        }

        private static string RarityLabel(FishRarity rarity)
        {
            return rarity switch
            {
                FishRarity.Legendary => "LEGENDARISK  ★★★★★",
                FishRarity.Epic => "MYCKET SÄLLSYNT  ★★★★",
                FishRarity.Rare => "SÄLLSYNT  ★★★",
                FishRarity.Uncommon => "OVANLIG  ★★",
                _ => "VANLIG  ★"
            };
        }

        private static Color RarityColor(FishRarity rarity)
        {
            return rarity switch
            {
                FishRarity.Legendary => RuntimeArt.Hex("#E14BCE"),
                FishRarity.Epic => RuntimeArt.Hex("#F05A28"),
                FishRarity.Rare => RuntimeArt.Hex("#E4A400"),
                FishRarity.Uncommon => RuntimeArt.Hex("#8B56D9"),
                _ => RuntimeArt.Hex("#2A87CE")
            };
        }

        private void OnApplicationPause(bool paused)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (paused)
            {
                stateMachine.Pause();
            }
            else
            {
                stateMachine.Resume();
            }
        }

        private void OnDestroy()
        {
            stateMachine.Changed -= HandleStateChanged;
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
                    "FishingPanel_" + name,
                    RuntimeArt.Hex("#5A3B28"),
                    fill,
                    Mathf.RoundToInt(size.x),
                    Mathf.RoundToInt(size.y),
                    38,
                    8
                )
            );
            SetRect(
                panel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size
            );
            return panel;
        }

        private Button CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color fill,
            int fontSize
        )
        {
            GameObject buttonObject = new(
                string.IsNullOrEmpty(label) ? "Knapp" : label + "-knapp"
            );
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "FishingButton_" + label + fill,
                RuntimeArt.Hex("#4A3424"),
                fill,
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y),
                38,
                8
            );
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(0.72f, 0.72f, 0.72f, 0.9f);
            button.colors = colors;
            SetRect(
                image.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size
            );
            if (!string.IsNullOrEmpty(label))
            {
                Text text = CreateText(
                    "Text",
                    buttonObject.transform,
                    label,
                    fontSize,
                    Color.white
                );
                Stretch(text.rectTransform);
                AddOutline(text, RuntimeArt.Hex("#4A3424"), 3f);
            }
            return button;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            Color color
        )
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

        private static Image CreateImage(
            string name,
            Transform parent,
            Sprite sprite
        )
        {
            GameObject imageObject = new(name);
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
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
}
