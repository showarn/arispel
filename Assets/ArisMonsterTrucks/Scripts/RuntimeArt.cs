using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public static class RuntimeArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private static AudioClip engineLoop;
        private static AudioClip celebrationClip;
        private static AudioClip boostClip;
        private static AudioClip coinClip;

        public static Sprite LoadSprite(
            string resourcePath,
            float pixelsPerUnit = 100f,
            Vector2? customPivot = null
        )
        {
            Vector2 pivot = customPivot ?? new Vector2(0.5f, 0.5f);
            string key = resourcePath + ":" + pixelsPerUnit + ":" + pivot;
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogError("Kunde inte hitta grafik: " + resourcePath);
                return null;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                pivot,
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );
            sprite.name = texture.name + "_RuntimeSprite";
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite CircleSprite(
            string key,
            Color edge,
            Color middle,
            Color shine,
            int size = 128
        )
        {
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.name = key;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 offset = new Vector2(x, y) - center;
                    float distance = offset.magnitude / radius;
                    if (distance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float rim = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.72f, 1f, distance));
                    Color color = Color.Lerp(middle, edge, rim);

                    Vector2 shineCenter = center + new Vector2(-size * 0.16f, size * 0.18f);
                    float shineDistance = Vector2.Distance(new Vector2(x, y), shineCenter);
                    float shineAmount = 1f - Mathf.SmoothStep(0f, size * 0.18f, shineDistance);
                    color = Color.Lerp(color, shine, shineAmount * 0.75f);
                    color.a = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.94f, 1f, distance));
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite GoldCoinSprite()
        {
            const string key = "GoldCoin3D";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            const int size = 160;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.name = key;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.47f;

            Color deepEdge = Hex("#9B4D00");
            Color orangeEdge = Hex("#E87900");
            Color brightRim = Hex("#FFCA28");
            Color face = Hex("#FFD94A");
            Color faceLight = Hex("#FFF09A");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 delta = new Vector2(x, y) - center;
                    float distance = delta.magnitude / radius;
                    if (distance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color color;
                    if (distance > 0.92f)
                    {
                        color = deepEdge;
                    }
                    else if (distance > 0.82f)
                    {
                        color = Color.Lerp(orangeEdge, brightRim, Mathf.InverseLerp(0.92f, 0.82f, distance));
                    }
                    else if (distance > 0.72f)
                    {
                        color = orangeEdge;
                    }
                    else
                    {
                        float diagonalLight = Mathf.Clamp01(
                            0.52f + (-delta.x + delta.y) / size * 0.7f
                        );
                        color = Color.Lerp(face, faceLight, diagonalLight);
                    }

                    float highlight = 1f - Mathf.SmoothStep(
                        0f,
                        size * 0.13f,
                        Vector2.Distance(new Vector2(x, y), center + new Vector2(-30f, 34f))
                    );
                    color = Color.Lerp(color, Color.white, highlight * 0.82f);

                    if (delta.x > size * 0.24f)
                    {
                        color *= Mathf.Lerp(1f, 0.76f, Mathf.InverseLerp(size * 0.24f, radius, delta.x));
                        color.a = 1f;
                    }

                    color.a = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.97f, 1f, distance));
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite PadlockSprite()
        {
            const string key = "FriendlyPadlock";
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            const int size = 160;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.name = key;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color outline = Hex("#40245F");
            Color shackleDark = Hex("#C96D12");
            Color shackleLight = Hex("#FFE68A");
            Color bodyDark = Hex("#E87816");
            Color body = Hex("#FFD84A");
            Color bodyLight = Hex("#FFF2A6");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color color = Color.clear;

                    float shackleX = (x - 80f) / 50f;
                    float shackleY = (y - 91f) / 55f;
                    float shackleOuter = Mathf.Sqrt(
                        shackleX * shackleX + shackleY * shackleY
                    );
                    float openingX = (x - 80f) / 29f;
                    float openingY = (y - 91f) / 34f;
                    float shackleInner = Mathf.Sqrt(
                        openingX * openingX + openingY * openingY
                    );
                    bool inShackle =
                        y >= 69
                        && shackleOuter <= 1f
                        && shackleInner >= 1f;
                    if (inShackle)
                    {
                        bool shackleEdge =
                            shackleOuter > 0.9f
                            || shackleInner < 1.18f;
                        color = shackleEdge
                            ? outline
                            : Color.Lerp(
                                shackleDark,
                                shackleLight,
                                Mathf.Clamp01((y - 70f) / 76f)
                            );
                    }

                    float bodyOuter = RoundedBoxDistance(
                        x - 18,
                        y - 15,
                        124,
                        88,
                        22
                    );
                    if (bodyOuter <= 0f)
                    {
                        float bodyInner = RoundedBoxDistance(
                            x - 26,
                            y - 23,
                            108,
                            72,
                            15
                        );
                        if (bodyInner > 0f)
                        {
                            color = outline;
                        }
                        else
                        {
                            float light = Mathf.Clamp01(
                                0.35f
                                + (y - 22f) / 105f
                                - (x - 35f) / 420f
                            );
                            color = Color.Lerp(bodyDark, body, light);
                            float gleam = 1f - Mathf.SmoothStep(
                                0f,
                                24f,
                                Vector2.Distance(
                                    new Vector2(x, y),
                                    new Vector2(49f, 78f)
                                )
                            );
                            color = Color.Lerp(
                                color,
                                bodyLight,
                                gleam * 0.72f
                            );
                        }
                    }

                    Vector2 keyDelta =
                        new Vector2(x, y) - new Vector2(80f, 57f);
                    bool keyholeCircle = keyDelta.sqrMagnitude <= 11f * 11f;
                    bool keyholeStem =
                        x >= 74
                        && x <= 86
                        && y >= 34
                        && y <= 58;
                    if (
                        (keyholeCircle || keyholeStem)
                        && bodyOuter <= 0f
                    )
                    {
                        color = outline;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect
            );
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite RoundedRectangleSprite(
            string key,
            Color border,
            Color fill,
            int width = 256,
            int height = 128,
            int cornerRadius = 30,
            int borderWidth = 8
        )
        {
            if (Cache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.name = key;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float outer = RoundedBoxDistance(x, y, width, height, cornerRadius);
                    float inner = RoundedBoxDistance(
                        x - borderWidth,
                        y - borderWidth,
                        width - borderWidth * 2,
                        height - borderWidth * 2,
                        Mathf.Max(1, cornerRadius - borderWidth)
                    );

                    Color color = outer > 0f
                        ? Color.clear
                        : inner > 0f ? border : fill;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(borderWidth, borderWidth, borderWidth, borderWidth)
            );
            Cache[key] = sprite;
            return sprite;
        }

        public static Material SpriteMaterial()
        {
            return new Material(Shader.Find("Sprites/Default"));
        }

        public static AudioClip EngineLoop()
        {
            if (engineLoop != null)
            {
                return engineLoop;
            }

            const int sampleRate = 44100;
            const int sampleCount = sampleRate;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float crank = Mathf.Sin(Mathf.PI * 2f * 12f * time);
                float combustion = Mathf.Pow(Mathf.Max(0f, crank), 5f);
                float rumble = Mathf.Sin(Mathf.PI * 2f * 43f * time) * 0.16f;
                float firing = combustion * (
                    Mathf.Sin(Mathf.PI * 2f * 82f * time) * 0.46f
                    + Mathf.Sin(Mathf.PI * 2f * 164f * time + 0.2f) * 0.19f
                    + Mathf.Sin(Mathf.PI * 2f * 246f * time + 0.7f) * 0.07f
                );
                float intake = Mathf.Sin(
                    Mathf.PI * 2f * 61f * time + Mathf.Sin(Mathf.PI * 2f * 12f * time) * 0.55f
                ) * 0.10f;
                float mechanical = Mathf.Sin(Mathf.PI * 2f * 328f * time) * 0.025f;
                samples[i] = Mathf.Clamp((rumble + firing + intake + mechanical) * 0.62f, -0.8f, 0.8f);
            }

            engineLoop = AudioClip.Create(
                "Mjuk monstertruckmotor",
                sampleCount,
                1,
                sampleRate,
                false
            );
            engineLoop.SetData(samples, 0);
            return engineLoop;
        }

        public static AudioClip CelebrationSound()
        {
            if (celebrationClip != null)
            {
                return celebrationClip;
            }

            const int sampleRate = 44100;
            const float duration = 2.8f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] noteStarts = { 0f, 0.28f, 0.56f, 0.92f, 1.25f };
            float[] noteFrequencies = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
            float[] burstStarts = { 0.16f, 0.64f, 1.1f, 1.55f, 2.05f };

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float value = 0f;

                for (int note = 0; note < noteStarts.Length; note++)
                {
                    float local = time - noteStarts[note];
                    if (local < 0f || local > 0.75f)
                    {
                        continue;
                    }

                    float envelope = Mathf.Exp(-local * 4.2f);
                    float frequency = noteFrequencies[note];
                    value += Mathf.Sin(Mathf.PI * 2f * frequency * local) * envelope * 0.18f;
                    value += Mathf.Sin(Mathf.PI * 4f * frequency * local) * envelope * 0.06f;
                }

                for (int burst = 0; burst < burstStarts.Length; burst++)
                {
                    float local = time - burstStarts[burst];
                    if (local < 0f || local > 0.32f)
                    {
                        continue;
                    }

                    float envelope = Mathf.Exp(-local * 13f);
                    float sparkle = Mathf.Sin((720f + burst * 91f) * local * local * 45f);
                    value += sparkle * envelope * 0.12f;
                }

                samples[i] = Mathf.Clamp(value, -0.75f, 0.75f);
            }

            celebrationClip = AudioClip.Create(
                "Stjärnfyverkeri och segerfanfar",
                sampleCount,
                1,
                sampleRate,
                false
            );
            celebrationClip.SetData(samples, 0);
            return celebrationClip;
        }

        public static AudioClip BoostSound()
        {
            if (boostClip != null)
            {
                return boostClip;
            }

            const int sampleRate = 44100;
            const float duration = 0.9f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float t = time / duration;
                float frequency = Mathf.Lerp(180f, 760f, t * t);
                float tone = Mathf.Sin(Mathf.PI * 2f * frequency * time);
                float shimmer = Mathf.Sin(Mathf.PI * 2f * (frequency * 1.5f) * time);
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
                samples[i] = (tone * 0.32f + shimmer * 0.12f) * envelope;
            }

            boostClip = AudioClip.Create("Regnbågsboost", sampleCount, 1, sampleRate, false);
            boostClip.SetData(samples, 0);
            return boostClip;
        }

        public static AudioClip CoinSound()
        {
            if (coinClip != null)
            {
                return coinClip;
            }

            const int sampleRate = 44100;
            const float duration = 0.34f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float firstEnvelope = Mathf.Exp(-time * 13f);
                float secondTime = Mathf.Max(0f, time - 0.075f);
                float secondEnvelope = time >= 0.075f ? Mathf.Exp(-secondTime * 15f) : 0f;
                float first = Mathf.Sin(Mathf.PI * 2f * 1174.7f * time) * firstEnvelope;
                float second = Mathf.Sin(Mathf.PI * 2f * 1568f * secondTime) * secondEnvelope;
                float metal = Mathf.Sin(Mathf.PI * 2f * 3136f * time) * Mathf.Exp(-time * 26f);
                samples[i] = Mathf.Clamp(first * 0.34f + second * 0.30f + metal * 0.08f, -0.75f, 0.75f);
            }

            coinClip = AudioClip.Create("Glittrande mynt", sampleCount, 1, sampleRate, false);
            coinClip.SetData(samples, 0);
            return coinClip;
        }

        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        private static float RoundedBoxDistance(
            int x,
            int y,
            int width,
            int height,
            int radius
        )
        {
            if (width <= 0 || height <= 0)
            {
                return 1f;
            }

            float clampedX = Mathf.Clamp(x, radius, width - radius - 1);
            float clampedY = Mathf.Clamp(y, radius, height - radius - 1);
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(clampedX, clampedY));
            return distance - radius;
        }
    }
}
