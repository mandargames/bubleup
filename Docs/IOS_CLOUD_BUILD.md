# iOS cloud compilation

This workflow exports the integrated Pocket Toys scene using Unity 6000.3.9f1 on GitHub's Linux runner, then compiles the generated Xcode project on a standard macOS runner.

## Output and limits
The output is a ZIP containing an unsigned iOS Simulator .app for the macOS runner's CPU architecture, plus the generated Xcode project and build log. It is **not an installable iPhone IPA**, and cannot be uploaded to TestFlight. Apple signing is deliberately not configured. A device build requires a separate device-SDK export, Apple signing configuration and a distribution method. Successful compilation does not establish that gameplay, rendering or haptics work on a physical device.

## Activation
All Unity builds require valid activation. The workflow checks for repository Actions secrets without printing their values:
- UNITY_EMAIL and UNITY_PASSWORD
- Exactly one of UNITY_LICENSE (Personal license file supported by GameCI) or UNITY_SERIAL (eligible serial-based license)

The owner must enter credentials directly in GitHub Settings > Secrets and variables > Actions, after reviewing the [GameCI activation instructions](https://game.ci/docs/github/activation/). Never commit credentials, upload them as artifacts, or paste them into chat. Activation availability depends on the Unity account/license; do not assume an arbitrary license file works. The workflow does not create credentials, buy a license or configure Apple access.

## Running
Pushes to dot/ios-cloud-build trigger the workflow. Once the workflow is merged into the default branch, GitHub's Actions tab also exposes its Run workflow button. An initial run with missing activation secrets is expected to fail the prerequisite step before downloading Unity. After supplying valid secrets, use Re-run all jobs for that run or push a new commit to the build branch.

Actions must be enabled for the repository. Runner availability, account quotas, permissions and the Unity image remain runtime prerequisites. No larger/paid runner is selected.

## Implementation
- .github/workflows/ios-simulator.yml
- Assets/_Game/Editor/IOSSimulatorBuild.cs
- Enabled integrated scene: Assets/_Game/App/PocketToys.unity
- Simulator SDK with Universal export; Xcode compiles for the runner's architecture
- /tmp/bubleup-unity export paths match across hosts; tar preserves executable permissions
- Action dependencies pinned to verified commit SHAs; contents permission is read-only

## Validation status
Before publication, the workflow was parsed as YAML and each embedded Bash script passed bash -n. Action SHA formats and read-only permissions were checked. Unity export, Xcode compilation, simulator launch and physical-device tests were not run locally. The Actions run is the source of truth for runtime results.

References: [GameCI iOS](https://game.ci/docs/github/deployment/ios/), [GameCI builder](https://game.ci/docs/github/builder/), [Unity iOS simulator SDK](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.iOS-sdkVersion.html).
