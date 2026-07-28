using System.Collections;
using System.Reflection;
using ArisMonsterTrucks.Stories;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Tests
{
    public sealed class StoryPlayModeTests
    {
        private GameObject root;
        private StorybookController controller;
        private StoryDefinition story;
        private bool backInvoked;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("Story test root", typeof(RectTransform));
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            story = Resources.Load<StoryDefinition>("Stories/lilla-lumi");
            controller = StorybookController.Create(
                root.transform,
                font,
                () => backInvoked = true
            );
            controller.Show(story);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null)
            {
                Object.Destroy(root);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator StoryOpensAtCoverAndCanLeaveCleanly()
        {
            Assert.IsTrue(controller.IsVisible);
            Assert.AreEqual(0, controller.CurrentPageIndex);
            RectTransform controllerRect =
                controller.GetComponent<RectTransform>();
            Assert.AreEqual(Vector2.zero, controllerRect.anchorMin);
            Assert.AreEqual(Vector2.one, controllerRect.anchorMax);
            Assert.AreEqual(Vector2.zero, controllerRect.offsetMin);
            Assert.AreEqual(Vector2.zero, controllerRect.offsetMax);

            FindButtonByLabel("←").onClick.Invoke();
            yield return null;

            Assert.IsTrue(backInvoked);
            Assert.IsFalse(controller.IsVisible);
            Assert.IsFalse(controller.IsNarrationPlaying);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator StoryChromeMatchesTheLillaLumiReferenceLayout()
        {
            RectTransform restart = FindChildByName(
                controller.transform,
                "BÖRJA OM knapp"
            ).GetComponent<RectTransform>();
            RectTransform previous = FindChildByName(
                controller.transform,
                "‹ knapp"
            ).GetComponent<RectTransform>();
            RectTransform playPause = FindChildByName(
                controller.transform,
                " knapp"
            ).GetComponent<RectTransform>();
            RectTransform next = FindChildByName(
                controller.transform,
                "› knapp"
            ).GetComponent<RectTransform>();
            RectTransform pageNumber = FindChildByName(
                controller.transform,
                "Sidnummer"
            ).GetComponent<RectTransform>();

            Assert.AreEqual(new Vector2(760f, 468f), restart.anchoredPosition);
            Assert.AreEqual(new Vector2(325f, -445f), previous.anchoredPosition);
            Assert.AreEqual(new Vector2(490f, -445f), playPause.anchoredPosition);
            Assert.AreEqual(new Vector2(655f, -445f), next.anchoredPosition);
            Assert.AreEqual(
                new Vector2(600f, -485f),
                pageNumber.anchoredPosition
            );
            Assert.IsNull(
                FindChildByName(
                    controller.transform,
                    "Visa eller dölj text"
                )
            );
            Assert.IsNull(
                FindChildByName(
                    controller.transform,
                    "Berättarröst av eller på"
                )
            );
            yield return null;
        }

        [UnityTest]
        public IEnumerator NextAndPreviousSeekToPageStart()
        {
            FindButtonByLabel("›").onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, controller.CurrentPageIndex);
            Assert.AreEqual(
                story.Pages[1].StartTime,
                controller.NarrationTime,
                0.2f
            );

            FindButtonByLabel("‹").onClick.Invoke();
            yield return null;
            Assert.AreEqual(0, controller.CurrentPageIndex);
            Assert.AreEqual(0f, controller.NarrationTime, 0.2f);
        }

        [UnityTest]
        public IEnumerator PlayPauseKeepsPositionAndContinues()
        {
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.IsTrue(controller.IsNarrationPlaying);

            FindButtonByLabel("PAUSA").onClick.Invoke();
            float pausedAt = controller.NarrationTime;
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.IsFalse(controller.IsNarrationPlaying);
            Assert.AreEqual(pausedAt, controller.NarrationTime, 0.04f);

            FindButtonByLabel("SPELA").onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.IsTrue(controller.IsNarrationPlaying);
            Assert.Greater(controller.NarrationTime, pausedAt);
        }

        [UnityTest]
        public IEnumerator SystemPauseDoesNotResetNarration()
        {
            controller.SeekToPage(5, true);
            yield return new WaitForSecondsRealtime(0.1f);
            float beforePause = controller.NarrationTime;

            controller.gameObject.SendMessage(
                "OnApplicationPause",
                true,
                SendMessageOptions.RequireReceiver
            );
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(beforePause, controller.NarrationTime, 0.05f);

            controller.gameObject.SendMessage(
                "OnApplicationPause",
                false,
                SendMessageOptions.RequireReceiver
            );
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.Greater(controller.NarrationTime, beforePause);
        }

        [UnityTest]
        public IEnumerator EveryAudioBoundarySelectsItsExpectedPage()
        {
            MethodInfo findPage = typeof(StorybookController).GetMethod(
                "FindPageForTime",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(findPage);

            for (int index = 0; index < story.Pages.Count; index++)
            {
                float probe = Mathf.Lerp(
                    story.Pages[index].StartTime,
                    story.Pages[index].EndTime,
                    0.5f
                );
                int result = (int)findPage.Invoke(
                    controller,
                    new object[] { probe }
                );
                Assert.AreEqual(index, result, "Fel sida vid " + probe);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator EndViewCanRestartWithoutOverlappingState()
        {
            MethodInfo showEnd = typeof(StorybookController).GetMethod(
                "ShowFinalView",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            showEnd.Invoke(controller, null);
            yield return null;

            FindButtonByLabel("LÄS IGEN").onClick.Invoke();
            yield return null;

            Assert.IsTrue(controller.IsVisible);
            Assert.AreEqual(0, controller.CurrentPageIndex);
            Assert.AreEqual(0f, controller.NarrationTime, 0.25f);
        }

        [UnityTest]
        public IEnumerator AriStoryUsesSamePlayerAndPreservesNavigationState()
        {
            StoryDefinition ari = Resources.Load<StoryDefinition>(
                "Stories/ari-och-lillasystern"
            );
            Assert.IsNotNull(ari);

            controller.Show(ari);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(20, ari.Pages.Count);
            Assert.AreEqual(0, controller.CurrentPageIndex);
            Assert.IsTrue(controller.IsNarrationPlaying);

            FindButtonByLabel("PAUSA").onClick.Invoke();
            FindButtonByLabel("›").onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, controller.CurrentPageIndex);
            Assert.IsFalse(controller.IsNarrationPlaying);
            Assert.AreEqual(
                ari.Pages[1].StartTime,
                controller.NarrationTime,
                0.2f
            );

            FindButtonByLabel("SPELA").onClick.Invoke();
            FindButtonByLabel("›").onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(2, controller.CurrentPageIndex);
            Assert.IsTrue(controller.IsNarrationPlaying);

            FindButtonByLabel("←").onClick.Invoke();
            yield return null;
            Assert.IsFalse(controller.IsNarrationPlaying);

            controller.Show(ari);
            yield return null;
            Assert.AreEqual(0, controller.CurrentPageIndex);
            Assert.AreEqual(0f, controller.NarrationTime, 0.25f);

            controller.ShowCompletionPreview();
            yield return null;
            FindButtonByLabel("LÄS IGEN").onClick.Invoke();
            yield return null;
            Assert.AreEqual(0, controller.CurrentPageIndex);
            Assert.AreEqual(0f, controller.NarrationTime, 0.25f);
        }

        [UnityTest]
        public IEnumerator EveryAriAudioBoundarySelectsItsExpectedPage()
        {
            StoryDefinition ari = Resources.Load<StoryDefinition>(
                "Stories/ari-och-lillasystern"
            );
            controller.Show(ari);
            MethodInfo findPage = typeof(StorybookController).GetMethod(
                "FindPageForTime",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(findPage);

            for (int index = 0; index < ari.Pages.Count; index++)
            {
                float probe = Mathf.Lerp(
                    ari.Pages[index].StartTime,
                    ari.Pages[index].EndTime,
                    0.5f
                );
                int result = (int)findPage.Invoke(
                    controller,
                    new object[] { probe }
                );
                Assert.AreEqual(index, result, "Fel Ari-sida vid " + probe);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator DashboardPageTwoOpensStoryHubWithLumiThenAri()
        {
            ParentalControls.Configure(
                "2468",
                true,
                true,
                true,
                true,
                true
            );
            EventSystem existingEventSystem =
                Object.FindFirstObjectByType<EventSystem>();
            GameObject cameraHost = new("Dashboard test host");
            Camera camera = cameraHost.AddComponent<Camera>();
            FrontEndController.Create(cameraHost, camera);
            yield return null;

            FrontEndController frontEnd =
                Object.FindFirstObjectByType<FrontEndController>();
            Assert.IsNotNull(frontEnd);
            Transform storyCategory = FindChildByName(
                frontEnd.transform,
                "Sagokategori startsida"
            );
            Assert.IsNotNull(storyCategory);
            Assert.AreEqual(
                2190f,
                storyCategory
                    .GetComponent<RectTransform>()
                    .anchoredPosition.x,
                0.01f
            );
            Assert.IsNull(
                FindChildByName(
                    frontEnd.transform,
                    "Berättelsekort startsida lilla-lumi"
                )
            );
            Assert.IsNull(
                FindChildByName(
                    frontEnd.transform,
                    "Berättelsekort startsida ari-och-lillasystern"
                )
            );

            Transform dashboardScroller = FindChildByName(
                frontEnd.transform,
                "Horisontell startsidesvep"
            );
            Assert.IsNotNull(dashboardScroller);
            LevelCarouselSnap dashboardSnap =
                dashboardScroller.GetComponent<LevelCarouselSnap>();
            Assert.IsNotNull(dashboardSnap);
            dashboardSnap.GoToPage(1);
            Assert.AreEqual(1, dashboardSnap.CurrentPage);

            Button storyCategoryButton =
                storyCategory.GetComponent<Button>();
            Assert.IsNotNull(storyCategoryButton);
            storyCategoryButton.onClick.Invoke();
            yield return null;

            Transform lumiCard = FindChildByName(
                frontEnd.transform,
                "Berättelsekort lilla-lumi"
            );
            Transform ariCard = FindChildByName(
                frontEnd.transform,
                "Berättelsekort ari-och-lillasystern"
            );
            Assert.IsNotNull(lumiCard);
            Assert.IsNotNull(ariCard);
            Assert.AreEqual(
                -230f,
                lumiCard.GetComponent<RectTransform>().anchoredPosition.x,
                0.01f
            );
            Assert.AreEqual(
                230f,
                ariCard.GetComponent<RectTransform>().anchoredPosition.x,
                0.01f
            );

            Button lumiButton = lumiCard.GetComponent<Button>();
            Button ariButton = ariCard.GetComponent<Button>();
            Assert.IsNotNull(lumiButton);
            Assert.IsNotNull(ariButton);

            lumiButton.onClick.Invoke();
            yield return null;
            StorybookController storybook =
                Object.FindFirstObjectByType<StorybookController>();
            Assert.IsNotNull(storybook);
            Assert.IsTrue(storybook.IsVisible);
            FieldInfo activeStoryField = typeof(StorybookController).GetField(
                "story",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(activeStoryField);
            StoryDefinition activeStory =
                (StoryDefinition)activeStoryField.GetValue(storybook);
            Assert.AreEqual("lilla-lumi", activeStory.StoryId);

            storybook.Hide();
            ariButton.onClick.Invoke();
            yield return null;
            Assert.IsTrue(storybook.IsVisible);
            activeStory = (StoryDefinition)activeStoryField.GetValue(storybook);
            Assert.AreEqual("ari-och-lillasystern", activeStory.StoryId);

            ParentalGateController parentalGate =
                Object.FindFirstObjectByType<ParentalGateController>();
            Assert.IsNotNull(parentalGate);
            FieldInfo togglesField =
                typeof(ParentalGateController).GetField(
                    "toggles",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
            Assert.IsNotNull(togglesField);
            var parentalToggles =
                (System.Collections.IDictionary)togglesField.GetValue(
                    parentalGate
                );
            Assert.IsTrue(parentalToggles.Contains(ParentalGame.Stories));

            Object.Destroy(frontEnd.gameObject);
            Object.Destroy(cameraHost);
            EventSystem currentEventSystem =
                Object.FindFirstObjectByType<EventSystem>();
            if (
                currentEventSystem != null
                && currentEventSystem != existingEventSystem
            )
            {
                Object.Destroy(currentEventSystem.gameObject);
            }
            yield return null;
        }

        private static Transform FindChildByName(
            Transform parent,
            string childName
        )
        {
            foreach (
                Transform child in parent.GetComponentsInChildren<Transform>(
                    true
                )
            )
            {
                if (child.name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        private Button FindButtonByLabel(string value)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                foreach (Text text in button.GetComponentsInChildren<Text>(true))
                {
                    if (text.text == value)
                    {
                        return button;
                    }
                }
            }
            Assert.Fail("Kunde inte hitta knappen " + value);
            return null;
        }
    }
}
