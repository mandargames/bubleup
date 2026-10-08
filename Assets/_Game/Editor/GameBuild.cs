using System;
using System.IO;
using PocketToys.WaterRingToss.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PocketToys.Editor
{
    public static class GameBuild
    {
        public const string ScenePath = "Assets/_Game/App/PocketToys.unity";
        public const string CampaignPath = "Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset";

        [MenuItem("Pocket Toys/Open integrated game")]
        public static void Setup()
        {
            Directory.CreateDirectory("Assets/_Game/App");
            AssetDatabase.Refresh();
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>(CampaignPath);
            if (campaign == null)
            {
                campaign = ScriptableObject.CreateInstance<CampaignDefinition>();
                campaign.levels = InitialContent();
                AssetDatabase.CreateAsset(campaign, CampaignPath);
            }
            campaign.surfaceMaterial = Material("GameSurface", "Standard");
            campaign.waterMaterial = Material("LagoonWater", "PocketToys/LagoonWater");
            campaign.glassMaterial = Material("AcrylicSheen", "PocketToys/AcrylicSheen");
            EditorUtility.SetDirty(campaign);
            if (!campaign.Validate(out var reason)) throw new Exception(reason);
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var game = new GameObject("Pocket Toys").AddComponent<GameSession>(); game.campaign = campaign;
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "Pocket Toys"; PlayerSettings.companyName = "Pocket Toys Local";
            PlayerSettings.bundleVersion = "0.2.2";
            PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { GameIcon.Create() }, IconKind.Any);
            PlayerSettings.defaultScreenWidth = 600; PlayerSettings.defaultScreenHeight = 1000;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.pockettoys.water");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.pockettoys.water");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 4;
            AssetDatabase.SaveAssets();
            Debug.Log("Integrated game ready: five authored levels and local progression.");
        }
        static Material Material(string name, string shader)
        {
            string path = "Assets/_Game/Toys/WaterRingToss/Levels/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var found = Shader.Find(shader); if (found == null) throw new Exception("Shader missing: " + shader);
            material = new Material(found); AssetDatabase.CreateAsset(material, path); return material;
        }
        static PegDefinition Peg(float x, float y, float length, int capacity, float movement = 0f)
            => new PegDefinition { tip = new Vector2(x, y), length = length, capacity = capacity, movement = movement };

        // One-time authoring recipe. Existing campaign assets are never overwritten by Setup.
        static LevelDefinition[] InitialContent() => new[]
        {
            new LevelDefinition {
                id="water_01", title="First Splash", chapter="FIRST SPLASH", lesson="One ring. Find the rhythm.",
                hint="Lift beside the peg, then steer over its tip.",
                rings=new[]{new Vector2(-1.1f,-1.85f)}, pegs=new[]{Peg(0f,.9f,1.4f,1)},
                goldSeconds=22f, silverSeconds=50f, goldPumps=8
            },
            new LevelDefinition {
                id="water_02", title="Double Dip", chapter="FIND THE BALANCE", lesson="Two rings, one satisfying stack.",
                hint="Landed rings lock in place. Stack the next one on top.",
                rings=new[]{new Vector2(-1.25f,-1.85f),new Vector2(1.25f,-1.85f)}, pegs=new[]{Peg(0f,1.15f,1.65f,2)},
                goldSeconds=36f, silverSeconds=70f, goldPumps=15, starsRequired=1
            },
            new LevelDefinition {
                id="water_03", title="Side by Side", chapter="FIND THE BALANCE", lesson="Choose a side. Share the rings.",
                hint="Dots below each peg show how many rings it holds.",
                rings=new[]{new Vector2(-1.55f,-1.85f),new Vector2(0f,-1.85f),new Vector2(1.55f,-1.85f)},
                pegs=new[]{Peg(-1.1f,.9f,1.4f,1),Peg(1.1f,1.35f,1.8f,2)},
                leftStrength=5.6f,rightStrength=6.1f,goldSeconds=52f,silverSeconds=95f,goldPumps=23,starsRequired=2
            },
            new LevelDefinition {
                id="water_04", title="The Scenic Route", chapter="CHOOSE YOUR PATH", lesson="Go around, then settle in.",
                hint="Use the side channels to lift past the golden baffle.",
                rings=new[]{new Vector2(-1.55f,-1.85f),new Vector2(0f,-1.85f),new Vector2(1.55f,-1.85f)},
                pegs=new[]{Peg(-1.18f,1.6f,1.35f,2),Peg(1.18f,1.6f,1.35f,1)},
                baffles=new[]{new BaffleDefinition{center=new Vector2(0f,-.05f),size=new Vector2(1.3f,.18f),angle=8f}},
                goldSeconds=65f,silverSeconds=115f,goldPumps=30,starsRequired=3
            },
            new LevelDefinition {
                id="water_05", title="A Little Symphony", chapter="MASTER THE TOY", lesson="Four rings. Three pegs. Your rhythm.",
                hint="The middle peg drifts gently. Anticipate its return.",
                rings=new[]{new Vector2(-1.8f,-1.85f),new Vector2(-.6f,-1.85f),new Vector2(.6f,-1.85f),new Vector2(1.8f,-1.85f)},
                pegs=new[]{Peg(-1.45f,.9f,1.4f,1),Peg(0f,1.75f,1.5f,2,.28f),Peg(1.45f,.9f,1.4f,1)},
                goldSeconds=80f,silverSeconds=140f,goldPumps=38,starsRequired=4
            }
        };

        [MenuItem("Pocket Toys/Build integrated Windows game")]
        public static void Windows()
        {
            Setup(); Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/PocketToys.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Windows build failed.");
        }

        [MenuItem("Pocket Toys/Build local Android APK")]
        public static void Android()
        {
            Setup(); Directory.CreateDirectory("Builds/Android");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Android/PocketToys.apk", target = BuildTarget.Android, options = BuildOptions.Development });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Android build failed.");
        }
    }
}
