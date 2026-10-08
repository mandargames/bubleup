# Integrated local build — release readiness

The current test candidate is the fifteen-stage 0.3.0 campaign with weights, mini-ring trays, timed ice, spatially authored currents and fish. Its [build record](Research/BUILD_0_3_0.md) records 52 passing automated checks, representative desktop renders and mobile package status. Full playthroughs of the new stages and physical-phone qualification remain outstanding. The historical five-level evidence below does not establish expansion readiness. This is not a claim of store readiness or proven retention.

The October UX implementation and its current validation are tracked in [Research/IMPLEMENTATION.md](Research/IMPLEMENTATION.md). Earlier iPhone cloud builds compiled successfully and an earlier version launched according to the owner. Those results do not qualify a new candidate's phone behavior.

| Area | Implemented | Remaining evidence or integration |
| --- | --- | --- |
| Physical interaction | Water drag and settling, gradual water jets, gentler filtered steering, hollow compound ring colliders, stable ring angle, solid pegs/shelves, locked catches and moving pegs | Real-phone comfort, latency and sustained hands-on difficulty tuning |
| Presentation | Original molded casing and pump meshes, five ring colours, layered acrylic/water shaders, pooled bubbles, responsive pumps, screen transitions and three shell colours | Art direction approval and representative device visual checks |
| Audio / haptics | Original synthesized effects and music, bounded pump voices, separate toggles, prioritized haptics and compiled native iOS feedback plugin | Speaker/headphone mix review and physical-device haptic checks |
| Five levels | Five distinct lessons with editable geometry, capacities, forces and star thresholds; each completed by control-only automation | New-player observation; automated completion does not establish human difficulty |
| Progression | One to three stars, sequential unlocks, best times and pump counts, attempt records and shell unlocks | Long-duration and device-upgrade save testing |
| Save / settings | Versioned local JSON, atomic replacement, backup recovery, future-schema protection, saved calibration/sensitivity/accessibility/audio choices | Platform storage failure and force-close testing on target phones |
| Accessibility | Touch fallback, keyboard input, sensor calibration, sensitivity, reduced motion, safe-area UI and pause on background | Notch/tablet layouts, motor-accessibility review and device rotation interruption tests |
| Analytics | Opt-in local events only; size-capped local log; no identifiers or remote endpoint | A deliberate vendor and consent decision if cloud analytics is desired later |
| Monetization | Provider interface defaults to unavailable; no ad or purchase offers are shown | Provider accounts, product IDs, real SDK integration, consent flow, receipt validation and sandbox purchase testing |
| Build / distribution | Windows 0.2.4 executable, unsigned ARM64 physical-iPhone 0.2.4 IPA, earlier Android APK, and original launcher icon | New-candidate phone installation/launch, personal or store signing, device qualification, store artwork and submission |

## Device review before release

Test at least a lower-powered Android phone, a recent Android phone and an iPhone. Check cold startup, 10-minute play, both pumps together, sensor absence, calibration in comfortable holding positions, touch-only play, headphone/speaker audio, interruptions, save recovery and safe areas. Record frame pacing and thermal behavior. The Windows GPU audit is not a mobile performance guarantee.

Capture player observations for every level: whether the objective is understood, completion time, retries, accidental inputs, frustrating misses and whether another attempt feels appealing. The expanded campaign's deadlines, group collection, new visuals and environmental effects need this review before release.

## Physics choice

The chamber uses 3D bodies with their centers constrained to a plane. Each ring has sixteen capsule segments around its open center; the rendered torus and colliders share a stable angle to prevent sideways wedging on pegs. Peg stems, shelves, baffles and other rings participate in collision response. Low-friction contacts, drag and gravity minus buoyancy operate throughout flight and descent. Pump force is vertical and distributed over a short pulse rather than directly overwriting velocity; it adds no lateral shove. The bounded 5.6 by 7.2 chamber retains its shape across phone proportions. Steering acceleration remains gentle, and the saved sensitivity setting applies to phone, keyboard and slider input.

A descending crossing through the peg's opening marks threading, but the ring remains dynamic. Scoring requires low-speed, supported contact near the shelf or stack. A scored ring becomes kinematic at its resting position relative to the peg and ignores pumps and steering permanently for that attempt. Its colliders remain solid for stacking, and it follows moving pegs. Completion follows after all rings lock; menus and the completion screen pause movement. This is a constrained toy simulation with simplified water forces, not unrestricted 3D fluid dynamics.

## Native feedback references

The Android adapter uses platform [haptic feedback constants](https://developer.android.com/reference/android/view/HapticFeedbackConstants). The iOS plugin uses [impact feedback generators](https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator) and notification feedback. The device can decline haptic feedback; gameplay does not rely on vibration.
