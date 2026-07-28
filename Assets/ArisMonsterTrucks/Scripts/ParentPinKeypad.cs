using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks
{
    public sealed class NumericKeypadState
    {
        private readonly int maxLength;
        private readonly StringBuilder value = new();

        public NumericKeypadState(int maxDigits)
        {
            maxLength = Mathf.Max(1, maxDigits);
        }

        public string Value => value.ToString();
        public int Length => value.Length;

        public bool Append(char digit)
        {
            if (digit < '0' || digit > '9' || value.Length >= maxLength)
            {
                return false;
            }
            value.Append(digit);
            return true;
        }

        public bool DeleteLast()
        {
            if (value.Length == 0)
            {
                return false;
            }
            value.Length--;
            return true;
        }

        public bool CanConfirm(int minDigits)
        {
            return value.Length >= Mathf.Max(1, minDigits);
        }

        public void Clear()
        {
            value.Clear();
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public sealed class ParentPinKeypad : MonoBehaviour
    {
        private NumericKeypadState state;
        private int minimumDigits;
        private bool masked;
        private Text displayText;
        private Button confirmButton;
        private Action<string> confirmed;
        private Action cancelled;

        public string Value => state?.Value ?? "";
        public bool UsesNativeKeyboard => false;
        public string DisplayValue => displayText?.text ?? "";

        public static ParentPinKeypad Create(
            Transform parent,
            Font font,
            int minDigits,
            int maxDigits,
            bool mask,
            Action<string> onConfirmed,
            Action onCancelled
        )
        {
            GameObject host = new("Eget numeriskt tangentbord", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -55f);
            rect.sizeDelta = new Vector2(660f, 650f);

            ParentPinKeypad keypad = host.AddComponent<ParentPinKeypad>();
            keypad.state = new NumericKeypadState(maxDigits);
            keypad.minimumDigits = minDigits;
            keypad.masked = mask;
            keypad.confirmed = onConfirmed;
            keypad.cancelled = onCancelled;
            keypad.Build(font);
            return keypad;
        }

        public void Clear()
        {
            state.Clear();
            Refresh();
        }

        public bool PressDigit(char digit)
        {
            bool changed = state.Append(digit);
            Refresh();
            return changed;
        }

        public bool DeleteLast()
        {
            bool changed = state.DeleteLast();
            Refresh();
            return changed;
        }

        private void Build(Font font)
        {
            Image display = CreatePanel(
                transform,
                new Vector2(0f, 255f),
                new Vector2(520f, 86f),
                Color.white
            );
            displayText = CreateText(display.transform, "", 42, RuntimeArt.Hex("#40245F"), font);
            Stretch(displayText.rectTransform);

            string[] labels =
            {
                "1", "2", "3", "4", "5", "6",
                "7", "8", "9", "←", "0", "✓"
            };
            for (int index = 0; index < labels.Length; index++)
            {
                int row = index / 3;
                int column = index % 3;
                string label = labels[index];
                Button button = CreateButton(
                    transform,
                    label,
                    new Vector2((column - 1) * 165f, 145f - row * 112f),
                    new Vector2(145f, 94f),
                    label == "✓"
                        ? RuntimeArt.Hex("#4FC66A")
                        : label == "←"
                            ? RuntimeArt.Hex("#D17B36")
                            : RuntimeArt.Hex("#7A5AA6"),
                    38,
                    font
                );
                if (label == "←")
                {
                    button.onClick.AddListener(() => DeleteLast());
                }
                else if (label == "✓")
                {
                    confirmButton = button;
                    button.onClick.AddListener(Confirm);
                }
                else
                {
                    char digit = label[0];
                    button.onClick.AddListener(() => PressDigit(digit));
                }
            }

            Button cancel = CreateButton(
                transform,
                "AVBRYT",
                new Vector2(0f, -330f),
                new Vector2(475f, 72f),
                RuntimeArt.Hex("#818795"),
                28,
                font
            );
            cancel.onClick.AddListener(() =>
            {
                Clear();
                cancelled?.Invoke();
            });
            Refresh();
        }

        private void Confirm()
        {
            if (!state.CanConfirm(minimumDigits))
            {
                return;
            }
            string submitted = state.Value;
            Clear();
            confirmed?.Invoke(submitted);
        }

        private void Refresh()
        {
            if (displayText != null)
            {
                displayText.text = masked
                    ? new string('●', state.Length)
                    : state.Value;
            }
            if (confirmButton != null)
            {
                confirmButton.interactable = state.CanConfirm(minimumDigits);
            }
        }

        private static Image CreatePanel(
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color fill
        )
        {
            GameObject host = new("Knappyta", typeof(RectTransform), typeof(Image));
            host.transform.SetParent(parent, false);
            Image image = host.GetComponent<Image>();
            image.sprite = RuntimeArt.RoundedRectangleSprite(
                "ParentKeypad_" + size + fill,
                RuntimeArt.Hex("#40245F"),
                fill,
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y),
                30,
                6
            );
            image.type = Image.Type.Sliced;
            SetRect(image.rectTransform, position, size);
            return image;
        }

        private static Button CreateButton(
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color fill,
            int fontSize,
            Font font
        )
        {
            Image image = CreatePanel(parent, position, size, fill);
            image.gameObject.name = label + "-knapp";
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.pressedColor = Color.Lerp(fill, Color.black, 0.18f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.48f, 0.65f);
            button.colors = colors;
            Text text = CreateText(image.transform, label, fontSize, Color.white, font);
            Stretch(text.rectTransform);
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = RuntimeArt.Hex("#40245F");
            outline.effectDistance = new Vector2(3f, -3f);
            return button;
        }

        private static Text CreateText(
            Transform parent,
            string value,
            int size,
            Color color,
            Font font
        )
        {
            GameObject host = new("Text", typeof(RectTransform), typeof(Text));
            host.transform.SetParent(parent, false);
            Text text = host.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
