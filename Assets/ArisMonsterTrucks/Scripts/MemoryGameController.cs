using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class MemoryGameController : MonoBehaviour
    {
        private const int LevelCount = 3;

        private static readonly string[] LevelTitles =
        {
            "FISKVÄNNER",
            "ARBETSFORDON",
            "DJURVÄNNER"
        };

        private static readonly string[][] LevelArt =
        {
            new[]
            {
                "Art/Memory/Fish/fish_1", "Art/Memory/Fish/fish_2",
                "Art/Memory/Fish/fish_3", "Art/Memory/Fish/fish_4",
                "Art/Memory/Fish/fish_5", "Art/Memory/Fish/fish_6"
            },
            new[]
            {
                "Art/Memory/Vehicles/vehicle_1", "Art/Memory/Vehicles/vehicle_2",
                "Art/Memory/Vehicles/vehicle_3", "Art/Memory/Vehicles/vehicle_4",
                "Art/Memory/Vehicles/vehicle_5", "Art/Memory/Vehicles/vehicle_6"
            },
            new[]
            {
                "Art/Memory/Animals/animal_1", "Art/Memory/Animals/animal_2",
                "Art/Memory/Animals/animal_3", "Art/Memory/Animals/animal_4",
                "Art/Memory/Animals/animal_5", "Art/Memory/Animals/animal_6"
            }
        };

        private readonly List<Button> cards = new();
        private readonly List<Image> cardPictures = new();
        private readonly List<Text> cardBacks = new();
        private readonly List<Button> levelButtons = new();
        private readonly List<GameObject> levelLocks = new();
        private readonly List<Text> levelActionTexts = new();
        private readonly int[] pairIds = new int[12];
        private readonly bool[] matched = new bool[12];

        private Font font;
        private Action onBack;
        private GameObject hubRoot;
        private GameObject playRoot;
        private GameObject completionPanel;
        private Text playTitle;
        private Text movesText;
        private Text bestText;
        private Text completionMovesText;
        private Text nextMemoryButtonText;
        private AudioSource audioSource;
        private AudioClip flipSound;
        private AudioClip matchSound;
        private int currentLevel = 1;
        private int firstCard = -1;
        private int moves;
        private int matchedCards;
        private bool inputLocked;

        public static MemoryGameController Create(
            Transform parent,
            Font uiFont,
            Action returnAction
        )
        {
            GameObject host = new("Memoryspelet", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            Stretch(host.GetComponent<RectTransform>());
            MemoryGameController controller = host.AddComponent<MemoryGameController>();
            controller.font = uiFont;
            controller.onBack = returnAction;
            controller.Build();
            controller.Hide();
            return controller;
        }

        public void Show()
        {
            ShowHub();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Build()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            flipSound = CreateFlipSound();
            matchSound = CreateMatchSound();
            BuildHub();
            BuildPlayArea();
        }

        private void ShowHub()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            hubRoot.SetActive(true);
            playRoot.SetActive(false);
            RefreshLevelCards();
        }

        private void BuildHub()
        {
            hubRoot = new GameObject("Memoryväljare", typeof(RectTransform));
            hubRoot.transform.SetParent(transform, false);
            Stretch(hubRoot.GetComponent<RectTransform>());
            SwipePageHandler swipeHandler = hubRoot.AddComponent<SwipePageHandler>();
            swipeHandler.Initialize(FocusMemoryCard);

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

            Button back = CreateButton(
                hubRoot.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            back.onClick.AddListener(() => onBack?.Invoke());

            Text title = CreateText(
                "Titel",
                hubRoot.transform,
                "VÄLJ MEMORY",
                64,
                RuntimeArt.Hex("#FFF3AD")
            );
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(900f, 105f));
            AddOutline(title, RuntimeArt.Hex("#40245F"), 5f);

            float[] positions = { -600f, 0f, 600f };
            for (int index = 0; index < LevelCount; index++)
            {
                int levelNumber = index + 1;
                Button card = CreateButton(
                    hubRoot.transform,
                    "",
                    new Vector2(positions[index], -35f),
                    new Vector2(510f, 700f),
                    RuntimeArt.Hex("#2A66DB"),
                    34
                );
                Image preview = CreateImage(
                    "Memorybild",
                    card.transform,
                    RuntimeArt.LoadSprite(LevelArt[index][0])
                );
                preview.type = Image.Type.Simple;
                preview.preserveAspect = false;
                SetRect(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 145f), new Vector2(465f, 315f));
                Text cardTitle = CreateText("Namn", card.transform, LevelTitles[index], 38, Color.white);
                SetRect(cardTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(460f, 75f));
                AddOutline(cardTitle, RuntimeArt.Hex("#40245F"), 3f);
                Text info = CreateText("Info", card.transform, "6 PAR • LÄTT", 27, RuntimeArt.Hex("#FFF3AD"));
                SetRect(info.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -145f), new Vector2(450f, 55f));
                Image actionButton = CreateImage(
                    "Spelaknapp",
                    card.transform,
                    RuntimeArt.RoundedRectangleSprite(
                        "MemoryPlayButton",
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
                Text action = CreateText("Åtgärd", card.transform, "SPELA", 32, Color.white);
                SetRect(action.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -255f), new Vector2(430f, 70f));
                AddOutline(action, RuntimeArt.Hex("#40245F"), 3f);
                card.onClick.AddListener(() => StartGame(levelNumber));
                levelButtons.Add(card);
                levelActionTexts.Add(action);
                levelLocks.Add(levelNumber == 1 ? null : CreateLock(card.transform));
            }
        }

        private void RefreshLevelCards()
        {
            for (int index = 0; index < levelButtons.Count; index++)
            {
                bool unlocked = MemoryProgress.IsLevelUnlocked(index + 1);
                levelButtons[index].interactable = unlocked;
                if (levelLocks[index] != null)
                {
                    levelLocks[index].SetActive(!unlocked);
                }
                levelActionTexts[index].text = unlocked ? "SPELA" : "LÅST";
            }
        }

        private void FocusMemoryCard(int direction)
        {
            currentLevel = Mathf.Clamp(currentLevel + direction, 1, LevelCount);
            for (int index = 0; index < levelButtons.Count; index++)
            {
                bool focused = index == currentLevel - 1;
                levelButtons[index].transform.localScale = focused
                    ? Vector3.one * 1.04f
                    : Vector3.one * 0.96f;
            }
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

        private void BuildPlayArea()
        {
            playRoot = new GameObject("Memorybord", typeof(RectTransform));
            playRoot.transform.SetParent(transform, false);
            Stretch(playRoot.GetComponent<RectTransform>());

            Image background = CreateImage(
                "Memorybakgrund",
                playRoot.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            Image shade = CreateImage("Memorytoning", playRoot.transform, null);
            shade.color = new Color(0.06f, 0.04f, 0.18f, 0.62f);
            Stretch(shade.rectTransform);

            Button back = CreateButton(
                playRoot.transform,
                "←",
                new Vector2(-855f, 470f),
                new Vector2(150f, 90f),
                RuntimeArt.Hex("#7A5AA6"),
                72
            );
            back.onClick.AddListener(ShowHub);

            playTitle = CreateText("Memorytitel", playRoot.transform, LevelTitles[0], 60, RuntimeArt.Hex("#FFF3AD"));
            SetRect(playTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(850f, 100f));
            AddOutline(playTitle, RuntimeArt.Hex("#40245F"), 5f);

            movesText = CreateCounter("Drag", new Vector2(775f, 470f), "DRAG: 0");
            bestText = CreateCounter("Bäst", new Vector2(775f, 380f), "BÄST: –");

            float[] xs = { -465f, -155f, 155f, 465f };
            float[] ys = { 220f, -25f, -270f };
            for (int index = 0; index < 12; index++)
            {
                int cardIndex = index;
                Button card = CreateButton(
                    playRoot.transform,
                    "",
                    new Vector2(xs[index % 4], ys[index / 4]),
                    new Vector2(270f, 210f),
                    RuntimeArt.Hex("#2A66DB"),
                    42
                );
                Image picture = CreateImage("Memorybild", card.transform, null);
                picture.type = Image.Type.Simple;
                picture.preserveAspect = false;
                SetRect(picture.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(238f, 178f));
                Text cardBack = CreateText("Kortbaksida", card.transform, "?", 86, RuntimeArt.Hex("#FFF3AD"));
                Stretch(cardBack.rectTransform);
                AddOutline(cardBack, RuntimeArt.Hex("#40245F"), 4f);
                card.onClick.AddListener(() => SelectCard(cardIndex));
                cards.Add(card);
                cardPictures.Add(picture);
                cardBacks.Add(cardBack);
            }

            BuildCompletionPanel();
        }

        private Text CreateCounter(string name, Vector2 position, string value)
        {
            Image panel = CreatePanel(name, playRoot.transform, position, new Vector2(300f, 76f), RuntimeArt.Hex("#FFF3AD"));
            Text text = CreateText(name + "text", panel.transform, value, 29, RuntimeArt.Hex("#4A266C"));
            Stretch(text.rectTransform);
            return text;
        }

        private void BuildCompletionPanel()
        {
            Image shade = CreateImage("Memory klart", playRoot.transform, null);
            shade.color = new Color(0.12f, 0.04f, 0.24f, 0.82f);
            Stretch(shade.rectTransform);
            completionPanel = shade.gameObject;
            Image card = CreatePanel("Memoryresultat", shade.transform, Vector2.zero, new Vector2(900f, 600f), RuntimeArt.Hex("#FFF3AD"));
            Text title = CreateText("Klart", card.transform, "MEMORY KLART!", 68, RuntimeArt.Hex("#4A266C"));
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(760f, 100f));
            Text stars = CreateText("Stjärnor", card.transform, "★  ★  ★", 82, RuntimeArt.Hex("#F2A900"));
            SetRect(stars.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(650f, 100f));
            completionMovesText = CreateText("Resultatdrag", card.transform, "KLART PÅ 0 DRAG", 40, RuntimeArt.Hex("#5A376E"));
            SetRect(completionMovesText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(700f, 70f));
            Button again = CreateButton(card.transform, "SPELA IGEN", new Vector2(-205f, -190f), new Vector2(360f, 105f), RuntimeArt.Hex("#FF6B35"), 36);
            again.onClick.AddListener(() => StartGame(currentLevel));
            Button next = CreateButton(card.transform, "NÄSTA MEMORY", new Vector2(205f, -190f), new Vector2(360f, 105f), RuntimeArt.Hex("#5A8CE8"), 32);
            nextMemoryButtonText = next.transform.Find("Text").GetComponent<Text>();
            next.onClick.AddListener(GoToNextMemory);
            completionPanel.SetActive(false);
        }

        private void GoToNextMemory()
        {
            if (
                currentLevel < LevelCount
                && MemoryProgress.IsLevelUnlocked(currentLevel + 1)
            )
            {
                StartGame(currentLevel + 1);
                return;
            }
            ShowHub();
        }

        private void StartGame(int levelNumber)
        {
            if (!MemoryProgress.IsLevelUnlocked(levelNumber))
            {
                return;
            }

            StopAllCoroutines();
            currentLevel = Mathf.Clamp(levelNumber, 1, LevelCount);
            gameObject.SetActive(true);
            hubRoot.SetActive(false);
            playRoot.SetActive(true);
            playTitle.text = LevelTitles[currentLevel - 1];
            moves = 0;
            matchedCards = 0;
            firstCard = -1;
            inputLocked = false;
            completionPanel.SetActive(false);

            for (int index = 0; index < pairIds.Length; index++)
            {
                pairIds[index] = index / 2;
                matched[index] = false;
            }
            for (int index = pairIds.Length - 1; index > 0; index--)
            {
                int other = UnityEngine.Random.Range(0, index + 1);
                (pairIds[index], pairIds[other]) = (pairIds[other], pairIds[index]);
            }
            for (int index = 0; index < cards.Count; index++)
            {
                cardPictures[index].sprite = RuntimeArt.LoadSprite(
                    LevelArt[currentLevel - 1][pairIds[index]]
                );
                HideCard(index);
                cards[index].interactable = true;
            }
            RefreshCounters();
        }

        private void SelectCard(int index)
        {
            if (inputLocked || matched[index] || index == firstCard)
            {
                return;
            }

            RevealCard(index);
            audioSource.PlayOneShot(flipSound, 0.72f);
            if (firstCard < 0)
            {
                firstCard = index;
                return;
            }

            moves++;
            int previous = firstCard;
            firstCard = -1;
            RefreshCounters();
            if (pairIds[previous] == pairIds[index])
            {
                matched[previous] = true;
                matched[index] = true;
                cards[previous].interactable = false;
                cards[index].interactable = false;
                matchedCards += 2;
                audioSource.PlayOneShot(matchSound, 0.9f);
                StartCoroutine(
                    CelebrateMatch(previous, index, matchedCards >= pairIds.Length)
                );
                return;
            }
            StartCoroutine(HideMismatch(previous, index));
        }

        private IEnumerator HideMismatch(int first, int second)
        {
            inputLocked = true;
            yield return new WaitForSecondsRealtime(0.75f);
            HideCard(first);
            HideCard(second);
            inputLocked = false;
        }

        private IEnumerator CelebrateMatch(int first, int second, bool completesGame)
        {
            if (completesGame)
            {
                inputLocked = true;
            }

            List<Text> particles = new();
            List<Vector2> velocities = new();
            Vector2 center = (
                cards[first].image.rectTransform.anchoredPosition
                + cards[second].image.rectTransform.anchoredPosition
            ) * 0.5f;
            Color[] colors =
            {
                RuntimeArt.Hex("#FF5E8E"), RuntimeArt.Hex("#FFD43B"),
                RuntimeArt.Hex("#45E07A"), RuntimeArt.Hex("#55C8FF"),
                RuntimeArt.Hex("#C070FF")
            };
            for (int index = 0; index < 26; index++)
            {
                Text particle = CreateText(
                    "Matchkonfetti",
                    playRoot.transform,
                    index % 2 == 0 ? "★" : "●",
                    30 + index % 3 * 7,
                    colors[index % colors.Length]
                );
                SetRect(
                    particle.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    center + UnityEngine.Random.insideUnitCircle * 55f,
                    new Vector2(55f, 55f)
                );
                particles.Add(particle);
                velocities.Add(
                    new Vector2(
                        UnityEngine.Random.Range(-250f, 250f),
                        UnityEngine.Random.Range(170f, 390f)
                    )
                );
            }

            const float duration = 0.85f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float delta = Time.unscaledDeltaTime;
                elapsed += delta;
                for (int index = 0; index < particles.Count; index++)
                {
                    velocities[index] += Vector2.down * 520f * delta;
                    particles[index].rectTransform.anchoredPosition +=
                        velocities[index] * delta;
                    Color color = particles[index].color;
                    color.a = 1f - elapsed / duration;
                    particles[index].color = color;
                }
                yield return null;
            }
            foreach (Text particle in particles)
            {
                Destroy(particle.gameObject);
            }

            if (completesGame)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
        {
            MemoryProgress.RecordCompletion(currentLevel, moves);
            completionMovesText.text = "KLART PÅ " + moves + " DRAG";
            nextMemoryButtonText.text = currentLevel < LevelCount
                ? "NÄSTA MEMORY"
                : "MEMORYVÄLJARE";
            RefreshCounters();
            completionPanel.transform.SetAsLastSibling();
            completionPanel.SetActive(true);
            inputLocked = false;
        }

        private void RevealCard(int index)
        {
            cardPictures[index].gameObject.SetActive(true);
            cardBacks[index].gameObject.SetActive(false);
        }

        private void HideCard(int index)
        {
            cardPictures[index].gameObject.SetActive(false);
            cardBacks[index].gameObject.SetActive(true);
        }

        private void RefreshCounters()
        {
            movesText.text = "DRAG: " + moves;
            int best = MemoryProgress.BestMoves(currentLevel);
            bestText.text = best > 0 ? "BÄST: " + best : "BÄST: –";
        }

        private static AudioClip CreateFlipSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.12f;
            int count = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[count];
            for (int index = 0; index < count; index++)
            {
                float time = index / (float)sampleRate;
                float t = time / duration;
                float frequency = Mathf.Lerp(520f, 820f, t);
                samples[index] = Mathf.Sin(time * frequency * Mathf.PI * 2f)
                    * Mathf.Sin(Mathf.PI * t)
                    * 0.22f;
            }
            AudioClip clip = AudioClip.Create("Memory kortvändning", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateMatchSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.38f;
            int count = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[count];
            float[] notes = { 523.25f, 659.25f, 783.99f };
            for (int index = 0; index < count; index++)
            {
                float time = index / (float)sampleRate;
                float sample = 0f;
                for (int note = 0; note < notes.Length; note++)
                {
                    float age = time - note * 0.075f;
                    if (age >= 0f)
                    {
                        sample += Mathf.Sin(age * notes[note] * Mathf.PI * 2f)
                            * Mathf.Exp(-age * 7f)
                            * 0.16f;
                    }
                }
                samples[index] = sample;
            }
            AudioClip clip = AudioClip.Create("Memory matchning", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
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
                    "MemoryPanel_" + name,
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
                "MemoryButton_" + label + fill,
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
    }

    public static class MemoryProgress
    {
        private const int LevelCount = 3;

        public static int BestMoves(int levelNumber)
        {
            return Mathf.Max(
                0,
                PlayerPrefs.GetInt(BestMovesKey(levelNumber), 0)
            );
        }

        public static bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= 1
                || (
                    levelNumber <= LevelCount
                    && PlayerPrefs.GetInt(CompletionKey(levelNumber - 1), 0) > 0
                );
        }

        public static void RecordCompletion(int levelNumber, int moves)
        {
            levelNumber = Mathf.Clamp(levelNumber, 1, LevelCount);
            int best = BestMoves(levelNumber);
            if (best == 0 || moves < best)
            {
                PlayerPrefs.SetInt(BestMovesKey(levelNumber), moves);
            }
            PlayerPrefs.SetInt(
                CompletionKey(levelNumber),
                PlayerPrefs.GetInt(CompletionKey(levelNumber), 0) + 1
            );
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            for (int level = 1; level <= LevelCount; level++)
            {
                PlayerPrefs.DeleteKey(BestMovesKey(level));
                PlayerPrefs.DeleteKey(CompletionKey(level));
            }
            PlayerPrefs.Save();
        }

        private static string BestMovesKey(int levelNumber)
        {
            return "memory.v2.level." + levelNumber + ".bestMoves";
        }

        private static string CompletionKey(int levelNumber)
        {
            return "memory.v2.level." + levelNumber + ".completions";
        }
    }
}
