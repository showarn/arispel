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

        public static LevelResult RecordResult(
            int levelNumber,
            float elapsedSeconds
        )
        {
            // Migrera tidigare banstjärnor innan den nya rundan sparas, så
            // den aktuella belöningen aldrig räknas dubbelt.
            _ = GlobalStarWallet.Balance;
            int rating = CalculateStars(elapsedSeconds);

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
            GlobalStarWallet.Add(rating);

            return new LevelResult(
                rating,
                bestRating,
                hasNextLevel && !nextLevelWasUnlocked
            );
        }

        public static int GetBestLevelOneRating()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(RatingKey(1), 0), 0, 4);
        }

        public static int TotalStars => GlobalStarWallet.Balance;

        public static int LegacyBestStarsTotal
        {
            get
            {
                int total = 0;
                for (int level = 1; level <= LevelCount; level++)
                {
                    total += Mathf.Clamp(
                        PlayerPrefs.GetInt(RatingKey(level), 0),
                        0,
                        4
                    );
                }
                return total;
            }
        }

        public static int CalculateStars(float elapsedSeconds)
        {
            elapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            if (elapsedSeconds <= 55f)
            {
                return 4;
            }
            if (elapsedSeconds <= 80f)
            {
                return 3;
            }
            return elapsedSeconds <= 120f ? 2 : 1;
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
            GlobalStarWallet.Reset();
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
