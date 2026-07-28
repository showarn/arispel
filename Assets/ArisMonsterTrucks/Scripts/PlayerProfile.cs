using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class PlayerProfile
    {
        private const string UsernameKey = "profile.v1.username";
        private const string OpenLevelSelectKey = "profile.v1.openLevelSelect";
        private const string StartRaceKey = "profile.v1.startRace";

        public static string Username =>
            PlayerPrefs.GetString(UsernameKey, "").Trim();

        public static void SaveUsername(string username)
        {
            string cleanName = (username ?? "").Trim();
            if (cleanName.Length > 18)
            {
                cleanName = cleanName.Substring(0, 18);
            }
            PlayerPrefs.SetString(UsernameKey, cleanName);
            PlayerPrefs.Save();
        }

        public static void ResetForFreshStart()
        {
            // Profilen är global i den här versionen. En full rensning tar även
            // bort äldre/utgångna sparnycklar som inte längre finns i koden.
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public static void RequestLevelSelect()
        {
            PlayerPrefs.SetInt(OpenLevelSelectKey, 1);
            PlayerPrefs.Save();
        }

        public static bool ConsumeLevelSelectRequest()
        {
            if (PlayerPrefs.GetInt(OpenLevelSelectKey, 0) != 1)
            {
                return false;
            }
            PlayerPrefs.DeleteKey(OpenLevelSelectKey);
            PlayerPrefs.Save();
            return true;
        }

        public static void RequestRaceStart()
        {
            PlayerPrefs.SetInt(StartRaceKey, 1);
            PlayerPrefs.Save();
        }

        public static bool ConsumeRaceStartRequest()
        {
            if (PlayerPrefs.GetInt(StartRaceKey, 0) != 1)
            {
                return false;
            }
            PlayerPrefs.DeleteKey(StartRaceKey);
            PlayerPrefs.Save();
            return true;
        }
    }
}
