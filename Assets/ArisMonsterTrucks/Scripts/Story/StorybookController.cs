using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Stories
{
    public sealed class StorybookController : MonoBehaviour
    {
        private const float PageFadeDuration = 0.9f;
        private const float EndTolerance = 0.18f;

        private Action onBack;
        private Font font;
        private GameObject root;
        private RectTransform safeRoot;
        private Image currentImage;
        private Image incomingImage;
        private Image softShade;
        private Text titleText;
        private Text pageText;
        private Text pageNumberText;
        private Text playPauseText;
        private Text textToggleText;
        private Text narrationToggleText;
        private Text endMessageText;
        private Button backButton;
        private Button restartButton;
        private Button playPauseButton;
        private Button textToggleButton;
        private Button narrationToggleButton;
        private Button previousButton;
        private Button nextButton;
        private GameObject textPanelObject;
        private GameObject endView;
        private AudioSource narrationSource;
        private StoryDefinition story;
        private int currentPage;
        private float pageShownAt;
        private float fadeStartedAt = -1f;
        private float lastNarrationTime;
        private bool pausedByUser;
        private bool resumeAfterSystemPause;
        private bool finalViewShown;
        private bool textVisible = true;
        private bool narrationEnabled = true;

        public bool IsVisible => root != null && root.activeSelf;
        public int CurrentPageIndex => currentPage;
        public float NarrationTime =>
            narrationSource == null ? 0f : narrationSource.time;
        public bool IsNarrationPlaying =>
            narrationSource != null && narrationSource.isPlaying;
        public bool IsTextVisible => textVisible;
        public bool IsNarrationEnabled => narrationEnabled;

        public static StorybookController Create(
            Transform parent,
            Font uiFont,
            Action backAction
        )
        {
            GameObject host = new("Sagoboksspelare", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            Stretch(host.GetComponent<RectTransform>());
            StorybookController controller =
                host.AddComponent<StorybookController>();
            controller.font = uiFont;
            controller.onBack = backAction;
            controller.Build();
            controller.Hide();
            return controller;
        }

        public void Show(StoryDefinition definition)
        {
            if (definition == null || !definition.IsValid)
            {
                Debug.LogError("Sagoboken saknar en giltig StoryDefinition.");
                return;
            }

            StopAndReset();
            textVisible = AppPreferences.StoryTextVisible;
            narrationEnabled = AppPreferences.SoundEnabled;
            story = definition;
            root.SetActive(true);
            titleText.text = story.Title.ToUpperInvariant();
            endMessageText.text = string.IsNullOrWhiteSpace(story.EndMessage)
                ? "Lilla Lumi är trygg hemma hos mamma."
                : story.EndMessage;
            narrationSource.clip = story.Narration;
            currentPage = 0;
            finalViewShown = false;
            endView.SetActive(false);
            SetReadingChromeVisible(true);
            SetPageVisual(0, false);
            narrationSource.time = 0f;
            if (narrationEnabled)
            {
                narrationSource.Play();
                pausedByUser = false;
            }
            else
            {
                pausedByUser = true;
            }
            RefreshReadingOptions();
        }

        public void Hide()
        {
            StopAndReset();
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void Build()
        {
            root = new GameObject("Sagoläge", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            Stretch(root.GetComponent<RectTransform>());

            currentImage = CreateImage("Aktuell illustration", root.transform);
            StretchBeyond(currentImage.rectTransform, 28f);
            currentImage.preserveAspect = false;
            currentImage.raycastTarget = false;

            incomingImage = CreateImage("Nästa illustration", root.transform);
            StretchBeyond(incomingImage.rectTransform, 28f);
            incomingImage.preserveAspect = false;
            incomingImage.raycastTarget = false;
            incomingImage.color = new Color(1f, 1f, 1f, 0f);

            softShade = CreateImage("Lugn toning", root.transform);
            Stretch(softShade.rectTransform);
            softShade.color = new Color(0.025f, 0.055f, 0.14f, 0.16f);
            softShade.raycastTarget = false;

            GameObject safe = new("Säker sagoyta", typeof(RectTransform));
            safe.transform.SetParent(root.transform, false);
            safeRoot = safe.GetComponent<RectTransform>();
            Stretch(safeRoot);
            safe.AddComponent<SafeAreaFitter>();

            backButton = CreateButton(
                safe.transform,
                "←",
                new Vector2(-850f, 468f),
                new Vector2(140f, 86f),
                RuntimeArt.Hex("#66507F"),
                62
            );
            backButton.onClick.AddListener(LeaveStory);
            SetRect(
                backButton.image.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(85f, -58f),
                new Vector2(140f, 86f)
            );

            titleText = CreateText(
                "Sagotitel",
                safe.transform,
                "",
                42,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                titleText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -55f),
                new Vector2(1100f, 80f)
            );
            AddOutline(titleText, RuntimeArt.Hex("#332052"), 3f);

            restartButton = CreateButton(
                safe.transform,
                "BÖRJA OM",
                new Vector2(760f, 468f),
                new Vector2(200f, 74f),
                RuntimeArt.Hex("#66507F"),
                24
            );
            restartButton.onClick.AddListener(RestartStory);
            SetRect(
                restartButton.image.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-125f, -58f),
                new Vector2(200f, 74f)
            );

            Image textPanel = CreatePanel(
                "Berättelsetext",
                safe.transform,
                new Vector2(-440f, -175f),
                new Vector2(960f, 430f),
                new Color(0.2f, 0.22f, 0.25f, 0.56f)
            );
            textPanel.raycastTarget = false;
            textPanelObject = textPanel.gameObject;

            pageText = CreateText(
                "Sidtext",
                textPanel.transform,
                "",
                26,
                Color.white
            );
            pageText.alignment = TextAnchor.MiddleLeft;
            pageText.lineSpacing = 0.88f;
            pageText.resizeTextForBestFit = true;
            pageText.resizeTextMinSize = 22;
            pageText.resizeTextMaxSize = 26;
            pageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            pageText.verticalOverflow = VerticalWrapMode.Truncate;
            AddOutline(pageText, new Color(0.04f, 0.06f, 0.14f, 0.92f), 2f);
            SetRect(
                pageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(870f, 382f)
            );

            GameObject controlObject = new(
                "Gemensam nedre sagokontroll",
                typeof(RectTransform)
            );
            controlObject.transform.SetParent(safe.transform, false);
            RectTransform controlBar = controlObject.GetComponent<RectTransform>();
            SetRect(
                controlBar,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 68f),
                new Vector2(1050f, 112f)
            );

            pageNumberText = CreateText(
                "Sidnummer",
                safe.transform,
                "",
                20,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(
                pageNumberText.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-125f, 68f),
                new Vector2(220f, 50f)
            );
            AddOutline(pageNumberText, RuntimeArt.Hex("#332052"), 2f);

            textToggleButton = CreateButton(
                controlObject.transform,
                "",
                new Vector2(-405f, 0f),
                new Vector2(180f, 82f),
                RuntimeArt.Hex("#66507F"),
                23
            );
            textToggleText = CreateText(
                "Textläge",
                textToggleButton.transform,
                "TEXT PÅ",
                25,
                Color.white
            );
            Stretch(textToggleText.rectTransform);
            AddOutline(textToggleText, RuntimeArt.Hex("#332052"), 2f);
            textToggleButton.onClick.AddListener(ToggleText);

            narrationToggleButton = CreateButton(
                controlObject.transform,
                "",
                new Vector2(-210f, 0f),
                new Vector2(180f, 82f),
                RuntimeArt.Hex("#49C968"),
                23
            );
            narrationToggleText = CreateText(
                "Ljudläge",
                narrationToggleButton.transform,
                "LJUD PÅ",
                25,
                Color.white
            );
            Stretch(narrationToggleText.rectTransform);
            AddOutline(narrationToggleText, RuntimeArt.Hex("#332052"), 2f);
            narrationToggleButton.onClick.AddListener(ToggleNarration);

            previousButton = CreateButton(
                controlObject.transform,
                "‹",
                new Vector2(-30f, 0f),
                new Vector2(92f, 96f),
                RuntimeArt.Hex("#7A5AA6"),
                68
            );
            previousButton.onClick.AddListener(() => MovePage(-1));

            playPauseButton = CreateButton(
                controlObject.transform,
                "",
                new Vector2(115f, 0f),
                new Vector2(170f, 82f),
                RuntimeArt.Hex("#49C968"),
                26
            );
            playPauseText = CreateText(
                "Spela eller pausa",
                playPauseButton.transform,
                "PAUSA",
                30,
                Color.white
            );
            Stretch(playPauseText.rectTransform);
            AddOutline(playPauseText, RuntimeArt.Hex("#332052"), 2f);
            playPauseButton.onClick.AddListener(TogglePlayback);

            nextButton = CreateButton(
                controlObject.transform,
                "›",
                new Vector2(260f, 0f),
                new Vector2(92f, 96f),
                RuntimeArt.Hex("#7A5AA6"),
                68
            );
            nextButton.onClick.AddListener(() => MovePage(1));

            narrationSource = gameObject.AddComponent<AudioSource>();
            narrationSource.playOnAwake = false;
            narrationSource.loop = false;
            narrationSource.spatialBlend = 0f;
            narrationSource.ignoreListenerPause = false;

            BuildEndView();
        }

        private void BuildEndView()
        {
            Image panel = CreatePanel(
                "Sagan är slut",
                safeRoot,
                Vector2.zero,
                new Vector2(920f, 520f),
                new Color(1f, 0.95f, 0.77f, 0.96f)
            );
            endView = panel.gameObject;

            Text heading = CreateText(
                "Slutrubrik",
                panel.transform,
                "SOV SÅ GOTT",
                60,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(
                heading.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 135f),
                new Vector2(780f, 95f)
            );

            endMessageText = CreateText(
                "Sluttext",
                panel.transform,
                "Lilla Lumi är trygg hemma hos mamma.",
                31,
                RuntimeArt.Hex("#66507F")
            );
            SetRect(
                endMessageText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 55f),
                new Vector2(760f, 58f)
            );

            Button again = CreateButton(
                panel.transform,
                "LÄS IGEN",
                new Vector2(0f, -55f),
                new Vector2(520f, 90f),
                RuntimeArt.Hex("#49C968"),
                35
            );
            again.onClick.AddListener(RestartStory);

            Button home = CreateButton(
                panel.transform,
                "TILL STARTSIDAN",
                new Vector2(0f, -170f),
                new Vector2(520f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                32
            );
            home.onClick.AddListener(LeaveStory);
            endView.SetActive(false);
        }

        private void Update()
        {
            if (!IsVisible || story == null || finalViewShown)
            {
                return;
            }

            float audioTime = narrationSource.time;
            if (narrationSource.isPlaying)
            {
                lastNarrationTime = audioTime;
                int timedPage = FindPageForTime(audioTime);
                if (timedPage != currentPage)
                {
                    SetPageVisual(timedPage, true);
                }
            }

            AnimateIllustration();
            UpdateFade();

            if (
                story.Narration != null
                && (
                    audioTime >= story.Narration.length - EndTolerance
                    || (
                        !narrationSource.isPlaying
                        && !pausedByUser
                        && lastNarrationTime
                            >= story.Narration.length - EndTolerance - 0.4f
                    )
                )
            )
            {
                ShowFinalView();
            }
        }

        private int FindPageForTime(float time)
        {
            int result = 0;
            for (int index = 0; index < story.Pages.Count; index++)
            {
                if (time + 0.01f >= story.Pages[index].StartTime)
                {
                    result = index;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private void SetPageVisual(int pageIndex, bool animate)
        {
            if (story == null || story.Pages.Count == 0)
            {
                return;
            }

            currentPage = Mathf.Clamp(pageIndex, 0, story.Pages.Count - 1);
            StoryPage page = story.Pages[currentPage];
            pageText.text = page.Text;
            pageNumberText.text = page.IsCover
                ? "SIDA 1 / " + (story.Pages.Count - 1)
                : "SIDA " + currentPage + " / " + (story.Pages.Count - 1);
            previousButton.interactable = currentPage > 0;
            nextButton.interactable =
                currentPage < story.Pages.Count - 1 || !narrationEnabled;
            pageShownAt = Time.unscaledTime;

            if (animate && currentImage.sprite != null)
            {
                incomingImage.sprite = page.Illustration;
                incomingImage.color = new Color(1f, 1f, 1f, 0f);
                incomingImage.rectTransform.anchoredPosition = Vector2.zero;
                incomingImage.rectTransform.localScale = Vector3.one;
                fadeStartedAt = Time.unscaledTime;
            }
            else
            {
                currentImage.sprite = page.Illustration;
                currentImage.color = Color.white;
                currentImage.rectTransform.anchoredPosition = Vector2.zero;
                currentImage.rectTransform.localScale = Vector3.one;
                incomingImage.color = new Color(1f, 1f, 1f, 0f);
                fadeStartedAt = -1f;
            }
        }

        private void UpdateFade()
        {
            if (fadeStartedAt < 0f)
            {
                return;
            }

            float progress = Mathf.Clamp01(
                (Time.unscaledTime - fadeStartedAt) / PageFadeDuration
            );
            float eased = progress * progress * (3f - 2f * progress);
            currentImage.color = new Color(1f, 1f, 1f, 1f - eased);
            incomingImage.color = new Color(1f, 1f, 1f, eased);
            if (progress < 1f)
            {
                return;
            }

            currentImage.sprite = incomingImage.sprite;
            currentImage.color = Color.white;
            currentImage.rectTransform.anchoredPosition =
                incomingImage.rectTransform.anchoredPosition;
            currentImage.rectTransform.localScale =
                incomingImage.rectTransform.localScale;
            incomingImage.color = new Color(1f, 1f, 1f, 0f);
            incomingImage.rectTransform.anchoredPosition = Vector2.zero;
            incomingImage.rectTransform.localScale = Vector3.one;
            fadeStartedAt = -1f;
        }

        private void AnimateIllustration()
        {
            if (story == null || currentPage >= story.Pages.Count)
            {
                return;
            }

            StoryPage page = story.Pages[currentPage];
            float elapsed = Time.unscaledTime - pageShownAt;
            float wave = Mathf.Sin(elapsed * 0.35f);
            Vector2 drift = page.ParallaxDirection.normalized * wave * 7f;
            float zoomAmount = Mathf.Lerp(
                1f,
                page.Zoom,
                0.5f + 0.5f * Mathf.Sin(elapsed * 0.16f)
            );

            switch (page.AnimationType)
            {
                case StoryAnimationType.SoftBob:
                    drift += Vector2.up * Mathf.Sin(elapsed * 0.55f) * 5f;
                    break;
                case StoryAnimationType.MoonlightGlow:
                    softShade.color = new Color(
                        0.025f,
                        0.055f,
                        0.14f,
                        0.13f + (wave + 1f) * 0.025f
                    );
                    break;
                case StoryAnimationType.SlowZoom:
                    zoomAmount = Mathf.Lerp(
                        1f,
                        page.Zoom,
                        Mathf.PingPong(elapsed * 0.035f, 1f)
                    );
                    break;
            }

            Image target =
                fadeStartedAt >= 0f && incomingImage.color.a > 0.5f
                    ? incomingImage
                    : currentImage;
            target.rectTransform.anchoredPosition = drift;
            target.rectTransform.localScale =
                new Vector3(zoomAmount, zoomAmount, 1f);
        }

        private void MovePage(int direction)
        {
            if (story == null || finalViewShown)
            {
                return;
            }

            int target = Mathf.Clamp(
                currentPage + direction,
                0,
                story.Pages.Count - 1
            );
            if (target == currentPage)
            {
                if (
                    !narrationEnabled
                    && direction > 0
                    && currentPage == story.Pages.Count - 1
                )
                {
                    ShowFinalView();
                }
                return;
            }

            bool wasPlaying =
                narrationEnabled && narrationSource.isPlaying;
            SeekToPage(target, wasPlaying);
        }

        public void SeekToPage(int pageIndex, bool continuePlayback)
        {
            if (story == null || finalViewShown)
            {
                return;
            }

            int target = Mathf.Clamp(
                pageIndex,
                0,
                story.Pages.Count - 1
            );
            narrationSource.Pause();
            narrationSource.time = Mathf.Clamp(
                story.Pages[target].StartTime,
                0f,
                Mathf.Max(0f, story.Narration.length - 0.01f)
            );
            lastNarrationTime = narrationSource.time;
            SetPageVisual(target, true);
            if (continuePlayback)
            {
                narrationSource.Play();
            }
            pausedByUser = !continuePlayback;
            playPauseText.text = continuePlayback ? "PAUSA" : "SPELA";
        }

        public void ShowCompletionPreview()
        {
            if (story != null)
            {
                ShowFinalView();
            }
        }

        private void TogglePlayback()
        {
            if (
                story == null
                || finalViewShown
                || !narrationEnabled
            )
            {
                return;
            }

            if (narrationSource.isPlaying)
            {
                narrationSource.Pause();
                pausedByUser = true;
                playPauseText.text = "SPELA";
            }
            else
            {
                narrationSource.Play();
                pausedByUser = false;
                playPauseText.text = "PAUSA";
            }
        }

        private void ToggleText()
        {
            textVisible = !textVisible;
            AppPreferences.StoryTextVisible = textVisible;
            RefreshReadingOptions();
        }

        private void ToggleNarration()
        {
            narrationEnabled = !narrationEnabled;
            AppPreferences.SoundEnabled = narrationEnabled;
            if (narrationEnabled)
            {
                lastNarrationTime = narrationSource.time;
                narrationSource.UnPause();
                if (!narrationSource.isPlaying)
                {
                    narrationSource.Play();
                }
                pausedByUser = false;
            }
            else
            {
                narrationSource.Pause();
                pausedByUser = true;
            }
            RefreshReadingOptions();
            SetPageVisual(currentPage, false);
        }

        private void RefreshReadingOptions()
        {
            if (textPanelObject != null)
            {
                textPanelObject.SetActive(textVisible);
            }
            if (textToggleText != null)
            {
                textToggleText.text = textVisible ? "TEXT PÅ" : "TEXT AV";
            }
            if (narrationToggleText != null)
            {
                narrationToggleText.text =
                    narrationEnabled ? "LJUD PÅ" : "LJUD AV";
            }
            if (playPauseButton != null)
            {
                playPauseButton.interactable = narrationEnabled;
            }
            if (playPauseText != null)
            {
                playPauseText.text = narrationEnabled
                    ? (narrationSource.isPlaying ? "PAUSA" : "SPELA")
                    : "LÄSLÄGE";
            }
        }

        private void RestartStory()
        {
            if (story != null)
            {
                Show(story);
            }
        }

        private void ShowFinalView()
        {
            narrationSource.Stop();
            finalViewShown = true;
            SetReadingChromeVisible(false);
            endView.SetActive(true);
        }

        private void SetReadingChromeVisible(bool visible)
        {
            backButton?.gameObject.SetActive(visible);
            restartButton?.gameObject.SetActive(visible);
            previousButton?.gameObject.SetActive(visible);
            playPauseButton?.gameObject.SetActive(visible);
            textToggleButton?.gameObject.SetActive(visible);
            narrationToggleButton?.gameObject.SetActive(visible);
            nextButton?.gameObject.SetActive(visible);
            textPanelObject?.SetActive(visible && textVisible);
            pageNumberText?.gameObject.SetActive(visible);
        }

        private void LeaveStory()
        {
            Hide();
            onBack?.Invoke();
        }

        private void StopAndReset()
        {
            if (narrationSource != null)
            {
                narrationSource.Stop();
                narrationSource.clip = null;
            }
            story = null;
            currentPage = 0;
            lastNarrationTime = 0f;
            pausedByUser = false;
            resumeAfterSystemPause = false;
            finalViewShown = false;
            if (endView != null)
            {
                endView.SetActive(false);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (!IsVisible || narrationSource == null)
            {
                return;
            }
            if (paused)
            {
                PauseForSystem();
            }
            else
            {
                ResumeFromSystem();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!IsVisible || narrationSource == null)
            {
                return;
            }
            if (!focused)
            {
                PauseForSystem();
            }
            else
            {
                ResumeFromSystem();
            }
        }

        private void PauseForSystem()
        {
            if (narrationSource.isPlaying)
            {
                resumeAfterSystemPause = true;
                narrationSource.Pause();
            }
        }

        private void ResumeFromSystem()
        {
            if (resumeAfterSystemPause && !pausedByUser)
            {
                narrationSource.UnPause();
            }
            resumeAfterSystemPause = false;
        }

        private void OnDisable()
        {
            if (narrationSource != null)
            {
                narrationSource.Stop();
            }
        }

        private static Image CreatePanel(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color fill
        )
        {
            Image panel = CreateImage(name, parent);
            panel.sprite = RuntimeArt.RoundedRectangleSprite(
                "StoryPanel_" + name,
                RuntimeArt.Hex("#3B285D"),
                fill,
                320,
                180,
                28,
                6
            );
            panel.type = Image.Type.Sliced;
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
            int textSize
        )
        {
            Image image = CreateImage(label + " knapp", parent);
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "StoryButton_" + label + "_" + fill,
                RuntimeArt.Hex("#40245F"),
                fill,
                300,
                120,
                34,
                7
            );
            image.type = Image.Type.Sliced;
            SetRect(
                image.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size
            );
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if (!string.IsNullOrEmpty(label))
            {
                Text text = CreateText(
                    label + " text",
                    button.transform,
                    label,
                    textSize,
                    Color.white
                );
                Stretch(text.rectTransform);
                AddOutline(text, RuntimeArt.Hex("#40245F"), 2f);
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
            GameObject textObject = new(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string name, Transform parent)
        {
            GameObject imageObject = new(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);
            return imageObject.AddComponent<Image>();
        }

        private static void AddOutline(
            Text text,
            Color color,
            float distance
        )
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

        private static void StretchBeyond(RectTransform rect, float overscan)
        {
            Stretch(rect);
            rect.offsetMin = new Vector2(-overscan, -overscan);
            rect.offsetMax = new Vector2(overscan, overscan);
        }
    }
}
