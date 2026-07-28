using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class PuzzleGameController : MonoBehaviour
    {
        private static readonly Vector2 BoardPosition = new(0f, -35f);
        private static readonly Vector2 BoardSize = new(1140f, 760f);
        private const float SourceWidth = 1536f;
        private const float SourceHeight = 1024f;
        private const float SidePieceScale = 0.34f;
        private const float SideRackX = 770f;
        private const int PuzzleCount = 9;
        private const int CardsPerPage = 3;

        private static readonly string[] PuzzleTitles =
        {
            "SKOGSVÄNNERNA",
            "SPINDELVÄNNEN",
            "BILKOMPISARNA",
            "GLADA TRAKTORER",
            "KORALLREVET",
            "NORDISKA HAVET",
            "BYGGMASKINERNA",
            "BRANDBILARNA",
            "DINOSAURIEVÄNNER"
        };

        private static readonly string[] PuzzlePrefixes =
        {
            "skogsvanner",
            "spindel",
            "bilar",
            "traktorer",
            "korall",
            "havsfiskar",
            "gravmaskiner",
            "brandbilar",
            "dinosaurier"
        };

        private readonly Vector4[] pieceCrops =
        {
            new(0f, 0f, 617f, 594f),
            new(512f, 0f, 513f, 513f),
            new(934f, 0f, 602f, 594f),
            new(0f, 512f, 513f, 512f),
            new(422f, 431f, 693f, 593f),
            new(1024f, 512f, 512f, 512f)
        };

        private GameObject hubRoot;
        private GameObject playRoot;
        private GameObject completionPanel;
        private RectTransform playRect;
        private RectTransform pieceRoot;
        private RectTransform hubSafeRoot;
        private RectTransform playSafeRoot;
        private Font font;
        private Action onBack;
        private Text puzzleTitleText;
        private Text hubScoreText;
        private Text playScoreText;
        private Text profileStarsText;
        private Text soundButtonText;
        private Text completionTimeText;
        private Text completionAwardText;
        private Text puzzlePageText;
        private Image puzzleBackground;
        private Image puzzleGuide;
        private Image completionPreview;
        private Button previousPuzzlePageButton;
        private Button nextPuzzlePageButton;
        private Button nextPuzzleButton;
        private Button helpButton;
        private Text nextPuzzleButtonText;
        private Image tutorialHand;
        private Sprite tutorialHandOpen;
        private Sprite tutorialHandPinched;
        private Coroutine tutorialRoutine;
        private readonly List<Button> puzzleCardButtons = new();
        private readonly List<GameObject> puzzleCardLocks = new();
        private readonly List<Text> puzzleCardActionTexts = new();
        private readonly List<GameObject> puzzleCards = new();
        private AudioSource audioSource;
        private AudioClip placementSound;
        private AudioClip completionSound;
        private int placedPieces;
        private int currentPuzzle = 1;
        private int currentPuzzlePage;
        private float puzzleStartedAt;

        public static PuzzleGameController Create(
            Transform parent,
            Font uiFont,
            Action returnAction
        )
        {
            GameObject host = new("Pusselspelet", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            Stretch(host.GetComponent<RectTransform>());
            PuzzleGameController controller = host.AddComponent<PuzzleGameController>();
            controller.font = uiFont;
            controller.onBack = returnAction;
            controller.Build();
            return controller;
        }

        public void ShowHub()
        {
            gameObject.SetActive(true);
            hubRoot.SetActive(true);
            playRoot.SetActive(false);
            currentPuzzlePage = (currentPuzzle - 1) / CardsPerPage;
            RefreshPuzzleScore();
            RefreshPuzzleCards();
            RefreshPuzzlePage();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void StartPuzzle()
        {
            StartPuzzle(1);
        }

        public void StartPuzzle(int puzzleNumber)
        {
            if (!PuzzleProgress.IsPuzzleUnlocked(puzzleNumber))
            {
                return;
            }

            currentPuzzle = Mathf.Clamp(puzzleNumber, 1, PuzzleCount);
            gameObject.SetActive(true);
            hubRoot.SetActive(false);
            playRoot.SetActive(true);
            ApplyCurrentPuzzleArt();
            BuildPieces();
            playSafeRoot.SetAsLastSibling();
            ResetPuzzle();
            helpButton.gameObject.SetActive(currentPuzzle == 1);
            if (
                currentPuzzle == 1
                && PuzzleProgress.ShouldShowDragTutorial
            )
            {
                PuzzleProgress.MarkDragTutorialSeen();
                StartHandTutorial();
            }
        }

        public void PiecePlaced(PuzzlePieceDrag piece)
        {
            placedPieces++;
            audioSource.PlayOneShot(placementSound, 0.82f);
            if (placedPieces >= 6)
            {
                float elapsedSeconds = Mathf.Max(
                    0f,
                    Time.realtimeSinceStartup - puzzleStartedAt
                );
                PuzzleProgress.RecordCompletion(
                    currentPuzzle,
                    elapsedSeconds
                );
                int earnedStars = PuzzleProgress.CalculateStars(elapsedSeconds);
                GlobalStarWallet.Add(earnedStars);
                completionAwardText.text =
                    "+" + earnedStars + (earnedStars == 1
                        ? " STJÄRNA"
                        : " STJÄRNOR");
                completionTimeText.text = "DIN TID: " + FormatTime(elapsedSeconds);
                nextPuzzleButtonText.text = currentPuzzle < PuzzleCount
                    ? "NÄSTA PUSSEL"
                    : "PUSSELVÄLJARE";
                RefreshPuzzleScore();
                RefreshPuzzleCards();
                completionPanel.transform.SetAsLastSibling();
                completionPanel.SetActive(true);
                audioSource.PlayOneShot(completionSound, 0.9f);
            }
        }

        private void Build()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            placementSound = CreateBlopSound();
            completionSound = CreateCompletionSound();
            BuildHub();
            BuildPuzzleTable();
            Hide();
        }

        private void BuildHub()
        {
            hubRoot = new GameObject("Pusselväljare", typeof(RectTransform));
            hubRoot.transform.SetParent(transform, false);
            Stretch(hubRoot.GetComponent<RectTransform>());
            SwipePageHandler swipeHandler = hubRoot.AddComponent<SwipePageHandler>();
            swipeHandler.Initialize(ChangePuzzlePage);

            Image background = CreateImage(
                "Bakgrund",
                hubRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            Image shade = CreateImage("Toning", hubRoot.transform, null);
            shade.color = new Color(0.08f, 0.04f, 0.2f, 0.48f);
            Stretch(shade.rectTransform);

            hubSafeRoot = CreateSafeRoot(hubRoot.transform, "Säker pusselmeny");
            Button back = CreateButton(
                hubSafeRoot,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            back.onClick.AddListener(() => onBack?.Invoke());
            SetRect(
                back.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(85f, -58f),
                new Vector2(150f, 90f)
            );

            Text title = CreateText(
                "Titel",
                hubSafeRoot,
                "VÄLJ PUSSEL",
                64,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(900f, 105f));
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            Image wallet = CreatePanel(
                "Pusselpoäng",
                hubSafeRoot,
                Vector2.zero,
                new Vector2(285f, 86f),
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                wallet.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-160f, -58f),
                new Vector2(285f, 86f)
            );
            hubScoreText = CreateText(
                "Pusselpoäng",
                wallet.transform,
                "★ 0",
                34,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(
                hubScoreText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(250f, 70f)
            );

            float[] cardPositions = { -600f, 0f, 600f };
            for (int index = 0; index < PuzzleCount; index++)
            {
                int puzzleNumber = index + 1;
                Button card = CreatePuzzleCard(
                    new Vector2(cardPositions[index % CardsPerPage], -35f),
                    PuzzleTitles[index],
                    "6 BITAR • LÄTT",
                    "Art/Puzzles/" + PuzzlePrefixes[index] + "_puzzle",
                    false,
                    () => StartPuzzle(puzzleNumber)
                );
                puzzleCards.Add(card.gameObject);
                puzzleCardButtons.Add(card);
                puzzleCardActionTexts.Add(
                    card.transform.Find("Åtgärd").GetComponent<Text>()
                );
                puzzleCardLocks.Add(
                    puzzleNumber == 1 ? null : CreateLock(card.transform)
                );
            }

            previousPuzzlePageButton = CreateButton(
                hubSafeRoot,
                "‹",
                new Vector2(-895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            previousPuzzlePageButton.onClick.AddListener(
                () => ChangePuzzlePage(-1)
            );
            nextPuzzlePageButton = CreateButton(
                hubSafeRoot,
                "›",
                new Vector2(895f, -35f),
                new Vector2(82f, 130f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            nextPuzzlePageButton.onClick.AddListener(
                () => ChangePuzzlePage(1)
            );
            puzzlePageText = CreateText(
                "Pusselsida",
                hubSafeRoot,
                "",
                27,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                puzzlePageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -480f),
                new Vector2(320f, 50f)
            );

            RefreshPuzzleCards();
            RefreshPuzzlePage();
        }

        private Button CreatePuzzleCard(
            Vector2 position,
            string title,
            string subtitle,
            string imagePath,
            bool locked,
            UnityEngine.Events.UnityAction action
        )
        {
            Button card = CreateButton(
                hubRoot.transform,
                "",
                position,
                new Vector2(510f, 700f),
                locked ? RuntimeArt.Hex("#6F6C78") : RuntimeArt.Hex("#2A66DB"),
                34
            );
            if (action != null)
            {
                card.onClick.AddListener(action);
            }
            else
            {
                card.interactable = false;
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                Image preview = CreateImage(
                    "Pusselbild",
                    card.transform,
                    RuntimeArt.LoadSprite(imagePath)
                );
                preview.type = Image.Type.Simple;
                preview.preserveAspect = true;
                SetRect(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 145f), new Vector2(465f, 315f));
            }
            else
            {
                Text pieces = CreateText(
                    "Pusselikon",
                    card.transform,
                    "◆  ◇  ◆",
                    72,
                    RuntimeArt.Hex("#D8D3DD")
                );
                SetRect(pieces.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 145f), new Vector2(430f, 250f));
            }

            Text cardTitle = CreateText("Namn", card.transform, title, 38, Color.white);
            SetRect(cardTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(460f, 75f));
            AddOutline(cardTitle, RuntimeArt.Hex("#40245F"), 3f);
            Text cardSubtitle = CreateText("Info", card.transform, subtitle, 27, RuntimeArt.Hex("#FFF3AD"));
            SetRect(cardSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -145f), new Vector2(450f, 55f));
            Image actionButton = CreateImage(
                "Spelaknapp",
                card.transform,
                RuntimeArt.RoundedRectangleSprite(
                    "PuzzlePlayButton",
                    RuntimeArt.Hex("#B93B18"),
                    RuntimeArt.Hex("#FF6B35"),
                    330,
                    82,
                    28,
                    7
                )
            );
            actionButton.raycastTarget = false;
            SetRect(actionButton.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -255f), new Vector2(330f, 82f));
            Text actionText = CreateText("Åtgärd", card.transform, locked ? "LÅST" : "SPELA", 34, Color.white);
            SetRect(actionText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -255f), new Vector2(430f, 70f));
            AddOutline(actionText, RuntimeArt.Hex("#40245F"), 3f);

            if (locked)
            {
                CreateLock(card.transform);
            }
            return card;
        }

        private void BuildPuzzleTable()
        {
            playRoot = new GameObject("Pusselbord", typeof(RectTransform));
            playRoot.transform.SetParent(transform, false);
            playRect = playRoot.GetComponent<RectTransform>();
            Stretch(playRect);

            puzzleBackground = CreateImage(
                "Pusselbakgrund",
                playRoot.transform,
                null
            );
            puzzleBackground.type = Image.Type.Simple;
            puzzleBackground.preserveAspect = false;
            puzzleBackground.color = RuntimeArt.Hex("#61C8D9");
            Stretch(puzzleBackground.rectTransform);

            Image leftRack = CreatePanel(
                "Vänster bitfält",
                playRoot.transform,
                new Vector2(-SideRackX, BoardPosition.y),
                new Vector2(250f, BoardSize.y + 36f),
                RuntimeArt.Hex("#FFF0C2")
            );
            leftRack.raycastTarget = false;
            Image rightRack = CreatePanel(
                "Höger bitfält",
                playRoot.transform,
                new Vector2(SideRackX, BoardPosition.y),
                new Vector2(250f, BoardSize.y + 36f),
                RuntimeArt.Hex("#FFF0C2")
            );
            rightRack.raycastTarget = false;

            playSafeRoot = CreateSafeRoot(playRoot.transform, "Säker pussel-HUD");
            Button back = CreateButton(
                playSafeRoot,
                "←",
                Vector2.zero,
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            back.onClick.AddListener(ShowHub);
            SetRect(
                back.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(85f, -58f),
                new Vector2(150f, 90f)
            );

            Image profile = CreatePanel(
                "Spelarprofil",
                playSafeRoot,
                Vector2.zero,
                new Vector2(330f, 90f),
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                profile.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(340f, -58f),
                new Vector2(330f, 90f)
            );
            Image avatar = CreateImage(
                "Profilbild",
                profile.transform,
                RuntimeArt.LoadSprite("Art/Fishing/Character/head_idle")
            );
            avatar.preserveAspect = true;
            avatar.raycastTarget = false;
            SetRect(
                avatar.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(49f, 0f),
                new Vector2(76f, 76f)
            );
            Text profileName = CreateText(
                "Profilnamn",
                profile.transform,
                string.IsNullOrEmpty(PlayerProfile.Username)
                    ? "ARI"
                    : PlayerProfile.Username.ToUpperInvariant(),
                25,
                RuntimeArt.Hex("#4A266C")
            );
            profileName.alignment = TextAnchor.MiddleLeft;
            SetRect(
                profileName.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(190f, 19f),
                new Vector2(190f, 38f)
            );
            profileStarsText = CreateText(
                "Globala stjärnor",
                profile.transform,
                "★ 0",
                26,
                RuntimeArt.Hex("#D17A00")
            );
            profileStarsText.alignment = TextAnchor.MiddleLeft;
            SetRect(
                profileStarsText.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(190f, -21f),
                new Vector2(190f, 38f)
            );

            Image titlePanel = CreatePanel(
                "Pusselrubrik",
                playSafeRoot,
                Vector2.zero,
                new Vector2(500f, 90f),
                RuntimeArt.Hex("#7A5AA6")
            );
            SetRect(
                titlePanel.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -58f),
                new Vector2(500f, 90f)
            );
            puzzleTitleText = CreateText(
                "Pusseltitel",
                titlePanel.transform,
                "PUSSEL",
                50,
                Color.white
            );
            Stretch(puzzleTitleText.rectTransform);
            AddOutline(puzzleTitleText, RuntimeArt.Hex("#40245F"), 3f);

            Button sound = CreateButton(
                playSafeRoot,
                "",
                Vector2.zero,
                new Vector2(170f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                28
            );
            SetRect(
                sound.image.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-95f, -58f),
                new Vector2(170f, 90f)
            );
            soundButtonText = CreateText(
                "Text",
                sound.transform,
                AppPreferences.SoundEnabled ? "LJUD PÅ" : "LJUD AV",
                27,
                Color.white
            );
            Stretch(soundButtonText.rectTransform);
            AddOutline(soundButtonText, RuntimeArt.Hex("#40245F"), 2f);
            sound.onClick.AddListener(ToggleSound);

            helpButton = CreateButton(
                playSafeRoot,
                "HJÄLP",
                Vector2.zero,
                new Vector2(170f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                27
            );
            SetRect(
                helpButton.image.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-280f, -58f),
                new Vector2(170f, 90f)
            );
            helpButton.onClick.AddListener(StartHandTutorial);

            Image board = CreatePanel(
                "Pusselram",
                playRoot.transform,
                BoardPosition,
                BoardSize + new Vector2(36f, 36f),
                RuntimeArt.Hex("#F3D49B")
            );
            puzzleGuide = CreateImage(
                "Svag motivguide",
                board.transform,
                RuntimeArt.LoadSprite("Art/Puzzles/skogsvanner_puzzle")
            );
            puzzleGuide.type = Image.Type.Simple;
            puzzleGuide.preserveAspect = false;
            puzzleGuide.color = new Color(1f, 1f, 1f, 0.32f);
            SetRect(puzzleGuide.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, BoardSize);

            GameObject pieceLayer = new(
                "Pusselbitar",
                typeof(RectTransform)
            );
            pieceLayer.transform.SetParent(playRoot.transform, false);
            pieceRoot = pieceLayer.GetComponent<RectTransform>();
            Stretch(pieceRoot);

            Image playScore = CreatePanel(
                "Stjärnpoäng under pussel",
                playSafeRoot,
                Vector2.zero,
                new Vector2(235f, 82f),
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                playScore.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(
                    0f,
                    BoardPosition.y + (BoardSize.y + 36f) * 0.5f
                ),
                new Vector2(235f, 82f)
            );
            playScoreText = CreateText(
                "Stjärnpoäng",
                playScore.transform,
                "★ 0",
                34,
                RuntimeArt.Hex("#4A266C")
            );
            Stretch(playScoreText.rectTransform);

            Image instruction = CreatePanel(
                "Pusselinstruktion",
                playSafeRoot,
                Vector2.zero,
                new Vector2(1040f, 58f),
                RuntimeArt.Hex("#6A438D")
            );
            SetRect(
                instruction.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -470f),
                new Vector2(1040f, 58f)
            );
            Text instructionText = CreateText(
                "Text",
                instruction.transform,
                "Dra en bit från sidan och släpp den på rätt plats!",
                27,
                Color.white
            );
            Stretch(instructionText.rectTransform);

            tutorialHandOpen = RuntimeArt.LoadSprite(
                "Art/UI/Tutorial/drag_hand_open"
            );
            tutorialHandPinched = RuntimeArt.LoadSprite(
                "Art/UI/Tutorial/drag_hand_pinched"
            );
            tutorialHand = CreateImage(
                "Animerad hjälphand",
                playRoot.transform,
                tutorialHandOpen
            );
            tutorialHand.preserveAspect = true;
            tutorialHand.raycastTarget = false;
            SetRect(
                tutorialHand.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(210f, 210f)
            );
            tutorialHand.gameObject.SetActive(false);

            BuildCompletionPanel();
            playSafeRoot.SetAsLastSibling();
        }

        private void StartHandTutorial()
        {
            if (currentPuzzle != 1 || !playRoot.activeInHierarchy)
            {
                return;
            }

            StopHandTutorial();
            tutorialRoutine = StartCoroutine(ShowFirstPieceHandTutorial());
        }

        private void StopHandTutorial()
        {
            if (tutorialRoutine != null)
            {
                StopCoroutine(tutorialRoutine);
                tutorialRoutine = null;
            }
            if (tutorialHand != null)
            {
                tutorialHand.gameObject.SetActive(false);
            }
            foreach (PuzzlePieceDrag piece in
                playRoot.GetComponentsInChildren<PuzzlePieceDrag>(true))
            {
                piece.CancelTutorial();
            }
        }

        private IEnumerator ShowFirstPieceHandTutorial()
        {
            yield return new WaitForSecondsRealtime(0.4f);
            PuzzlePieceDrag[] pieces =
                pieceRoot.GetComponentsInChildren<PuzzlePieceDrag>(true);
            PuzzlePieceDrag tutorialPiece = Array.Find(
                pieces,
                piece => !piece.IsPlaced
            );
            if (tutorialPiece != null)
            {
                yield return tutorialPiece.PlayHandTutorial(
                    tutorialHand,
                    tutorialHandOpen,
                    tutorialHandPinched
                );
            }
            tutorialRoutine = null;
        }

        public void PlayTutorialPickupSound()
        {
            if (AppPreferences.SoundEnabled)
            {
                audioSource.PlayOneShot(placementSound, 0.48f);
            }
        }

        public void PlayTutorialPlacementSound()
        {
            if (AppPreferences.SoundEnabled)
            {
                audioSource.PlayOneShot(placementSound, 0.82f);
            }
        }

        private void BuildPieces()
        {
            foreach (PuzzlePieceDrag oldPiece in playRoot.GetComponentsInChildren<PuzzlePieceDrag>(true))
            {
                Destroy(oldPiece.gameObject);
            }

            float scale = BoardSize.x / SourceWidth;
            float[] sideYs = { 190f, -45f, -280f };
            string artPrefix = PuzzlePrefixes[currentPuzzle - 1];
            for (int i = 0; i < pieceCrops.Length; i++)
            {
                Vector4 crop = pieceCrops[i];
                Image pieceImage = CreateImage(
                    "Pusselbit " + (i + 1),
                    pieceRoot,
                    RuntimeArt.LoadSprite(
                        "Art/Puzzles/" + artPrefix + "_piece_" + (i + 1)
                    )
                );
                pieceImage.type = Image.Type.Simple;
                pieceImage.preserveAspect = false;
                Vector2 pieceSize = new(crop.z * scale, crop.w * scale);
                Vector2 targetPosition = new(
                    BoardPosition.x - BoardSize.x * 0.5f + (crop.x + crop.z * 0.5f) * scale,
                    BoardPosition.y + BoardSize.y * 0.5f - (crop.y + crop.w * 0.5f) * scale
                );
                bool leftSide = i < 3;
                Vector2 startPosition = new(
                    leftSide ? -SideRackX : SideRackX,
                    sideYs[i % 3]
                );
                SetRect(
                    pieceImage.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    startPosition,
                    pieceSize
                );
                pieceImage.rectTransform.localScale =
                    Vector3.one * SidePieceScale;

                PuzzlePieceDrag drag = pieceImage.gameObject.AddComponent<PuzzlePieceDrag>();
                drag.Initialize(
                    this,
                    playRect,
                    startPosition,
                    targetPosition,
                    SidePieceScale,
                    Mathf.Clamp(Mathf.Min(pieceSize.x, pieceSize.y) * 0.42f, 155f, 205f)
                );
            }
        }

        private void BuildCompletionPanel()
        {
            Image shade = CreateImage("Färdigt", playRoot.transform, null);
            shade.color = new Color(0.12f, 0.04f, 0.24f, 0.78f);
            Stretch(shade.rectTransform);
            completionPanel = shade.gameObject;

            Image card = CreatePanel(
                "Färdigkort",
                shade.transform,
                Vector2.zero,
                new Vector2(1120f, 720f),
                RuntimeArt.Hex("#FFF3AD")
            );
            Text title = CreateText("Färdig", card.transform, "PUSSLET ÄR KLART!", 64, RuntimeArt.Hex("#4A266C"));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 290f), new Vector2(920f, 90f));

            Image previewFrame = CreatePanel(
                "Färdigt pussel",
                card.transform,
                new Vector2(-245f, 25f),
                new Vector2(590f, 410f),
                Color.white
            );
            completionPreview = CreateImage(
                "Förhandsvisning av färdigt pussel",
                previewFrame.transform,
                RuntimeArt.LoadSprite("Art/Puzzles/skogsvanner_puzzle")
            );
            completionPreview.type = Image.Type.Simple;
            completionPreview.preserveAspect = false;
            SetRect(
                completionPreview.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(558f, 372f)
            );

            Text stars = CreateText("Stjärnor", card.transform, "★  ★  ★", 76, RuntimeArt.Hex("#F2A900"));
            SetRect(stars.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 175f), new Vector2(430f, 90f));

            completionAwardText = CreateText(
                "Uppmuntran",
                card.transform,
                "+1 STJÄRNA",
                44,
                RuntimeArt.Hex("#D17A00")
            );
            SetRect(
                completionAwardText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, 80f),
                new Vector2(430f, 75f)
            );
            completionTimeText = CreateText(
                "Pusseltid",
                card.transform,
                "DIN TID: 0:00",
                31,
                RuntimeArt.Hex("#5A376E")
            );
            SetRect(
                completionTimeText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, 10f),
                new Vector2(430f, 58f)
            );

            Button again = CreateButton(card.transform, "LÄGG IGEN", new Vector2(-245f, -285f), new Vector2(450f, 105f), RuntimeArt.Hex("#FF6B35"), 38);
            again.onClick.AddListener(ResetPuzzle);
            nextPuzzleButton = CreateButton(card.transform, "NÄSTA PUSSEL", new Vector2(245f, -285f), new Vector2(450f, 105f), RuntimeArt.Hex("#5A8CE8"), 32);
            nextPuzzleButtonText = nextPuzzleButton.transform
                .Find("Text")
                .GetComponent<Text>();
            nextPuzzleButton.onClick.AddListener(GoToNextPuzzle);
            completionPanel.SetActive(false);
        }

        private void GoToNextPuzzle()
        {
            if (
                currentPuzzle < PuzzleCount
                && PuzzleProgress.IsPuzzleUnlocked(currentPuzzle + 1)
            )
            {
                StartPuzzle(currentPuzzle + 1);
                return;
            }
            ShowHub();
        }

        private void ApplyCurrentPuzzleArt()
        {
            int puzzleIndex = Mathf.Clamp(currentPuzzle - 1, 0, PuzzleCount - 1);
            string artPath =
                "Art/Puzzles/" + PuzzlePrefixes[puzzleIndex] + "_puzzle";
            Sprite art = RuntimeArt.LoadSprite(artPath);
            puzzleGuide.sprite = art;
            completionPreview.sprite = art;
        }

        private void ResetPuzzle()
        {
            StopHandTutorial();
            placedPieces = 0;
            puzzleStartedAt = Time.realtimeSinceStartup;
            completionPanel.SetActive(false);
            RefreshPuzzleScore();
            foreach (PuzzlePieceDrag piece in playRoot.GetComponentsInChildren<PuzzlePieceDrag>(true))
            {
                piece.ResetPiece();
            }
        }

        private void RefreshPuzzleScore()
        {
            if (hubScoreText != null)
            {
                hubScoreText.text = "★ " + GlobalStarWallet.Balance;
            }
            if (playScoreText != null)
            {
                playScoreText.text = "★ " + GlobalStarWallet.Balance;
            }
            if (profileStarsText != null)
            {
                profileStarsText.text = "★ " + GlobalStarWallet.Balance;
            }
        }

        private void ToggleSound()
        {
            AppPreferences.SoundEnabled = !AppPreferences.SoundEnabled;
            if (soundButtonText != null)
            {
                soundButtonText.text = AppPreferences.SoundEnabled
                    ? "LJUD PÅ"
                    : "LJUD AV";
            }
        }

        private void RefreshPuzzleCards()
        {
            for (int index = 0; index < puzzleCardButtons.Count; index++)
            {
                int puzzleNumber = index + 1;
                bool unlocked = PuzzleProgress.IsPuzzleUnlocked(puzzleNumber);
                puzzleCardButtons[index].interactable = unlocked;
                if (puzzleCardLocks[index] != null)
                {
                    puzzleCardLocks[index].SetActive(!unlocked);
                }
                puzzleCardActionTexts[index].text = unlocked
                    ? "SPELA"
                    : "LÅST";
            }
        }

        private void ChangePuzzlePage(int direction)
        {
            int pageCount = Mathf.CeilToInt(PuzzleCount / (float)CardsPerPage);
            currentPuzzlePage = Mathf.Clamp(
                currentPuzzlePage + direction,
                0,
                pageCount - 1
            );
            RefreshPuzzlePage();
        }

        private void RefreshPuzzlePage()
        {
            int pageCount = Mathf.CeilToInt(PuzzleCount / (float)CardsPerPage);
            for (int index = 0; index < puzzleCards.Count; index++)
            {
                puzzleCards[index].SetActive(
                    index / CardsPerPage == currentPuzzlePage
                );
            }
            if (previousPuzzlePageButton != null)
            {
                previousPuzzlePageButton.interactable = currentPuzzlePage > 0;
            }
            if (nextPuzzlePageButton != null)
            {
                nextPuzzlePageButton.interactable =
                    currentPuzzlePage < pageCount - 1;
            }
            if (puzzlePageText != null)
            {
                puzzlePageText.text = "";
            }
        }

        private static string FormatTime(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds));
            return (totalSeconds / 60) + ":" + (totalSeconds % 60).ToString("00");
        }

        private static AudioClip CreateBlopSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float t = time / duration;
                float frequency = Mathf.Lerp(430f, 690f, t);
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t)) * (1f - t);
                samples[i] =
                    (Mathf.Sin(time * frequency * Mathf.PI * 2f)
                    + Mathf.Sin(time * frequency * Mathf.PI * 4f) * 0.22f)
                    * envelope
                    * 0.32f;
            }
            AudioClip clip = AudioClip.Create("Barnvänligt pusselblopp", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateCompletionSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.55f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] notes = { 523.25f, 659.25f, 783.99f };
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float sample = 0f;
                for (int note = 0; note < notes.Length; note++)
                {
                    float noteStart = note * 0.11f;
                    if (time < noteStart)
                    {
                        continue;
                    }
                    float age = time - noteStart;
                    sample += Mathf.Sin(age * notes[note] * Mathf.PI * 2f)
                        * Mathf.Exp(-age * 5.5f)
                        * 0.16f;
                }
                samples[i] = sample;
            }
            AudioClip clip = AudioClip.Create("Glatt pusselklart-ljud", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private GameObject CreateLock(Transform parent)
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
                    "PuzzlePanel_" + name,
                    RuntimeArt.Hex("#40245F"),
                    fill,
                    Mathf.RoundToInt(size.x),
                    Mathf.RoundToInt(size.y),
                    38,
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
            int fontSize
        )
        {
            GameObject buttonObject = new(label + "-knapp");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "PuzzleButton_" + label + fill,
                RuntimeArt.Hex("#40245F"),
                fill,
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y),
                34,
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

        private static RectTransform CreateSafeRoot(
            Transform parent,
            string name
        )
        {
            GameObject safe = new(name, typeof(RectTransform));
            safe.transform.SetParent(parent, false);
            RectTransform rect = safe.GetComponent<RectTransform>();
            Stretch(rect);
            safe.AddComponent<SafeAreaFitter>();
            return rect;
        }
    }

    public sealed class PuzzlePieceDrag :
        MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private PuzzleGameController controller;
        private RectTransform root;
        private RectTransform rect;
        private Vector2 trayPosition;
        private Vector2 targetPosition;
        private float trayScale;
        private float snapDistance;
        private bool placed;

        public bool IsPlaced => placed;

        public void Initialize(
            PuzzleGameController owner,
            RectTransform rootRect,
            Vector2 start,
            Vector2 target,
            float startScale,
            float allowedSnapDistance
        )
        {
            controller = owner;
            root = rootRect;
            rect = GetComponent<RectTransform>();
            trayPosition = start;
            targetPosition = target;
            trayScale = startScale;
            snapDistance = allowedSnapDistance;
        }

        public void ResetPiece()
        {
            placed = false;
            rect.anchoredPosition = trayPosition;
            rect.localScale = Vector3.one * trayScale;
            GetComponent<Image>().raycastTarget = true;
        }

        public IEnumerator PlayHandTutorial(
            Image hand,
            Sprite openHand,
            Sprite pinchedHand
        )
        {
            if (placed || hand == null)
            {
                yield break;
            }

            Image image = GetComponent<Image>();
            image.raycastTarget = false;
            RectTransform handRect = hand.rectTransform;
            hand.sprite = openHand;
            hand.color = new Color(1f, 1f, 1f, 0f);
            handRect.anchoredPosition = trayPosition + new Vector2(25f, 180f);
            hand.gameObject.SetActive(true);

            float elapsed = 0f;
            const float flyDuration = 0.55f;
            Vector2 hoverPosition = trayPosition + new Vector2(25f, 55f);
            while (elapsed < flyDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(elapsed / flyDuration)
                );
                handRect.anchoredPosition = Vector2.Lerp(
                    trayPosition + new Vector2(25f, 180f),
                    hoverPosition,
                    progress
                );
                hand.color = new Color(1f, 1f, 1f, progress);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.25f);
            hand.sprite = pinchedHand;
            controller.PlayTutorialPickupSound();
            yield return new WaitForSecondsRealtime(0.22f);

            elapsed = 0f;
            const float dragDuration = 1.05f;
            Vector2 handTarget = targetPosition + new Vector2(25f, 55f);
            while (elapsed < dragDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(elapsed / dragDuration)
                );
                rect.anchoredPosition = Vector2.Lerp(
                    trayPosition,
                    targetPosition,
                    progress
                );
                rect.localScale = Vector3.one
                    * Mathf.Lerp(trayScale, 1f, progress);
                handRect.anchoredPosition = Vector2.Lerp(
                    hoverPosition,
                    handTarget,
                    progress
                );
                yield return null;
            }

            controller.PlayTutorialPlacementSound();
            yield return new WaitForSecondsRealtime(0.45f);
            hand.sprite = openHand;
            elapsed = 0f;
            const float fadeDuration = 0.3f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeDuration);
                hand.color = new Color(1f, 1f, 1f, 1f - progress);
                yield return null;
            }

            hand.gameObject.SetActive(false);
            ResetPiece();
        }

        public void CancelTutorial()
        {
            if (rect == null)
            {
                return;
            }
            if (placed)
            {
                return;
            }
            rect.anchoredPosition = trayPosition;
            rect.localScale = Vector3.one * trayScale;
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = !placed;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (placed)
            {
                return;
            }
            rect.SetAsLastSibling();
            rect.localScale = Vector3.one;
            MoveToPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!placed)
            {
                MoveToPointer(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (placed)
            {
                return;
            }
            if (Vector2.Distance(rect.anchoredPosition, targetPosition) <= snapDistance)
            {
                placed = true;
                rect.anchoredPosition = targetPosition;
                rect.localScale = Vector3.one;
                GetComponent<Image>().raycastTarget = false;
                controller.PiecePlaced(this);
            }
            else
            {
                rect.anchoredPosition = trayPosition;
                rect.localScale = Vector3.one * trayScale;
            }
        }

        private void MoveToPointer(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            ))
            {
                rect.anchoredPosition = localPoint;
            }
        }
    }
}
