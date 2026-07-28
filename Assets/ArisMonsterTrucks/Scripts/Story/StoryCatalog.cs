using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Stories
{
    public static class StoryCatalog
    {
        private static IReadOnlyList<StoryDefinition> stories;

        public static IReadOnlyList<StoryDefinition> All
        {
            get
            {
                if (stories == null)
                {
                    StoryDefinition[] loaded =
                        Resources.LoadAll<StoryDefinition>("Stories");
                    Array.Sort(
                        loaded,
                        (left, right) =>
                        {
                            int order = left.SortOrder.CompareTo(
                                right.SortOrder
                            );
                            return order != 0
                                ? order
                                : string.CompareOrdinal(
                                    left.StoryId,
                                    right.StoryId
                                );
                        }
                    );
                    stories = loaded;
                }
                return stories;
            }
        }

        public static StoryDefinition Get(string storyId)
        {
            foreach (StoryDefinition story in All)
            {
                if (
                    story != null
                    && string.Equals(
                        story.StoryId,
                        storyId,
                        StringComparison.Ordinal
                    )
                )
                {
                    return story;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public static void ClearCache()
        {
            stories = null;
        }
#endif
    }
}
