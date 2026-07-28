using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks.Fishing
{
    public enum FishingSound
    {
        Button,
        Cast,
        Land,
        Bubble,
        Bite,
        Early,
        SwimAway,
        Reel,
        Splash,
        Catch,
        NewFish,
        Common,
        Uncommon,
        Rare,
        Popup,
        FishBook
    }

    public static class FishingAudioLibrary
    {
        private const int SampleRate = 44100;
        private static readonly Dictionary<FishingSound, AudioClip> Clips = new();
        private static readonly Dictionary<string, AudioClip> FishVoices = new();

        public static AudioClip Get(FishingSound sound)
        {
            if (Clips.TryGetValue(sound, out AudioClip clip))
            {
                return clip;
            }

            clip = sound switch
            {
                FishingSound.Button => CreateTone("Mjuk knapp", 420f, 620f, 0.1f, 0.22f),
                FishingSound.Cast => CreateSweep("Kast", 260f, 720f, 0.45f, 0.2f),
                FishingSound.Land => CreateWaterPop("Flötet landar", 0.26f, 0.25f),
                FishingSound.Bubble => CreateWaterPop("Bubblor", 0.18f, 0.14f),
                FishingSound.Bite => CreateSequence("Napp", new[] { 620f, 820f }, 0.16f, 0.27f),
                FishingSound.Early => CreateTone("Nästan", 330f, 390f, 0.22f, 0.14f),
                FishingSound.SwimAway => CreateSweep("Simma vidare", 520f, 280f, 0.42f, 0.13f),
                FishingSound.Reel => CreateSweep("Vevning", 300f, 680f, 0.55f, 0.16f),
                FishingSound.Splash => CreateWaterPop("Vattenstänk", 0.42f, 0.28f),
                FishingSound.Catch => CreateSequence("Fångst", new[] { 520f, 690f, 880f }, 0.18f, 0.28f),
                FishingSound.NewFish => CreateSequence("Ny fisk", new[] { 660f, 880f, 1040f }, 0.2f, 0.3f),
                FishingSound.Common => CreateSequence("Vanlig fisk", new[] { 500f, 640f }, 0.15f, 0.2f),
                FishingSound.Uncommon => CreateSequence("Ovanlig fisk", new[] { 560f, 720f, 900f }, 0.16f, 0.24f),
                FishingSound.Rare => CreateSequence("Sällsynt fisk", new[] { 620f, 820f, 1040f, 1240f }, 0.17f, 0.28f),
                FishingSound.Popup => CreateTone("Popup öppnas", 460f, 760f, 0.2f, 0.19f),
                FishingSound.FishBook => CreateSequence("Fiskbok öppnas", new[] { 420f, 560f }, 0.15f, 0.2f),
                _ => CreateTone("Fiskeljud", 440f, 520f, 0.15f, 0.15f)
            };
            Clips[sound] = clip;
            return clip;
        }

        public static AudioClip GetFishVoice(string stableId)
        {
            string safeId = string.IsNullOrWhiteSpace(stableId)
                ? "fish"
                : stableId;
            if (FishVoices.TryGetValue(safeId, out AudioClip clip))
            {
                return clip;
            }

            int hash = 17;
            for (int index = 0; index < safeId.Length; index++)
            {
                hash = hash * 31 + safeId[index];
            }
            hash = Mathf.Abs(hash);
            float baseFrequency = 420f + hash % 240;
            clip = CreateSequence(
                "Fiskröst " + safeId,
                new[]
                {
                    baseFrequency,
                    baseFrequency * (1.12f + hash % 5 * 0.025f),
                    baseFrequency * 0.94f
                },
                0.1f,
                0.14f
            );
            FishVoices[safeId] = clip;
            return clip;
        }

        private static AudioClip CreateTone(
            string name,
            float fromFrequency,
            float toFrequency,
            float duration,
            float volume
        )
        {
            int count = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[count];
            for (int index = 0; index < count; index++)
            {
                float time = index / (float)SampleRate;
                float progress = index / (float)Mathf.Max(1, count - 1);
                float frequency = Mathf.Lerp(fromFrequency, toFrequency, progress);
                float envelope = Mathf.Sin(progress * Mathf.PI);
                samples[index] =
                    Mathf.Sin(time * frequency * Mathf.PI * 2f)
                    * envelope
                    * volume;
            }
            return CreateClip(name, samples);
        }

        private static AudioClip CreateSweep(
            string name,
            float fromFrequency,
            float toFrequency,
            float duration,
            float volume
        )
        {
            return CreateTone(name, fromFrequency, toFrequency, duration, volume);
        }

        private static AudioClip CreateSequence(
            string name,
            float[] frequencies,
            float noteDuration,
            float volume
        )
        {
            int noteSamples = Mathf.CeilToInt(SampleRate * noteDuration);
            float[] samples = new float[noteSamples * frequencies.Length];
            for (int note = 0; note < frequencies.Length; note++)
            {
                for (int index = 0; index < noteSamples; index++)
                {
                    float progress = index / (float)Mathf.Max(1, noteSamples - 1);
                    float envelope = Mathf.Sin(progress * Mathf.PI);
                    float time = index / (float)SampleRate;
                    samples[note * noteSamples + index] =
                        Mathf.Sin(time * frequencies[note] * Mathf.PI * 2f)
                        * envelope
                        * volume;
                }
            }
            return CreateClip(name, samples);
        }

        private static AudioClip CreateWaterPop(
            string name,
            float duration,
            float volume
        )
        {
            int count = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[count];
            uint noise = 2463534242u;
            for (int index = 0; index < count; index++)
            {
                float progress = index / (float)Mathf.Max(1, count - 1);
                noise ^= noise << 13;
                noise ^= noise >> 17;
                noise ^= noise << 5;
                float random = (noise / (float)uint.MaxValue) * 2f - 1f;
                float tone = Mathf.Sin(
                    index / (float)SampleRate
                    * Mathf.Lerp(240f, 90f, progress)
                    * Mathf.PI
                    * 2f
                );
                samples[index] =
                    (tone * 0.65f + random * 0.35f)
                    * Mathf.Pow(1f - progress, 2f)
                    * volume;
            }
            return CreateClip(name, samples);
        }

        private static AudioClip CreateClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(
                name,
                samples.Length,
                1,
                SampleRate,
                false
            );
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
