using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PocketToys.CI
{
    // Called only by the CI workflow's explicit buildMethod.
    public static class IOSDeviceBuild
    {
        public static void Build()
        {
            const string scene = "Assets/_Game/App/PocketToys.unity";
            const int arm64Architecture = 1;
            string root = Directory.GetParent(Application.dataPath).FullName;
            if (!File.Exists(Path.Combine(root, scene)))
                throw new BuildFailedException("Required scene is missing: " + scene);
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new BuildFailedException("This Unity editor needs the iOS Build Support module.");

            var previousSdk = PlayerSettings.iOS.sdkVersion;
            var previousArchitecture = PlayerSettings.GetArchitecture(NamedBuildTarget.iOS);
            var previousBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS);
            var previousSigning = PlayerSettings.iOS.appleEnableAutomaticSigning;
            try
            {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, arm64Architecture);
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
                PlayerSettings.iOS.appleEnableAutomaticSigning = false;

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = Path.Combine(root, "build", "iOS"),
                    target = BuildTarget.iOS,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException("iOS device export failed: " + report.summary.result);
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = previousSdk;
                PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, previousArchitecture);
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, previousBackend);
                PlayerSettings.iOS.appleEnableAutomaticSigning = previousSigning;
            }
        }
    }
}
