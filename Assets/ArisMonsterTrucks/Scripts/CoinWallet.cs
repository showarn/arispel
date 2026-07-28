using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class CoinWallet
    {
        private const string BalanceKey = "economy.v1.coins";

        public static int Balance => Mathf.Max(0, PlayerPrefs.GetInt(BalanceKey, 0));

        public static void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(BalanceKey, Balance + amount);
            PlayerPrefs.Save();
        }

        public static bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            int balance = Balance;
            if (balance < amount)
            {
                return false;
            }

            PlayerPrefs.SetInt(BalanceKey, balance - amount);
            PlayerPrefs.Save();
            return true;
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(BalanceKey);
            PlayerPrefs.Save();
        }
    }
}
