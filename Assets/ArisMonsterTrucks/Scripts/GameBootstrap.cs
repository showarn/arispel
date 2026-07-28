using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

namespace ArisMonsterTrucks
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameExists()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null)
            {
                return;
            }

            new GameObject("Aris Monstertrucks").AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            string[] arguments = Environment.GetCommandLineArgs();
            const string resetNamePrefix = "-arisResetPlayerName=";
            string resetPlayerName = Array.Find(
                arguments,
                argument => argument.StartsWith(
                    resetNamePrefix,
                    StringComparison.Ordinal
                )
            );
            if (!string.IsNullOrEmpty(resetPlayerName))
            {
                PlayerProfile.ResetForFreshStart();
                PlayerProfile.SaveUsername(
                    resetPlayerName.Substring(resetNamePrefix.Length)
                );
            }
            else if (Array.Exists(
                arguments,
                argument => argument == "-arisResetPlayer"
            ))
            {
                PlayerProfile.ResetForFreshStart();
            }
            AppPreferences.ApplyAudio();
            _ = TruckLayout.Current;
            if (
                Array.Exists(
                    arguments,
                    argument =>
                        argument == "-arisParentPreviewDashboard"
                        || argument == "-arisParentPreviewPuzzle"
                )
            )
            {
                bool puzzlePreview = Array.Exists(
                    arguments,
                    argument => argument == "-arisParentPreviewPuzzle"
                );
                ParentalControls.Configure(
                    "2468",
                    true,
                    puzzlePreview,
                    true,
                    puzzlePreview
                );
            }

            Camera camera = SetupCamera();
            SetupBackground(camera);

            if (Array.Exists(
                Environment.GetCommandLineArgs(),
                argument => argument == "-arisAutoDrive"
            ))
            {
                RaceDirector director = gameObject.AddComponent<RaceDirector>();
                int testLevel = 1;
                for (int level = 2; level <= LevelProgression.LevelCount; level++)
                {
                    if (Array.Exists(
                        Environment.GetCommandLineArgs(),
                        argument => argument == "-arisLevel" + level
                    ))
                    {
                        testLevel = level;
                    }
                }
                director.BuildGame(camera, testLevel);
                return;
            }

            FrontEndController.Create(gameObject, camera);
            string screenshotArgument = Array.Find(
                arguments,
                argument => argument.StartsWith(
                    "-arisScreenshot=",
                    StringComparison.Ordinal
                )
            );
            if (!string.IsNullOrEmpty(screenshotArgument))
            {
                float screenshotDelay = 1.5f;
                const string delayPrefix = "-arisScreenshotDelay=";
                string delayArgument = Array.Find(
                    arguments,
                    argument => argument.StartsWith(
                        delayPrefix,
                        StringComparison.Ordinal
                    )
                );
                if (
                    !string.IsNullOrEmpty(delayArgument)
                    && float.TryParse(
                        delayArgument.Substring(delayPrefix.Length),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float parsedDelay
                    )
                )
                {
                    screenshotDelay = Mathf.Clamp(parsedDelay, 0.25f, 30f);
                }
                StartCoroutine(
                    CapturePreview(
                        screenshotArgument.Substring(
                            "-arisScreenshot=".Length
                        ),
                        screenshotDelay
                    )
                );
            }
        }

        private static IEnumerator CapturePreview(string path, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ScreenCapture.CaptureScreenshot(path);
        }

        private static Camera SetupCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new("Huvudkamera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 6.6f;
            camera.backgroundColor = RuntimeArt.Hex("#3BC9FF");
            camera.transform.position = new Vector3(4f, 4.2f, -10f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
            return camera;
        }

        private static void SetupBackground(Camera camera)
        {
            GameObject backgroundObject = new("Färgglad bakgrund");
            backgroundObject.transform.SetParent(camera.transform, false);
            backgroundObject.transform.localPosition = new Vector3(0f, 0f, 20f);

            SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArt.LoadSprite("Art/Environment/colorful_background");
            renderer.sortingOrder = -1000;

            if (renderer.sprite != null)
            {
                float viewportHeight = camera.orthographicSize * 2f;
                float viewportWidth = viewportHeight * camera.aspect;
                Vector2 spriteSize = renderer.sprite.bounds.size;
                float scale = Mathf.Max(
                    viewportWidth / spriteSize.x,
                    viewportHeight / spriteSize.y
                ) * 1.04f;
                backgroundObject.transform.localScale = Vector3.one * scale;
            }
        }
    }

    public sealed class CameraFollow : MonoBehaviour
    {
        private MonsterTruckVehicle target;
        private Vector3 velocity;

        public void Initialize(MonsterTruckVehicle vehicle)
        {
            target = vehicle;
        }

        private void LateUpdate()
        {
            if (target == null || target.Body == null)
            {
                return;
            }

            Vector2 bodyPosition = target.Body.position;
            float desiredX = Mathf.Clamp(bodyPosition.x + 4.5f, -0.7f, ColorTrackBuilder.FinishX + 2f);
            float desiredY = Mathf.Clamp(bodyPosition.y + 1.2f, 4.2f, 8.2f);
            Vector3 desired = new(desiredX, desiredY, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.24f);
        }
    }
}
