# Pocket Toys — five-adventure local game

A portrait Water Ring Toss game built in Unity **6000.3.9f1**, with five authored levels and shared game systems. Source repository: [mandargames/bubleup](https://github.com/mandargames/bubleup). Gameplay saves and diagnostics remain local; no game backend is configured.

The repository contains `Assets` (including Unity `.meta` files), `Packages`, `ProjectSettings`, tests, build tools, and documentation. Unity caches, generated executables/APKs, local backups, player saves, and signing credentials are excluded. Build the game locally after cloning; the prebuilt files described below are available only in the original development workspace.

## Play on Windows

Run `Builds/Windows/PocketToys.exe`. Keep its adjacent data folder and DLLs together. The Home screen leads into five authored levels; complete each one to unlock the next. Every completion earns a star, with extra stars for time and pump efficiency. Stars also unlock new toy shell colours.

| Action | Desktop | Phone |
| --- | --- | --- |
| Pump | Q / E, or click the round buttons | Tap either pump; simultaneous touches supported |
| Steer | A / D or arrows, or drag the slider | Tilt the phone, or disable Phone tilt in Settings to use the slider |
| Restart | R or RESTART | RESTART |
| Pause / resume | Escape or PAUSE | PAUSE / KEEP PLAYING |
| Calibrate | C while playing, or Settings | Hold the phone comfortably, then Settings → Calibrate |

The app pauses when backgrounded. Calibration, sensitivity, audio, haptics, reduced motion and shell choice are saved. Music, effects and haptics have separate switches. Local diagnostics are optional and off by default.

Pumps lift vertically; use keyboard, the touch slider or phone tilt to steer left and right. Both outer lanes receive enough lift to reach the corners of the expanded tank. Steering sensitivity in Settings applies to all steering inputs. Once a ring lands and settles on a peg, it locks in place and cannot be pumped off. Locked rings follow moving pegs and support the next ring in a stack.

## What is included

- Five distinct levels: First Splash, Double Dip, Side by Side, The Scenic Route, and A Little Symphony.
- Multiple rings, capacity-limited peg stacks, asymmetric pumps, a baffle, and a moving peg.
- Hollow ring colliders, solid pegs and shelves, gradual water-jet impulses, and physical settling before a catch scores. Lift beside a peg and steer over its tip; shelves also block rings coming from below.
- Original rounded toy meshes, layered water/acrylic shaders, ring motion, bubbles and responsive physical pump buttons.
- Illustrated coral lagoon, mint enamel cabinet, animated underwater light, floating bubbles, and a collectible-toy home screen with bundled display typography. See [visual design and art credits](Docs/VISUAL_DESIGN.md).
- Original synthesized water/contact/celebration sounds and quiet music.
- Home, level selection, pause, completion, settings and collectible shell screens.
- Stars, sequential unlocks, best times/pump counts, versioned local saves and backup recovery.
- Keyboard/touch controls, sensor calibration, reduced-motion mode and native mobile haptic adapters.

This is an integrated local beta. Device feel, mobile frame pacing and live store services remain unverified; see [release readiness](Docs/RELEASE_READINESS.md). Ads and purchases are unavailable until a real provider is configured; no simulated transactions are presented.

## Open and tune in Unity

1. Clone or download this repository, add its root folder to Unity Hub, and open with **6000.3.9f1**. The original development checkout is `D:\GameDevelopment\CasualGames`.
2. Open `Assets/_Game/App/PocketToys.unity`, or use **Pocket Toys → Open integrated game**.
3. Set the Game view to portrait and press Play. The game creates its meshes and interface at runtime.
4. Edit `Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset` for level positions, capacities, obstacles, pump strengths and star thresholds.

The original Phase 0 scene remains at `Assets/_Game/Toys/WaterRingToss/Scenes/WaterRingTossSpike.unity` for comparison. Use the **integrated** build command for the current game.

## Build locally

- **Windows:** Pocket Toys → Build integrated Windows game. Output: `Builds/Windows/PocketToys.exe`.
- **Android:** Pocket Toys → Build local Android APK. Output: `Builds/Android/PocketToys.apk`. Uses the installed Unity Android SDK/NDK/JDK, ARM64 and a development signature for local testing. A physical device is required to assess motion and haptic feel.
- **iOS:** The `dot/ios-cloud-build` branch exports Unity and compiles an unsigned ARM64 iPhone IPA using GitHub Actions. Download the `PocketToys-ios-device-unsigned` artifact and import the IPA into AltStore Classic using your own Apple account. See [iPhone build instructions](Docs/IOS_CLOUD_BUILD.md). The previous 0.2.0 build was installed successfully by the user; each new build still needs device testing.

## Saves and backups

Unity saves to `Application.persistentDataPath`. On this Windows configuration, that is normally `%USERPROFILE%\AppData\LocalLow\Pocket Toys Local\Pocket Toys`. `pocket-toys-v1.json` holds progress and settings; `.bak` holds the previous save. An optional `local-diagnostics.jsonl` stays in the same local directory.

`Backups/Before-integrated-build-*.zip` preserves the project before this development pass. To make future backups, copy `Assets` including `.meta` files, `Packages`, `ProjectSettings`, `Tools`, `Docs`, and the root Markdown documents. Keep `Builds` to preserve a playable version. `Library`, `Temp` and logs are regenerable.

## Verification

Close the interactive Unity editor, then run:

```powershell
.\Tools\Run-Checks.ps1
```

Reports are saved to `Logs`. The tests cover calibration, forces, save recovery, star evaluation, content validation, pause/restart, UI paths and completing all five levels through actual pump/tilt inputs. Rendered previews are under `Logs/Previews`.

A standalone build can run an isolated audit without changing your real save:

```powershell
.\Builds\Windows\PocketToys.exe -batchmode -screen-width 720 -screen-height 1280 -pocketToysAudit D:\GameDevelopment\CasualGames\Logs\PlayerAudit
```

That audit exports screenshots, an isolated QA save and `report.json`, then exits. Desktop measurements do not establish mobile performance.

The authoritative product brief is [GAME_SPEC.md](GAME_SPEC.md). See [level design](Docs/LEVEL_DESIGN.md), [release readiness](Docs/RELEASE_READINESS.md) and [development history](CHANGELOG_DEV.md).
