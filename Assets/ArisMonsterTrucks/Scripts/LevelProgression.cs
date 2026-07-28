using UnityEngine;

namespace ArisMonsterTrucks
{
    public readonly struct LevelResult
    {
        public int Rating { get; }
        public int BestRating { get; }
        public bool NextLevelUnlocked { get; }

        public LevelResult(int rating, int bestRating, bool nextLevelUnlocked)
        {
            Rating = rating;
            BestRating = bestRating;
            NextLevelUnlocked = nextLevelUnlocked;
        }
    }

    public static class LevelProgression
    {
        public const int LevelCount = 12;
        private const string SelectedLevelKey = "progress.v2.selectedLevel";

        public static LevelResult RecordResult(int levelNumber, int collectedCoins, bool playerWon)
        {
            int rating = 1;
            if (playerWon)
            {
                rating = 2;
            }
            if (playerWon && collectedCoins >= ColorTrackBuilder.ThreeDotCoinRequirement)
            {
                rating = 3;
            }

            levelNumber = Mathf.Clamp(levelNumber, 1, LevelCount);
            string ratingKey = RatingKey(levelNumber);
            int bestRating = Mathf.Max(PlayerPrefs.GetInt(ratingKey, 0), rating);
            int nextLevel = levelNumber + 1;
            bool hasNextLevel = nextLevel <= LevelCount;
            bool nextLevelWasUnlocked = hasNextLevel && IsLevelUnlocked(nextLevel);
            PlayerPrefs.SetInt(ratingKey, bestRating);
            if (hasNextLevel)
            {
                PlayerPrefs.SetInt(UnlockedKey(nextLevel), 1);
            }
            PlayerPrefs.Save();

            return new LevelResult(
                rating,
                bestRating,
                hasNextLevel && !nextLevelWasUnlocked
            );
        }

        public static int GetBestLevelOneRating()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(RatingKey(1), 0), 0, 3);
        }

        public static bool IsLevelTwoUnlocked()
        {
            return IsLevelUnlocked(2);
        }

        public static bool IsLevelThreeUnlocked()
        {
            return IsLevelUnlocked(3);
        }

        public static bool IsLevelUnlocked(int levelNumber)
        {
            if (levelNumber == 1)
            {
                return true;
            }
            if (levelNumber < 1 || levelNumber > LevelCount)
            {
                return false;
            }

            // Ett tidigare klarat föregående lopp låser också upp den nya nivån
            // när en uppdatering lägger till fler banor.
            return PlayerPrefs.GetInt(UnlockedKey(levelNumber), 0) == 1
                || PlayerPrefs.GetInt(RatingKey(levelNumber - 1), 0) > 0;
        }

        public static int GetSelectedLevel()
        {
            int selected = Mathf.Clamp(
                PlayerPrefs.GetInt(SelectedLevelKey, 1),
                1,
                LevelCount
            );
            return IsLevelUnlocked(selected) ? selected : 1;
        }

        public static bool TrySelectLevel(int levelNumber)
        {
            if (!IsLevelUnlocked(levelNumber))
            {
                return false;
            }

            PlayerPrefs.SetInt(
                SelectedLevelKey,
                Mathf.Clamp(levelNumber, 1, LevelCount)
            );
            PlayerPrefs.Save();
            return true;
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(SelectedLevelKey);
            for (int level = 1; level <= LevelCount; level++)
            {
                PlayerPrefs.DeleteKey(RatingKey(level));
                PlayerPrefs.DeleteKey(UnlockedKey(level));
            }
            PlayerPrefs.Save();
        }

        private static string RatingKey(int levelNumber)
        {
            return "progress.v2.level." + levelNumber + ".bestRating";
        }

        private static string UnlockedKey(int levelNumber)
        {
            return "progress.v2.level." + levelNumber + ".unlocked";
        }
    }
}
