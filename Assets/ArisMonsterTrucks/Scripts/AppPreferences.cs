using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class AppPreferences
    {
        private const string SoundKey = "settings.v1.sound";
        private const string VibrationKey = "settings.v1.vibration";
        private const string ReducedMotionKey = "settings.v1.reducedMotion";

        public static bool SoundEnabled
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
                PlayerPrefs.Save();
                ApplyAudio();
            }
        }

        public static bool VibrationEnabled
        {
            get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool ReducedMotion
        {
            get => PlayerPrefs.GetInt(ReducedMotionKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ReducedMotionKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void ApplyAudio()
        {
            AudioListener.volume = SoundEnabled ? 1f : 0f;
        }

        public static void TryVibrate()
        {
            if (!VibrationEnabled)
            {
                return;
            }
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
