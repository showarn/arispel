using NUnit.Framework;

namespace ArisMonsterTrucks.Tests
{
    public sealed class ParentalControlsEditModeTests
    {
        [TearDown]
        public void LeaveEditorTestsUnlocked()
        {
            ParentalControls.Configure(
                "2468",
                true,
                true,
                true,
                true
            );
        }

        [TestCase("1234", true)]
        [TestCase("12345678", true)]
        [TestCase("123", false)]
        [TestCase("123456789", false)]
        [TestCase("12A4", false)]
        [TestCase("１２３４", false)]
        [TestCase("١٢٣٤", false)]
        [TestCase("12 4", false)]
        [TestCase("12.4", false)]
        [TestCase("", false)]
        public void ParentPinRequiresFourToEightDigits(
            string value,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ParentalControls.IsValidPasswordFormat(value)
            );
        }

        [Test]
        public void CorrectParentPinUnlocksButWrongPinDoesNot()
        {
            ParentalControls.Configure(
                "7531",
                true,
                false,
                true,
                false
            );

            Assert.IsTrue(ParentalControls.VerifyPassword("7531"));
            Assert.IsFalse(ParentalControls.VerifyPassword("7532"));
            Assert.IsFalse(ParentalControls.VerifyPassword(""));
        }

        [Test]
        public void EveryGamePermissionIsStoredIndependently()
        {
            ParentalControls.Configure(
                "8642",
                true,
                false,
                true,
                false
            );

            Assert.IsTrue(
                ParentalControls.IsEnabled(ParentalGame.MonsterTrucks)
            );
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Puzzle));
            Assert.IsTrue(ParentalControls.IsEnabled(ParentalGame.Memory));
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Fishing));
            Assert.IsTrue(ParentalControls.IsEnabled(ParentalGame.Stories));

            ParentalControls.SetEnabled(ParentalGame.Puzzle, true);
            ParentalControls.SetEnabled(ParentalGame.Stories, false);
            Assert.IsTrue(ParentalControls.IsEnabled(ParentalGame.Puzzle));
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Fishing));
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Stories));
        }

        [Test]
        public void ChangingPinKeepsAllExistingGamePermissions()
        {
            ParentalControls.Configure(
                "2468",
                true,
                false,
                true,
                false,
                true
            );

            ParentalControls.ChangePassword("9753");

            Assert.IsFalse(ParentalControls.VerifyPassword("2468"));
            Assert.IsTrue(ParentalControls.VerifyPassword("9753"));
            Assert.IsTrue(
                ParentalControls.IsEnabled(ParentalGame.MonsterTrucks)
            );
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Puzzle));
            Assert.IsTrue(ParentalControls.IsEnabled(ParentalGame.Memory));
            Assert.IsFalse(ParentalControls.IsEnabled(ParentalGame.Fishing));
            Assert.IsTrue(ParentalControls.IsEnabled(ParentalGame.Stories));
        }
    }
}
