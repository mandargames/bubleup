## 2026-10-08 - Research baseline implementation (0.2.4, build 5)

- Replaced the stretched tall-phone tank with a bounded, proportionally framed arena. Narrower casing keeps rings prominent; the header and touch targets clear the simulated safe areas.
- Added contextual first-level guidance, revisitable help, finger-sized targets, touch release/disable cleanup, and restart confirmation. Separated controls/calibration from comfort settings.
- Made completion the primary reward. Time, pump counts and stars remain available in optional results; existing progress, bests, cosmetics and saved preferences remain compatible.
- Softened pump audio with short low bubble resonances and capped simultaneous pump voices. Haptic events coalesce by importance; softer native iPhone impacts replace medium landing impacts.
- Fixed a scoring deadlock when a physically supported ring sat higher in an offset stack than the old acceptance range allowed. Support, settling, threading and capacity checks remain required.
- Validation: 17 EditMode and 26 PlayMode checks passed, including all five levels through public controls, offset stacks, release/pause/help/restart, mute, waveform bounds and three phone proportions. Reviewed rendered menus and gameplay. Physical iPhone feel remains unverified for this candidate.
- Markdown research and implementation records are in Docs/Research; build and device status are tracked in Docs/Research/IMPLEMENTATION.md.

## 2026-10-08 - Full-screen phone gameplay and gentle bubbles (0.2.3, build 4)

- Replaced the large gameplay branding/header and bottom menu row with a compact level title and top-right settings control. The control pauses play and exposes restart, levels, settings and home. Its touch area is larger than its visible icon.
- Framed the toy to phone width and expanded the visible water, enclosure and real collision boundaries vertically into the safe screen area. Boundary transforms update only when the layout or level changes. Pump hit targets follow the rendered controls; tilt-enabled devices hide the unused touch slider.
- Replaced the noisy pump hiss/thump with a quieter four-bubble resonant cluster, soft onset/release and slight pitch variation.
- Validation: 16 EditMode and 23 PlayMode checks passed, including all five levels, corner containment, pause controls and bubble waveform limits. Reviewed 360x800, 390x844 and 375x667 previews with simulated safe-area margins. Both focused preview/layout checks passed after correcting screenshot aspect restoration.
- iPhone compilation and physical-device validation are recorded separately with the release artifact.

## 2026-10-08 — iPhone build 0.2.1 (2)

- Prepared the current local game changes for a new physical-iPhone build: Coral Club artwork, larger tank, compact peg supports, hollow ring collisions, gentler controls, and rings that lock after landing.
- Bumped the app version to 0.2.1 and iPhone build number to 2, retaining `com.pockettoys.water` for updating the existing AltStore installation.
- Reused the existing unsigned ARM64 iPhone cloud workflow. The IPA still requires personal signing through AltStore; no Apple credentials or store submission are involved.
- Validation before cloud compilation: 16 EditMode and 21 PlayMode tests passed in the isolated build checkout, including all five levels completed through pump and steering input. New-build device launch remains unverified.

# Development changelog

## 2026-10-08 — Vertical pumps and a roomier tank

- Removed the fixed inward impulse from red/yellow pumps. Jets now lift vertically while steering controls horizontal travel, preserving existing momentum and the gentler steering response.
- Expanded the chamber from about 5.2 × 6.13 to 6.8 × 7.2 units. Updated physical walls, cabinet, water, glass, camera framing and HUD placement together; rings and pegs retain their size and authored layout.
- Moved both nozzles and pump buttons farther apart, widened jet coverage to lift rings in the outer lanes, and aligned particle effects with the vertical jets. Landed rings still lock permanently.
- Added regressions for each pump's lateral drift and steering/pumping into and back out of both upper corners.
- Validation: all 21 PlayMode checks passed, including all five levels and the new pump/corner regressions (`Logs/RoomierTank-results.xml`). Reviewed standard and tall portrait previews, corrected the Home subtitle spacing, and passed the focused preview check. Windows rebuild succeeded (`Logs/RoomierTank-build.log`); mobile packages were not rebuilt.

## 2026-10-08 — More room beside the pegs

- Shortened peg landing bars from 0.95 to 0.58 units (39%) in both the visible mesh and collider, opening wider upward routes beside each peg. Kept solid peg and support contact, permanent landing locks, and the gentler controls.
- Adjusted the automated pilot's clearance estimate to the new support size and added a physics regression for rising past both ends of a support. The separate level-four baffle remains unchanged.
- Validation: all 19 PlayMode checks passed, including the five-level control playthrough, side clearance, solid support contact, stacking and permanent locks (`Logs/CompactSupports-results.xml`). Reviewed the updated gameplay preview and rebuilt Windows successfully (`Logs/CompactSupports-build.log`). Mobile packages were not rebuilt.

## 2026-10-08 — Lock landed rings, retain gentler controls

- Following the clarified gameplay preference, a ring locks permanently once it physically lands and settles on a peg. Pumps and steering cannot remove it; pause/resume preserves the lock, and restarting clears the attempt.
- Locked rings retain solid colliders for stacking and follow moving pegs from their resting offset. Rings remain dynamic before landing. The gentler steering, smoothing and pump response are unchanged.
- Replaced the release regression with repeated-pump, steering, pause/resume and restart checks, added moving-peg lock coverage, and updated the in-game hint and documentation.
- Validation: all 18 PlayMode checks passed, including completing all five levels through public controls (`Logs/LockedRings-results.xml`). Windows rebuild succeeded (`Logs/LockedRings-build.log`); mobile packages were not rebuilt.

## 2026-10-08 — Gentler controls and removable rings

- Removed the permanent seated-ring physics lock and pump rejection. Rings remain dynamic during play, can lift off their peg and settle again, and release their counted slot when fully removed.
- Restored a stable ring angle to prevent a sideways torus wedging against a peg in the constrained play plane. Kept hollow ring, peg and shelf collisions.
- Reduced steering acceleration from 5.5 to 4 and lateral pump force by 25%; spread each water pulse over 0.22 seconds instead of 0.16. Threaded rings receive gentler jets, so a tap nudges them while repeated pumps can remove them.
- The existing sensitivity setting now applies to keyboard and slider controls too, without changing saved preference values. Input smoothing is slightly gentler.
- Added regression checks for lifting a seated ring completely free, returning it without duplicate scoring, pausing seated rings, and changing sensitivity with virtual input. Updated the second-level hint and physics documentation.
- Validation: 16 EditMode checks and 16 of 17 PlayMode checks passed, including all focused collision, release, scoring, pause and sensitivity regressions. The full-game control pilot timed out on Side by Side while repeatedly lifting a crowded stack; the latest tuning does not have a clean five-level automated playthrough. Windows build succeeded (`Logs/GentleControls-build.log`). Android/iOS packages were not rebuilt.

## 2026-10-08 — Ring contact and weight correction

- Replaced the filled capsule collision proxy with sixteen convex capsule segments matching the visible torus and its actual rigid-body orientation.
- Added solid peg stems and landing shelves; the moving peg now moves a kinematic rigid body on physics steps. Baffle collision depth matches its visible geometry.
- Pumps apply a short water-jet impulse over multiple steps instead of replacing velocity. Rings retain gravity minus buoyancy, water drag, low-friction contacts and physical roll throughout their movement.
- Crossing a peg tip marks a ring as threaded but keeps it dynamic. A catch scores only after supported contact near the shelf/stack and a short settling period. Caught rings keep their colliders and support subsequent rings without a scripted slide or teleport.
- Added regression checks for side impacts below the peg tip, impacts from below a shelf, dynamic threading and stacking, ring-to-ring momentum transfer, and pump/pause/settling behavior. All 16 EditMode and 15 PlayMode checks passed, including completing all five levels with public pump/tilt controls.
- Updated the QA pilot to lift beside solid shelves and stop pumping during a landing, and clarified the first-level hint.
- Rebuilt the Windows game and verified all five levels in the packaged player. `Logs/RingPhysicsAudit/report.json` reports no errors over 8,602 gameplay frames (16.68 ms average on the local desktop GPU). Mobile packages have not been rebuilt in this correction.

## 2026-10-08 — Coral Club visual refresh

- Added original generated coral-lagoon art inside the playable aquarium, with animated refraction, light caustics and ambient bubble outlines.
- Rebuilt the cabinet silhouette with rolled rounded corners, a mint enamel finish, brighter warm/cool lighting, champagne trim and a detailed pump console.
- Redesigned Home around a collectible toy display with floating showcase rings, new typography, warm raised buttons, framed cards and a procedural ocean backdrop.
- Added readable gameplay counter capsules, adventure number badges and miniature toy previews for collectible shells. Preserved the existing five-level gameplay, saves and input behavior.
- Bundled Lilita One and Lato with their font licenses; recorded the artwork prompt and asset provenance in `Docs/VISUAL_DESIGN.md`.
- Validation: 16 EditMode and 10 PlayMode checks passed. Reviewed 720 × 1280 and 360 × 800 previews; the packaged Windows audit completed all five levels at 600 × 1000 without errors (16.74 ms average frame interval on the local desktop GPU). Final UI adjustments were checked by the focused rendered-preview test.
- Rebuilt `Builds/Windows/PocketToys.exe`. Android/iOS packages and real-device performance have not been revalidated in this visual pass.

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
