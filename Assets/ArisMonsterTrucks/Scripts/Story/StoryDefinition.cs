using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Stories
{
    public enum StoryAnimationType
    {
        GentleDrift,
        SlowZoom,
        SoftBob,
        MoonlightGlow
    }

    [Serializable]
    public sealed class StoryPage
    {
        [SerializeField] private Sprite illustration;
        [SerializeField, TextArea(4, 12)] private string text;
        [SerializeField, Min(0f)] private float startTime;
        [SerializeField, Min(0f)] private float endTime;
        [SerializeField] private StoryAnimationType animationType;
        [SerializeField] private Vector2 parallaxDirection = new(1f, 0.25f);
        [SerializeField, Range(1f, 1.08f)] private float zoom = 1.025f;
        [SerializeField] private bool cover;

        public Sprite Illustration => illustration;
        public string Text => text;
        public float StartTime => startTime;
        public float EndTime => endTime;
        public StoryAnimationType AnimationType => animationType;
        public Vector2 ParallaxDirection => parallaxDirection;
        public float Zoom => zoom;
        public bool IsCover => cover;

#if UNITY_EDITOR
        public void Configure(
            Sprite pageIllustration,
            string pageText,
            float pageStartTime,
            float pageEndTime,
            StoryAnimationType pageAnimation,
            Vector2 pageParallax,
            float pageZoom,
            bool isCover = false
        )
        {
            illustration = pageIllustration;
            text = pageText;
            startTime = pageStartTime;
            endTime = pageEndTime;
            animationType = pageAnimation;
            parallaxDirection = pageParallax;
            zoom = pageZoom;
            cover = isCover;
        }
#endif
    }

    [CreateAssetMenu(
        fileName = "StoryDefinition",
        menuName = "Aris/Story Definition"
    )]
    public sealed class StoryDefinition : ScriptableObject
    {
        [SerializeField] private string storyId;
        [SerializeField] private string title;
        [SerializeField] private int sortOrder;
        [SerializeField] private string endMessage;
        [SerializeField] private Sprite cover;
        [SerializeField] private AudioClip narration;
        [SerializeField] private List<StoryPage> pages = new();

        public string StoryId => storyId;
        public string Title => title;
        public int SortOrder => sortOrder;
        public string EndMessage => endMessage;
        public Sprite Cover => cover;
        public AudioClip Narration => narration;
        public IReadOnlyList<StoryPage> Pages => pages;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(storyId)
            && !string.IsNullOrWhiteSpace(title)
            && cover != null
            && narration != null
            && pages != null
            && pages.Count > 0;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string storyTitle,
            Sprite storyCover,
            AudioClip narrationClip,
            List<StoryPage> storyPages,
            int storySortOrder = 0,
            string storyEndMessage = null
        )
        {
            storyId = id;
            title = storyTitle;
            sortOrder = storySortOrder;
            endMessage = storyEndMessage;
            cover = storyCover;
            narration = narrationClip;
            pages = storyPages ?? new List<StoryPage>();
        }
#endif
    }
}
