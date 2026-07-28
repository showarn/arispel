using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class PuzzleProgress
    {
        private const string CoinBalanceKey = "puzzle.v1.coins";
        private const string CompletionCountKey = "puzzle.v1.completions";
        private const string BestTimeKey = "puzzle.v1.bestTimeSeconds";
        private const string DragTutorialSeenKey =
            "puzzle.v1.dragTutorialSeen";
        private const float FullRewardTime = 30f;
        private const float MinimumRewardTime = 240f;
        private const int MinimumReward = 25;
        private const int MaximumReward = 250;
        private const int PuzzleCount = 9;

        public static int Score =>
            Mathf.Max(0, PlayerPrefs.GetInt(CoinBalanceKey, 0));

        public static bool ShouldShowDragTutorial =>
            PlayerPrefs.GetInt(DragTutorialSeenKey, 0) != 1;

        public static void MarkDragTutorialSeen()
        {
            PlayerPrefs.SetInt(DragTutorialSeenKey, 1);
            PlayerPrefs.Save();
        }

        public static int CompletionCount(int puzzleNumber)
        {
            return Mathf.Max(
                0,
                PlayerPrefs.GetInt(CompletionKey(puzzleNumber), 0)
            );
        }

        public static float BestTime(int puzzleNumber)
        {
            return Mathf.Max(
                0f,
                PlayerPrefs.GetFloat(BestTimeForPuzzleKey(puzzleNumber), 0f)
            );
        }

        public static bool IsPuzzleUnlocked(int puzzleNumber)
        {
            return puzzleNumber <= 1
                || (
                    puzzleNumber <= PuzzleCount
                    && CompletionCount(puzzleNumber - 1) > 0
                );
        }

        public static int RecordCompletion(int puzzleNumber, float elapsedSeconds)
        {
            puzzleNumber = Mathf.Clamp(puzzleNumber, 1, PuzzleCount);
            elapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            float rewardProgress = Mathf.InverseLerp(
                MinimumRewardTime,
                FullRewardTime,
                elapsedSeconds
            );
            int reward = Mathf.RoundToInt(
                Mathf.Lerp(MinimumReward, MaximumReward, rewardProgress) / 5f
            ) * 5;
            reward = Mathf.Clamp(reward, MinimumReward, MaximumReward);

            PlayerPrefs.SetInt(CoinBalanceKey, Score + reward);
            PlayerPrefs.SetInt(
                CompletionKey(puzzleNumber),
                CompletionCount(puzzleNumber) + 1
            );

            float bestTime = BestTime(puzzleNumber);
            if (bestTime <= 0f || elapsedSeconds < bestTime)
            {
                PlayerPrefs.SetFloat(
                    BestTimeForPuzzleKey(puzzleNumber),
                    elapsedSeconds
                );
            }

            PlayerPrefs.Save();
            return reward;
        }

        public static int CalculateStars(float elapsedSeconds)
        {
            elapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            if (elapsedSeconds <= 45f)
            {
                return 4;
            }
            if (elapsedSeconds <= 90f)
            {
                return 3;
            }
            return elapsedSeconds <= 180f ? 2 : 1;
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(CoinBalanceKey);
            PlayerPrefs.DeleteKey(CompletionCountKey);
            PlayerPrefs.DeleteKey(BestTimeKey);
            PlayerPrefs.DeleteKey(DragTutorialSeenKey);
            for (int puzzleNumber = 2; puzzleNumber <= PuzzleCount; puzzleNumber++)
            {
                PlayerPrefs.DeleteKey(CompletionKey(puzzleNumber));
                PlayerPrefs.DeleteKey(BestTimeForPuzzleKey(puzzleNumber));
            }
            PlayerPrefs.Save();
        }

        private static string CompletionKey(int puzzleNumber)
        {
            return puzzleNumber <= 1
                ? CompletionCountKey
                : "puzzle.v1.puzzle." + puzzleNumber + ".completions";
        }

        private static string BestTimeForPuzzleKey(int puzzleNumber)
        {
            return puzzleNumber <= 1
                ? BestTimeKey
                : "puzzle.v1.puzzle." + puzzleNumber + ".bestTimeSeconds";
        }
    }
}
