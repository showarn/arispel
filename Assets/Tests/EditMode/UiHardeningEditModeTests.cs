using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Tests
{
    public sealed class UiHardeningEditModeTests
    {
        [Test]
        public void NumericKeypadAcceptsOnlyAsciiDigitsAndSupportsDelete()
        {
            NumericKeypadState state = new(8);

            Assert.IsTrue(state.Append('0'));
            Assert.IsTrue(state.Append('9'));
            Assert.IsFalse(state.Append('A'));
            Assert.IsFalse(state.Append('١'));
            Assert.IsFalse(state.Append(' '));
            Assert.AreEqual("09", state.Value);
            Assert.IsTrue(state.DeleteLast());
            Assert.AreEqual("0", state.Value);
            state.Clear();
            Assert.AreEqual("", state.Value);
        }

        [Test]
        public void NumericKeypadUsesTheCentralPinLengthRule()
        {
            NumericKeypadState state = new(
                ParentalControls.MaximumPinLength
            );
            foreach (char digit in "123")
            {
                state.Append(digit);
            }
            Assert.IsFalse(
                state.CanConfirm(ParentalControls.MinimumPinLength)
            );
            state.Append('4');
            Assert.IsTrue(
                state.CanConfirm(ParentalControls.MinimumPinLength)
            );
            foreach (char digit in "56789")
            {
                state.Append(digit);
            }
            Assert.AreEqual(ParentalControls.MaximumPinLength, state.Length);
            Assert.AreEqual("12345678", state.Value);
        }

        [Test]
        public void ParentKeypadMasksPinAndContainsNoNativeInputField()
        {
            GameObject root = new("Keypad test", typeof(RectTransform));
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            ParentPinKeypad keypad = ParentPinKeypad.Create(
                root.transform,
                font,
                ParentalControls.MinimumPinLength,
                ParentalControls.MaximumPinLength,
                true,
                _ => { },
                () => { }
            );

            keypad.PressDigit('1');
            keypad.PressDigit('2');
            Assert.AreEqual("●●", keypad.DisplayValue);
            Assert.IsFalse(keypad.UsesNativeKeyboard);
            Assert.IsNull(
                keypad.GetComponentInChildren<InputField>(true)
            );

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SafeAreaCalculationIsStableAndClamped()
        {
            Rect safe = new(90f, 34f, 2340f, 1092f);
            Vector2Int screen = new(2532, 1170);
            SafeAreaFitter.CalculateAnchors(
                safe,
                screen,
                out Vector2 minimum,
                out Vector2 maximum
            );
            SafeAreaFitter.CalculateAnchors(
                safe,
                screen,
                out Vector2 secondMinimum,
                out Vector2 secondMaximum
            );

            Assert.AreEqual(minimum, secondMinimum);
            Assert.AreEqual(maximum, secondMaximum);
            Assert.Greater(minimum.x, 0f);
            Assert.Greater(minimum.y, 0f);
            Assert.Less(maximum.x, 1f);
            Assert.Less(maximum.y, 1f);
        }

        [TestCase(40f, 4)]
        [TestCase(55f, 4)]
        [TestCase(56f, 3)]
        [TestCase(80f, 3)]
        [TestCase(81f, 2)]
        [TestCase(120f, 2)]
        [TestCase(121f, 1)]
        public void RaceStarsAreAwardedFromElapsedTime(
            float elapsedSeconds,
            int expectedStars
        )
        {
            Assert.AreEqual(
                expectedStars,
                LevelProgression.CalculateStars(elapsedSeconds)
            );
        }
    }
}
