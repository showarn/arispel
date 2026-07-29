using System;
using System.Collections;
using ArisMonsterTrucks.Fishing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Fishing.Tests
{
    public sealed class FishingPlayModeTests
    {
        private GameObject root;
        private FishingGameController controller;
        private bool backInvoked;
        private string savedCollection;
        private bool hadSavedCollection;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadSavedCollection = PlayerPrefs.HasKey(
                FishingSaveService.SaveKey
            );
            savedCollection = PlayerPrefs.GetString(
                FishingSaveService.SaveKey,
                ""
            );
            PlayerPrefs.DeleteKey(FishingSaveService.SaveKey);
            root = new GameObject("Fishing test root", typeof(RectTransform));
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            controller = FishingGameController.Create(
                root.transform,
                font,
                () => backInvoked = true
            );
            controller.Show();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            AppPreferences.SoundEnabled = true;
            if (hadSavedCollection)
            {
                PlayerPrefs.SetString(
                    FishingSaveService.SaveKey,
                    savedCollection
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(FishingSaveService.SaveKey);
            }
            PlayerPrefs.Save();
            if (root != null)
            {
                UnityEngine.Object.Destroy(root);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneCanOpenWithoutExceptions()
        {
            Assert.IsNotNull(controller);
            LogAssert.NoUnexpectedReceived();
            yield return null;
        }

        [UnityTest]
        public IEnumerator IdleShowsCast()
        {
            Assert.AreEqual(FishingState.Idle, controller.CurrentState);
            Assert.AreEqual("KASTA", controller.MainButtonLabel);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CastStartsCasting()
        {
            controller.PressPrimaryButton();
            Assert.AreEqual(FishingState.Casting, controller.CurrentState);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CastingAdvancesToWaiting()
        {
            controller.PressPrimaryButton();
            yield return new WaitForSecondsRealtime(1.1f);
            Assert.AreEqual(
                FishingState.WaitingForBite,
                controller.CurrentState
            );
        }

        [UnityTest]
        public IEnumerator EarlyPressDoesNotCatchFish()
        {
            controller.PressPrimaryButton();
            yield return new WaitForSecondsRealtime(1.1f);
            controller.PressPrimaryButton();
            Assert.AreEqual(
                FishingState.WaitingForBite,
                controller.CurrentState
            );
        }

        [UnityTest]
        public IEnumerator FishBitingShowsReelPrompt()
        {
            yield return WaitForBite();
            Assert.AreEqual(FishingState.FishBiting, controller.CurrentState);
            Assert.AreEqual("DRA UPP!", controller.MainButtonLabel);
        }

        [UnityTest]
        public IEnumerator CorrectPressStartsReeling()
        {
            yield return WaitForBite();
            controller.PressPrimaryButton();
            Assert.AreEqual(FishingState.ReelingIn, controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator CatchOpensReveal()
        {
            yield return CatchFish();
            Assert.AreEqual(FishingState.CatchReveal, controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator CaughtFishIsSavedInBook()
        {
            int before = new FishCollectionService(
                new FishingSaveService(new PlayerPrefsStore())
            ).DiscoveredCount;
            yield return CatchFish();
            int after = new FishCollectionService(
                new FishingSaveService(new PlayerPrefsStore())
            ).DiscoveredCount;
            Assert.GreaterOrEqual(after, before);
        }

        [UnityTest]
        public IEnumerator ContinueReturnsToIdle()
        {
            yield return CatchFish();
            Button continueButton = FindButton("BRA JOBBAT!-knapp");
            Assert.IsNotNull(continueButton);
            continueButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.AreEqual(FishingState.Idle, controller.CurrentState);
            Assert.AreEqual("KASTA IGEN", controller.MainButtonLabel);
        }

        [UnityTest]
        public IEnumerator MissedFishDoesNotCreateGameOver()
        {
            yield return WaitForBite();
            yield return new WaitForSecondsRealtime(4.5f);
            Assert.AreEqual(FishingState.Idle, controller.CurrentState);
            Assert.AreEqual("KASTA IGEN", controller.MainButtonLabel);
        }

        [UnityTest]
        public IEnumerator GameCanPauseAndResume()
        {
            controller.gameObject.SendMessage(
                "OnApplicationPause",
                true,
                SendMessageOptions.RequireReceiver
            );
            Assert.AreEqual(FishingState.Paused, controller.CurrentState);
            controller.gameObject.SendMessage(
                "OnApplicationPause",
                false,
                SendMessageOptions.RequireReceiver
            );
            Assert.AreEqual(FishingState.Idle, controller.CurrentState);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BackReturnsToMiniGameMenu()
        {
            Button back = FindButton("←-knapp");
            Assert.IsNotNull(back);
            back.onClick.Invoke();
            Assert.IsTrue(backInvoked);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SoundToggleControlsGlobalAudio()
        {
            AppPreferences.SoundEnabled = false;
            Assert.AreEqual(0f, AudioListener.volume);
            AppPreferences.SoundEnabled = true;
            Assert.AreEqual(1f, AudioListener.volume);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DoublePressCannotStartTwoCatchFlows()
        {
            controller.PressPrimaryButton();
            controller.PressPrimaryButton();
            Assert.AreEqual(FishingState.Casting, controller.CurrentState);
            yield return null;
        }

        private IEnumerator WaitForBite()
        {
            controller.PressPrimaryButton();
            float deadline = Time.realtimeSinceStartup + 5.4f;
            while (
                controller.CurrentState != FishingState.FishBiting
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            Assert.AreEqual(FishingState.FishBiting, controller.CurrentState);
        }

        private IEnumerator CatchFish()
        {
            yield return WaitForBite();
            controller.PressPrimaryButton();
            float deadline = Time.realtimeSinceStartup + 2f;
            while (
                controller.CurrentState != FishingState.CatchReveal
                && Time.realtimeSinceStartup < deadline
            )
            {
                yield return null;
            }
            Assert.AreEqual(FishingState.CatchReveal, controller.CurrentState);
        }

        private Button FindButton(string name)
        {
            Button[] buttons = controller.GetComponentsInChildren<Button>(true);
            for (int index = 0; index < buttons.Length; index++)
            {
                if (buttons[index].gameObject.name == name)
                {
                    return buttons[index];
                }
            }
            return null;
        }
    }
}
