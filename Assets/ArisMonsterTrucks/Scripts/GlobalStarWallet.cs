using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class GlobalStarWallet
    {
        private const string BalanceKey = "rewards.v1.globalStars";
        private const string MigrationKey = "rewards.v1.globalStarsMigrated";

        public static int Balance
        {
            get
            {
                EnsureMigrated();
                return Mathf.Max(0, PlayerPrefs.GetInt(BalanceKey, 0));
            }
        }

        public static void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureMigrated();
            PlayerPrefs.SetInt(BalanceKey, Balance + amount);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(BalanceKey);
            PlayerPrefs.DeleteKey(MigrationKey);
            PlayerPrefs.Save();
        }

        private static void EnsureMigrated()
        {
            if (PlayerPrefs.GetInt(MigrationKey, 0) == 1)
            {
                return;
            }

            PlayerPrefs.SetInt(
                BalanceKey,
                Mathf.Max(
                    PlayerPrefs.GetInt(BalanceKey, 0),
                    LevelProgression.LegacyBestStarsTotal
                )
            );
            PlayerPrefs.SetInt(MigrationKey, 1);
            PlayerPrefs.Save();
        }
    }
}
