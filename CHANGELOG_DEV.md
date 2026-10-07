# Development changelog

## 2026-10-07 — GitHub source setup

- The user requested a source push to `https://github.com/mandargames/bubleup`, superseding the earlier local-only/no-Git instruction.
- Added Unity-aware Git exclusions for generated builds, caches, logs, backups, local settings, player saves, and signing credentials.
- Added consistent source/Unity YAML line-ending rules and documented how to open a fresh checkout.
- Game code and previously validated builds are unchanged by this source-control setup.

## 2026-10-07 — Integrated five-level game, v0.2.0

The user selected five polished levels with all core systems. The larger 40-level campaign is not part of this delivery. This development pass was completed locally before the subsequent GitHub source setup.

### Game and presentation

- Added `Assets/_Game/App/PocketToys.unity` as the integrated entry scene; retained the original spike for comparison.
- Created five authored levels covering a first landing, stacking, peg capacities, asymmetric pumps, a baffle, and a moving peg.
- Added multi-ring physics, shallow collision proxies, visible ring wobble, capacity-limited assisted catches and stable stacks.
- Replaced the spike presentation with original rounded enamel casing/pump meshes, dimensional rings, acrylic/water shaders, pooled bubbles, studio lighting and a matching launcher icon.
- Added Home, level selection, pause, completion, settings and collectible shell screens, star feedback and screen transitions.
- Added original synthesized water effects, contact/landing sounds, completion audio and music, plus Android/iOS haptic adapters.

### Core systems

- Added versioned local JSON progress/settings with atomic writes, backup recovery and protection against overwriting newer schemas.
- Added stars, sequential level unlocks, best time/pump records, attempt counts, and cosmetic shell unlocks at five and ten stars.
- Added saved sensor calibration/sensitivity, touch fallback, independent audio/haptic settings, reduced motion, safe-area handling and background pause.
- Added optional local-only analytics behind an interface. No identifiers or remote service are configured.
- Added an unavailable-by-default commerce facade. No ads, purchase offers or simulated transactions are shown.
- Added local Windows/Android build recipes and an isolated standalone audit that does not modify the player's actual save.
- Archived the pre-change project in `Backups/Before-integrated-build-20261007-200038.zip`.

### Validation

- The final complete suite passed **26 checks**: 16 Edit Mode and 10 Play Mode, including the taller-portrait layout check.
- Control-only automated play completed all five levels in both the Unity test runner and the packaged Windows player.
- The final Windows player audit completed all five levels at 600 × 1000, measuring 2,634 gameplay frames with a 16.68 ms mean and 16.67 ms 95th-percentile frame interval on the local AMD Radeon RX 7900 GRE, with a 60 fps cap. This is a desktop measurement, not mobile performance evidence. The final report and screenshots are in `Logs/FinalPlayerAudit`.
- Inspected Home, gameplay, completion, level selection, settings and collection renders. Fixed ring depth/initial pose, overly bright casing, faceted corners, stretched slider handles, missing star rendering and faint instruction text.
- The final ARM64 Android development APK was built successfully at `Builds/Android/PocketToys.apk`. Android installation and real-device tests remain pending.
- The matching Windows release build succeeded at `Builds/Windows/PocketToys.exe`; the editor's active build target was restored to Windows. APK metadata confirms version 0.2.0 and ARM64 code.

### Remaining release work

- Human difficulty/feel evaluation and real-phone tilt/haptic testing remain necessary. Scripted reachability is not a substitute for new-player testing.
- The native iOS plugin and Xcode build have not been compiled on a Mac. No device installation or store submission was performed.
- Ads/IAP provider integration, store signing, consent decisions and mobile performance qualification remain pending. See `Docs/RELEASE_READINESS.md`.
- Physics is authored 2.5D with assisted captures, not unrestricted torus/peg collisions or a fluid simulation.

## 2026-10-07 — Phase 0 implementation

- Created the local Unity 6000.3.9f1 project from the installed editor. No Git repository or remote configured.
- Added one-ring, one-peg Water Ring Toss scene, generated primitive visuals and portrait HUD.
- Added fixed-step filtered accelerometer input, neutral calibration, keyboard simulation and a spring-centered touch slider.
- Added localized left/right pumps, authored water settling/damping, wall collisions and assisted descending capture.
- Added immediate restart, pause/resume, background pause, attempt timer and pump count.
- Added semantic audio/haptic interfaces, four audio voices, generated placeholder tones and independent toggles.
- Added editable content/physics configuration, local scene/build tools, logic tests and physics smoke tests.

### Known limits

- Phase 0 remains pending real-phone feel validation. This implementation does not establish the on-device exit criterion.
- Ring threading is a 2.5D assisted crossing rule with a sphere collision proxy, not a full torus/peg collision simulation.
- Haptic semantics currently map to Unity's generic `Handheld.Vibrate`; pulse intensity/duration is device dependent. Desktop haptics are a no-op.
- Audio is placeholder synthesized tones; visuals are primitive meshes. No production art, gyro comparison, fluid simulation, menus, progression, ads, IAP or second toy.
- Settings and calibration are session-only. UI targets portrait; landscape is not the intended play orientation.
- iOS installation requires a Mac, Xcode and signing. Android/iOS builds and device behavior are not yet verified.

### Validation

- Unity Editor compilation and scene/config generation succeeded with Unity 6000.3.9f1.
- Edit Mode: 6/6 passed (calibration, clamping, dead zone, filter timing, pump locality, catch conditions and content validation).
- Play Mode: 6/6 passed (pump/reset, pause, capture/reset, containment, control-only win from spawn and preview rendering).
- Inspected the 600 × 1000 rendered preview in `Logs/prototype-preview.png`.
- Windows development build succeeded at `Builds/Windows/PocketToys.exe`. A background startup smoke run initialized graphics/physics without logged runtime errors, then was closed.
- Fixed a Windows player compilation issue by guarding the mobile-only haptic API. Added a serialized material dependency so the runtime-generated geometry's shader is retained in player builds.
- Test reports: `Logs/EditMode-results.xml` and `Logs/PlayMode-results.xml`. Build/startup logs: `Logs/build-windows.log` and `Logs/desktop-smoke.log`.
