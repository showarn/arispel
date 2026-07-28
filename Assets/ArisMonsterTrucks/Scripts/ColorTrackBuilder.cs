using System.Collections.Generic;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public sealed class ColorTrackBuilder : MonoBehaviour
    {
        public const float FinishX = 235f;
        public const int TotalCoins = 62;
        public const int ThreeDotCoinRequirement = 56;
        public const float LoopCenterX = 76f;
        public const float LoopCenterY = 8f;
        public const float LoopRadius = 8f;

        private readonly List<Vector2> preLoopPath = new();
        private readonly List<Vector2> postLoopPath = new();
        private readonly List<Vector2> loopPath = new();
        private RaceDirector director;
        private EdgeCollider2D preLoopCollider;
        private EdgeCollider2D loopCollider;
        private EdgeCollider2D postLoopCollider;
        private int levelNumber = 1;

        public bool HasLoop => levelNumber == 1;

        public void Build(RaceDirector raceDirector, int selectedLevel = 1)
        {
            director = raceDirector;
            levelNumber = Mathf.Clamp(selectedLevel, 1, LevelProgression.LevelCount);
            if (levelNumber == 1)
            {
                BuildMainPath();
                BuildLoopPath();
                CreateTrackRibbon("Bana före loopen", preLoopPath, false);
                CreateTrackRibbon("Bana efter loopen", postLoopPath, false);
                CreateTrackRibbon("Regnbågsloop", loopPath, true);
                preLoopCollider = CreatePhysicalCollider("Fysik före loopen", preLoopPath);
                loopCollider = CreatePhysicalCollider("Fysik i loopen", loopPath);
                postLoopCollider = CreatePhysicalCollider("Fysik efter loopen", postLoopPath);
            }
            else if (levelNumber == 2)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.28f, DirtTrackHeight);
                CreateTrackRibbon("Dirtbanans platåer och hopp", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på dirtbanan", preLoopPath);
                CreateDirtScenery();
            }
            else if (levelNumber == 3)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.22f, MountainTrackHeight);
                CreateTrackRibbon("Steniga bergsklättringen", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på bergsbanan", preLoopPath);
                CreateMountainScenery();
            }
            else if (levelNumber == 4)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, IceTrackHeight);
                CreateTrackRibbon("Glittrande isbanan", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på isbanan", preLoopPath);
                CreateIceScenery();
            }
            else if (levelNumber == 5)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, LavaTrackHeight);
                CreateTrackRibbon("Lavabanans basaltväg", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på lavabanan", preLoopPath);
                CreateLavaScenery();
            }
            else if (levelNumber == 6)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, HauntedTrackHeight);
                CreateTrackRibbon("Spökbanans månstig", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på spökbanan", preLoopPath);
                CreateHauntedScenery();
            }
            else if (levelNumber == 7)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, JungleTrackHeight);
                CreateTrackRibbon("Djungelbanans lianstig", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på djungelbanan", preLoopPath);
                CreateJungleScenery();
            }
            else if (levelNumber == 8)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, AfricaTrackHeight);
                CreateTrackRibbon("Afrikabanans savannstig", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på Afrikabanan", preLoopPath);
                CreateAfricaScenery();
            }
            else if (levelNumber == 9)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.24f, DesertTrackHeight);
                CreateTrackRibbon("Ökenbanans sanddyner", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på ökenbanan", preLoopPath);
                CreateDesertScenery();
            }
            else if (levelNumber == 10)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.22f, WaterTrackHeight);
                CreateTrackRibbon("Vattenbanans rutschkanor", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på vattenbanan", preLoopPath);
                CreateWaterScenery();
            }
            else if (levelNumber == 11)
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.22f, SpaceTrackHeight);
                CreateTrackRibbon("Rymdbanans kraterstig", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på rymdbanan", preLoopPath);
                CreateSpaceScenery();
            }
            else
            {
                AddSampledSection(preLoopPath, -18f, 245f, 0.22f, CandyTrackHeight);
                CreateTrackRibbon("Godisbanans kakväg", preLoopPath, false);
                preLoopCollider = CreatePhysicalCollider("Fysik på godisbanan", preLoopPath);
                CreateCandyScenery();
            }
            CreateCoins();
            CreateBoosters();
            CreateFinishLine();
            CreateDecorations();
        }

        public float HeightAt(float x)
        {
            if (levelNumber == 2)
            {
                return DirtTrackHeight(x);
            }
            if (levelNumber == 3)
            {
                return MountainTrackHeight(x);
            }
            if (levelNumber == 4)
            {
                return IceTrackHeight(x);
            }
            if (levelNumber == 5)
            {
                return LavaTrackHeight(x);
            }
            if (levelNumber == 6)
            {
                return HauntedTrackHeight(x);
            }
            if (levelNumber == 7)
            {
                return JungleTrackHeight(x);
            }
            if (levelNumber == 8)
            {
                return AfricaTrackHeight(x);
            }
            if (levelNumber == 9)
            {
                return DesertTrackHeight(x);
            }
            if (levelNumber == 10)
            {
                return WaterTrackHeight(x);
            }
            if (levelNumber == 11)
            {
                return SpaceTrackHeight(x);
            }
            if (levelNumber == 12)
            {
                return CandyTrackHeight(x);
            }
            if (x >= LoopCenterX - LoopRadius && x <= LoopCenterX + LoopRadius)
            {
                return 0f;
            }

            return TrackHeight(x);
        }

        public void PrepareTruckForLoop(MonsterTruckVehicle truck)
        {
            if (!HasLoop)
            {
                return;
            }
            SetTruckCollision(truck, preLoopCollider, false);
            SetTruckCollision(truck, loopCollider, true);
            SetTruckCollision(truck, postLoopCollider, false);
        }

        public void EnterLoop(MonsterTruckVehicle truck)
        {
            if (!HasLoop)
            {
                return;
            }
            // Under den snabba arkadloopen styrs hela fordonsriggen runt banan.
            // Ignorera markcolliders tills bilen släpps ut på den raka vägen.
            SetTruckCollision(truck, preLoopCollider, true);
            SetTruckCollision(truck, loopCollider, true);
            SetTruckCollision(truck, postLoopCollider, true);
        }

        public void ExitLoop(MonsterTruckVehicle truck)
        {
            if (!HasLoop)
            {
                return;
            }
            SetTruckCollision(truck, preLoopCollider, false);
            SetTruckCollision(truck, loopCollider, true);
            SetTruckCollision(truck, postLoopCollider, false);
        }

        private void BuildMainPath()
        {
            AddSampledSection(preLoopPath, -18f, 72f, 0.35f, TrackHeight);
            // Överlappa loopens utgång, men börja efter dess botten så den raka
            // banan inte kan fånga hjulen mitt inne i loopen.
            AddSampledSection(postLoopPath, 79.5f, 245f, 0.35f, TrackHeight);
        }

        private void BuildLoopPath()
        {
            loopPath.Add(new Vector2(72f, 0f));

            const int steps = 112;
            for (int i = 0; i <= steps; i++)
            {
                float angle = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 1.5f, i / (float)steps);
                loopPath.Add(new Vector2(
                    LoopCenterX + Mathf.Cos(angle) * LoopRadius,
                    LoopCenterY + Mathf.Sin(angle) * LoopRadius
                ));
            }

            loopPath.Add(new Vector2(80f, 0f));
        }

        private void CreateTrackRibbon(string objectName, List<Vector2> points, bool rainbow)
        {
            GameObject root = new(objectName);
            root.transform.SetParent(transform, false);

            Color shadow = RuntimeArt.Hex(levelNumber switch
            {
                2 => "#382315",
                3 => "#26323B",
                4 => "#315675",
                5 => "#190F1E",
                6 => "#17142D",
                7 => "#173A25",
                8 => "#4A2D18",
                9 => "#5A321C",
                10 => "#164B65",
                11 => "#25164F",
                12 => "#6E3049",
                _ => "#2E2148"
            });
            Color outer = RuntimeArt.Hex(levelNumber switch
            {
                2 => "#6F3D20",
                3 => "#4A5660",
                4 => "#5BA7D4",
                5 => "#392B42",
                6 => "#44356E",
                7 => "#28663A",
                8 => "#8A5427",
                9 => "#A55229",
                10 => "#248FC0",
                11 => "#6743B8",
                12 => "#D95A91",
                _ => "#59337B"
            });
            Color surface = rainbow
                ? RuntimeArt.Hex("#FF7A59")
                : RuntimeArt.Hex(levelNumber switch
                {
                    2 => "#A85A2B",
                    3 => "#78848B",
                    4 => "#A9E9FF",
                    5 => "#5B414B",
                    6 => "#705C91",
                    7 => "#6F9B47",
                    8 => "#C58A43",
                    9 => "#E09A50",
                    10 => "#55CBE8",
                    11 => "#9A76F0",
                    12 => "#F2A4C6",
                    _ => "#C96D3A"
                });
            Color highlight = RuntimeArt.Hex(levelNumber switch
            {
                2 => "#D3833F",
                3 => "#C4B89C",
                4 => "#E8FCFF",
                5 => "#FF6A24",
                6 => "#B79BDE",
                7 => "#B7D96A",
                8 => "#F3C66B",
                9 => "#FFD07A",
                10 => "#B8F5FF",
                11 => "#D7C5FF",
                12 => "#FFF0A8",
                _ => "#E58A48"
            });
            Color edge = RuntimeArt.Hex(levelNumber switch
            {
                2 => "#3FAD45",
                3 => "#477A46",
                4 => "#EAFBFF",
                5 => "#B52D20",
                6 => "#45A59E",
                7 => "#33A956",
                8 => "#E1A842",
                9 => "#FFBE55",
                10 => "#E6FCFF",
                11 => "#55DFF5",
                12 => "#FFFFFF",
                _ => "#55D94A"
            });
            Color edgeLight = RuntimeArt.Hex(levelNumber switch
            {
                2 => "#8DE05B",
                3 => "#A8C76A",
                4 => "#FFFFFF",
                5 => "#FFB13B",
                6 => "#83F0DD",
                7 => "#9AF07B",
                8 => "#FFE69A",
                9 => "#FFF0A3",
                10 => "#FFFFFF",
                11 => "#DDFBFF",
                12 => "#FFE1F0",
                _ => "#B8F56A"
            });
            float outerWidth = levelNumber == 3 ? 2.08f : levelNumber >= 4 ? 1.95f : levelNumber == 2 ? 1.9f : 1.72f;
            float surfaceWidth = levelNumber == 3 ? 1.7f : levelNumber >= 4 ? 1.58f : levelNumber == 2 ? 1.52f : 1.34f;

            CreateLine(
                root.transform,
                "Mjuk skugga",
                points,
                outerWidth + 0.42f,
                shadow,
                -7
            );
            CreateLine(
                root.transform,
                "Banan ytterkant",
                points,
                outerWidth,
                outer,
                -6
            );
            CreateLine(
                root.transform,
                rainbow ? "Regnbågsfyllning" : "Banyta",
                points,
                surfaceWidth,
                surface,
                -5
            );
            if (!rainbow)
            {
                CreateLine(
                    root.transform,
                    "Ljus banrand",
                    points,
                    surfaceWidth * 0.66f,
                    highlight,
                    -4
                );
            }
            CreateLine(
                root.transform,
                "Banans överkant",
                points,
                levelNumber >= 3 ? 0.34f : levelNumber == 2 ? 0.42f : 0.52f,
                edge,
                -3
            );
            CreateLine(
                root.transform,
                "Glittrande kant",
                points,
                0.12f,
                edgeLight,
                -2
            );

            if (rainbow)
            {
                CreateLine(root.transform, "Loopgul", points, 0.25f, RuntimeArt.Hex("#FFE14C"), -2);
                CreateLine(root.transform, "Loopglans", points, 0.08f, Color.white, -1);
            }
            else if (levelNumber <= 3)
            {
                CreateTrackDetails(root.transform, points);
            }
        }

        private static void CreateTrackDetails(Transform parent, List<Vector2> points)
        {
            Color[] pebbleColors =
            {
                RuntimeArt.Hex("#8F4D35"),
                RuntimeArt.Hex("#FFD05A"),
                RuntimeArt.Hex("#A75A3C")
            };
            for (int i = 10; i < points.Count - 4; i += 15)
            {
                Vector2 point = points[i];
                GameObject pebble = new("Färgad sten i banan");
                pebble.transform.SetParent(parent, false);
                pebble.transform.localPosition = new Vector3(
                    point.x,
                    point.y - 0.48f,
                    0f
                );
                SpriteRenderer renderer = pebble.AddComponent<SpriteRenderer>();
                Color color = pebbleColors[(i / 15) % pebbleColors.Length];
                renderer.sprite = RuntimeArt.CircleSprite(
                    "TrackPebble_" + ColorUtility.ToHtmlStringRGB(color),
                    color * 0.72f,
                    color,
                    RuntimeArt.Hex("#FFF0A0"),
                    64
                );
                renderer.sortingOrder = -2;
                pebble.transform.localScale = new Vector3(0.24f, 0.15f, 1f);
            }

            for (int i = 18; i < points.Count - 4; i += 28)
            {
                Vector2 point = points[i];
                GameObject flower = new("Liten banblomma");
                flower.transform.SetParent(parent, false);
                flower.transform.localPosition = new Vector3(
                    point.x,
                    point.y + 0.34f,
                    0f
                );
                SpriteRenderer renderer = flower.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.CircleSprite(
                    "TrackFlower",
                    RuntimeArt.Hex("#D43D78"),
                    RuntimeArt.Hex("#FF72AD"),
                    Color.white,
                    64
                );
                renderer.sortingOrder = -1;
                flower.transform.localScale = new Vector3(0.16f, 0.22f, 1f);
            }
        }

        private EdgeCollider2D CreatePhysicalCollider(string objectName, List<Vector2> path)
        {
            GameObject collisionObject = new(objectName);
            collisionObject.transform.SetParent(transform, false);
            EdgeCollider2D collider = collisionObject.AddComponent<EdgeCollider2D>();
            collider.points = path.ToArray();
            collider.edgeRadius = 0.12f;
            collider.sharedMaterial = new PhysicsMaterial2D("BanaGrepp")
            {
                friction = levelNumber == 4 || levelNumber == 10 || levelNumber == 11
                    ? 0.86f
                    : 1.18f,
                bounciness = 0.015f
            };
            return collider;
        }

        private static void SetTruckCollision(
            MonsterTruckVehicle truck,
            Collider2D trackCollider,
            bool ignore
        )
        {
            if (truck == null || trackCollider == null)
            {
                return;
            }

            Collider2D[] truckColliders = truck.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D truckCollider in truckColliders)
            {
                Physics2D.IgnoreCollision(truckCollider, trackCollider, ignore);
            }
        }

        private static void CreateLine(
            Transform parent,
            string lineName,
            List<Vector2> points,
            float width,
            Color color,
            int sortingOrder
        )
        {
            GameObject lineObject = new(lineName);
            lineObject.transform.SetParent(parent, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = points.Count;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 6;
            line.numCornerVertices = 4;
            line.material = RuntimeArt.SpriteMaterial();
            line.startColor = color;
            line.endColor = color;
            line.sortingOrder = sortingOrder;

            Vector3[] positions = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                positions[i] = points[i];
            }
            line.SetPositions(positions);
        }

        private void CreateCoins()
        {
            float[] groundCoins = levelNumber >= 2
                ? new[]
                {
                    4f, 8f, 12f, 16f, 20f, 24f, 28f, 32f, 36f, 40f,
                    44f, 48f, 52f, 56f, 60f, 64f, 68f, 72f, 76f, 80f,
                    84f, 88f, 92f, 96f, 100f, 104f, 108f, 112f, 116f, 120f,
                    124f, 128f, 132f, 136f, 140f, 144f, 148f, 152f, 156f, 160f,
                    164f, 168f, 172f, 176f, 180f, 184f, 188f, 192f, 196f, 200f,
                    204f, 208f, 212f, 216f, 220f, 224f, 228f, 232f
                }
                : new[]
            {
                5f, 8f, 11f, 18f, 21f, 24f, 31f, 34f, 38f,
                44f, 48f, 52f, 57f, 61f,
                82f, 85f, 88f, 93f, 97f, 101f, 106f, 110f,
                114f, 119f, 123f, 128f, 132f, 136f, 141f,
                145f, 149f, 154f, 158f, 162f, 166f,
                171f, 175f, 180f, 184f, 188f, 193f, 197f,
                201f, 206f, 210f, 214f, 219f, 223f, 227f, 231f
            };

            foreach (float x in groundCoins)
            {
                float wave = Mathf.Sin(x * 1.37f) * 0.35f;
                CreateCoin(new Vector2(x, HeightAt(x) + 2.45f + wave));
            }

            if (!HasLoop)
            {
                return;
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = Mathf.Lerp(-Mathf.PI * 0.42f, Mathf.PI * 1.42f, i / 11f);
                float radius = LoopRadius - 2.5f;
                CreateCoin(new Vector2(
                    LoopCenterX + Mathf.Cos(angle) * radius,
                    LoopCenterY + Mathf.Sin(angle) * radius
                ));
            }
        }

        private void CreateCoin(Vector2 position)
        {
            GameObject coin = new("Glittrande mynt");
            coin.transform.SetParent(transform, false);
            coin.transform.position = position;

            SpriteRenderer renderer = coin.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.GoldCoinSprite();
            renderer.sortingOrder = 5;
            coin.transform.localScale = Vector3.one * 0.88f;

            CircleCollider2D trigger = coin.AddComponent<CircleCollider2D>();
            trigger.radius = 0.52f;
            trigger.isTrigger = true;

            CoinPickup pickup = coin.AddComponent<CoinPickup>();
            pickup.Initialize(director, renderer);

            GameObject inner = new("Stjärna");
            inner.transform.SetParent(coin.transform, false);
            TextMesh star = inner.AddComponent<TextMesh>();
            star.text = "★";
            star.anchor = TextAnchor.MiddleCenter;
            star.alignment = TextAlignment.Center;
            star.fontSize = 60;
            star.characterSize = 0.08f;
            star.color = RuntimeArt.Hex("#FFF7A8");
            star.fontStyle = FontStyle.Bold;
            star.GetComponent<MeshRenderer>().sortingOrder = 6;
        }

        private void CreateBoosters()
        {
            // 65 är den kraftiga loopboosten. Den sena boostern vid 190 är
            // avsiktligt borttagen så slutsträckan körs med bilens egen motor.
            float[] boosterXs = levelNumber switch
            {
                2 => new[] { 52f, 139f },
                3 => new[] { 58f, 139f, 205f },
                4 => new[] { 62f, 146f, 210f },
                5 => new[] { 54f, 126f, 198f },
                6 => new[] { 66f, 151f, 218f },
                7 => new[] { 48f, 124f, 202f },
                8 => new[] { 57f, 137f, 211f },
                9 => new[] { 52f, 128f, 204f },
                10 => new[] { 44f, 116f, 188f, 224f },
                _ => new[] { 65f, 103f }
            };
            foreach (float x in boosterXs)
            {
                GameObject booster = new("Turbo-booster");
                booster.transform.SetParent(transform, false);
                booster.transform.position = new Vector3(x, HeightAt(x) + 2.45f, 0f);

                SpriteRenderer outer = booster.AddComponent<SpriteRenderer>();
                outer.sprite = RuntimeArt.LoadSprite("Art/UI/turbo_booster", 430f);
                outer.sortingOrder = 12;
                booster.transform.localScale = Vector3.one * 1.08f;

                CircleCollider2D trigger = booster.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.56f;
                booster.AddComponent<AirBooster>().Initialize(
                    levelNumber == 1 && Mathf.Abs(x - 65f) < 0.1f
                );
            }
        }

        private void CreateFinishLine()
        {
            GameObject finish = new("MÅL");
            finish.transform.SetParent(transform, false);
            finish.transform.position = new Vector3(FinishX, HeightAt(FinishX), 0f);

            CreateBlock(finish.transform, "Vänster stolpe", new Vector2(-2.1f, 3.1f), new Vector2(0.35f, 6.2f), RuntimeArt.Hex("#623C8D"), 2);
            CreateBlock(finish.transform, "Höger stolpe", new Vector2(2.1f, 3.1f), new Vector2(0.35f, 6.2f), RuntimeArt.Hex("#623C8D"), 2);
            CreateBlock(finish.transform, "Målband", new Vector2(0f, 6f), new Vector2(4.6f, 0.9f), RuntimeArt.Hex("#FF4F87"), 3);

            for (int i = 0; i < 8; i++)
            {
                Color color = (i % 2 == 0) ? Color.white : RuntimeArt.Hex("#3B226E");
                CreateBlock(
                    finish.transform,
                    "Målruta",
                    new Vector2(-1.75f + i * 0.5f, 6f),
                    new Vector2(0.5f, 0.45f),
                    color,
                    4
                );
            }

            BoxCollider2D trigger = finish.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.7f, 9f);
            trigger.offset = new Vector2(0f, 3.5f);
            finish.AddComponent<FinishTrigger>().Initialize(director);
        }

        private void CreateDecorations()
        {
            if (levelNumber >= 4)
            {
                Color themeColor = levelNumber switch
                {
                    4 => RuntimeArt.Hex("#7DE5FF"),
                    5 => RuntimeArt.Hex("#FF702A"),
                    6 => RuntimeArt.Hex("#9B78E8"),
                    7 => RuntimeArt.Hex("#62D66F"),
                    8 => RuntimeArt.Hex("#F0B94E"),
                    9 => RuntimeArt.Hex("#FF9A42"),
                    11 => RuntimeArt.Hex("#B889FF"),
                    12 => RuntimeArt.Hex("#FF75B5"),
                    _ => RuntimeArt.Hex("#54DFFF")
                };
                float[] themeFlags = { 28f, 78f, 128f, 178f, 226f };
                foreach (float x in themeFlags)
                {
                    CreateFlag(x, themeColor);
                }
                return;
            }

            if (levelNumber == 3)
            {
                float[] summitFlags = { 48f, 119f, 176f, 222f, 235f };
                Color[] summitColors =
                {
                    RuntimeArt.Hex("#FFB82E"),
                    RuntimeArt.Hex("#8D67E8"),
                    RuntimeArt.Hex("#4FC7E8")
                };
                for (int i = 0; i < summitFlags.Length; i++)
                {
                    CreateFlag(summitFlags[i], summitColors[i % summitColors.Length]);
                }
                return;
            }

            float[] balloonXs = { -4f, 14f, 29f, 68f, 87f, 106f };
            balloonXs = new[] { -4f, 14f, 29f, 48f, 84f, 105f, 128f, 151f, 174f, 198f, 220f, 232f };
            Color[] colors =
            {
                RuntimeArt.Hex("#FF5E8E"),
                RuntimeArt.Hex("#7B61FF"),
                RuntimeArt.Hex("#FFB82E"),
                RuntimeArt.Hex("#36D6D0")
            };

            for (int i = 0; i < balloonXs.Length; i++)
            {
                float x = balloonXs[i];
                CreateBalloon(new Vector2(x, HeightAt(x) + 6.5f + (i % 2) * 1.2f), colors[i % colors.Length]);
            }

            float[] flagXs = { 2f, 27f, 55f, 84f, 112f, 139f, 160f, 185f, 211f, 228f };
            for (int i = 0; i < flagXs.Length; i++)
            {
                CreateFlag(flagXs[i], colors[(i + 1) % colors.Length]);
            }

            float[] obstacleXs = levelNumber == 2
                ? new[] { 31f, 82f, 117f, 166f, 214f }
                : new[] { 23f, 53f, 99f, 127f, 151f, 179f, 205f, 225f };
            for (int i = 0; i < obstacleXs.Length; i++)
            {
                CreateSmashObstacle(obstacleXs[i], colors[i % colors.Length]);
            }
        }

        private void CreateDirtScenery()
        {
            float[] rockXs = { 18f, 43f, 72f, 101f, 132f, 159f, 191f, 222f };
            for (int i = 0; i < rockXs.Length; i++)
            {
                float x = rockXs[i];
                GameObject rock = new("Dirtsten");
                rock.transform.SetParent(transform, false);
                rock.transform.position = new Vector3(x, HeightAt(x) - 0.2f, 0f);
                SpriteRenderer renderer = rock.AddComponent<SpriteRenderer>();
                Color color = i % 2 == 0
                    ? RuntimeArt.Hex("#79513A")
                    : RuntimeArt.Hex("#9A6845");
                renderer.sprite = RuntimeArt.CircleSprite(
                    "DirtRock_" + i % 2,
                    color * 0.62f,
                    color,
                    RuntimeArt.Hex("#D7A56E"),
                    96
                );
                renderer.sortingOrder = -1;
                rock.transform.localScale = new Vector3(1.2f, 0.7f, 1f);
            }

            float[] signXs = { 12f, 58f, 108f, 154f, 202f };
            foreach (float x in signXs)
            {
                GameObject sign = new("Dirtbaneskylt");
                sign.transform.SetParent(transform, false);
                sign.transform.position = new Vector3(x, HeightAt(x), 0f);
                CreateBlock(sign.transform, "Trästolpe", new Vector2(0f, 1.1f), new Vector2(0.16f, 2.2f), RuntimeArt.Hex("#70401F"), -1);
                CreateBlock(sign.transform, "Pilskylt", new Vector2(0.65f, 2f), new Vector2(1.55f, 0.72f), RuntimeArt.Hex("#FFB62E"), 0);
            }
        }

        private void CreateMountainScenery()
        {
            float[] rockXs =
            {
                7f, 18f, 31f, 45f, 57f, 75f, 93f, 108f,
                121f, 137f, 154f, 172f, 188f, 207f, 223f, 239f
            };
            for (int i = 0; i < rockXs.Length; i++)
            {
                float x = rockXs[i];
                GameObject rock = new("Bergssten");
                rock.transform.SetParent(transform, false);
                rock.transform.position = new Vector3(
                    x,
                    HeightAt(x) - 0.25f,
                    0f
                );
                SpriteRenderer renderer = rock.AddComponent<SpriteRenderer>();
                Color color = (i % 3) switch
                {
                    0 => RuntimeArt.Hex("#596770"),
                    1 => RuntimeArt.Hex("#75838B"),
                    _ => RuntimeArt.Hex("#8D8A7C")
                };
                renderer.sprite = RuntimeArt.CircleSprite(
                    "MountainRock_" + i % 3,
                    color * 0.65f,
                    color,
                    RuntimeArt.Hex("#D8D1BC"),
                    96
                );
                renderer.sortingOrder = -1;
                float size = 0.85f + (i % 4) * 0.18f;
                rock.transform.localScale = new Vector3(size * 1.35f, size, 1f);
            }

            float[] pineXs = { 13f, 52f, 83f, 128f, 161f, 196f, 229f };
            foreach (float x in pineXs)
            {
                CreatePineTree(x);
            }
        }

        private void CreatePineTree(float x)
        {
            GameObject pine = new("Bergstall");
            pine.transform.SetParent(transform, false);
            pine.transform.position = new Vector3(x, HeightAt(x), 0f);
            CreateBlock(
                pine.transform,
                "Stam",
                new Vector2(0f, 1.05f),
                new Vector2(0.22f, 2.1f),
                RuntimeArt.Hex("#60462F"),
                -2
            );
            Color needles = RuntimeArt.Hex("#28624A");
            for (int i = 0; i < 3; i++)
            {
                GameObject crown = new("Barr");
                crown.transform.SetParent(pine.transform, false);
                crown.transform.localPosition = new Vector3(0f, 1.4f + i * 0.72f, 0f);
                SpriteRenderer renderer = crown.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.CircleSprite(
                    "PineNeedles_" + i,
                    needles * 0.65f,
                    needles,
                    RuntimeArt.Hex("#75B86B"),
                    96
                );
                renderer.sortingOrder = -1;
                float scale = 1.75f - i * 0.35f;
                crown.transform.localScale = new Vector3(scale, 0.8f, 1f);
            }
        }

        private void CreateIceScenery()
        {
            float[] crystalXs = { 14f, 37f, 68f, 96f, 123f, 157f, 187f, 216f };
            foreach (float x in crystalXs)
            {
                GameObject crystal = new("Iskristall");
                crystal.transform.SetParent(transform, false);
                crystal.transform.position = new Vector3(x, HeightAt(x) + 0.7f, 0f);
                CreateBlock(
                    crystal.transform,
                    "Kristall",
                    Vector2.zero,
                    new Vector2(0.42f, 1.45f),
                    RuntimeArt.Hex("#8DEBFF"),
                    0
                );
                crystal.transform.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    (Mathf.RoundToInt(x) % 2 == 0) ? -18f : 18f
                );
            }

            float[] snowballXs = { 24f, 55f, 109f, 143f, 176f, 205f, 232f };
            for (int i = 0; i < snowballXs.Length; i++)
            {
                CreateThemedOrb(
                    "Snöboll",
                    snowballXs[i],
                    RuntimeArt.Hex("#B9E9F7"),
                    Color.white,
                    0.7f + (i % 3) * 0.16f
                );
            }
        }

        private void CreateLavaScenery()
        {
            float[] lavaRockXs = { 16f, 41f, 73f, 104f, 136f, 164f, 194f, 223f };
            for (int i = 0; i < lavaRockXs.Length; i++)
            {
                CreateThemedOrb(
                    "Glödande lavasten",
                    lavaRockXs[i],
                    RuntimeArt.Hex("#3A243D"),
                    RuntimeArt.Hex(i % 2 == 0 ? "#FF6A21" : "#FFB22E"),
                    0.8f + (i % 3) * 0.2f
                );
            }

            float[] flameXs = { 31f, 88f, 149f, 211f };
            foreach (float x in flameXs)
            {
                GameObject flame = new("Lavaplym");
                flame.transform.SetParent(transform, false);
                flame.transform.position = new Vector3(x, HeightAt(x) + 0.9f, 0f);
                SpriteRenderer renderer = flame.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.CircleSprite(
                    "LavaFlame",
                    RuntimeArt.Hex("#C72C20"),
                    RuntimeArt.Hex("#FF6B21"),
                    RuntimeArt.Hex("#FFE06A"),
                    96
                );
                renderer.sortingOrder = 1;
                flame.transform.localScale = new Vector3(0.55f, 1.25f, 1f);
            }
        }

        private void CreateHauntedScenery()
        {
            float[] ghostXs = { 18f, 54f, 91f, 132f, 171f, 208f, 230f };
            for (int i = 0; i < ghostXs.Length; i++)
            {
                CreateGhost(ghostXs[i], 3.8f + (i % 3) * 0.8f);
            }

            float[] boneXs = { 35f, 76f, 116f, 156f, 196f, 222f };
            foreach (float x in boneXs)
            {
                GameObject bones = new("Korsade skelettben");
                bones.transform.SetParent(transform, false);
                bones.transform.position = new Vector3(x, HeightAt(x) + 0.45f, 0f);
                CreateBlock(
                    bones.transform,
                    "Ben ett",
                    Vector2.zero,
                    new Vector2(1.35f, 0.18f),
                    RuntimeArt.Hex("#E8E1C7"),
                    1
                );
                CreateBlock(
                    bones.transform,
                    "Ben två",
                    Vector2.zero,
                    new Vector2(1.35f, 0.18f),
                    RuntimeArt.Hex("#E8E1C7"),
                    1
                );
                bones.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 28f);
                bones.transform.GetChild(1).localRotation = Quaternion.Euler(0f, 0f, -28f);
            }
        }

        private void CreateJungleScenery()
        {
            float[] treeXs = { 12f, 46f, 83f, 119f, 158f, 198f, 228f };
            foreach (float x in treeXs)
            {
                GameObject tree = new("Djungelträd");
                tree.transform.SetParent(transform, false);
                tree.transform.position = new Vector3(x, HeightAt(x), 0f);
                CreateBlock(tree.transform, "Stam", new Vector2(0f, 1.5f), new Vector2(0.42f, 3f), RuntimeArt.Hex("#6E4427"), -2);
                CreateThemedCrown(tree.transform, new Vector2(0f, 3.1f), RuntimeArt.Hex("#268B43"), RuntimeArt.Hex("#78DD5D"), 2.15f);
            }

            float[] flowerXs = { 25f, 66f, 103f, 143f, 181f, 216f };
            for (int i = 0; i < flowerXs.Length; i++)
            {
                CreateThemedOrb(
                    "Tropisk blomma",
                    flowerXs[i],
                    RuntimeArt.Hex(i % 2 == 0 ? "#E6377D" : "#6F42C1"),
                    RuntimeArt.Hex("#FFE56B"),
                    0.55f
                );
            }
        }

        private void CreateAfricaScenery()
        {
            float[] acaciaXs = { 18f, 58f, 104f, 151f, 195f, 229f };
            foreach (float x in acaciaXs)
            {
                GameObject tree = new("Akaciaträd");
                tree.transform.SetParent(transform, false);
                tree.transform.position = new Vector3(x, HeightAt(x), 0f);
                CreateBlock(tree.transform, "Stam", new Vector2(0f, 1.35f), new Vector2(0.28f, 2.7f), RuntimeArt.Hex("#76502C"), -2);
                CreateThemedCrown(tree.transform, new Vector2(0f, 2.9f), RuntimeArt.Hex("#50752E"), RuntimeArt.Hex("#A4B94B"), 2.35f);
            }

            float[] animalXs = { 37f, 86f, 132f, 177f, 216f };
            for (int i = 0; i < animalXs.Length; i++)
            {
                float x = animalXs[i];
                GameObject animal = new(i % 2 == 0 ? "Vänlig elefant" : "Vänlig giraff");
                animal.transform.SetParent(transform, false);
                animal.transform.position = new Vector3(x, HeightAt(x) + 0.75f, 0f);
                CreateThemedCrown(animal.transform, Vector2.zero, RuntimeArt.Hex(i % 2 == 0 ? "#7C8490" : "#C8943D"), RuntimeArt.Hex("#F1C36B"), 0.75f);
                CreateBlock(animal.transform, "Ben vänster", new Vector2(-0.28f, -0.65f), new Vector2(0.16f, 1.15f), RuntimeArt.Hex(i % 2 == 0 ? "#6F7782" : "#AA732E"), -1);
                CreateBlock(animal.transform, "Ben höger", new Vector2(0.28f, -0.65f), new Vector2(0.16f, 1.15f), RuntimeArt.Hex(i % 2 == 0 ? "#6F7782" : "#AA732E"), -1);
            }
        }

        private void CreateDesertScenery()
        {
            float[] cactusXs = { 16f, 55f, 96f, 139f, 183f, 224f };
            foreach (float x in cactusXs)
            {
                GameObject cactus = new("Kaktus");
                cactus.transform.SetParent(transform, false);
                cactus.transform.position = new Vector3(x, HeightAt(x), 0f);
                CreateBlock(cactus.transform, "Kaktusstam", new Vector2(0f, 1.15f), new Vector2(0.34f, 2.3f), RuntimeArt.Hex("#399C57"), -1);
                CreateBlock(cactus.transform, "Kaktusarm", new Vector2(0.42f, 1.3f), new Vector2(0.85f, 0.26f), RuntimeArt.Hex("#4DBA66"), -1);
            }

            float[] snakeXs = { 34f, 78f, 121f, 166f, 207f, 232f };
            for (int snakeIndex = 0; snakeIndex < snakeXs.Length; snakeIndex++)
            {
                float x = snakeXs[snakeIndex];
                float y = HeightAt(x) + 0.55f;
                List<Vector2> snake = new();
                for (int i = 0; i <= 18; i++)
                {
                    float t = i / 18f;
                    snake.Add(new Vector2(
                        x - 1.1f + t * 2.2f,
                        y + Mathf.Sin(t * Mathf.PI * 3f) * 0.22f
                    ));
                }
                CreateLine(
                    transform,
                    "Snäll ökenorm",
                    snake,
                    0.24f,
                    RuntimeArt.Hex(snakeIndex % 2 == 0 ? "#55A84D" : "#7E55C7"),
                    1
                );
            }
        }

        private void CreateWaterScenery()
        {
            float[] splashXs = { 18f, 62f, 104f, 148f, 192f, 226f };
            for (int i = 0; i < splashXs.Length; i++)
            {
                CreateThemedOrb(
                    "Vattenplask",
                    splashXs[i],
                    RuntimeArt.Hex("#36BFE4"),
                    Color.white,
                    0.65f + (i % 2) * 0.18f
                );
            }

            float[] slideStarts = { 30f, 112f, 190f };
            Color[] slideColors =
            {
                RuntimeArt.Hex("#FF5E9E"),
                RuntimeArt.Hex("#8B63EA"),
                RuntimeArt.Hex("#35D2A0")
            };
            for (int slideIndex = 0; slideIndex < slideStarts.Length; slideIndex++)
            {
                float start = slideStarts[slideIndex];
                List<Vector2> slide = new();
                for (int i = 0; i <= 28; i++)
                {
                    float t = i / 28f;
                    float x = start + t * 22f;
                    slide.Add(new Vector2(
                        x,
                        HeightAt(x) + 4.8f - t * 2.4f + Mathf.Sin(t * Mathf.PI * 2f) * 0.8f
                    ));
                }
                CreateLine(transform, "Färgglad vattenrutschkana", slide, 0.52f, slideColors[slideIndex], -1);
                CreateLine(transform, "Vattenglans", slide, 0.16f, RuntimeArt.Hex("#D7FAFF"), 0);
            }
        }

        private void CreateSpaceScenery()
        {
            float[] crystalXs = { 15f, 43f, 76f, 108f, 139f, 173f, 205f, 230f };
            for (int i = 0; i < crystalXs.Length; i++)
            {
                float x = crystalXs[i];
                CreateThemedOrb(
                    "Rymdkristall",
                    x,
                    RuntimeArt.Hex(i % 2 == 0 ? "#6F4DDF" : "#18BEE0"),
                    RuntimeArt.Hex("#F6D8FF"),
                    0.7f + (i % 3) * 0.15f
                );
            }

            float[] moonRockXs = { 29f, 62f, 94f, 126f, 158f, 191f, 219f };
            for (int i = 0; i < moonRockXs.Length; i++)
            {
                CreateThemedOrb(
                    "Månsten",
                    moonRockXs[i],
                    RuntimeArt.Hex("#493874"),
                    RuntimeArt.Hex("#B9A7E8"),
                    0.55f + (i % 2) * 0.18f
                );
            }
        }

        private void CreateCandyScenery()
        {
            float[] gumdropXs = { 14f, 39f, 68f, 98f, 128f, 158f, 188f, 216f, 232f };
            Color[] candyColors =
            {
                RuntimeArt.Hex("#FF5E9E"),
                RuntimeArt.Hex("#59D36F"),
                RuntimeArt.Hex("#7B61E8"),
                RuntimeArt.Hex("#FFB52E")
            };
            for (int i = 0; i < gumdropXs.Length; i++)
            {
                CreateThemedOrb(
                    "Gelégodis",
                    gumdropXs[i],
                    candyColors[i % candyColors.Length],
                    Color.white,
                    0.62f + (i % 3) * 0.13f
                );
            }

            float[] candyCaneXs = { 27f, 83f, 144f, 202f };
            foreach (float x in candyCaneXs)
            {
                GameObject cane = new("Polkagris");
                cane.transform.SetParent(transform, false);
                cane.transform.position = new Vector3(x, HeightAt(x), 0f);
                CreateBlock(
                    cane.transform,
                    "Vit pinne",
                    new Vector2(0f, 1.35f),
                    new Vector2(0.32f, 2.7f),
                    Color.white,
                    -1
                );
                CreateBlock(
                    cane.transform,
                    "Röd rand",
                    new Vector2(0f, 1.35f),
                    new Vector2(0.15f, 2.7f),
                    RuntimeArt.Hex("#EF476F"),
                    0
                );
            }
        }

        private static void CreateThemedCrown(
            Transform parent,
            Vector2 position,
            Color edge,
            Color shine,
            float scale
        )
        {
            GameObject crown = new("Mjuk form");
            crown.transform.SetParent(parent, false);
            crown.transform.localPosition = position;
            SpriteRenderer renderer = crown.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.CircleSprite(
                "ThemedCrown_" + ColorUtility.ToHtmlStringRGB(edge),
                edge * 0.7f,
                edge,
                shine,
                96
            );
            renderer.sortingOrder = -1;
            crown.transform.localScale = new Vector3(scale * 1.55f, scale, 1f);
        }

        private void CreateThemedOrb(
            string objectName,
            float x,
            Color edge,
            Color shine,
            float scale
        )
        {
            GameObject orb = new(objectName);
            orb.transform.SetParent(transform, false);
            orb.transform.position = new Vector3(x, HeightAt(x) - 0.05f, 0f);
            SpriteRenderer renderer = orb.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.CircleSprite(
                objectName + ColorUtility.ToHtmlStringRGB(edge),
                edge * 0.7f,
                edge,
                shine,
                96
            );
            renderer.sortingOrder = -1;
            orb.transform.localScale = new Vector3(scale * 1.2f, scale, 1f);
        }

        private void CreateGhost(float x, float height)
        {
            GameObject ghost = new("Vänligt spöke");
            ghost.transform.SetParent(transform, false);
            ghost.transform.position = new Vector3(x, HeightAt(x) + height, 0f);
            SpriteRenderer renderer = ghost.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.CircleSprite(
                "FriendlyGhost",
                RuntimeArt.Hex("#54BFC3"),
                RuntimeArt.Hex("#A7FFF2"),
                Color.white,
                128
            );
            renderer.color = new Color(1f, 1f, 1f, 0.82f);
            renderer.sortingOrder = -1;
            ghost.transform.localScale = new Vector3(0.82f, 1.12f, 1f);

            GameObject faceObject = new("Snällt spökansikte");
            faceObject.transform.SetParent(ghost.transform, false);
            faceObject.transform.localPosition = new Vector3(0f, 0.05f, -0.1f);
            TextMesh face = faceObject.AddComponent<TextMesh>();
            face.text = "••";
            face.anchor = TextAnchor.MiddleCenter;
            face.alignment = TextAlignment.Center;
            face.fontSize = 42;
            face.characterSize = 0.08f;
            face.color = RuntimeArt.Hex("#3B2869");
            face.GetComponent<MeshRenderer>().sortingOrder = 0;
        }

        private void CreateSmashObstacle(float x, Color color)
        {
            float y = HeightAt(x);
            GameObject obstacle = new("Mjuka kraschblock");
            obstacle.transform.SetParent(transform, false);
            obstacle.transform.position = new Vector3(x, y, 0f);

            CreateBlock(obstacle.transform, "Block vänster", new Vector2(-0.88f, 0.88f), new Vector2(1.65f, 1.68f), color, 8);
            CreateBlock(obstacle.transform, "Block höger", new Vector2(0.88f, 0.88f), new Vector2(1.65f, 1.68f), RuntimeArt.Hex("#FFD84A"), 8);
            CreateBlock(obstacle.transform, "Block topp", new Vector2(0f, 2.33f), new Vector2(1.72f, 1.18f), RuntimeArt.Hex("#63E6FF"), 9);

            CreateSmashBall(obstacle.transform, "Boll rosa", new Vector2(-1.16f, 3.35f), RuntimeArt.Hex("#FF5E8E"));
            CreateSmashBall(obstacle.transform, "Boll grön", new Vector2(0f, 3.55f), RuntimeArt.Hex("#69E56B"));
            CreateSmashBall(obstacle.transform, "Boll lila", new Vector2(1.16f, 3.35f), RuntimeArt.Hex("#A968FF"));

            BoxCollider2D trigger = obstacle.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(4f, 4.5f);
            trigger.offset = new Vector2(0f, 1.75f);
            obstacle.AddComponent<SmashObstacle>();
        }

        private static void CreateSmashBall(
            Transform parent,
            string objectName,
            Vector2 localPosition,
            Color color
        )
        {
            GameObject ball = new(objectName);
            ball.transform.SetParent(parent, false);
            ball.transform.localPosition = localPosition;
            ball.transform.localScale = Vector3.one * 0.82f;

            SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.CircleSprite(
                "SmashBall_" + ColorUtility.ToHtmlStringRGB(color),
                color * 0.62f,
                color,
                Color.white
            );
            renderer.sortingOrder = 10;
        }

        private void CreateBalloon(Vector2 position, Color color)
        {
            GameObject balloon = new("Ballong");
            balloon.transform.SetParent(transform, false);
            balloon.transform.position = position;

            SpriteRenderer renderer = balloon.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.CircleSprite(
                "Balloon_" + ColorUtility.ToHtmlStringRGB(color),
                color * 0.72f,
                color,
                Color.white
            );
            renderer.sortingOrder = -1;
            balloon.transform.localScale = new Vector3(0.85f, 1.12f, 1f);

            GameObject stringObject = new("Snöre");
            stringObject.transform.SetParent(balloon.transform, false);
            LineRenderer line = stringObject.AddComponent<LineRenderer>();
            line.material = RuntimeArt.SpriteMaterial();
            line.positionCount = 2;
            line.useWorldSpace = false;
            line.SetPosition(0, new Vector3(0f, -0.45f, 0f));
            line.SetPosition(1, new Vector3(0.16f, -1.45f, 0f));
            line.startWidth = 0.035f;
            line.endWidth = 0.02f;
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.sortingOrder = -2;
        }

        private void CreateFlag(float x, Color color)
        {
            float y = HeightAt(x);
            GameObject flag = new("Banförflagga");
            flag.transform.SetParent(transform, false);
            flag.transform.position = new Vector3(x, y, 0f);
            CreateBlock(flag.transform, "Stång", new Vector2(0f, 1.5f), new Vector2(0.12f, 3f), RuntimeArt.Hex("#FFF4D0"), -1);
            CreateBlock(flag.transform, "Flagga", new Vector2(0.65f, 2.55f), new Vector2(1.3f, 0.75f), color, 0);
        }

        private static void CreateBlock(
            Transform parent,
            string name,
            Vector2 localPosition,
            Vector2 size,
            Color color,
            int order
        )
        {
            GameObject block = new(name);
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.RoundedRectangleSprite(
                "Block_" + ColorUtility.ToHtmlStringRGBA(color),
                color,
                color,
                32,
                32,
                3,
                0
            );
            renderer.color = color;
            renderer.sortingOrder = order;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
        }

        private void AddSampledSection(
            List<Vector2> path,
            float start,
            float end,
            float step,
            System.Func<float, float> heightFunction
        )
        {
            for (float x = start; x < end; x += step)
            {
                path.Add(new Vector2(x, heightFunction(x)));
            }
            path.Add(new Vector2(end, heightFunction(end)));
        }

        private static float TrackHeight(float x)
        {
            if (x < 2f)
            {
                return 0f;
            }
            if (x < 10f)
            {
                float t = Mathf.InverseLerp(2f, 10f, x);
                return (1f - Mathf.Cos(t * Mathf.PI * 2f)) * 1.35f;
            }
            if (x < 13f)
            {
                return 0f;
            }
            if (x < 21f)
            {
                float t = Mathf.InverseLerp(13f, 21f, x);
                return (1f - Mathf.Cos(t * Mathf.PI * 2f)) * 1.1f;
            }
            if (x < 27f)
            {
                return 0f;
            }
            if (x < 39f)
            {
                float t = Mathf.InverseLerp(27f, 39f, x);
                return Mathf.Sin(t * Mathf.PI) * 2.5f;
            }
            if (x < 51f)
            {
                float t = Mathf.InverseLerp(39f, 51f, x);
                return Mathf.Sin(t * Mathf.PI * 2f) * 0.55f;
            }
            if (x < 79.5f)
            {
                return 0f;
            }
            if (x < 93f)
            {
                return 0f;
            }
            if (x < 108f)
            {
                float t = Mathf.InverseLerp(93f, 108f, x);
                return Mathf.Sin(t * Mathf.PI * 3f) * 0.55f;
            }
            if (x < 124f)
            {
                float t = Mathf.InverseLerp(108f, 124f, x);
                return Mathf.Sin(t * Mathf.PI) * 3.4f;
            }
            if (x < 140f)
            {
                float t = Mathf.InverseLerp(124f, 140f, x);
                return Mathf.Sin(t * Mathf.PI * 2f) * 0.75f;
            }
            if (x < 154f)
            {
                float t = Mathf.InverseLerp(140f, 154f, x);
                return Mathf.Sin(t * Mathf.PI) * 2.6f;
            }
            if (x < 166f)
            {
                float t = Mathf.InverseLerp(154f, 166f, x);
                return Mathf.Sin(t * Mathf.PI * 3f) * 0.5f;
            }
            if (x < 180f)
            {
                float t = Mathf.InverseLerp(166f, 180f, x);
                return Mathf.Sin(t * Mathf.PI) * 2.7f;
            }
            if (x < 194f)
            {
                float t = Mathf.InverseLerp(180f, 194f, x);
                return Mathf.Sin(t * Mathf.PI * 3f) * 0.65f;
            }
            if (x < 210f)
            {
                float t = Mathf.InverseLerp(194f, 210f, x);
                return Mathf.Sin(t * Mathf.PI) * 3.5f;
            }
            if (x < 224f)
            {
                float t = Mathf.InverseLerp(210f, 224f, x);
                return Mathf.Sin(t * Mathf.PI * 2f) * 0.72f;
            }
            if (x < 235f)
            {
                float t = Mathf.InverseLerp(224f, 235f, x);
                return Mathf.Sin(t * Mathf.PI) * 1.35f;
            }
            return 0f;
        }

        private static float DirtTrackHeight(float x)
        {
            if (x < 8f)
            {
                return 0f;
            }
            if (x < 22f)
            {
                return Mathf.SmoothStep(0f, 3.8f, Mathf.InverseLerp(8f, 22f, x));
            }
            if (x < 34f)
            {
                return 3.8f;
            }
            if (x < 43f)
            {
                return Mathf.SmoothStep(3.8f, 0f, Mathf.InverseLerp(34f, 43f, x));
            }
            if (x < 58f)
            {
                return Mathf.Sin(Mathf.InverseLerp(43f, 58f, x) * Mathf.PI) * 1.15f;
            }
            if (x < 72f)
            {
                return Mathf.SmoothStep(0f, 5.2f, Mathf.InverseLerp(58f, 72f, x));
            }
            if (x < 82f)
            {
                return 5.2f;
            }
            if (x < 94f)
            {
                return Mathf.SmoothStep(5.2f, -0.4f, Mathf.InverseLerp(82f, 94f, x));
            }
            if (x < 112f)
            {
                return -0.4f + Mathf.Sin(Mathf.InverseLerp(94f, 112f, x) * Mathf.PI * 2f) * 0.75f;
            }
            if (x < 126f)
            {
                return Mathf.SmoothStep(-0.4f, 4.3f, Mathf.InverseLerp(112f, 126f, x));
            }
            if (x < 141f)
            {
                return 4.3f;
            }
            if (x < 153f)
            {
                return Mathf.SmoothStep(4.3f, 0.3f, Mathf.InverseLerp(141f, 153f, x));
            }
            if (x < 171f)
            {
                return 0.3f + Mathf.Sin(Mathf.InverseLerp(153f, 171f, x) * Mathf.PI) * 2.2f;
            }
            if (x < 184f)
            {
                return Mathf.SmoothStep(0.3f, 5.7f, Mathf.InverseLerp(171f, 184f, x));
            }
            if (x < 198f)
            {
                return 5.7f;
            }
            if (x < 211f)
            {
                return Mathf.SmoothStep(5.7f, 0f, Mathf.InverseLerp(198f, 211f, x));
            }
            if (x < 232f)
            {
                return Mathf.Sin(Mathf.InverseLerp(211f, 232f, x) * Mathf.PI * 3f) * 0.65f;
            }
            return 0f;
        }

        private static float MountainTrackHeight(float x)
        {
            if (x < 7f)
            {
                return 0f;
            }
            if (x < 38f)
            {
                return Mathf.SmoothStep(0f, 7.5f, Mathf.InverseLerp(7f, 38f, x));
            }
            if (x < 51f)
            {
                return 7.5f + Mathf.Sin(Mathf.InverseLerp(38f, 51f, x) * Mathf.PI * 2f) * 0.3f;
            }
            if (x < 69f)
            {
                return Mathf.SmoothStep(7.5f, 1.8f, Mathf.InverseLerp(51f, 69f, x));
            }
            if (x < 85f)
            {
                return 1.8f + Mathf.Sin(Mathf.InverseLerp(69f, 85f, x) * Mathf.PI * 3f) * 0.65f;
            }
            if (x < 112f)
            {
                return Mathf.SmoothStep(1.8f, 9.4f, Mathf.InverseLerp(85f, 112f, x));
            }
            if (x < 126f)
            {
                return 9.4f + Mathf.Sin(Mathf.InverseLerp(112f, 126f, x) * Mathf.PI * 2f) * 0.28f;
            }
            if (x < 145f)
            {
                return Mathf.SmoothStep(9.4f, 1.1f, Mathf.InverseLerp(126f, 145f, x));
            }
            if (x < 169f)
            {
                return Mathf.SmoothStep(1.1f, 8.2f, Mathf.InverseLerp(145f, 169f, x));
            }
            if (x < 183f)
            {
                return 8.2f + Mathf.Sin(Mathf.InverseLerp(169f, 183f, x) * Mathf.PI * 3f) * 0.4f;
            }
            if (x < 202f)
            {
                return Mathf.SmoothStep(8.2f, 0.6f, Mathf.InverseLerp(183f, 202f, x));
            }
            if (x < 221f)
            {
                return 0.6f + Mathf.Sin(Mathf.InverseLerp(202f, 221f, x) * Mathf.PI) * 4.2f;
            }
            if (x < 235f)
            {
                return Mathf.SmoothStep(0.6f, 0f, Mathf.InverseLerp(221f, 235f, x));
            }
            return 0f;
        }

        private static float IceTrackHeight(float x)
        {
            if (x < 8f)
            {
                return 0f;
            }
            if (x < 34f)
            {
                return Mathf.SmoothStep(0f, 5.2f, Mathf.InverseLerp(8f, 34f, x));
            }
            if (x < 52f)
            {
                return Mathf.SmoothStep(5.2f, 0.4f, Mathf.InverseLerp(34f, 52f, x));
            }
            if (x < 76f)
            {
                return 0.4f + Mathf.Sin(Mathf.InverseLerp(52f, 76f, x) * Mathf.PI * 3f) * 1.15f;
            }
            if (x < 104f)
            {
                return Mathf.SmoothStep(0.4f, 6.2f, Mathf.InverseLerp(76f, 104f, x));
            }
            if (x < 126f)
            {
                return Mathf.SmoothStep(6.2f, 0f, Mathf.InverseLerp(104f, 126f, x));
            }
            if (x < 154f)
            {
                return Mathf.SmoothStep(0f, 4.8f, Mathf.InverseLerp(126f, 154f, x));
            }
            if (x < 178f)
            {
                return Mathf.SmoothStep(4.8f, 0.5f, Mathf.InverseLerp(154f, 178f, x));
            }
            if (x < 222f)
            {
                return 0.5f + Mathf.Sin(Mathf.InverseLerp(178f, 222f, x) * Mathf.PI * 4f) * 1.25f;
            }
            if (x < 235f)
            {
                return Mathf.SmoothStep(0.5f, 0f, Mathf.InverseLerp(222f, 235f, x));
            }
            return 0f;
        }

        private static float LavaTrackHeight(float x)
        {
            if (x < 10f)
            {
                return 0f;
            }
            if (x < 31f)
            {
                return Mathf.SmoothStep(0f, 4.4f, Mathf.InverseLerp(10f, 31f, x));
            }
            if (x < 47f)
            {
                return 4.4f;
            }
            if (x < 64f)
            {
                return Mathf.SmoothStep(4.4f, -0.3f, Mathf.InverseLerp(47f, 64f, x));
            }
            if (x < 91f)
            {
                return -0.3f + Mathf.Sin(Mathf.InverseLerp(64f, 91f, x) * Mathf.PI) * 5.8f;
            }
            if (x < 112f)
            {
                return Mathf.SmoothStep(-0.3f, 1.2f, Mathf.InverseLerp(91f, 112f, x));
            }
            if (x < 139f)
            {
                return 1.2f + Mathf.Sin(Mathf.InverseLerp(112f, 139f, x) * Mathf.PI * 3f) * 1.05f;
            }
            if (x < 165f)
            {
                return Mathf.SmoothStep(1.2f, 6.4f, Mathf.InverseLerp(139f, 165f, x));
            }
            if (x < 184f)
            {
                return Mathf.SmoothStep(6.4f, 0.2f, Mathf.InverseLerp(165f, 184f, x));
            }
            if (x < 221f)
            {
                return 0.2f + Mathf.Sin(Mathf.InverseLerp(184f, 221f, x) * Mathf.PI * 3f) * 1.4f;
            }
            if (x < 235f)
            {
                return Mathf.SmoothStep(0.2f, 0f, Mathf.InverseLerp(221f, 235f, x));
            }
            return 0f;
        }

        private static float HauntedTrackHeight(float x)
        {
            if (x < 8f)
            {
                return 0f;
            }
            if (x < 48f)
            {
                return Mathf.Sin(Mathf.InverseLerp(8f, 48f, x) * Mathf.PI * 3f) * 1.45f;
            }
            if (x < 75f)
            {
                return Mathf.SmoothStep(0f, 5.4f, Mathf.InverseLerp(48f, 75f, x));
            }
            if (x < 96f)
            {
                return Mathf.SmoothStep(5.4f, 0f, Mathf.InverseLerp(75f, 96f, x));
            }
            if (x < 132f)
            {
                return Mathf.Sin(Mathf.InverseLerp(96f, 132f, x) * Mathf.PI * 4f) * 1.1f;
            }
            if (x < 160f)
            {
                return Mathf.SmoothStep(0f, 4.7f, Mathf.InverseLerp(132f, 160f, x));
            }
            if (x < 181f)
            {
                return Mathf.SmoothStep(4.7f, 0.3f, Mathf.InverseLerp(160f, 181f, x));
            }
            if (x < 222f)
            {
                return 0.3f + Mathf.Sin(Mathf.InverseLerp(181f, 222f, x) * Mathf.PI * 5f) * 1.2f;
            }
            if (x < 235f)
            {
                return Mathf.SmoothStep(0.3f, 0f, Mathf.InverseLerp(222f, 235f, x));
            }
            return 0f;
        }

        private static float JungleTrackHeight(float x)
        {
            if (x < 5f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 15f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(221f, 235f, x));
            float rolling =
                1.7f
                + Mathf.Sin(x * 0.075f) * 1.35f
                + Mathf.Sin(x * 0.21f) * 0.62f;
            return rolling * start * end;
        }

        private static float AfricaTrackHeight(float x)
        {
            if (x < 6f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 16f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(222f, 235f, x));
            float savanna =
                1.25f
                + Mathf.Sin(x * 0.055f) * 0.85f
                + Mathf.Pow(Mathf.Sin(x * 0.145f), 2f) * 1.45f;
            return savanna * start * end;
        }

        private static float DesertTrackHeight(float x)
        {
            if (x < 5f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 14f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(221f, 235f, x));
            float dunes =
                Mathf.Pow(Mathf.Abs(Mathf.Sin(x * 0.075f)), 1.55f) * 3.5f
                + Mathf.Sin(x * 0.23f) * 0.38f;
            return dunes * start * end;
        }

        private static float WaterTrackHeight(float x)
        {
            if (x < 5f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 15f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(220f, 235f, x));
            float slides =
                0.7f
                + Mathf.Pow(Mathf.Sin(x * 0.053f), 2f) * 4.4f
                + Mathf.Sin(x * 0.17f) * 0.55f;
            return slides * start * end;
        }

        private static float SpaceTrackHeight(float x)
        {
            if (x < 5f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 15f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(220f, 235f, x));
            float craters =
                1.1f
                + Mathf.Sin(x * 0.082f) * 1.25f
                + Mathf.Pow(Mathf.Sin(x * 0.19f), 2f) * 1.65f;
            return craters * start * end;
        }

        private static float CandyTrackHeight(float x)
        {
            if (x < 5f || x >= 235f)
            {
                return 0f;
            }
            float start = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 14f, x));
            float end = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(221f, 235f, x));
            float cookieHills =
                0.8f
                + Mathf.Pow(Mathf.Abs(Mathf.Sin(x * 0.061f)), 1.35f) * 3.7f
                + Mathf.Sin(x * 0.22f) * 0.45f;
            return cookieHills * start * end;
        }
    }

    public sealed class AirBooster : MonoBehaviour
    {
        private Vector3 startPosition;
        private float phase;
        private bool used;
        private bool loopBooster;

        public void Initialize(bool isLoopBooster)
        {
            loopBooster = isLoopBooster;
        }

        private void Start()
        {
            startPosition = transform.position;
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * 2.4f + phase);
            transform.position = startPosition + Vector3.up * wave * 0.22f;
            transform.localScale = Vector3.one * (1.06f + wave * 0.035f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used)
            {
                return;
            }

            MonsterTruckVehicle truck = other.GetComponentInParent<MonsterTruckVehicle>();
            if (truck == null || !truck.IsPlayer)
            {
                return;
            }

            used = true;
            truck.ActivateBoost(loopBooster);

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.enabled = false;
            }
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 0.25f);
        }
    }

    public sealed class SmashObstacle : MonoBehaviour
    {
        private bool smashed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (smashed)
            {
                return;
            }

            MonsterTruckVehicle truck = other.GetComponentInParent<MonsterTruckVehicle>();
            if (truck == null || !truck.IsPlayer)
            {
                return;
            }

            smashed = true;
            SpriteRenderer[] pieces = GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < pieces.Length; i++)
            {
                Transform piece = pieces[i].transform;
                piece.SetParent(null, true);
                Rigidbody2D body = piece.gameObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0.8f;
                body.linearVelocity = new Vector2(
                    2f + i * 1.2f,
                    4.5f + i * 0.8f
                );
                body.angularVelocity = (i % 2 == 0 ? 1f : -1f) * (170f + i * 45f);

                if (piece.name.StartsWith("Boll"))
                {
                    CircleCollider2D ballCollider = piece.gameObject.AddComponent<CircleCollider2D>();
                    ballCollider.radius = 0.47f;
                    PhysicsMaterial2D bounce = new("Mjuk boll");
                    bounce.bounciness = 0.82f;
                    bounce.friction = 0.32f;
                    ballCollider.sharedMaterial = bounce;
                    body.gravityScale = 1.15f;
                    body.linearVelocity += new Vector2((i - 3.5f) * 0.7f, 3.8f);
                    Destroy(bounce, 4f);
                    Destroy(piece.gameObject, 4f);
                }
                else
                {
                    Destroy(piece.gameObject, 2.5f);
                }
            }
            Destroy(gameObject);
        }
    }

    public sealed class CoinPickup : MonoBehaviour
    {
        private RaceDirector director;
        private SpriteRenderer spriteRenderer;
        private Vector3 startPosition;
        private float phase;
        private bool collected;

        public void Initialize(RaceDirector raceDirector, SpriteRenderer renderer)
        {
            director = raceDirector;
            spriteRenderer = renderer;
            startPosition = transform.position;
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float time = Time.time * 3f + phase;
            transform.position = startPosition + Vector3.up * (Mathf.Sin(time) * 0.16f);
            transform.localScale = Vector3.one * 0.88f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, RuntimeArt.Hex("#FFF0A0"), (Mathf.Sin(time) + 1f) * 0.25f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected)
            {
                return;
            }

            MonsterTruckVehicle truck = other.GetComponentInParent<MonsterTruckVehicle>();
            if (truck == null || !truck.IsPlayer)
            {
                return;
            }

            collected = true;
            director.CollectCoin(transform.position);
            Destroy(gameObject);
        }
    }

    public sealed class FinishTrigger : MonoBehaviour
    {
        private RaceDirector director;

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            MonsterTruckVehicle truck = other.GetComponentInParent<MonsterTruckVehicle>();
            if (truck != null)
            {
                director.TruckReachedFinish(truck);
            }
        }
    }
}
