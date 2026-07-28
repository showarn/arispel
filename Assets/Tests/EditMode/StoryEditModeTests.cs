using ArisMonsterTrucks.Stories;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArisMonsterTrucks.Tests
{
    public sealed class StoryEditModeTests
    {
        [Test]
        public void LillaLumiDefinitionIsComplete()
        {
            StoryDefinition story =
                Resources.Load<StoryDefinition>("Stories/lilla-lumi");

            Assert.IsNotNull(story);
            Assert.IsTrue(story.IsValid);
            Assert.AreEqual("lilla-lumi", story.StoryId);
            Assert.AreEqual("Lilla Lumi", story.Title);
            Assert.IsNotNull(story.Cover);
            Assert.IsNotNull(story.Narration);
            Assert.AreEqual(17, story.Pages.Count);
            Assert.IsTrue(story.Pages[0].IsCover);
            Assert.AreEqual(16, story.Pages.Count - 1);
        }

        [Test]
        public void AriAndSisterDefinitionIsComplete()
        {
            StoryDefinition story =
                Resources.Load<StoryDefinition>(
                    "Stories/ari-och-lillasystern"
                );

            Assert.IsNotNull(story);
            Assert.IsTrue(story.IsValid);
            Assert.AreEqual("ari-och-lillasystern", story.StoryId);
            Assert.AreEqual("Ari och lillasystern", story.Title);
            Assert.AreEqual(1, story.SortOrder);
            Assert.AreEqual("Ari är storebror.", story.EndMessage);
            Assert.IsNotNull(story.Cover);
            Assert.IsNotNull(story.Narration);
            Assert.AreEqual(20, story.Pages.Count);
            Assert.IsTrue(story.Pages[0].IsCover);
            Assert.AreEqual(19, story.Pages.Count - 1);
            Assert.AreEqual(0f, story.Pages[0].StartTime, 0.001f);
            Assert.AreEqual(
                story.Narration.length,
                story.Pages[story.Pages.Count - 1].EndTime,
                0.01f
            );
        }

        [TestCase("Stories/lilla-lumi", 17)]
        [TestCase("Stories/ari-och-lillasystern", 20)]
        public void EveryStoryPageHasContinuousAudioAndFinalArt(
            string resourcePath,
            int expectedPages
        )
        {
            StoryDefinition story =
                Resources.Load<StoryDefinition>(resourcePath);

            Assert.IsNotNull(story);
            Assert.AreEqual(expectedPages, story.Pages.Count);
            for (int index = 0; index < story.Pages.Count; index++)
            {
                StoryPage page = story.Pages[index];
                Assert.IsNotNull(
                    page.Illustration,
                    "Illustration saknas på index " + index
                );
                Assert.IsNotEmpty(page.Text);
                Assert.Greater(page.EndTime, page.StartTime);
                if (index > 0)
                {
                    Assert.AreEqual(
                        story.Pages[index - 1].EndTime,
                        page.StartTime,
                        0.001f
                    );
                }
            }

            Assert.AreEqual(
                story.Narration.length,
                story.Pages[story.Pages.Count - 1].EndTime,
                0.25f
            );
        }

        [Test]
        public void StoryCatalogLoadsLumiFromData()
        {
            StoryDefinition story = StoryCatalog.Get("lilla-lumi");

            Assert.IsNotNull(story);
            Assert.Contains(story, (System.Collections.IList)StoryCatalog.All);
        }

        [Test]
        public void StoryCatalogOrdersLumiBeforeAriAndSister()
        {
            StoryDefinition lumi = StoryCatalog.Get("lilla-lumi");
            StoryDefinition ari = StoryCatalog.Get("ari-och-lillasystern");

            Assert.IsNotNull(lumi);
            Assert.IsNotNull(ari);
            Assert.AreEqual(lumi, StoryCatalog.All[0]);
            Assert.AreEqual(ari, StoryCatalog.All[1]);
        }

        [Test]
        public void PadlockSpriteHasARealShackleBodyAndTransparentCorners()
        {
            Sprite padlock = RuntimeArt.PadlockSprite();

            Assert.IsNotNull(padlock);
            Assert.AreEqual(160f, padlock.rect.width);
            Assert.AreEqual(160f, padlock.rect.height);
            Texture2D texture = padlock.texture;
            Assert.Less(texture.GetPixel(0, 0).a, 0.01f);
            Assert.Greater(texture.GetPixel(80, 138).a, 0.9f);
            Assert.Greater(texture.GetPixel(80, 75).a, 0.9f);
            Assert.Greater(texture.GetPixel(80, 48).a, 0.9f);
        }

        [Test]
        public void CarouselSnapsAndRejectsShortSwipe()
        {
            GameObject host = new("Carousel test");
            GameObject viewport = new("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(host.transform);
            GameObject content = new("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform);
            ScrollRect scroll = host.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = content.GetComponent<RectTransform>();
            scroll.horizontal = true;
            LevelCarouselSnap snap = host.AddComponent<LevelCarouselSnap>();
            snap.Initialize(scroll, 2, null);

            PointerEventData drag = new(null);
            drag.position = new Vector2(500f, 200f);
            snap.OnBeginDrag(drag);
            drag.position = new Vector2(460f, 200f);
            snap.OnEndDrag(drag);
            Assert.AreEqual(0, snap.CurrentPage);

            drag.position = new Vector2(500f, 200f);
            snap.OnBeginDrag(drag);
            drag.position = new Vector2(390f, 200f);
            snap.OnEndDrag(drag);
            Assert.AreEqual(1, snap.CurrentPage);
            Assert.IsFalse(snap.CanActivateContent);

            Object.DestroyImmediate(host);
        }
    }
}
