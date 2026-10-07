# iOS cloud compilation

The cloud workflows export the integrated Pocket Toys scene using Unity 6000.3.9f1 on GitHub's Linux runner, then compile the generated Xcode project on a standard macOS runner. The device workflow produces an unsigned ARM64 IPA for local signing and sideloading. No Apple credentials, paid Apple membership, or TestFlight setup are used by these workflows.

## Physical iPhone output

`.github/workflows/ios-device.yml` uses a separate device-SDK export and compiles for `iphoneos`, `generic/platform=iOS`, and ARM64 with code signing disabled. The output artifact, **PocketToys-ios-device-unsigned**, contains:

- `PocketToys-ios-device-unsigned.ipa`
- A SHA-256 checksum file
- `INSTALL.txt` with the signing requirement and setup link

The IPA contains the complete `Payload/*.app`, including Unity data and `Frameworks/UnityFramework.framework`. Before packaging, the workflow verifies that the app's Info.plist targets iPhoneOS and supports iPhone, and that the main executable and embedded frameworks/dylibs are ARM64 binaries with the physical iOS Mach-O platform. ARM64 alone is insufficient because Apple Silicon simulator binaries also use ARM64.

The unsigned IPA cannot install or launch directly on iPhone. It must be signed and provisioned for the user's device. A local route is **AltStore Classic with AltServer**, using the user's own Apple Account. Follow the [official Windows setup guide](https://faq.altstore.io/altstore-classic/how-to-install-altstore-windows), then import the IPA into AltStore Classic. Signing must cover the app and its embedded frameworks. Do not send Apple credentials to GitHub or paste them into chat.

With a free Apple Account, [AltStore apps expire after seven days unless refreshed](https://faq.altstore.io/altstore-classic/your-altstore); Apple also limits [active sideloaded apps to three](https://faq.altstore.io/altstore-classic/activating-apps), including the sideloaded AltStore app. iOS 16 and later require Developer Mode, as described in the setup guide. The project currently targets iOS 15.0 or later. Actual signing, installation, and device launch remain separate checks.

The workflow also uploads the generated Xcode project and Xcode build log. Artifacts expire after seven days. The IPA is not an App Store or TestFlight release.

## Simulator output

`.github/workflows/ios-simulator.yml` remains available through `workflow_dispatch` only, so pushes do not start a redundant Unity activation and simulator build alongside the device workflow. Its output is a ZIP containing an unsigned Simulator `.app` for the macOS runner's CPU architecture, plus the generated Xcode project and build log. The simulator app cannot be installed on a physical iPhone.

## Activation

Both workflows use the existing Unity activation setup. They check for repository Actions secrets without printing their values:

- `UNITY_EMAIL` and `UNITY_PASSWORD`
- Exactly one of `UNITY_LICENSE` (Personal license file supported by GameCI) or `UNITY_SERIAL` (eligible serial-based license)

If activation needs to be reconfigured, the owner must enter credentials directly in GitHub Settings > Secrets and variables > Actions after reviewing the [GameCI activation instructions](https://game.ci/docs/github/activation/). Never commit credentials, upload them as artifacts, or paste them into chat. Activation availability depends on the Unity account/license. The workflows do not create credentials, buy a license, or configure Apple access.

## Running

Pushes to `dot/ios-cloud-build` trigger the device workflow. Both workflows declare `workflow_dispatch`; GitHub's Actions tab exposes the Run workflow button once the corresponding workflow exists on the default branch. If activation prerequisites are missing, the workflow fails before downloading Unity. After supplying valid secrets, use Re-run all jobs for the failed run or push a new commit to the build branch.

Actions must be enabled for the repository. Runner availability, account quotas, permissions, and the Unity image remain runtime prerequisites. Standard `ubuntu-24.04` and `macos-15` runners are selected; no larger runner is configured. Runner billing still depends on GitHub's account and repository rules.

## Implementation

- Device helper: `Assets/_Game/Editor/IOSDeviceBuild.cs`
- Simulator helper: `Assets/_Game/Editor/IOSSimulatorBuild.cs`
- Both helpers are in the existing Editor-only assembly and build `Assets/_Game/App/PocketToys.unity`
- Device helper selects `iOSSdkVersion.DeviceSDK`, `PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1)` (ARM64), IL2CPP, and disabled automatic signing, restoring prior settings afterward
- Simulator helper selects the simulator SDK with Universal export; Xcode compiles for the runner's architecture
- `/tmp/bubleup-unity` export paths match across hosts; tar preserves executable permissions
- Action dependencies are pinned to verified commit SHAs; workflow contents permission is read-only
- IPA packaging uses the built `.app` directly rather than Xcode's signing-dependent archive export

## Validation status

The simulator workflow **passed** in [run 37647861429, attempt 2](https://github.com/mandargames/bubleup/actions/runs/37647861429/attempts/2) at commit `d73b629816490ad0d56c3cefb96163838e5bc013`. Unity activation, simulator export, and native Xcode compilation completed successfully. This does not establish simulator launch or physical-device behavior.

The new device workflow has been parsed as YAML; all embedded Bash blocks passed `bash -n`, and the embedded Python passed syntax validation. Action pins, trigger scope, read-only permissions, Unity-only secret references, and the shared export path were checked. Nine mocked verifier fixtures passed, including rejection of simulator metadata, ARM64 simulator binaries, x86_64 binaries, and missing framework executables. These fixtures do not execute Xcode or inspect a real device build.

At this revision, the new device workflow has not yet completed an Actions run. Unity device export, Xcode device compilation, signing, installation, launch, touch input, motion sensors, haptics, rendering, and gameplay remain unverified. The Actions run for the exact commit is the source of truth for compilation results.

References: [GameCI iOS](https://game.ci/docs/github/deployment/ios/), [GameCI builder](https://game.ci/docs/github/builder/), [Unity iOS SDK selection](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.iOS-sdkVersion.html), [Unity CPU architecture API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.SetArchitecture.html).
