using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArisMonsterTrucks
{
    public sealed class RaceDirector : MonoBehaviour
    {
        public MonsterTruckVehicle PlayerTruck { get; private set; }
        public MonsterTruckVehicle NpcTruck { get; private set; }

        private ColorTrackBuilder track;
        private GameHud hud;
        private bool gasHeld;
        private bool raceRunning;
        private bool raceFinished;
        private bool npcFinished;
        private bool autoDriveTest;
        private float nextAutoTestLog;
        private float nextAutoScreenshot;
        private int autoScreenshotIndex;
        private AudioSource celebrationAudio;
        private int coins;
        private bool npcLoopBoostGranted;
        private bool npcSecondBoostGranted;
        private int levelNumber = 1;
        private float raceStartedAt;

        public bool TrackHasLoop => track != null && track.HasLoop;
        public int LevelNumber => levelNumber;
        public bool HasNextLevel => levelNumber < LevelProgression.LevelCount;

        public void BuildGame(Camera gameCamera, int selectedLevel = 1)
        {
            levelNumber = Mathf.Clamp(selectedLevel, 1, LevelProgression.LevelCount);
            celebrationAudio = gameObject.AddComponent<AudioSource>();
            celebrationAudio.spatialBlend = 0f;
            celebrationAudio.playOnAwake = false;

            string trackName = levelNumber switch
            {
                2 => "Dirtbanan",
                3 => "Bergsklättringen",
                4 => "Isbanan",
                5 => "Lavabanan",
                6 => "Spökbanan",
                7 => "Djungelbanan",
                8 => "Afrikabanan",
                9 => "Ökenbanan",
                10 => "Vattenbanan",
                11 => "Rymdbanan",
                12 => "Godisbanan",
                _ => "Färgglada banan"
            };
            GameObject trackObject = new(trackName);
            track = trackObject.AddComponent<ColorTrackBuilder>();
            track.Build(this, levelNumber);

            SpriteRenderer background = gameCamera.GetComponentInChildren<SpriteRenderer>();
            if (background != null)
            {
                string backgroundPath = levelNumber switch
                {
                    2 => "Art/Environment/dirt_background",
                    3 => "Art/Environment/mountain_background",
                    4 => "Art/Environment/ice_background",
                    5 => "Art/Environment/lava_background",
                    6 => "Art/Environment/haunted_background",
                    7 => "Art/Environment/jungle_background",
                    8 => "Art/Environment/africa_background",
                    9 => "Art/Environment/desert_background",
                    10 => "Art/Environment/waterpark_background",
                    11 => "Art/Environment/space_background",
                    12 => "Art/Environment/candy_background",
                    _ => "Art/Environment/colorful_background"
                };
                background.sprite = RuntimeArt.LoadSprite(backgroundPath);
                background.color = Color.white;
                ScaleBackgroundToCamera(background, gameCamera);
            }

            PlayerTruck = MonsterTruckVehicle.Create(
                "Blå monstertruck",
                new Vector2(-5.5f, 3.02f),
                true,
                this
            );
            NpcTruck = MonsterTruckVehicle.Create(
                "Lila monstertruck",
                new Vector2(-12.5f, 3.02f),
                false,
                this
            );
            track.PrepareTruckForLoop(PlayerTruck);
            track.PrepareTruckForLoop(NpcTruck);
            IgnoreTruckCollisions();
            autoDriveTest = System.Array.Exists(
                System.Environment.GetCommandLineArgs(),
                argument => argument == "-arisAutoDrive"
            );
            if (autoDriveTest)
            {
                Directory.CreateDirectory("/tmp/aris-test-frames");
                nextAutoScreenshot = 0.5f;
            }

            CameraFollow follow = gameCamera.gameObject.AddComponent<CameraFollow>();
            follow.Initialize(PlayerTruck);

            hud = GameHud.Create(this);
            PlayerTruck.SetControlsEnabled(false);
            NpcTruck.SetControlsEnabled(false);

            StartCoroutine(Countdown());
        }

        private static void ScaleBackgroundToCamera(
            SpriteRenderer background,
            Camera gameCamera
        )
        {
            if (background.sprite == null || gameCamera == null)
            {
                return;
            }

            float viewportHeight = gameCamera.orthographicSize * 2f;
            float viewportWidth = viewportHeight * gameCamera.aspect;
            Vector2 spriteSize = background.sprite.bounds.size;
            float scale = Mathf.Max(
                viewportWidth / spriteSize.x,
                viewportHeight / spriteSize.y
            ) * 1.04f;
            background.transform.localScale = Vector3.one * scale;
        }

        public void SetGasHeld(bool held)
        {
            gasHeld = held;
            hud?.SetGasPressed(held);
        }

        public void CollectCoin(Vector3 worldPosition)
        {
            coins++;
            hud?.SetCoins(coins);
            hud?.PulseCoinCounter();
            celebrationAudio.PlayOneShot(RuntimeArt.CoinSound(), 0.72f);
            CreateCoinBurst(worldPosition);
        }

        public void TruckReachedFinish(MonsterTruckVehicle truck)
        {
            if (raceFinished)
            {
                return;
            }

            if (!truck.IsPlayer)
            {
                npcFinished = true;
                return;
            }

            if (autoDriveTest)
            {
                Debug.Log(
                    "ARIS_AUTOTEST FINISH coins=" + coins
                    + " npcFirst=" + npcFinished
                );
            }
            raceFinished = true;
            raceRunning = false;
            gasHeld = false;
            PlayerTruck.SetThrottle(0f);
            NpcTruck.SetThrottle(0f);
            PlayerTruck.ParkForFinish();
            NpcTruck.ParkForFinish();
            CoinWallet.Add(coins);
            float elapsedSeconds = Mathf.Max(
                0f,
                Time.realtimeSinceStartup - raceStartedAt
            );
            LevelResult levelResult = LevelProgression.RecordResult(
                levelNumber,
                elapsedSeconds
            );
            hud.ShowFinish(
                coins,
                npcFinished,
                levelResult.Rating,
                levelResult.NextLevelUnlocked
            );
            CreateConfetti(PlayerTruck.Body.position + Vector2.up * 2.5f);
            celebrationAudio.PlayOneShot(RuntimeArt.CelebrationSound(), 0.85f);
            if (autoDriveTest)
            {
                StartCoroutine(QuitAutoDriveTest());
            }
        }

        public float TrackHeightAt(float x)
        {
            return track == null ? 0f : track.HeightAt(x);
        }

        public void EnterLoop(MonsterTruckVehicle truck)
        {
            track?.EnterLoop(truck);
        }

        public void ExitLoop(MonsterTruckVehicle truck)
        {
            track?.ExitLoop(truck);
        }

        public void PrepareTruckForLoop(MonsterTruckVehicle truck)
        {
            track?.PrepareTruckForLoop(truck);
        }

        public void RestartRace()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ExitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ExitToLevelSelect()
        {
            Time.timeScale = 1f;
            PlayerProfile.RequestLevelSelect();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void StartNextLevel()
        {
            if (!HasNextLevel || !LevelProgression.TrySelectLevel(levelNumber + 1))
            {
                ExitToLevelSelect();
                return;
            }

            Time.timeScale = 1f;
            PlayerProfile.RequestRaceStart();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Update()
        {
            if (!raceRunning || raceFinished)
            {
                return;
            }

            bool keyboardGas = Input.GetKey(KeyCode.Space)
                || Input.GetKey(KeyCode.RightArrow)
                || Input.GetKey(KeyCode.D);
            PlayerTruck.SetThrottle(gasHeld || keyboardGas || autoDriveTest ? 1f : 0f);

            float distance = PlayerTruck.Progress - NpcTruck.Progress;
            float npcThrottle = 0.76f + Mathf.Clamp(distance * 0.045f, -0.20f, 0.24f);

            if (NpcTruck.Progress > PlayerTruck.Progress + 5.5f)
            {
                npcThrottle = 0.32f;
            }
            if (
                NpcTruck.Progress > ColorTrackBuilder.FinishX - 10f
                && PlayerTruck.Progress < ColorTrackBuilder.FinishX - 4f
            )
            {
                npcThrottle = 0.18f;
            }
            if (TrackHasLoop && NpcTruck.Progress > 60f && NpcTruck.Progress < 86f)
            {
                npcThrottle = 1f;
            }
            if (TrackHasLoop && !npcLoopBoostGranted && NpcTruck.Progress > 63f)
            {
                npcLoopBoostGranted = true;
                NpcTruck.ActivateBoost(true);
            }
            if (!TrackHasLoop && !npcLoopBoostGranted && NpcTruck.Progress > 50f)
            {
                npcLoopBoostGranted = true;
                NpcTruck.ActivateBoost(false);
            }
            if (!TrackHasLoop && !npcSecondBoostGranted && NpcTruck.Progress > 137f)
            {
                npcSecondBoostGranted = true;
                NpcTruck.ActivateBoost(false);
            }

            NpcTruck.SetThrottle(npcThrottle);
            NpcTruck.SetOverlapFade(
                Vector2.Distance(PlayerTruck.Body.position, NpcTruck.Body.position) < 5.2f
            );
            hud.SetProgress(PlayerTruck.Progress, NpcTruck.Progress);

            if (autoDriveTest && Time.time >= nextAutoTestLog)
            {
                nextAutoTestLog = Time.time + 1f;
                Debug.Log(
                    "ARIS_AUTOTEST player=" + PlayerTruck.Body.position
                    + " angle=" + PlayerTruck.Body.rotation.ToString("0.0")
                    + " npc=" + NpcTruck.Body.position
                    + " coins=" + coins
                );
            }

            if (autoDriveTest && Time.time >= nextAutoScreenshot)
            {
                nextAutoScreenshot = Time.time + 0.25f;
                string fileName = string.Format(
                    "/tmp/aris-test-frames/frame-{0:000}.png",
                    autoScreenshotIndex++
                );
                ScreenCapture.CaptureScreenshot(fileName);
            }
        }

        private IEnumerator Countdown()
        {
            yield return new WaitForSeconds(0.45f);

            string[] messages = { "3", "2", "1", "KÖR!" };
            foreach (string message in messages)
            {
                hud.ShowCountdown(message);
                yield return new WaitForSeconds(message == "KÖR!" ? 0.8f : 0.72f);
            }

            hud.HideCountdown();
            raceStartedAt = Time.realtimeSinceStartup;
            raceRunning = true;
            PlayerTruck.SetControlsEnabled(true);
            NpcTruck.SetControlsEnabled(true);
        }

        private IEnumerator QuitAutoDriveTest()
        {
            yield return new WaitForSeconds(1f);
            Application.Quit(npcFinished ? 2 : 0);
        }

        private void IgnoreTruckCollisions()
        {
            Collider2D[] playerColliders = PlayerTruck.GetComponentsInChildren<Collider2D>();
            Collider2D[] npcColliders = NpcTruck.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D playerCollider in playerColliders)
            {
                foreach (Collider2D npcCollider in npcColliders)
                {
                    Physics2D.IgnoreCollision(playerCollider, npcCollider, true);
                }
            }
        }

        private void CreateCoinBurst(Vector3 position)
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject sparkle = new("Myntglitter");
                sparkle.transform.position = position;
                SpriteRenderer renderer = sparkle.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.CircleSprite(
                    "Sparkle",
                    RuntimeArt.Hex("#FF9B21"),
                    RuntimeArt.Hex("#FFF27A"),
                    Color.white,
                    48
                );
                renderer.sortingOrder = 80;
                sparkle.transform.localScale = Vector3.one * 0.22f;

                Rigidbody2D body = sparkle.AddComponent<Rigidbody2D>();
                body.gravityScale = 0.7f;
                float angle = (Mathf.PI * 2f * i / 7f) + Random.Range(-0.2f, 0.2f);
                body.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 3.5f;
                Destroy(sparkle, 0.75f);
            }
        }

        private void CreateConfetti(Vector3 position)
        {
            Color[] colors =
            {
                RuntimeArt.Hex("#FF4F87"),
                RuntimeArt.Hex("#FFD43B"),
                RuntimeArt.Hex("#45E07A"),
                RuntimeArt.Hex("#55C8FF"),
                RuntimeArt.Hex("#A96BFF")
            };

            for (int i = 0; i < 44; i++)
            {
                GameObject piece = new("Konfetti");
                piece.transform.position = position + new Vector3(Random.Range(-4f, 4f), Random.Range(0f, 4f), 0f);
                SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArt.RoundedRectangleSprite(
                    "Confetti_" + i % 5,
                    colors[i % colors.Length],
                    colors[i % colors.Length],
                    20,
                    40,
                    3,
                    0
                );
                renderer.sortingOrder = 100;
                piece.transform.localScale = Vector3.one * 0.45f;
                piece.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

                Rigidbody2D body = piece.AddComponent<Rigidbody2D>();
                body.gravityScale = Random.Range(0.2f, 0.65f);
                body.angularVelocity = Random.Range(-320f, 320f);
                body.linearVelocity = new Vector2(Random.Range(-3f, 3f), Random.Range(2f, 7f));
                Destroy(piece, 4f);
            }
        }
    }
}
