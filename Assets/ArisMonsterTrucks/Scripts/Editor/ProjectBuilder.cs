#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArisMonsterTrucks.EditorTools;

namespace ArisMonsterTrucks.Editor
{
    public static class ProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/MonsterTruckRace.unity";

        [MenuItem("Aris Monstertrucks/Skapa första versionen")]
        public static void CreateFirstVersion()
        {
            Directory.CreateDirectory("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            GameObject root = new("Aris Monstertrucks");
            root.AddComponent<GameBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.companyName = "Aris Familjespel";
            PlayerSettings.productName = "Aris Monstertrucks";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.iOS,
                "se.arisfamiljespel.monstertrucks"
            );
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Första versionen skapad: " + ScenePath);
        }

        [MenuItem("Aris Monstertrucks/Bygg Linux-test")]
        public static void BuildLinuxTest()
        {
            PrepareGeneratedAssets();
            Directory.CreateDirectory("Builds/Linux");

            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Linux/ArisMonstertrucks",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            EnsureBuildSucceeded(report, "Linux");
        }

        [MenuItem("Aris Monstertrucks/Bygg Android development-APK")]
        public static void BuildAndroidDevelopment()
        {
            PrepareGeneratedAssets();
            Directory.CreateDirectory("Builds/Android");

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                "se.arisfamiljespel.arisspel"
            );
            PlayerSettings.companyName = "Aris Familjespel";
            PlayerSettings.productName = "Arisspel";
            PlayerSettings.bundleVersion = "1.0";
            if (
                int.TryParse(
                    Environment.GetEnvironmentVariable("CM_BUILD_NUMBER"),
                    out int buildNumber
                )
            )
            {
                PlayerSettings.Android.bundleVersionCode =
                    Mathf.Max(1, buildNumber);
            }

            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            EditorUserBuildSettings.buildAppBundle = false;

            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/Arisspel.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            EnsureBuildSucceeded(report, "Android");
        }

        [MenuItem("Aris Monstertrucks/Exportera iOS Xcode-projekt")]
        public static void ExportIosXcodeProject()
        {
            PrepareGeneratedAssets();
            Directory.CreateDirectory("Builds/iOS");

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.iOS,
                "se.arisfamiljespel.arisspel"
            );
            PlayerSettings.companyName = "Aris Familjespel";
            PlayerSettings.productName = "Arisspel";
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.iOS.buildNumber =
                Environment.GetEnvironmentVariable("CM_BUILD_NUMBER") ?? "1";
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            AssignIosAppIcon();

            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/iOS",
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            EnsureBuildSucceeded(report, "iOS Xcode-export");
            AddIosMarketingIcon();
        }

        private static void AssignIosAppIcon()
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Branding/app_icon.png"
            );
            if (icon == null)
            {
                throw new BuildFailedException(
                    "Appikonen Assets/Branding/app_icon.png kunde inte laddas."
                );
            }

            int iconCount = PlayerSettings.GetIconSizes(
                NamedBuildTarget.iOS,
                IconKind.Application
            ).Length;
            Texture2D[] icons = new Texture2D[iconCount];
            for (int index = 0; index < icons.Length; index++)
            {
                icons[index] = icon;
            }

            PlayerSettings.SetIcons(
                NamedBuildTarget.iOS,
                icons,
                IconKind.Application
            );
        }

        private static void AddIosMarketingIcon()
        {
            const string iconSource = "Assets/Branding/app_icon.png";
            const string iconDirectory =
                "Builds/iOS/Unity-iPhone/Images.xcassets/AppIcon.appiconset";
            string contentsPath = Path.Combine(iconDirectory, "Contents.json");
            string destinationPath = Path.Combine(
                iconDirectory,
                "Icon-AppStore-1024.png"
            );

            if (!File.Exists(contentsPath))
            {
                throw new BuildFailedException(
                    "Xcodes AppIcon-katalog saknas efter iOS-exporten."
                );
            }

            File.Copy(iconSource, destinationPath, true);
            string contents = File.ReadAllText(contentsPath);
            const string marker = "\"images\" : [";
            const string marketingIcon =
                "\n\t\t{\n"
                + "\t\t\t\"filename\" : \"Icon-AppStore-1024.png\",\n"
                + "\t\t\t\"idiom\" : \"ios-marketing\",\n"
                + "\t\t\t\"scale\" : \"1x\",\n"
                + "\t\t\t\"size\" : \"1024x1024\"\n"
                + "\t\t},";
            if (!contents.Contains("Icon-AppStore-1024.png"))
            {
                contents = contents.Replace(
                    marker,
                    marker + marketingIcon
                );
                File.WriteAllText(contentsPath, contents);
            }
        }

        private static void PrepareGeneratedAssets()
        {
            FishingAssetBuilder.EnsureFishingAssets();
            StoryAssetBuilder.BuildLillaLumi();
            AriSisterStoryAssetBuilder.BuildAriAndSister();
            StoryAssetBuilder.ValidateLillaLumi();
            AriSisterStoryAssetBuilder.ValidateAriAndSister();
        }

        private static void EnsureBuildSucceeded(
            BuildReport report,
            string platform
        )
        {
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    platform
                    + "-bygget misslyckades: "
                    + report.summary.result
                    + " ("
                    + report.summary.totalErrors
                    + " fel)."
                );
            }
        }
    }
}
#endif
