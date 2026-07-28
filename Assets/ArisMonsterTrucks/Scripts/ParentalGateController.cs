using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class ParentalGateController : MonoBehaviour
    {
        private readonly Dictionary<ParentalGame, ParentalSwipeToggle> toggles =
            new();
        private Font font;
        private Action initialSetupCompleted;
        private Action settingsChanged;
        private GameObject challengeRoot;
        private GameObject passwordRoot;
        private GameObject settingsRoot;
        private GameObject unlockRoot;
        private InputField answerInput;
        private InputField passwordInput;
        private InputField confirmInput;
        private InputField unlockInput;
        private Text challengeStatus;
        private Text passwordStatus;
        private Text settingsStatus;
        private Text unlockStatus;
        private Text settingsTitle;
        private Text settingsStep;
        private bool initialSetupMode;
        private string pendingPassword = "";

        public static ParentalGateController Create(
            Transform parent,
            Font uiFont,
            Action onInitialSetupCompleted,
            Action onSettingsChanged
        )
        {
            GameObject host = new("Föräldrakontroll", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            Stretch(host.GetComponent<RectTransform>());
            ParentalGateController controller =
                host.AddComponent<ParentalGateController>();
            controller.font = uiFont;
            controller.initialSetupCompleted = onInitialSetupCompleted;
            controller.settingsChanged = onSettingsChanged;
            controller.Build();
            return controller;
        }

        public void ShowInitialSetup()
        {
            HideAll();
            challengeStatus.text = "";
            answerInput.text = "";
            challengeRoot.SetActive(true);
            challengeRoot.transform.SetAsLastSibling();
            answerInput.ActivateInputField();
        }

        public void ShowUnlock()
        {
            HideAll();
            unlockStatus.text = "";
            unlockInput.text = "";
            unlockRoot.SetActive(true);
            unlockRoot.transform.SetAsLastSibling();
            unlockInput.ActivateInputField();
        }

        public void ShowInitialSettingsPreview()
        {
            pendingPassword = "2468";
            ShowSettings(true);
        }

        public void ShowPasswordSetupPreview()
        {
            ShowPasswordSetup();
        }

        public void Hide()
        {
            HideAll();
        }

        private void Build()
        {
            BuildChallenge();
            BuildPasswordSetup();
            BuildSettings();
            BuildUnlock();
            HideAll();
        }

        private void BuildChallenge()
        {
            challengeRoot = CreateBackdrop("Vuxenkontroll");
            Image panel = CreatePanel(
                challengeRoot.transform,
                new Vector2(0f, 5f),
                new Vector2(1040f, 760f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "VUXENKONTROLL", 68, 275f);
            Text step = CreateText(
                "Steg",
                panel.transform,
                "STEG 1 AV 3",
                22,
                RuntimeArt.Hex("#8B7694")
            );
            SetRect(step.rectTransform, new Vector2(0f, 222f), new Vector2(500f, 40f));
            Text intro = CreateText(
                "Förklaring",
                panel.transform,
                "INNAN FÖRÄLDRAINSTÄLLNINGARNA ÖPPNAS\nBEHÖVER EN VUXEN SVARA PÅ FRÅGAN.",
                28,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(intro.rectTransform, new Vector2(0f, 150f), new Vector2(850f, 90f));

            Text question = CreateText(
                "Vuxenfråga",
                panel.transform,
                "25 + 75 = ?",
                72,
                RuntimeArt.Hex("#3E2858")
            );
            SetRect(question.rectTransform, new Vector2(0f, 30f), new Vector2(720f, 100f));

            answerInput = CreateInput(
                panel.transform,
                "Skriv svaret",
                new Vector2(0f, -90f),
                new Vector2(460f, 95f),
                false,
                3
            );
            Button continueButton = CreateButton(
                panel.transform,
                "FORTSÄTT",
                new Vector2(0f, -220f),
                new Vector2(500f, 105f),
                RuntimeArt.Hex("#FF6B35"),
                38
            );
            continueButton.onClick.AddListener(CheckAdultAnswer);
            answerInput.onEndEdit.AddListener(_ =>
            {
                if (
                    Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter)
                )
                {
                    CheckAdultAnswer();
                }
            });

            challengeStatus = CreateText(
                "Svarstatus",
                panel.transform,
                "",
                27,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(challengeStatus.rectTransform, new Vector2(0f, -310f), new Vector2(820f, 55f));
        }

        private void BuildPasswordSetup()
        {
            passwordRoot = CreateBackdrop("Skapa föräldrakod");
            Image panel = CreatePanel(
                passwordRoot.transform,
                Vector2.zero,
                new Vector2(1040f, 780f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "SKAPA FÖRÄLDRAKOD", 62, 285f);
            Text step = CreateText(
                "Steg",
                panel.transform,
                "STEG 2 AV 3",
                22,
                RuntimeArt.Hex("#8B7694")
            );
            SetRect(step.rectTransform, new Vector2(0f, 230f), new Vector2(500f, 40f));
            Text explanation = CreateText(
                "Förklaring",
                panel.transform,
                "KODEN BEHÖVS NÄR SPEL SKA SLÅS AV ELLER PÅ SENARE.",
                27,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(explanation.rectTransform, new Vector2(0f, 155f), new Vector2(850f, 60f));
            Text passwordLabel = CreateText(
                "Kodetikett",
                panel.transform,
                "VÄLJ 4–8 SIFFROR",
                25,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(passwordLabel.rectTransform, new Vector2(0f, 85f), new Vector2(700f, 45f));
            passwordInput = CreateInput(
                panel.transform,
                "Ny kod",
                new Vector2(0f, 15f),
                new Vector2(520f, 90f),
                true,
                8
            );
            confirmInput = CreateInput(
                panel.transform,
                "Upprepa koden",
                new Vector2(0f, -95f),
                new Vector2(520f, 90f),
                true,
                8
            );
            Button continueButton = CreateButton(
                panel.transform,
                "FORTSÄTT TILL SPELVAL",
                new Vector2(0f, -225f),
                new Vector2(610f, 105f),
                RuntimeArt.Hex("#FF6B35"),
                34
            );
            continueButton.onClick.AddListener(ConfirmNewPassword);
            passwordStatus = CreateText(
                "Kodstatus",
                panel.transform,
                "",
                26,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(passwordStatus.rectTransform, new Vector2(0f, -320f), new Vector2(800f, 50f));
        }

        private void BuildSettings()
        {
            settingsRoot = CreateBackdrop("Föräldradashboard");
            Image panel = CreatePanel(
                settingsRoot.transform,
                Vector2.zero,
                new Vector2(1500f, 1000f),
                RuntimeArt.Hex("#FFF7D6")
            );
            settingsTitle = CreateHeading(
                panel.transform,
                "VÄLJ TILLÅTNA SPEL",
                58,
                420f
            );

            settingsStep = CreateText(
                "Steg",
                panel.transform,
                "STEG 3 AV 3  •  KODEN ÄR SPARAD",
                21,
                RuntimeArt.Hex("#238A3E")
            );
            SetRect(settingsStep.rectTransform, new Vector2(0f, 370f), new Vector2(850f, 38f));
            Text explanation = CreateText(
                "Förklaring",
                panel.transform,
                "SVEP ELLER TRYCK PÅ VARJE REGLAGE: AV ELLER PÅ",
                27,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(explanation.rectTransform, new Vector2(0f, 320f), new Vector2(1050f, 48f));

            CreateGameToggle(panel.transform, ParentalGame.MonsterTrucks, "MONSTERTRUCKS", 190f);
            CreateGameToggle(panel.transform, ParentalGame.Puzzle, "PUSSEL", 80f);
            CreateGameToggle(panel.transform, ParentalGame.Memory, "MEMORY", -30f);
            CreateGameToggle(panel.transform, ParentalGame.Fishing, "FISKE", -140f);
            CreateGameToggle(panel.transform, ParentalGame.Stories, "SAGOR", -250f);

            settingsStatus = CreateText(
                "Inställningsstatus",
                panel.transform,
                "",
                25,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(settingsStatus.rectTransform, new Vector2(0f, -325f), new Vector2(1150f, 42f));

            Button save = CreateButton(
                panel.transform,
                "SPARA OCH FORTSÄTT",
                new Vector2(0f, -382f),
                new Vector2(610f, 92f),
                RuntimeArt.Hex("#4FC66A"),
                34
            );
            save.onClick.AddListener(SaveSettings);

            Text privacy = CreateText(
                "Lokal lagring",
                panel.transform,
                "KOD, SPELVAL OCH SPELPROGRESS SPARAS BARA LOKALT PÅ DEN HÄR ENHETEN.",
                20,
                RuntimeArt.Hex("#7B6B7E")
            );
            SetRect(privacy.rectTransform, new Vector2(0f, -438f), new Vector2(1250f, 26f));
        }

        private void BuildUnlock()
        {
            unlockRoot = CreateBackdrop("Lås upp föräldradashboard");
            Image panel = CreatePanel(
                unlockRoot.transform,
                Vector2.zero,
                new Vector2(930f, 650f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "FÖRÄLDRAR", 66, 230f);
            Text prompt = CreateText(
                "Kodfråga",
                panel.transform,
                "ANGE FÖRÄLDRAKODEN",
                30,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(prompt.rectTransform, new Vector2(0f, 115f), new Vector2(700f, 60f));
            unlockInput = CreateInput(
                panel.transform,
                "Föräldrakod",
                new Vector2(0f, 20f),
                new Vector2(500f, 95f),
                true,
                8
            );
            Button open = CreateButton(
                panel.transform,
                "ÖPPNA",
                new Vector2(185f, -110f),
                new Vector2(390f, 98f),
                RuntimeArt.Hex("#4FC66A"),
                36
            );
            open.onClick.AddListener(VerifyUnlock);
            Button cancel = CreateButton(
                panel.transform,
                "AVBRYT",
                new Vector2(-250f, -110f),
                new Vector2(360f, 98f),
                RuntimeArt.Hex("#818795"),
                34
            );
            cancel.onClick.AddListener(HideAll);
            unlockStatus = CreateText(
                "Kodstatus",
                panel.transform,
                "",
                26,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(unlockStatus.rectTransform, new Vector2(0f, -215f), new Vector2(700f, 55f));
        }

        private void CheckAdultAnswer()
        {
            if (answerInput.text.Trim() != "100")
            {
                challengeStatus.text = "DET VAR INTE RÄTT – FÖRSÖK IGEN";
                answerInput.text = "";
                answerInput.ActivateInputField();
                return;
            }

            ShowPasswordSetup();
        }

        private void ShowPasswordSetup()
        {
            HideAll();
            pendingPassword = "";
            passwordInput.text = "";
            confirmInput.text = "";
            passwordStatus.text = "";
            passwordRoot.SetActive(true);
            passwordRoot.transform.SetAsLastSibling();
            passwordInput.ActivateInputField();
        }

        private void ConfirmNewPassword()
        {
            if (!ParentalControls.IsValidPasswordFormat(passwordInput.text))
            {
                passwordStatus.text = "KODEN SKA VARA 4–8 SIFFROR";
                passwordInput.ActivateInputField();
                return;
            }
            if (passwordInput.text != confirmInput.text)
            {
                passwordStatus.text = "KODERNA ÄR INTE LIKADANA";
                confirmInput.ActivateInputField();
                return;
            }

            pendingPassword = passwordInput.text;
            passwordInput.text = "";
            confirmInput.text = "";
            ShowSettings(true);
        }

        private void VerifyUnlock()
        {
            if (!ParentalControls.VerifyPassword(unlockInput.text))
            {
                unlockStatus.text = "FEL KOD – FÖRSÖK IGEN";
                unlockInput.text = "";
                unlockInput.ActivateInputField();
                return;
            }

            ShowSettings(false);
        }

        private void ShowSettings(bool isInitialSetup)
        {
            HideAll();
            initialSetupMode = isInitialSetup;
            settingsTitle.text = isInitialSetup
                ? "VÄLJ TILLÅTNA SPEL"
                : "FÖRÄLDRADASHBOARD";
            settingsStep.text = isInitialSetup
                ? "STEG 3 AV 3  •  KODEN ÄR SPARAD"
                : "ÄNDRA SPELENS LÄGE OCH SPARA";
            settingsStatus.text = "";
            foreach (KeyValuePair<ParentalGame, ParentalSwipeToggle> pair in toggles)
            {
                pair.Value.SetValue(
                    isInitialSetup
                        ? false
                        : ParentalControls.IsEnabled(pair.Key),
                    false
                );
            }
            settingsRoot.SetActive(true);
            settingsRoot.transform.SetAsLastSibling();
        }

        private void SaveSettings()
        {
            bool anyEnabled = false;
            foreach (ParentalSwipeToggle toggle in toggles.Values)
            {
                anyEnabled |= toggle.IsOn;
            }
            if (!anyEnabled)
            {
                settingsStatus.text = "AKTIVERA MINST ETT SPEL";
                return;
            }

            if (initialSetupMode)
            {
                if (!ParentalControls.IsValidPasswordFormat(pendingPassword))
                {
                    ShowPasswordSetup();
                    return;
                }
                ParentalControls.Configure(
                    pendingPassword,
                    toggles[ParentalGame.MonsterTrucks].IsOn,
                    toggles[ParentalGame.Puzzle].IsOn,
                    toggles[ParentalGame.Memory].IsOn,
                    toggles[ParentalGame.Fishing].IsOn,
                    toggles[ParentalGame.Stories].IsOn
                );
                HideAll();
                initialSetupCompleted?.Invoke();
                return;
            }

            foreach (KeyValuePair<ParentalGame, ParentalSwipeToggle> pair in toggles)
            {
                ParentalControls.SetEnabled(pair.Key, pair.Value.IsOn);
            }
            HideAll();
            settingsChanged?.Invoke();
        }

        private void CreateGameToggle(
            Transform parent,
            ParentalGame game,
            string label,
            float y
        )
        {
            Image row = CreatePanel(
                parent,
                new Vector2(0f, y),
                new Vector2(1080f, 108f),
                RuntimeArt.Hex("#F1E4BE")
            );
            Text gameName = CreateText(
                "Spelnamn",
                row.transform,
                label,
                32,
                RuntimeArt.Hex("#40245F")
            );
            gameName.alignment = TextAnchor.MiddleLeft;
            SetRect(gameName.rectTransform, new Vector2(-265f, 0f), new Vector2(430f, 70f));
            Image track = CreateImage("AV PÅ-reglage", row.transform, null);
            track.sprite = RuntimeArt.RoundedRectangleSprite(
                "ParentalToggleTrack",
                RuntimeArt.Hex("#5B5265"),
                Color.white,
                360,
                76,
                35,
                4
            );
            track.type = Image.Type.Sliced;
            SetRect(track.rectTransform, new Vector2(305f, 0f), new Vector2(350f, 76f));
            Image handle = CreateImage("Draghandtag", track.transform, null);
            handle.sprite = RuntimeArt.RoundedRectangleSprite(
                "ParentalToggleHandle",
                RuntimeArt.Hex("#40245F"),
                RuntimeArt.Hex("#FFF7D6"),
                165,
                62,
                30,
                5
            );
            handle.type = Image.Type.Sliced;
            SetRect(handle.rectTransform, Vector2.zero, new Vector2(165f, 62f));
            handle.raycastTarget = false;
            Text no = CreateText("Av", track.transform, "AV", 24, Color.white);
            SetRect(no.rectTransform, new Vector2(-87f, 0f), new Vector2(150f, 60f));
            Text yes = CreateText("På", track.transform, "PÅ", 24, Color.white);
            SetRect(yes.rectTransform, new Vector2(87f, 0f), new Vector2(150f, 60f));
            no.raycastTarget = false;
            yes.raycastTarget = false;

            ParentalSwipeToggle swipe =
                track.gameObject.AddComponent<ParentalSwipeToggle>();
            swipe.Initialize(
                track.rectTransform,
                handle.rectTransform,
                track,
                no,
                yes,
                false,
                _ => { }
            );
            toggles[game] = swipe;
        }

        private GameObject CreateBackdrop(string name)
        {
            GameObject root = new(name, typeof(RectTransform));
            root.transform.SetParent(transform, false);
            Stretch(root.GetComponent<RectTransform>());
            Image background = CreateImage(
                "Bakgrund",
                root.transform,
                RuntimeArt.LoadSprite("Art/Environment/colorful_background")
            );
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            Image shade = CreateImage("Vuxentoning", root.transform, null);
            shade.color = new Color(0.04f, 0.08f, 0.18f, 0.72f);
            Stretch(shade.rectTransform);
            return root;
        }

        private InputField CreateInput(
            Transform parent,
            string placeholderValue,
            Vector2 position,
            Vector2 size,
            bool password,
            int characterLimit
        )
        {
            Image panel = CreatePanel(parent, position, size, Color.white);
            InputField input = panel.gameObject.AddComponent<InputField>();
            input.targetGraphic = panel;
            input.characterLimit = characterLimit;
            input.lineType = InputField.LineType.SingleLine;
            input.characterValidation = InputField.CharacterValidation.Integer;
            input.keyboardType = TouchScreenKeyboardType.NumberPad;
            input.contentType = password
                ? InputField.ContentType.Pin
                : InputField.ContentType.IntegerNumber;

            Text value = CreateText(
                "Värde",
                panel.transform,
                "",
                38,
                RuntimeArt.Hex("#40245F")
            );
            value.supportRichText = false;
            value.alignment = TextAnchor.MiddleCenter;
            Stretch(value.rectTransform);
            value.rectTransform.offsetMin = new Vector2(22f, 6f);
            value.rectTransform.offsetMax = new Vector2(-22f, -6f);
            input.textComponent = value;

            Text placeholder = CreateText(
                "Platshållare",
                panel.transform,
                placeholderValue,
                28,
                RuntimeArt.Hex("#A79BAE")
            );
            placeholder.fontStyle = FontStyle.Italic;
            Stretch(placeholder.rectTransform);
            input.placeholder = placeholder;
            return input;
        }

        private Text CreateHeading(
            Transform parent,
            string value,
            int size,
            float y
        )
        {
            Text heading = CreateText(
                "Rubrik",
                parent,
                value,
                size,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(heading.rectTransform, new Vector2(0f, y), new Vector2(1200f, 90f));
            return heading;
        }

        private void HideAll()
        {
            challengeRoot?.SetActive(false);
            passwordRoot?.SetActive(false);
            settingsRoot?.SetActive(false);
            unlockRoot?.SetActive(false);
        }

        private Image CreatePanel(
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color fill
        )
        {
            Image panel = CreateImage("Panel", parent, null);
            panel.sprite = RuntimeArt.RoundedRectangleSprite(
                "ParentalPanel_" + fill + size,
                RuntimeArt.Hex("#40245F"),
                fill,
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y),
                42,
                8
            );
            panel.type = Image.Type.Sliced;
            SetRect(panel.rectTransform, position, size);
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
            Image image = CreatePanel(parent, position, size, fill);
            image.gameObject.name = label + "-knapp";
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Text", image.transform, label, fontSize, Color.white);
            Stretch(text.rectTransform);
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = RuntimeArt.Hex("#40245F");
            outline.effectDistance = new Vector2(3f, -3f);
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
            GameObject host = new(name);
            host.transform.SetParent(parent, false);
            Text text = host.AddComponent<Text>();
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
            GameObject host = new(name);
            host.transform.SetParent(parent, false);
            Image image = host.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == null ? Image.Type.Simple : Image.Type.Sliced;
            return image;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 position,
            Vector2 size
        )
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
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
