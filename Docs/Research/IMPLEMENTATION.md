# Pocket Toys: baseline implementation

8 October 2026. Implementation candidate following the owner's instruction to proceed. The [research baseline](POCKET_TOYS_BASELINE.md) remains a historical record of the research stage. This document records subsequent decisions and validation; its implementation status supersedes the baseline's closing statement that no implementation has started.

## Intended player and experience

The working audience is adults who want a brief, tactile, low-pressure break. Nostalgia for handheld water toys and the pleasure of getting better are secondary motivations. This is a provisional audience hypothesis, not a demographic finding or a claim that the game treats stress or any medical condition.

The experience promise is a small physical-feeling toy: a press lifts nearby rings, a deliberate steering action guides them, and a stable landing provides a clear reward. Finishing is the main goal. There is no countdown failure or requirement to improve a score to continue.

## Decisions implemented

| Area | Decision | Reason and boundary |
| --- | --- | --- |
| Composition | Fixed 5.6 × 7.2 world-unit chamber; camera fits the intact toy between the safe-area header and steering space. | The previous tall-phone change stretched the chamber and enlarged empty water without moving the gameplay. A bounded composition keeps ring and peg proportions consistent. It intentionally does not fill every vertical pixel with water. |
| Graphics | Retain the coral art, enamel casing, ring geometry and palette; narrow the surrounding shell, keep text outside the water. | Preserve a coherent art direction and improve relative ring size. New artwork is not required to fix composition. Floor/background contrast still needs phone observation. |
| Goal | Persistent landed-ring count, level title and one contextual instruction. | The objective is visible without a running timer competing for attention. A first-level hint changes after the player's first pump. |
| Controls | Retain local vertical pumps plus independent steering. Tilt remains available; touch steering works with the phone resting still. | This is the existing control model, clarified rather than silently replaced with untested directional jets. Touch supports pumping and then steering with one finger; simultaneous two-thumb play is also available. |
| Reach | Settings has a compact visible icon with a larger touch area. Primary UI controls have at least 88 reference pixels of height at a 720-pixel reference width. | At a simulated 360-point phone width this corresponds to 44 points. This is an engineering target, not proof of accessibility on every display configuration. |
| Input lifetime | One pump per press; release permits the next press. Steering centers on release and when its UI is disabled. Screen transitions clear steering. | Prevent duplicate input and steering that continues after pause or menu changes. |
| Help | Pause → How to play explains local lift, steering, descending over the tip and resting on the shelf. | Instructions can be revisited without restarting or advancing the timer. |
| Comfort | Separate controls/calibration screen; independent effects, music, haptics and reduced motion. | Give calibration room and keep controls legible. Existing saved preferences are retained. |
| Progression | Every completion receives the same main celebration. Time, pump count, earned stars and efficiency goals are available through View results & stars. | Completion should feel successful. Existing star records, bests and shell unlocks remain compatible. |
| Mistakes | Restarting during an attempt asks whether to keep the attempt or restart. | Prevent an accidental tap from losing a near-complete level. Completed progress is never cleared by this action. |
| Audio | Softer, lower bubble resonances; bounded pump voices; distinct landing and completion sounds. | Reduce repeated sensory intensity without removing feedback. Generated sound is a candidate pending actual speaker/headphone listening. |
| Haptics | Coalesce same-frame events; completion outranks landing, landing outranks pump. A stronger event can bypass the brief weak-event cooldown. Softer iPhone pump and landing impacts. | Avoid collision buzz and avoid suppressing a meaningful event just because a pump occurred first. Device hardware and system settings determine actual tactile output. |

## Deliberately retained

The five authored levels, ring capture rules, physically supported stacks, moving peg, earned progress and saved calibration remain. The earlier iPhone startup fix is preserved: decorative backdrop geometry is created explicitly without relying on an implicit MeshCollider that IL2CPP can strip.

This pass does not add purchases, advertising, online accounts, notifications, engagement streaks, new levels, or analytics uploads. The existing diagnostic log remains optional and local. Research themes are not a mandate to expand scope indiscriminately.

## Verification record

Validation is recorded below after the candidate is built. Automated reachability establishes that a controller can complete the game through its public pump and steering inputs. It does not establish that a new player understands the controls or enjoys the difficulty.

- Unity EditMode: 17 passed, including progress/settings reload, backup recovery, future-save protection and haptic priority/coalescing.
- Unity PlayMode: 26 passed. Public-control automation completed all five levels. Includes release/pause/help/restart input lifetime, bounded pump audio, mute, supported stacks and physics regressions.
- Portrait renders inspected at 360 × 800, 390 × 844 and 375 × 667 with simulated safe-area insets. Gameplay hit targets stay inside those safe areas and are at least 44 simulated points high. Water proportions remain constant. Also inspected Home, help, settings, controls, completion and optional results at 720 × 1280.
- Windows 0.2.4 build: succeeded with Unity 6000.3.9f1. Packaged five-level audit is tracked below when complete.
- Native iPhone compilation: pending build.
- Physical iPhone installation, speakers, touch, tilt and haptics: not verified by this implementation session.

The first gameplay regression run exposed an existing overly tight stack-height acceptance range. A stable threaded ring resting against a neighboring loose ring could sit above that range and never score. The limit now uses the torus's projected vertical diameter while retaining physical support, low-speed settling and peg-capacity requirements. A targeted offset-stack regression and the complete five-level journey pass after this fix. Automated completion times are not human difficulty benchmarks.

## Phone review procedure

1. Launch the new version after an older saved installation. Confirm earned stars, unlocked shells and settings survive.
2. Start First Splash. Before coaching, note what the player thinks each pump does and where they expect the ring to land. Check that the hint is readable above the toy.
3. Complete an attempt with tilt, then disable Phone tilt and repeat with the phone resting on a table. Try one finger and two thumbs. Release steering, open settings mid-motion, return, and check for unwanted input.
4. Confirm every menu clears the notch and home indicator. Check the smallest available iPhone as well as a tall model, including the last level with all four rings.
5. Pause, read help, cancel restart, resume, background the app and return. Check the attempt and timer stay paused while outside gameplay. Complete, inspect optional results and verify the next level opens.
6. Compare gentle pump audio at comfortable phone and headphone volumes. Try alternating pumps for two minutes, then an optional ten-minute play session. Record harshness, repetition and fatigue separately; lower measured amplitude does not prove a pleasant sound.
7. Check pump, landing and completion haptics individually and during rapid input. Check every event with haptics disabled. No collision should vibrate. Repeat with sound off and reduced motion on; essential play must remain understandable.
8. Record launch failures, wrong-size UI, input delay, stuck controls and save failures as defects. Record confusion and sensory discomfort as usability findings, with device and circumstances. Do not replace these observations with an automated test score.

The baseline's proposed 6–8-player rounds remain future human research. No participants or favorable reactions are invented in this implementation record. Broader audience fit, retention and commercial demand remain unmeasured.

## Technical references

- [Apple game controls](https://developer.apple.com/design/human-interface-guidelines/game-controls): reach and safe areas.
- [Apple playing haptics](https://developer.apple.com/design/human-interface-guidelines/playing-haptics): meaningful, restrained optional feedback.
- [UIKit impact intensity](https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator/impactoccurred(intensity:)): native impact API used by the iPhone adapter; queried 8 October 2026.
- See the baseline for the complete source register and the limits of the small competitor-review sample.
