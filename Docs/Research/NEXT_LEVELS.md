# Pocket Toys: weights, small rings and changing environments

8 October 2026. **Source prototypes implemented; builds and gameplay testing deferred at the owner's request.** This is the current expansion plan and implementation record. The earlier [ring repair](RING_DESIGN.md) and its 46 passing checks describe the preceding revision, not verification of this expansion.

## Design direction

Each new stage changes the player's decision. Weight changes the response to a pump. Small rings make guiding a group the goal. Ice creates a deadline. Lava provides an extra source of lift. Fish temporarily change a ring's path. Teach these separately before combining them, and keep the controls consistent: pump, steer, release to settle.

The owner chose a hard deadline for ice: **finish before the tank freezes; expiry ends the attempt**. It is not a freeze–thaw cycle. The level list labels the timed challenge before entry, and the countdown begins when active play starts. Pausing or backgrounding pauses it. Finishing the goal before expiry wins even if the completion animation has not finished.

The provisional audience remains people seeking tactile casual play, with a growing mastery component. The timed ice stage deliberately introduces pressure; audience fit and the appropriate deadline need observation. No new audience research or successful player trials are claimed here.

## Implemented stage sequence

The existing five layouts and IDs remain the introduction. Six stages follow, bringing the source campaign to eleven. The legacy asset filename `FiveAdventures.asset` is retained to preserve its Unity references.

| Stage | Goal and setup | What the player learns |
| --- | --- | --- |
| 06 — Feather and Pebble | One mint light ring, one violet heavy ring; two pegs at different heights. No deadline. | Compare lift and settling, then choose which ring to finish first. A ring's weight does not restrict it to a particular peg. |
| 07 — Bubble Garden | Eight small gold rings; collect any six in two open trays. Each tray holds four. | Steer a group through the middle and into a broad tray. The last two rings are not mandatory. |
| 08 — First Frost | Two standard rings on two pegs; 60-second deadline. | Plan an efficient route under clearly displayed time pressure. |
| 09 — Lava Lift | Two standard rings and two raised pegs; left and right thermal currents alternate. No deadline. | Use a current for lift and a resting phase for landing. |
| 10 — Passing Company | Two standard rings; one fish crosses at a time. No deadline. | Read an entry warning and choose whether to wait, lift or steer around the crossing. |
| 11 — Reef Rhythm | Light, standard and heavy rings, three pegs, a baffle and fish. No deadline. | Combine weight, route choice and the timing of a crossing without also adding a timer. |

The level menu now has pages of five entries. Completing a stage still earns at least one star and unlocks the next. Previous IDs, earned stars, shell unlocks and settings are retained.

## Ring identity and feel

| Ring | Appearance | Lift / settling / steering relative to standard | Role |
| --- | --- | --- | --- |
| Standard | Coral, two dark marks | 1.00 / 1.00 / 1.00 | Reference behaviour; all introductory rings use this type. |
| Light | Mint, one dark mark | 1.20 / 0.75 / 1.15 | Rises readily, stays aloft longer, responds quickly. |
| Heavy | Violet, three dark marks | 0.82 / 1.25 / 0.85 | Needs more lift, settles sooner, resists a fish nudge more. |
| Mini | Gold, visibly smaller, one dark mark | 1.30 / 0.80 / 1.30 | Agile group play; stronger damping and a lower speed cap keep “agile” from meaning unlimited speed. |

The profiles also set distinct collision masses and nudge responses. Forces explicitly use the response multipliers: changing only mass would cancel out in the original mass-scaled pump and gravity code. The current values are tuning hypotheses, not final balance.

Mini rings are 68% of the standard size. Their visible geometry and physical opening/thickness scale together. All types retain the new flat molded shape. Palette assignments are now semantic: the first five levels no longer give identical rings unrelated decorative colours. Marks and the help legend reinforce colour; their visibility on a small phone remains a required review item.

## Ice: a deadline with a clear end

- A persistent in-tank label displays seconds remaining. Frost grows along the background edges while leaving the rings and targets in front.
- The prototype does not secretly slow the controls as the deadline approaches. The timer itself is the new challenge.
- At zero, the attempt freezes and the pumps stop. A dedicated screen offers Try again, Choose a level and Home. It states that previously earned progress is safe.
- Retry resets the attempt and timer. Expiry does not award a completion, erase earlier stars or automatically restart the player.
- The first prototype uses 60 seconds. Do not treat this as a validated difficulty target.

## Lava: timed help from the environment

This is a stylized molten toy chamber with a dark basalt background and amber currents. It is a lift-and-timing mechanic; rings are not randomly destroyed by touching the background.

One side cycles through a 1.5-second warning, 2.5 seconds of upward current, then a rest. The next cycle uses the other side. Each side's cycle lasts 7.5 seconds. Both the world effect and a text label indicate the active side and phase. The current occupies a bounded area around the corresponding nozzle and fades near the ceiling.

The same current affects a light ring more than a heavy ring. Pumps remain usable. A locked ring stays locked. Pausing stops the cycle. Reduced motion removes decorative flow animation while preserving the essential side/phase cue and the actual gameplay effect.

## Fish: variable, readable crossings

- One fish at a time, on authored heights that remain within the arena.
- Direction and lane vary through a local seeded sequence. Restarting repeats the sequence, so a retry can build on what the player learned. This does not alter random state elsewhere in the game.
- A 1.4-second entry warning appears at the relevant edge, alongside a text cue. It precedes the visible crossing.
- A swept contact check gives a loose ring a small sideways/upward impulse when the fish reaches its rim area. One ring receives at most one nudge per crossing; a fish does not repeatedly buzz a ring through overlapping frames.
- Locked rings and collected rings are protected. Pausing stops the fish and warning clock. Reduced motion retains the fish's essential crossing movement.
- Fish swim visually in front of the fixed pegs. The prototype uses a simple 3D toy-fish silhouette and an approximate contact envelope, which need rendered and device review.

No reef fish are placed in lava stages.

## Group collection and feedback

The two trays are open at the top, with solid floors and side walls. A ring counts only after coming to rest inside the tray, supported by its floor or a previously collected ring. Touching an outside edge does not intentionally count. Captured rings retain colliders and cannot be knocked out. Each tray has a capacity of four, so six required catches involve both sides.

The counter reads the level's required total, rather than always requiring every spawned ring. Simultaneous catches share a restrained landing sound and existing haptic priority/coalescing. Pump, steering, mute and reduced-motion choices remain available.

## Next level concepts, not yet authored

| Concept | New decision | Keep it distinct |
| --- | --- | --- |
| Frost Crossing | One light and one heavy ring, a low obstacle and a longer ice deadline. Decide the order before spending pumps. | Combine two learned mechanics; do not shorten the timer and narrow every route at once. |
| Thermal Staircase | Two staggered lift zones help reach a high target in stages. | Change the route through space rather than just increasing the number of rings. Requires spatially authored currents beyond the current nozzle pair. |
| Passing Window | A slow moving peg and a fish crossing create alternating safe landing windows. | Keep both cycles readable and leave a recovery route. Add only after fish timing feels fair. |
| Quiet Shoal | Mini rings drift toward collectors through one slow current. | Focus on steering a group; avoid a deadline and the requirement to hunt the last tiny ring. |

Do not add all combinations to fill a level count. First ask whether each creates a different plan that the player can understand and repeat.

## Deferred verification

This session performed C# syntax parsing and source/data review. **It did not run Unity compilation, EditMode/PlayMode suites, rendering, device tests or build a new IPA/APK.** No claim is made that the new campaign is already winnable, visually approved or comfortable on a phone.

Before delivery, the next verification pass must cover:

1. Import and compile the new scripts, metadata, campaign and shader in Unity. Verify all eleven serialized lessons and the fresh-project content recipe agree.
2. Re-run the existing regression suites. The content-count assertion now expects eleven. Existing control-only journey coverage still covers the original five. The standalone audit explicitly reports that scope, rather than claiming to validate the full expansion.
3. Add/run checks for type-specific lift/settling, mini geometry, tray support and capacity, quota completion, a final catch just before expiry, timer expiry, pause/background/resume, retry, and saved progress surviving failure.
4. Verify lava warning/active/rest transitions, pause behavior and ceiling containment. Verify fish warning, crossing contact, one nudge per pass, protected catches, restart repeatability and pause behavior.
5. Render short/tall phones, all three level pages, help, the frost failure screen, all biomes and fish cues. Review text fit, marker readability, ring contrast and tray openings.
6. Play every new stage with touch and tilt, without an automated controller deciding the route. Judge whether a player understands what changed, can recover from misses and wants another attempt. Tune the 60-second deadline from those observations.
7. Profile the eight-ring scene on lower-powered phones. It currently has up to 256 ring contact segments, twice the four-ring repair scene. Profile CPU physics, GPU shader cost and frame pacing before deciding whether it is acceptable.
8. Bump the release version/build numbers, then create and install fresh packages. The previously delivered 0.2.4 files contain none of these expansion changes or the later flat-ring repair.

The implementation is saved locally for continued changes. No cloud build, purchase, merge or publication was triggered.
