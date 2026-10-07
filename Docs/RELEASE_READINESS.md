# Integrated local build — release readiness

The approved delivery is five polished levels with the core game systems. This is a local playable beta, not a claim of store readiness or proven retention.

| Area | Implemented | Remaining evidence or integration |
| --- | --- | --- |
| Physical interaction | Water drag and settling, localized jets, shared filtered tilt, ring wobble, shallow ring collisions, assisted descending catches, stacking and moving pegs | Real-phone comfort, latency and sustained hands-on difficulty tuning |
| Presentation | Original molded casing and pump meshes, five ring colours, layered acrylic/water shaders, pooled bubbles, responsive pumps, screen transitions and three shell colours | Art direction approval and representative device visual checks |
| Audio / haptics | Original layered synthesized effects and music, separate toggles, Android semantic feedback adapter and native iOS feedback plugin | Speaker/headphone mix review, device haptic checks and Xcode compilation |
| Five levels | Five distinct lessons with editable geometry, capacities, forces and star thresholds; each completed by control-only automation | New-player observation; automated completion does not establish human difficulty |
| Progression | One to three stars, sequential unlocks, best times and pump counts, attempt records and shell unlocks | Long-duration and device-upgrade save testing |
| Save / settings | Versioned local JSON, atomic replacement, backup recovery, future-schema protection, saved calibration/sensitivity/accessibility/audio choices | Platform storage failure and force-close testing on target phones |
| Accessibility | Touch fallback, keyboard input, sensor calibration, sensitivity, reduced motion, safe-area UI and pause on background | Notch/tablet layouts, motor-accessibility review and device rotation interruption tests |
| Analytics | Opt-in local events only; size-capped local log; no identifiers or remote endpoint | A deliberate vendor and consent decision if cloud analytics is desired later |
| Monetization | Provider interface defaults to unavailable; no ad or purchase offers are shown | Provider accounts, product IDs, real SDK integration, consent flow, receipt validation and sandbox purchase testing |
| Build / distribution | Local Windows executable, compiled ARM64 Android APK, and original launcher icon | Android device installation, store signing, iOS Mac/Xcode export/build, store artwork and submission |

## Device review before release

Test at least a lower-powered Android phone, a recent Android phone and an iPhone. Check cold startup, 10-minute play, both pumps together, sensor absence, calibration in comfortable holding positions, touch-only play, headphone/speaker audio, interruptions, save recovery and safe areas. Record frame pacing and thermal behavior. The Windows GPU audit is not a mobile performance guarantee.

Capture player observations for each level: whether the objective is understood, completion time, retries, accidental inputs, frustrating misses and whether another attempt feels appealing. Adjust the existing five levels before expanding the campaign.

## Physics choice

The chamber uses 3D meshes with gameplay constrained to a plane. Ring rotation is visual; a shallow capsule handles ring/wall/baffle collisions. Peg threading uses a descending-crossing rule and assisted settling into capacity-limited slots. It does not simulate full fluid dynamics or unrestricted torus/peg contact. These choices keep the controls consistent and the simulation inexpensive.

## Native feedback references

The Android adapter uses platform [haptic feedback constants](https://developer.android.com/reference/android/view/HapticFeedbackConstants). The iOS plugin uses [impact feedback generators](https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator) and notification feedback. The device can decline haptic feedback; gameplay does not rely on vibration.
