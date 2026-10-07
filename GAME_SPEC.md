# Pocket Toys — Master Game & Development Spec

> **Current build scope — 7 October 2026:** The user approved **five polished levels with all core systems** as one integrated build. This overrides the earlier 30–50-level content target for the current delivery. The larger campaign remains a future expansion. The user subsequently authorised pushing the source project to `https://github.com/mandargames/bubleup`, superseding the original local-only/no-Git requirement. Keep generated builds, caches, backups, player saves, and signing credentials outside source control. The current implementation and validation status are recorded in `CHANGELOG_DEV.md` and `Docs/RELEASE_READINESS.md`.

> **Status:** Single source of truth for product design, MVP scope, technical architecture, level/content rules, monetization principles, analytics, milestones, and Codex development instructions.
>
> **Working title:** Pocket Toys  
> **Primary platforms:** iOS + Android  
> **Recommended engine:** Unity  
> **MVP:** Water Ring Toss  
> **Second toy:** Steel Ball Maze / Ball-in-Hole

> **MVP DECISION**  
> Build one excellent toy first: Water Ring Toss. Prove that tilting, pumping, haptics, physics and short-session retention feel satisfying. Then add Steel Ball Maze using the same shared game shell.

| **Working title**        | Pocket Toys                                   |
|--------------------------|-----------------------------------------------|
| **Primary platform**     | iOS + Android                                 |
| **Recommended engine**   | Unity (2D/3D physics + mobile sensor support) |
| **Initial monetization** | Rewarded ads + optional remove-ads purchase   |

# 1. Executive Summary

Pocket Toys is a casual mobile game that recreates the tactile pleasure of inexpensive childhood pocket toys using phone motion sensors, physics, haptics and satisfying audio. Instead of building one isolated novelty app, the product is designed as a reusable “toy cabinet”: each toy is a compact skill game with short sessions, simple controls and a strong physical feel.

> **Core opportunity**  
> The differentiator is not “another maze” or “another ring game.” It is the combination of nostalgia + convincing physical interaction + collectible toy presentation + a growing library of familiar pocket toys.

## Product thesis

- Players understand the objective within seconds without a tutorial-heavy onboarding.

- Motion controls and haptics make the phone itself feel like the physical toy.

- Each toy supports 15–90 second sessions, making it suitable for casual repeat play.

- A shared shell lets one developer add new toys without rebuilding progression, ads, save data, analytics or UI.

- The first release should prioritize feel, replayability and store creatives over feature breadth.

## Initial release recommendation

| **Area**             | **Decision**                                                                       |
|----------------------|------------------------------------------------------------------------------------|
| MVP toy              | Water Ring Toss                                                                    |
| Second toy           | Steel Ball Maze / Ball-in-Hole                                                     |
| Orientation          | Portrait first                                                                     |
| Controls             | Phone tilt + on-screen pump buttons; touch fallback for accessibility/testing      |
| Session design       | Very short rounds with instant restart                                             |
| Progression          | Stars, toy unlocks, cosmetic variants, daily challenge                             |
| Commercial model     | Free-to-play; conservative interstitials, rewarded retries/bonuses, remove-ads IAP |
| Development strategy | Ship a highly polished vertical slice before building a large toy catalog          |

# 2. Audience, Positioning & Design Pillars

## Target audience

- Primary: adults and older teens who remember small physical skill toys and enjoy relaxing casual games.

- Secondary: puzzle/skill-game players who like precision, physics and short levels.

- Broad appeal: visual clarity, low reading requirement, offline-friendly core play and one-handed portrait sessions.

## Positioning statement

> **One-line pitch**  
> Your childhood pocket toys, rebuilt inside your phone — tilt, pump, roll and land every piece with satisfying physics and haptics.

## Design pillars

| **Pillar**             | **Meaning in the product**                                           |
|------------------------|----------------------------------------------------------------------|
| Tactile                | Every input should produce visible motion, sound or haptic response. |
| Immediate              | The player should be playing within 5–10 seconds of launch.          |
| Skillful but forgiving | Physics rewards mastery, while quick retries prevent frustration.    |
| Nostalgic, not dated   | Retro toy inspiration with modern lighting, UI and polish.           |
| Expandable             | Every new toy plugs into the same shell, economy and progression.    |

# 3. Core Game Loop

1.  Choose a toy or current level.

2.  Play a 15–90 second physics challenge.

3.  Earn stars/score based on completion, speed, precision or efficiency.

4.  Retry instantly or continue to the next challenge.

5.  Unlock levels, toy variants and eventually additional toys.

## Session loop

| **Moment** | **Player experience**                       | **Design requirement**               |
|------------|---------------------------------------------|--------------------------------------|
| Open app   | See current toy and “Continue” immediately  | Avoid menu friction                  |
| Start      | Physics activates instantly                 | No long countdown                    |
| Manipulate | Tilt/pump/tap and observe physical response | Low latency, stable sensor filtering |
| Success    | Strong visual/audio/haptic payoff           | Celebration under 2 seconds          |
| Next       | Next level or quick retry                   | One tap maximum                      |

# 4. Toy \#1 — Water Ring Toss (MVP)

This is the launch toy and should be the benchmark for the entire product. It recreates the handheld water toy where button presses create upward water jets that push floating rings toward pegs.

## Core mechanics

- Two pump buttons at the bottom left/right generate localized upward impulses.

- Rings are affected by buoyancy, drag, turbulence, collisions and gravity-like settling.

- Device tilt influences horizontal drift and/or the whole toy orientation.

- Goal: land a target number of rings on one or more pegs before a move/time constraint ends.

- Instant restart preserves the “one more try” feeling.

## MVP level variables

| **Variable**    | **Examples**                                                    |
|-----------------|-----------------------------------------------------------------|
| Ring count      | 3, 5, 8, 12                                                     |
| Peg count       | 1–5 pegs                                                        |
| Peg position    | Centered, offset, moving, staggered heights                     |
| Ring properties | Size, mass, buoyancy, drag                                      |
| Pump strength   | Fixed in early levels; asymmetric later                         |
| Obstacles       | Baffles, arches, gates, bumpers                                 |
| Win condition   | All rings, exact peg distribution, score threshold, time target |

## Feel targets

> **Non-negotiable**  
> The rings must feel “alive” but controllable. Purely realistic fluid simulation is unnecessary; use authored forces and tuned damping to create the illusion of water while keeping outcomes learnable.

- Pump press: short, crisp haptic + bubble/jet sound + visible plume.

- Ring-to-peg contact: distinct tick/plop sound and light haptic.

- Successful landing: brief magnetic-like settling assistance may be used invisibly to reduce frustrating near-misses.

- Physics should remain deterministic enough that skilled players can improve.

# 5. Toy \#2 — Steel Ball Maze / Ball-in-Hole

The second toy reuses device tilt, physics, level progression and feedback systems while introducing a very different challenge. The player tilts the phone to roll one or more metal balls into target holes or through a compact maze.

## Core variants

- Classic: roll all balls into matching holes.

- Maze: reach the finish without falling into hazard holes.

- Order puzzle: fill numbered holes in sequence.

- Multi-ball: place several balls while previously placed balls can escape if the board tilts too far.

- Precision challenge: complete within a tilt-angle or time limit.

## Shared systems reused from Toy \#1

| **Shared system** | **Reuse**                                            |
|-------------------|------------------------------------------------------|
| Sensor input      | Same filtered accelerometer/gyroscope abstraction    |
| Physics tuning    | Common fixed timestep and mobile performance targets |
| Haptics/audio     | Shared feedback service                              |
| Level flow        | Same restart, success, score and next-level UI       |
| Save/progression  | Same stars/unlocks model                             |
| Analytics         | Same session and completion event structure          |

# 6. Future Toy Library

Do not implement these in the MVP. They exist to validate the platform architecture and future content strategy.

| **Toy concept**    | **Primary interaction** | **Why it fits**                               |
|--------------------|-------------------------|-----------------------------------------------|
| Magnetic bead maze | Drag magnet / tilt      | Strong tactile illusion and simple objectives |
| Cup-and-ball       | Flick / swing timing    | Short mastery loop; highly visual             |
| Pocket pinball     | Tap flippers / tilt     | Familiar physics and scoring                  |
| Marble labyrinth   | Tilt                    | Direct reuse of sensor system                 |
| Balance stack      | Tilt / tap              | Physics spectacle and short rounds            |
| Water basketball   | Pump buttons            | Reuses water-jet system with new goals        |

# 7. Progression & Retention

## Launch progression

- Level map inside each toy: 30–50 handcrafted/tuned challenges for MVP.

- 1–3 stars per level based on completion quality.

- Star gates unlock later level packs and cosmetic toy shells.

- Daily challenge selects one authored seed/configuration and gives a small cosmetic/progression reward.

- No energy system in MVP. Friction should come from challenge, not waiting.

## Longer-term retention

| **Feature**        | **When to add**           | **Purpose**                                           |
|--------------------|---------------------------|-------------------------------------------------------|
| Daily challenge    | MVP or shortly after      | Habit and lightweight return trigger                  |
| Toy skins          | After core feel is proven | Collection and monetization                           |
| Streaks            | Post-MVP                  | Return motivation; avoid punitive loss                |
| Weekly skill event | Post-MVP                  | Competitive score chase without real-time multiplayer |
| New toy drops      | Growth phase              | Major retention/marketing beats                       |

# 8. Monetization Principles

The game should first prove retention and satisfying play. Aggressive ads can destroy the tactile/nostalgic appeal, so monetization should be designed around natural breaks.

| **Mechanic**     | **Recommendation**                                                             |
|------------------|--------------------------------------------------------------------------------|
| Interstitial ads | Only after several completed/failed rounds; never interrupt active physics.    |
| Rewarded ads     | Optional extra retry, bonus stars, cosmetic currency or daily challenge bonus. |
| Remove ads       | One-time IAP, clearly presented.                                               |
| Cosmetics        | Toy shells, backgrounds, ring/ball styles; no pay-to-win benefit.              |
| Subscription     | Not recommended for MVP.                                                       |

> **Commercial rule**  
> Do not optimize ad frequency before D1 retention and average session quality are acceptable. A polished tactile game with weaker monetization is more fixable than a high-ad game players uninstall immediately.

# 9. UX, Art & Audio Direction

## Visual direction

- Toy-like 3D or 2.5D presentation with rounded plastic, clear acrylic, bright but controlled colors and soft studio lighting.

- The active toy dominates the screen; UI should feel secondary and lightweight.

- Avoid copying identifiable commercial toy brands or exact product artwork.

- Use strong silhouettes and readable physics objects for small screens.

## Audio direction

- Close-mic tactile sounds: plastic clicks, water pumps, bubbles, metal rolling and soft impacts.

- Music optional or minimal; the physical sounds should carry the experience.

- Provide independent toggles for music, sound and haptics.

## Accessibility / control options

- Sensor calibration on first use and from Settings.

- Tilt sensitivity slider.

- Touch/virtual-tilt fallback so the game remains playable without motion controls.

- Left/right pump button placement customization if needed after testing.

- Reduce-motion option for camera/background movement.

# 10. Recommended Technical Architecture

Unity is the recommended starting point because the project needs mobile sensor input, stable 2D/3D physics, haptics integrations, ad/IAP SDKs and rapid iteration. Godot is viable, but Unity reduces integration risk for a commercial mobile MVP.

## Architecture principle

> **Build a toy platform, not a one-off scene**  
> The app shell owns progression, analytics, ads, settings, save data and scene flow. Each toy implements a small common interface so new toys can be added independently.

| **Layer**      | **Responsibilities**                                                        |
|----------------|-----------------------------------------------------------------------------|
| App Shell      | Boot, navigation, save/load, settings, remote config hooks, consent.        |
| Services       | Audio, haptics, ads, IAP, analytics, sensor input, time/daily challenge.    |
| Game Framework | Level lifecycle, pause/restart, scoring, star evaluation, win/fail.         |
| Toy Module     | Toy-specific controller, physics objects, level rules and presentation.     |
| Content/Data   | ScriptableObjects/JSON for toy definitions, levels, difficulty and rewards. |

## Suggested Unity project layout

Assets/  
\_Game/  
Core/  
App/  
Services/  
Input/  
Progression/  
UI/  
Toys/  
WaterRingToss/  
Runtime/  
Physics/  
Levels/  
UI/  
SteelBallMaze/  
Runtime/  
Physics/  
Levels/  
UI/  
Content/  
Shared/  
Audio/  
Materials/  
Tests/  
EditMode/  
PlayMode/

# 11. Key Systems for Codex to Implement

| **System**         | **MVP requirement**                                                                                |
|--------------------|----------------------------------------------------------------------------------------------------|
| SensorInputService | Read acceleration/attitude, calibrate neutral pose, low-pass filter noise, expose normalized tilt. |
| HapticsService     | Simple semantic calls: LightTap, MediumImpact, Success, Warning.                                   |
| AudioService       | Pooled one-shots + volume settings; no direct AudioSource calls from gameplay logic.               |
| LevelManager       | Start, restart, pause, success, fail, next level; emits events.                                    |
| SaveService        | Local JSON or PlayerPrefs-backed structured save for settings/progression; versioned schema.       |
| ToyRegistry        | Maps toy IDs to scenes/content metadata.                                                           |
| Scoring            | Toy-specific score provider feeding shared star thresholds.                                        |
| AnalyticsFacade    | Event abstraction so gameplay code is not coupled to a vendor SDK.                                 |

## Sensor processing requirements

- Sample sensor input independently from rendering; smooth sudden jitter with configurable low-pass filtering.

- Expose calibration so “flat” can match how the player naturally holds the phone.

- Clamp extreme tilt values and provide dead zones near neutral.

- In the Unity Editor, provide keyboard/mouse simulation so development does not require deploying to a phone for every test.

# 12. Content & Level Data Model

Levels should be data-driven. Codex should avoid hard-coding level parameters inside scene scripts.

| **Field**                | **Example**                  |
|--------------------------|------------------------------|
| levelId                  | water_001                    |
| toyId                    | water_ring_toss              |
| objectiveType            | LandAllRings                 |
| timeLimitSeconds         | 0 for untimed / 45 for timed |
| ringCount                | 5                            |
| pegLayoutId              | center_triple                |
| pumpStrengthLeft / Right | 1.0 / 1.0                    |
| difficultyTier           | 1–5                          |
| starThresholds           | Completion / 35 sec / 20 sec |

# 13. Analytics Plan

Analytics should answer whether the toy is fun, where difficulty spikes occur, and whether ads damage retention. Keep events small and intentional.

| **Event**             | **Key properties**                         |
|-----------------------|--------------------------------------------|
| app_session_start     | app_version, platform, days_since_install  |
| toy_opened            | toy_id                                     |
| level_started         | toy_id, level_id, attempt_number           |
| level_completed       | toy_id, level_id, duration, stars, retries |
| level_failed          | toy_id, level_id, duration, fail_reason    |
| sensor_calibrated     | toy_id, sensitivity                        |
| rewarded_offer_shown  | placement                                  |
| rewarded_ad_completed | placement, reward                          |
| interstitial_shown    | placement, levels_since_last_ad            |
| iap_completed         | product_id                                 |

## Metrics to watch

- Tutorial/first-level completion rate.

- Median retries per level and completion time distribution.

- D1 and D7 retention once enough users exist.

- Average levels/session and sessions/day.

- Ad impressions/session and retention split by ad exposure.

- Crash-free sessions and performance by device tier.

# 14. MVP Scope

| **In MVP**                               | **Not in MVP**                                 |
|------------------------------------------|------------------------------------------------|
| Water Ring Toss with polished physics    | Large multi-toy catalog                        |
| 30–50 levels                             | Real-time multiplayer                          |
| Tilt + pump controls                     | Account system / cloud sync                    |
| Calibration + touch fallback             | Complex currencies                             |
| Stars + basic unlock progression         | Subscription                                   |
| Sound + haptics + settings               | UGC level editor                               |
| Local save                               | Live events platform                           |
| Basic analytics abstraction              | Heavy backend infrastructure                   |
| Rewarded + conservative interstitial ads | Advanced social features                       |
| Remove-ads IAP                           | Competitive leaderboards unless trivial to add |

# 15. Development Milestones

| **Phase**                  | **Deliverable**                                 | **Exit criterion**                                          |
|----------------------------|-------------------------------------------------|-------------------------------------------------------------|
| 0\. Technical spike        | Sensor + one ring + one peg + pump force        | Feels controllable on a real phone.                         |
| 1\. Vertical slice         | One polished level with final-ish feedback      | A new player understands and enjoys it without explanation. |
| 2\. Core framework         | Level flow, save, settings, data-driven content | Can add a new level without changing code.                  |
| 3\. Content pass           | 30–50 levels + difficulty tuning                | No obvious difficulty cliffs; restart loop is fast.         |
| 4\. Monetization/analytics | Ads/IAP facade + key events                     | No ad can interrupt active gameplay.                        |
| 5\. Store-ready beta       | Performance, consent, QA, icons/screens         | Stable on representative low/mid/high devices.              |
| 6\. Second toy prototype   | Steel Ball Maze using shared shell              | Proves platform architecture is reusable.                   |

# 16. MVP Definition of Done

- Cold launch to playable level in under a few seconds on a mid-range device after initial load.

- Water rings respond consistently to both pump buttons and device tilt.

- Sensor calibration is reliable across common holding angles.

- Thirty or more playable levels are data-driven and can be edited without code changes.

- Restart is near-instant and requires one tap or less.

- Core audio/haptics can be disabled independently.

- Progress survives app restart and save schema is versioned.

- No ad appears during an active attempt.

- Game remains playable with touch fallback when motion input is unavailable.

- Basic automated tests cover save serialization, star evaluation and level-data validation.

- No known blocker crashes; acceptable performance on target Android/iOS device tiers.

# 17. Major Risks & Mitigations

| **Risk**                       | **Mitigation**                                                                                                  |
|--------------------------------|-----------------------------------------------------------------------------------------------------------------|
| Physics feels random           | Favor authored forces and damping over physically pure simulation; add subtle assist near success states.       |
| Motion sickness / awkward tilt | Keep camera stable; move the board/forces rather than the entire world; offer touch fallback.                   |
| Novelty wears off              | Use progression, harder configurations and multiple toy types rather than stretching one mechanic indefinitely. |
| Too many systems before fun    | Technical spike first; do not integrate monetization/backends until one level feels excellent.                  |
| Solo-dev content bottleneck    | Data-driven level tools, reusable prefabs and parameterized obstacles.                                          |
| Copycat/IP risk                | Use generic toy concepts and original visual design, names, assets and level layouts.                           |

# 18. Codex Handoff

This section incorporates the former standalone Codex handoff. The previous DOCX and separate handoff file are now superseded by this master Markdown specification.

When development begins, place this file in the repository root as the authoritative product and implementation context. Give Codex one narrow milestone at a time and require it to keep the architecture modular. Do not maintain a second competing spec.

## Recommended first Codex task

> **Phase 0 prompt**  
> Create the Unity project foundation for Pocket Toys. Implement only the technical spike: a sensor-input abstraction with editor simulation, a simple Water Ring Toss scene containing one ring and one peg, and two pump inputs that apply tunable forces. Add calibration, basic haptic/audio interfaces with no vendor dependency, and a minimal play-mode test setup. Do not add ads, progression, menus or production art yet. Keep toy-specific code isolated under Toys/WaterRingToss and shared services under Core/. Document how to run the scene on-device and in the Editor.

## Rules for Codex

- Implement one milestone at a time; avoid speculative systems not required by the current phase.

- Prefer small components and interfaces over monolithic managers.

- Keep third-party SDKs behind facades so they can be swapped later.

- Write tests for deterministic/data logic; use play-mode tests only where engine behavior is required.

- Never hard-code level content that belongs in ScriptableObjects/JSON/configuration.

- Before large refactors, explain the reason and affected files in the task summary.

- Maintain a CHANGELOG_DEV.md containing milestone-level changes and known issues.

# 19. Decisions to Validate During Prototype

| **Question**                    | **Prototype decision method**                                                                       |
|---------------------------------|-----------------------------------------------------------------------------------------------------|
| 2D vs 3D presentation?          | Choose whichever gives better physical feel with lower tuning complexity; test both only if needed. |
| Accelerometer vs attitude/gyro? | Compare stability across devices; use a normalized abstraction so implementation can change.        |
| How strong should assist be?    | A/B internally: no assist vs subtle peg capture near valid landing.                                 |
| Portrait vs landscape?          | Start portrait; change only if toy ergonomics clearly demand landscape.                             |
| Real fluid simulation?          | Avoid unless cheap and stable; visual fluid effect can be separate from gameplay forces.            |
| Exact ad cadence?               | Do not decide until retention/session data exists.                                                  |

# 20. Prototype Review Checklist

- Can a new player understand the goal from watching 3 seconds of gameplay?

- Does tilting the phone feel natural rather than noisy or delayed?

- Do pumps feel meaningfully different based on timing and side?

- Are near-misses motivating rather than frustrating?

- Can a player restart fast enough to attempt again immediately?

- Does the game still feel satisfying with music off?

- Does the toy look distinctive in a 1–2 second store-ad clip?

- Can a new level be created by data/configuration rather than code changes?

- Does the architecture support Steel Ball Maze without touching unrelated systems?

# Appendix A — One-Page Build Brief

> **Vision**  
> A polished mobile “toy cabinet” that recreates nostalgic physical skill toys with motion controls, physics, haptics and short repeatable challenges.

| **Item**                  | **Current decision**                                          |
|---------------------------|---------------------------------------------------------------|
| Working title             | Pocket Toys                                                   |
| MVP                       | Water Ring Toss                                               |
| Next toy                  | Steel Ball Maze                                               |
| Engine                    | Unity                                                         |
| Orientation               | Portrait                                                      |
| Controls                  | Tilt + on-screen pumps; touch fallback                        |
| Content target            | 30–50 Water Ring Toss levels                                  |
| Progression               | Stars + level/toy unlocks                                     |
| Monetization              | Rewarded ads, conservative interstitials, remove-ads IAP      |
| Architecture              | Shared app shell/services + modular Toy modules               |
| First implementation goal | Sensor + one ring + one peg + two pump forces on a real phone |
