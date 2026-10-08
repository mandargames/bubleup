# Pocket Toys 0.3.0 — fifteen-stage test build

8 October 2026. The owner requested builds after the source expansion. Version 0.3.0 uses iPhone build number 6 and Android version code 6, retaining `com.pockettoys.water` for an update to the existing app.

## Candidate and checks

Build source: `a8999fe5ba922bbc23ca64f82702e96a63b1245e` on the existing `dot/ios-cloud-build` branch. The draft PR remains unmerged.

- **19 EditMode checks passed.** Includes all fifteen campaign entries, agreement between the ten expansion entries and the fresh-project recipe, water-shader import, bounded current force and warning/rest timing, saves and haptic coalescing.
- **33 PlayMode checks passed, none skipped.** Includes the original five levels completed through pump/steering controls, all fifteen levels loading, new level tips remaining paused, physical mini-ring tray catches, quota completion with two spare rings, freeze expiry/retry/progress retention, final-catch update ordering, current pause/restart and repeatable fish warnings.
- The tray regression places rings above collectors and lets physics settle them; it establishes scoring behavior, not a complete human or control-only playthrough of those new stages. The deadline boundary check injects completed catch events to isolate update ordering; other regressions cover physical landings.
- Rendered Thermal Staircase, Quiet Shoal, Passing Window and the paused level-tip page were inspected. The freeze screen was recaptured after its transition and inspected; its focused regression passed again. Existing phone-layout regressions passed at 360 × 800, 390 × 844 and 375 × 667. This is desktop evidence, not a phone performance or comfort guarantee.

Local evidence in the build checkout: `Logs/EditMode-results.xml`, `Logs/PlayMode-results.xml` and `Logs/Previews/`.

## Packages

- **Android ARM64 APK built successfully** with Unity 6000.3.9f1 and IL2CPP. APK metadata confirms `com.pockettoys.water`, version 0.3.0, code 6, minimum SDK 26 (Android 8.0), target SDK 36 and only `arm64-v8a`. Android's `apksigner verify` and ZIP integrity check passed; the IL2CPP library is ARM64 and all fifteen campaign IDs are present in packaged Unity assets. The signing certificate matches the previous local 0.2.4 APK. This remains a development package with the local Android debug signing certificate; no Android device was connected for installation/launch verification.
- APK: `D:/GameDevelopment/CasualGames/Builds/Android-0.3.0/PocketToys-0.3.0.apk` (29,933,063 bytes). SHA-256: `9e97ad1f9177dfdd106dfadd794ac47ffc4269c12dbb1836f75bfd71123f1bf4`.
- **Physical-iPhone unsigned IPA built successfully** in [cloud run 37816964189](https://github.com/mandargames/bubleup/actions/runs/37816964189), with Unity export followed by macOS/Xcode compilation. Downloaded metadata confirms version 0.3.0, build 6, `com.pockettoys.water`, minimum iOS 15.0 and the iPhoneOS platform. Both the app executable and UnityFramework are ARM64 with Mach-O platform iOS device and no code-signature command; no provisioning profile is embedded. ZIP integrity and all fifteen packaged campaign IDs were checked.
- [Download the iPhone ZIP artifact](https://github.com/mandargames/bubleup/actions/runs/37816964189/artifacts/11569195980). Expires **15 October 2026 at 17:51 UTC**. Unzip and import `PocketToys-ios-device-unsigned.ipa` into AltStore Classic with AltServer running. Personal signing is required before iOS can install it.
- IPA SHA-256: `f649bf92fe0af0e0111942d78ac154e6dc83bd96bbfb175fb5c3da5cd1256fe3`; matches the packaged manifest.
- Artifact ZIP SHA-256: `dd50d17af6b5598664d788ea19c2ae8cd127577bb4360a9da3ee233f3adf86f2`; matches GitHub's artifact digest.
- Local iPhone package, archive and verification record: `D:/GameDevelopment/CasualGames/Builds/iOS-0.3.0/`.

Compilation and package checks do not establish installation or launch on a physical phone. No Apple credentials, paid membership, TestFlight/App Store submission or PR merge were used.

## Phone testing still required

Install the new candidate over the existing version and confirm saved progress/settings survive. Check cold launch, touch and tilt, muted/audio feedback, actual haptics, safe areas, freeze/pause/retry, current cues, collector scoring and fish contacts. Play the ten new layouts for reachability, understandable routes and difficulty. The existing automated pilot still completes only the original five; no full-campaign completion claim is made.

The audience and difficulty assumptions in [NEXT_LEVELS.md](NEXT_LEVELS.md) remain provisional. The 60/75-second deadlines, spatial-current transfers, mini-ring readability and fish/peg combination need hands-on tuning.
