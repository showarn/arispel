using System.Collections;
using System.Reflection;
using ArisMonsterTrucks.Fishing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Fishing.Tests
{
    public sealed class UiConsistencyPlayModeTests
    {
        private GameObject root;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null)
            {
                Object.Destroy(root);
            }

            GameObject frontEnd = GameObject.Find("Startskärm och garage");
            if (frontEnd != null)
            {
                Object.Destroy(frontEnd);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MonstertruckSelectorKeepsTopControlsInsideSafeArea()
        {
            root = new GameObject("Level selector test host");
            Camera camera = root.AddComponent<Camera>();
            FrontEndController.Create(root, camera);
            yield return null;

            FrontEndController controller =
                Object.FindFirstObjectByType<FrontEndController>();
            Invoke(controller, "ShowLevelSelect");
            yield return null;

            Transform selector = Find(controller.transform, "Banväljare");
            AssertTopControl(selector, "←-knapp", 85f, -58f);
            AssertTopControl(selector, "VERKSTAD-knapp", 300f, -58f);
            AssertTopControl(selector, "Myntsaldo", -160f, -58f);
        }

        [UnityTest]
        public IEnumerator FishingSelectorUsesLiveWorldAndHidesGameplayHud()
        {
            root = new GameObject(
                "Fishing selector test host",
                typeof(RectTransform)
            );
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            FishingGameController controller = FishingGameController.Create(
                root.transform,
                font,
                () => { }
            );
            controller.Show();
            yield return null;

            Transform selector = Find(
                controller.transform,
                "Fiskeplatsväljare"
            );
            Assert.IsNull(
                selector.GetComponent<Image>(),
                "Fiskeplatsväljaren ska inte täcka den levande fiskevyn."
            );
            AssertTopControl(selector, "←-knapp", 85f, -58f);
            Assert.IsFalse(
                Find(
                    controller.transform,
                    "Gemensam övre fiske-HUD"
                ).gameObject.activeSelf
            );
            Assert.IsFalse(
                Find(
                    controller.transform,
                    "Stor kontrolltouchyta"
                ).gameObject.activeSelf
            );
        }

        [UnityTest]
        public IEnumerator MemoryGameplayHasOnlyTheConsistentTopLeftBackButton()
        {
            root = new GameObject(
                "Memory layout test host",
                typeof(RectTransform)
            );
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            MemoryGameController controller = MemoryGameController.Create(
                root.transform,
                font,
                () => { }
            );
            Invoke(controller, "StartGame", 1);
            yield return null;

            Transform playArea = Find(controller.transform, "Memorybord");
            Transform safeRoot = Find(playArea, "Säker memory-HUD");
            AssertTopControl(safeRoot, "←-knapp", 85f, -58f);
            Assert.IsNull(FindOptional(playArea, "Drag"));
            Assert.IsNull(FindOptional(playArea, "Bäst"));
        }

        [UnityTest]
        public IEnumerator PuzzleScoreSitsBelowTitleAndAbovePieceLayer()
        {
            root = new GameObject(
                "Puzzle layout test host",
                typeof(RectTransform)
            );
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );
            PuzzleGameController controller = PuzzleGameController.Create(
                root.transform,
                font,
                () => { }
            );
            controller.StartPuzzle(1);
            yield return null;

            Transform playArea = Find(controller.transform, "Pusselbord");
            Transform safeRoot = Find(playArea, "Säker pussel-HUD");
            RectTransform score = Find(
                safeRoot,
                "Stjärnpoäng under pussel"
            ).GetComponent<RectTransform>();
            RectTransform title = Find(
                safeRoot,
                "Pusseltitel"
            ).GetComponent<RectTransform>();
            Transform pieceLayer = Find(playArea, "Pusselbitar");

            Assert.AreEqual(-145f, score.anchoredPosition.y, 0.01f);
            Assert.Less(score.anchoredPosition.y, title.anchoredPosition.y);
            Assert.Greater(
                safeRoot.GetSiblingIndex(),
                pieceLayer.GetSiblingIndex(),
                "HUD-lagret ska renderas ovanpå pusselbitarna."
            );
        }

        private static void AssertTopControl(
            Transform parent,
            string name,
            float expectedX,
            float expectedY
        )
        {
            RectTransform rect = Find(parent, name)
                .GetComponent<RectTransform>();
            if (expectedX < 0f)
            {
                Assert.AreEqual(Vector2.one, rect.anchorMin);
                Assert.AreEqual(Vector2.one, rect.anchorMax);
            }
            else
            {
                Assert.AreEqual(new Vector2(0f, 1f), rect.anchorMin);
                Assert.AreEqual(new Vector2(0f, 1f), rect.anchorMax);
            }
            Assert.AreEqual(expectedX, rect.anchoredPosition.x, 0.01f);
            Assert.AreEqual(expectedY, rect.anchoredPosition.y, 0.01f);
        }

        private static Transform Find(Transform rootTransform, string name)
        {
            Transform result = FindOptional(rootTransform, name);
            Assert.IsNotNull(result, "Saknar UI-elementet " + name);
            return result;
        }

        private static Transform FindOptional(
            Transform rootTransform,
            string name
        )
        {
            if (rootTransform.name == name)
            {
                return rootTransform;
            }

            for (int index = 0; index < rootTransform.childCount; index++)
            {
                Transform result = FindOptional(
                    rootTransform.GetChild(index),
                    name
                );
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void Invoke(
            object target,
            string methodName,
            params object[] arguments
        )
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(method, "Saknar metoden " + methodName);
            method.Invoke(target, arguments);
        }
    }
}
