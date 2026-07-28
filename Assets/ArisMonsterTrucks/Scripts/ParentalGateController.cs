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
        private ParentPinKeypad challengeKeypad;
        private ParentPinKeypad passwordKeypad;
        private ParentPinKeypad unlockKeypad;
        private Text challengeStatus;
        private Text passwordStatus;
        private Text settingsStatus;
        private Text unlockStatus;
        private Text settingsTitle;
        private Text settingsStep;
        private bool initialSetupMode;
        private bool passwordChangeMode;
        private string pendingPassword = "";
        private string firstPasswordEntry = "";
        private Text passwordPrompt;
        private Button saveSettingsButton;
        private Button changePinButton;

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
            challengeKeypad.Clear();
            challengeRoot.SetActive(true);
            challengeRoot.transform.SetAsLastSibling();
        }

        public void ShowUnlock()
        {
            HideAll();
            unlockStatus.text = "";
            unlockKeypad.Clear();
            unlockRoot.SetActive(true);
            unlockRoot.transform.SetAsLastSibling();
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

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                HideAll();
            }
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
                Vector2.zero,
                new Vector2(1280f, 1010f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "VUXENKONTROLL", 58, 430f);
            Text step = CreateText(
                "Steg",
                panel.transform,
                "STEG 1 AV 3",
                22,
                RuntimeArt.Hex("#8B7694")
            );
            SetRect(step.rectTransform, new Vector2(0f, 375f), new Vector2(500f, 40f));
            Text intro = CreateText(
                "Förklaring",
                panel.transform,
                "INNAN FÖRÄLDRAINSTÄLLNINGARNA ÖPPNAS\nBEHÖVER EN VUXEN SVARA PÅ FRÅGAN.",
                28,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(intro.rectTransform, new Vector2(0f, 305f), new Vector2(1000f, 75f));

            Text question = CreateText(
                "Vuxenfråga",
                panel.transform,
                "25 + 75 = ?",
                72,
                RuntimeArt.Hex("#3E2858")
            );
            SetRect(question.rectTransform, new Vector2(0f, 225f), new Vector2(720f, 85f));
            challengeKeypad = ParentPinKeypad.Create(
                panel.transform,
                font,
                1,
                3,
                false,
                CheckAdultAnswer,
                HideAll
            );
            challengeKeypad.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0f, -65f);

            challengeStatus = CreateText(
                "Svarstatus",
                panel.transform,
                "",
                27,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(challengeStatus.rectTransform, new Vector2(0f, -455f), new Vector2(900f, 45f));
        }

        private void BuildPasswordSetup()
        {
            passwordRoot = CreateBackdrop("Skapa föräldrakod");
            Image panel = CreatePanel(
                passwordRoot.transform,
                Vector2.zero,
                new Vector2(1280f, 1010f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "SKAPA FÖRÄLDRAKOD", 56, 430f);
            Text step = CreateText(
                "Steg",
                panel.transform,
                "STEG 2 AV 3",
                22,
                RuntimeArt.Hex("#8B7694")
            );
            SetRect(step.rectTransform, new Vector2(0f, 375f), new Vector2(500f, 40f));
            Text explanation = CreateText(
                "Förklaring",
                panel.transform,
                "KODEN BEHÖVS NÄR SPEL SKA SLÅS AV ELLER PÅ SENARE.",
                27,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(explanation.rectTransform, new Vector2(0f, 325f), new Vector2(1000f, 50f));
            passwordPrompt = CreateText(
                "Kodetikett",
                panel.transform,
                "VÄLJ 4–8 SIFFROR",
                25,
                RuntimeArt.Hex("#4A266C")
            );
            SetRect(passwordPrompt.rectTransform, new Vector2(0f, 275f), new Vector2(700f, 45f));
            passwordKeypad = ParentPinKeypad.Create(
                panel.transform,
                font,
                ParentalControls.MinimumPinLength,
                ParentalControls.MaximumPinLength,
                true,
                ConfirmNewPassword,
                HideAll
            );
            passwordKeypad.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0f, -65f);
            passwordStatus = CreateText(
                "Kodstatus",
                panel.transform,
                "",
                26,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(passwordStatus.rectTransform, new Vector2(0f, -455f), new Vector2(900f, 45f));
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

            changePinButton = CreateButton(
                panel.transform,
                "ÄNDRA KOD",
                new Vector2(-410f, -382f),
                new Vector2(330f, 92f),
                RuntimeArt.Hex("#7A5AA6"),
                30
            );
            changePinButton.onClick.AddListener(BeginPasswordChange);

            saveSettingsButton = CreateButton(
                panel.transform,
                "SPARA OCH FORTSÄTT",
                new Vector2(0f, -382f),
                new Vector2(610f, 92f),
                RuntimeArt.Hex("#4FC66A"),
                34
            );
            saveSettingsButton.onClick.AddListener(SaveSettings);

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
                new Vector2(1100f, 1010f),
                RuntimeArt.Hex("#FFF7D6")
            );
            CreateHeading(panel.transform, "FÖRÄLDRAR", 58, 430f);
            Text prompt = CreateText(
                "Kodfråga",
                panel.transform,
                "ANGE FÖRÄLDRAKODEN",
                30,
                RuntimeArt.Hex("#66546F")
            );
            SetRect(prompt.rectTransform, new Vector2(0f, 355f), new Vector2(700f, 60f));
            unlockKeypad = ParentPinKeypad.Create(
                panel.transform,
                font,
                ParentalControls.MinimumPinLength,
                ParentalControls.MaximumPinLength,
                true,
                VerifyUnlock,
                HideAll
            );
            unlockKeypad.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0f, -65f);
            unlockStatus = CreateText(
                "Kodstatus",
                panel.transform,
                "",
                26,
                RuntimeArt.Hex("#C33C55")
            );
            SetRect(unlockStatus.rectTransform, new Vector2(0f, -455f), new Vector2(800f, 45f));
        }

        private void CheckAdultAnswer(string answer)
        {
            if (answer != "100")
            {
                challengeStatus.text = "DET VAR INTE RÄTT – FÖRSÖK IGEN";
                challengeKeypad.Clear();
                return;
            }

            ShowPasswordSetup();
        }

        private void ShowPasswordSetup()
        {
            HideAll();
            passwordChangeMode = false;
            PreparePasswordSetup();
        }

        private void BeginPasswordChange()
        {
            HideAll();
            passwordChangeMode = true;
            PreparePasswordSetup();
        }

        private void PreparePasswordSetup()
        {
            pendingPassword = "";
            firstPasswordEntry = "";
            passwordPrompt.text = passwordChangeMode
                ? "VÄLJ EN NY KOD MED 4–8 SIFFROR"
                : "VÄLJ 4–8 SIFFROR";
            passwordKeypad.Clear();
            passwordStatus.text = "";
            passwordRoot.SetActive(true);
            passwordRoot.transform.SetAsLastSibling();
        }

        private void ConfirmNewPassword(string value)
        {
            if (!ParentalControls.IsValidPasswordFormat(value))
            {
                passwordStatus.text = "KODEN SKA VARA 4–8 SIFFROR";
                return;
            }
            if (string.IsNullOrEmpty(firstPasswordEntry))
            {
                firstPasswordEntry = value;
                passwordPrompt.text = "UPPREPA KODEN";
                passwordStatus.text = "";
                return;
            }
            if (firstPasswordEntry != value)
            {
                passwordStatus.text = "KODERNA ÄR INTE LIKADANA";
                firstPasswordEntry = "";
                passwordPrompt.text = "VÄLJ EN NY KOD";
                return;
            }

            pendingPassword = firstPasswordEntry;
            firstPasswordEntry = "";
            if (passwordChangeMode)
            {
                ParentalControls.ChangePassword(pendingPassword);
                pendingPassword = "";
                passwordChangeMode = false;
                ShowSettings(false);
                settingsStatus.text = "KODEN ÄR ÄNDRAD";
                return;
            }
            ShowSettings(true);
        }

        private void VerifyUnlock(string value)
        {
            if (!ParentalControls.VerifyPassword(value))
            {
                unlockStatus.text = "FEL KOD – FÖRSÖK IGEN";
                unlockKeypad.Clear();
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
            changePinButton.gameObject.SetActive(!isInitialSetup);
            SetRect(
                saveSettingsButton.image.rectTransform,
                isInitialSetup
                    ? new Vector2(0f, -382f)
                    : new Vector2(220f, -382f),
                isInitialSetup
                    ? new Vector2(610f, 92f)
                    : new Vector2(610f, 92f)
            );
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
            background.gameObject.AddComponent<SafeAreaFullBleed>();
            background.type = Image.Type.Simple;
            Stretch(background.rectTransform);
            Image shade = CreateImage("Vuxentoning", root.transform, null);
            shade.gameObject.AddComponent<SafeAreaFullBleed>();
            shade.color = new Color(0.04f, 0.08f, 0.18f, 0.72f);
            Stretch(shade.rectTransform);
            return root;
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
            challengeKeypad?.Clear();
            passwordKeypad?.Clear();
            unlockKeypad?.Clear();
            firstPasswordEntry = "";
            passwordChangeMode = false;
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
