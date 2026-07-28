using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public enum ParentalGame
    {
        MonsterTrucks,
        Puzzle,
        Memory,
        Fishing,
        Stories
    }

    public static class ParentalControls
    {
        public const int MinimumPinLength = 4;
        public const int MaximumPinLength = 8;
        private const string ConfiguredKey = "parental.v1.configured";
        private const string SaltKey = "parental.v1.salt";
        private const string PasswordHashKey = "parental.v1.passwordHash";
        private const string EnabledPrefix = "parental.v1.enabled.";

        public static bool IsConfigured =>
            PlayerPrefs.GetInt(ConfiguredKey, 0) == 1
            && !string.IsNullOrEmpty(PlayerPrefs.GetString(PasswordHashKey, ""));

        public static bool IsEnabled(ParentalGame game)
        {
            return IsConfigured
                && PlayerPrefs.GetInt(
                    EnabledPrefix + game,
                    game == ParentalGame.Stories ? 1 : 0
                ) == 1;
        }

        public static bool IsValidPasswordFormat(string password)
        {
            string value = password ?? "";
            if (
                value.Length < MinimumPinLength
                || value.Length > MaximumPinLength
            )
            {
                return false;
            }
            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] < '0' || value[index] > '9')
                {
                    return false;
                }
            }
            return true;
        }

        public static void Configure(
            string password,
            bool monsterTrucks,
            bool puzzle,
            bool memory,
            bool fishing,
            bool stories = true
        )
        {
            if (!IsValidPasswordFormat(password))
            {
                throw new ArgumentException(
                    "Föräldrakoden måste bestå av 4–8 siffror.",
                    nameof(password)
                );
            }

            string salt = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(SaltKey, salt);
            PlayerPrefs.SetString(PasswordHashKey, Hash(password, salt));
            PlayerPrefs.SetInt(ConfiguredKey, 1);
            SetEnabledWithoutSave(ParentalGame.MonsterTrucks, monsterTrucks);
            SetEnabledWithoutSave(ParentalGame.Puzzle, puzzle);
            SetEnabledWithoutSave(ParentalGame.Memory, memory);
            SetEnabledWithoutSave(ParentalGame.Fishing, fishing);
            SetEnabledWithoutSave(ParentalGame.Stories, stories);
            PlayerPrefs.Save();
        }

        public static bool VerifyPassword(string password)
        {
            if (!IsConfigured || !IsValidPasswordFormat(password))
            {
                return false;
            }

            string salt = PlayerPrefs.GetString(SaltKey, "");
            string expected = PlayerPrefs.GetString(PasswordHashKey, "");
            string actual = Hash(password ?? "", salt);
            if (actual.Length != expected.Length)
            {
                return false;
            }

            int difference = 0;
            for (int index = 0; index < actual.Length; index++)
            {
                difference |= actual[index] ^ expected[index];
            }
            return difference == 0;
        }

        public static void ChangePassword(string password)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "Föräldrakontrollen måste vara konfigurerad."
                );
            }
            if (!IsValidPasswordFormat(password))
            {
                throw new ArgumentException(
                    "Föräldrakoden måste bestå av 4–8 siffror.",
                    nameof(password)
                );
            }

            string salt = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(SaltKey, salt);
            PlayerPrefs.SetString(PasswordHashKey, Hash(password, salt));
            PlayerPrefs.Save();
        }

        public static void SetEnabled(ParentalGame game, bool enabled)
        {
            if (!IsConfigured)
            {
                return;
            }
            SetEnabledWithoutSave(game, enabled);
            PlayerPrefs.Save();
        }

        private static void SetEnabledWithoutSave(
            ParentalGame game,
            bool enabled
        )
        {
            PlayerPrefs.SetInt(EnabledPrefix + game, enabled ? 1 : 0);
        }

        private static string Hash(string password, string salt)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(salt + ":" + password);
            byte[] digest = sha.ComputeHash(bytes);
            return BitConverter.ToString(digest).Replace("-", "");
        }
    }
}
