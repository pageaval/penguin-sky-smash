using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PSS.EditorTools
{
    /// One-click / batchmode setup + Android build for Penguin Sky Smash.
    public static class PSSBuild
    {
        const string ScenePath = "Assets/Scenes/Game.unity";
        const string PackageId = "com.gadwords.penguinskysmash";

        [MenuItem("PSS/1. Setup Scene and Settings")]
        public static void SetupProject()
        {
            // 2D editor defaults
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

            // Boot scene: a single GameBootstrap object builds everything at runtime.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var boot = new GameObject("GameBootstrap");
            boot.AddComponent<GameBootstrap>();
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            // Player settings
            PlayerSettings.companyName = "Gadwords";
            PlayerSettings.productName = "Penguin Sky Smash";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Android specifics
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            AssetDatabase.SaveAssets();
            Debug.Log("[PSS] Setup complete: scene, build settings and Android player settings configured.");
        }

        [MenuItem("PSS/2. Build Android APK")]
        public static void BuildAndroid()
        {
            if (!File.Exists(ScenePath)) SetupProject();

            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);

            Directory.CreateDirectory("Builds");
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/PenguinSkySmash.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;
            Debug.Log($"[PSS] Build {s.result}  size={s.totalSize} bytes  errors={s.totalErrors}  output={s.outputPath}");
            if (s.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
