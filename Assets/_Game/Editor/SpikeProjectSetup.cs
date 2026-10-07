using System.IO;
using PocketToys.WaterRingToss;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PocketToys.Editor
{
    public static class SpikeProjectSetup
    {
        public const string ScenePath = "Assets/_Game/Toys/WaterRingToss/Scenes/WaterRingTossSpike.unity";
        public const string ConfigPath = "Assets/_Game/Toys/WaterRingToss/Levels/SpikeConfig.asset";

        [MenuItem("Pocket Toys/Create or open Phase 0 scene")]
        public static void Create()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            AssetDatabase.Refresh();
            var config = AssetDatabase.LoadAssetAtPath<WaterRingTossConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<WaterRingTossConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            const string materialPath = "Assets/_Game/Toys/WaterRingToss/Levels/SpikeSurface.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            config.prototypeMaterial = material;
            EditorUtility.SetDirty(config);
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var controller = new GameObject("Water Ring Toss - Phase 0").AddComponent<SpikeController>();
                controller.config = config;
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.companyName = "Pocket Toys Local";
            PlayerSettings.productName = "Pocket Toys";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 600;
            PlayerSettings.defaultScreenHeight = 1000;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.pockettoys.localspike");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.pockettoys.localspike");
            Time.fixedDeltaTime = 1f / 60f;
            var timeManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0]);
            timeManager.FindProperty("Fixed Timestep").floatValue = 1f / 60f;
            timeManager.FindProperty("Maximum Allowed Timestep").floatValue = .1f;
            timeManager.ApplyModifiedPropertiesWithoutUndo();
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var inputHandler = settings.FindProperty("activeInputHandler");
            if (inputHandler != null) inputHandler.intValue = 0;
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("Pocket Toys Phase 0 scene and project settings are ready.");
        }

        [MenuItem("Pocket Toys/Build local Windows prototype")]
        public static void BuildWindows()
        {
            Create();
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/PocketToys.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Windows build failed: " + result.summary.result);
        }
    }
}
