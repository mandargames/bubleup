# Pocket Toys: weights, small rings and changing environments

8 October 2026. **Source prototypes implemented; builds and gameplay testing deferred at the owner's request.** This is the current expansion plan and implementation record. The earlier [ring repair](RING_DESIGN.md) and its 46 passing checks describe the preceding revision, not verification of this expansion.

## Design direction

Each new stage changes the player's decision. Weight changes the response to a pump. Small rings make guiding a group the goal. Ice creates a deadline. Lava provides an extra source of lift. Fish temporarily change a ring's path. Teach these separately before combining them, and keep the controls consistent: pump, steer, release to settle.

The owner chose a hard deadline for ice: **finish before the tank freezes; expiry ends the attempt**. It is not a freeze–thaw cycle. The level list labels the timed challenge before entry, and the countdown begins when active play starts. Pausing or backgrounding pauses it. Finishing the goal before expiry wins even if the completion animation has not finished.

The provisional audience remains people seeking tactile casual play, with a growing mastery component. The timed ice stage deliberately introduces pressure; audience fit and the appropriate deadline need observation. No new audience research or successful player trials are claimed here.

## Implemented stage sequence

The existing five layouts and IDs remain the introduction. Ten stages follow, bringing the source campaign to fifteen. Stages 12–15 implement the four follow-on concepts approved by the owner. The legacy asset filename `FiveAdventures.asset` is retained to preserve its Unity references.

| Stage | Goal and setup | What the player learns |
| --- | --- | --- |
| 06 — Feather and Pebble | One mint light ring, one violet heavy ring; two pegs at different heights. No deadline. | Compare lift and settling, then choose which ring to finish first. A ring's weight does not restrict it to a particular peg. |
| 07 — Bubble Garden | Eight small gold rings; collect any six in two open trays. Each tray holds four. | Steer a group through the middle and into a broad tray. The last two rings are not mandatory. |
| 08 — First Frost | Two standard rings on two pegs; 60-second deadline. | Plan an efficient route under clearly displayed time pressure. |
| 09 — Lava Lift | Two standard rings and two raised pegs; left and right thermal currents alternate. No deadline. | Use a current for lift and a resting phase for landing. |
| 10 — Passing Company | Two standard rings; one fish crosses at a time. No deadline. | Read an entry warning and choose whether to wait, lift or steer around the crossing. |
| 11 — Reef Rhythm | Light, standard and heavy rings, three pegs, a baffle and fish. No deadline. | Combine weight, route choice and the timing of a crossing without also adding a timer. |
| 12 — Frost Crossing | One light and one heavy ring, two different-height pegs, a low angled baffle; 75-second freeze deadline. | Choose a route and landing order using familiar weight differences. The longer deadline allows for the extra obstacle. |
| 13 — Thermal Staircase | Two standard rings, one intermediate peg and one high peg; two staggered, visibly bounded lift zones. No deadline. | Ride the lower burst, steer into the upper zone, then release during its rest. Pumps remain available for recovery. |
| 14 — Passing Window | Two standard rings, one fixed peg and one slowly drifting peg; fish cross at one predictable height. No deadline. | Watch both the peg and the warned crossing before committing to a descent. A fixed peg offers a simpler first catch. |
| 15 — Quiet Shoal | Six mini rings; collect any four in two broad trays, each holding three. One slow rightward current. No deadline. | Guide a group with and against a steady drift; both trays are needed, but the last two rings are optional. |

The level menu has three pages of five entries. Completing a stage still earns at least one star and unlocks the next. Previous IDs, earned stars, shell unlocks and settings are retained. Settings → How to play → Level tip shows the current stage's lesson, authored hint and relevant rules while gameplay is paused.

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
- First Frost uses 60 seconds; Frost Crossing allows 75 seconds for its mixed weights and low baffle. Neither deadline is a validated difficulty target.

## Lava: timed help from the environment

This is a stylized molten toy chamber with a dark basalt background and amber currents. It is a lift-and-timing mechanic; rings are not randomly destroyed by touching the background.

In Lava Lift, one side cycles through a 1.5-second warning, 2.5 seconds of upward current, then a rest. The next cycle uses the other side. Each side's cycle lasts 7.5 seconds. Both the world effect and a text label indicate the active side and phase. The current is strongest around the corresponding nozzle and fades near the ceiling. Thermal Staircase instead uses the two authored zones below; it does not also apply hidden nozzle currents.

The same current affects a light ring more than a heavy ring. Pumps remain usable. A locked ring stays locked. Pausing stops the cycle. Reduced motion removes decorative flow animation while preserving the essential side/phase cue and the actual gameplay effect.

## Fish: variable, readable crossings

- One fish at a time, on authored heights that remain within the arena.
- Direction and lane vary through a local seeded sequence. Restarting repeats the sequence, so a retry can build on what the player learned. This does not alter random state elsewhere in the game.
- A 1.4-second entry warning appears at the relevant edge, alongside a text cue. It precedes the visible crossing.
- A swept contact check gives a loose ring a small sideways/upward impulse when the fish reaches its rim area. One ring receives at most one nudge per crossing; a fish does not repeatedly buzz a ring through overlapping frames.
- Locked rings and collected rings are protected. Pausing stops the fish and warning clock. Reduced motion retains the fish's essential crossing movement.
- Fish swim visually in front of the fixed pegs. The prototype uses a simple 3D toy-fish silhouette and an approximate contact envelope, which need rendered and device review.

No reef fish are placed in lava stages. Passing Window uses one crossing height (y = 1.5), a slower speed of 1 unit/second and a nine-second nominal rest after exiting (with the existing ±15% seeded variation). Its moving peg travels ±0.65 units around x = 0.6 with a roughly 14-second period. The two rhythms need not synchronize; there is no deadline, the outer lanes remain open and a missed landing can be recovered. Whether the combined timing feels fair is still a playtest question.

## Group collection and feedback

The trays are open at the top, with solid floors and side walls. A ring counts only after coming to rest inside the tray, supported by its floor or a previously collected ring. Touching an outside edge does not intentionally count. Captured rings retain colliders and cannot be knocked out. Bubble Garden has four slots per tray and requires six catches. Quiet Shoal uses broader trays with three slots each and requires four catches. Both goals involve both sides.

The counter reads the level's required total, rather than always requiring every spawned ring. Simultaneous catches share a restrained landing sound and existing haptic priority/coalescing. Pump, steering, mute and reduced-motion choices remain available.

## Authored currents and follow-on tuning

Current zones are editable level data: a label, center, width/height, acceleration vector and either a steady flow or a warned cycle. Their force fades through the outer quarter of the rectangle and is zero outside it. The water shader receives the same bounds, direction and state used by the physics. An outline identifies the resting zone; dim stationary chevrons warn; brighter moving chevrons show active flow. Reduced motion keeps the outline, static direction chevrons, state contrast and text cue. Ring silhouettes remain in front of the water effect.

| Zone | Center / size in tank units | Peak acceleration before ring response | Timing from attempt start |
| --- | --- | --- | --- |
| Thermal Staircase — lower | (-1.65, -0.70) / (1.45, 3.20) | 5.6 upward | Warn 0–1.5s; lift 1.5–5.5s; rest to 12s; repeat. |
| Thermal Staircase — upper | (0.55, 1.55) / (2.10, 3.80) | 5.4 upward | Initially rests; warn 4.5–6s; lift 6–10s; rest to 16.5s; repeat every 12s. |
| Quiet Shoal — gentle drift | (0, 1.65) / (4.90, 2.70) | 0.48 rightward | Steady; no hidden cycle or warning delay. |

The lower and upper zones are separated horizontally, with overlapping height ranges. Steering and momentum provide the transfer, with the pumps available to help. This route is an authored hypothesis; a control-only playthrough and human tuning must establish reachability and comfort. Quiet Shoal deliberately has fewer rings than Bubble Garden, broad trays and a four-of-six quota.

Zone clocks use the same elapsed-attempt clock as the freeze timer and moving pegs. Pause/background stops them; retry starts their sequence again. Locked rings bypass environmental forces. Validation rejects more than four zones, non-finite or out-of-bounds geometry/forces, invisible zero-strength currents and cycles without a readable warning/rest. The four-zone shader support is a content ceiling, not a mobile performance result. These levels use at most two.

Future stages should still earn their place through a different understandable plan. The follow-on layouts are implemented for the later review pass, not evidence that every combination belongs in the release campaign.

## Deferred verification

This session performed C# syntax parsing and source/data review. **It did not run Unity compilation, EditMode/PlayMode suites, rendering, device tests or build a new IPA/APK.** No claim is made that the new campaign is already winnable, visually approved or comfortable on a phone.

Before delivery, the next verification pass must cover:

1. Import and compile the new scripts, metadata, campaign and shader in Unity. Verify all fifteen serialized lessons and the fresh-project content recipe agree. Check the water shader on both mobile graphics backends; the added zone overlay requires shader model 3.0.
2. Re-run the existing regression suites. The content-count assertion now expects fifteen. Existing control-only journey coverage still covers the original five. The standalone audit explicitly reports that scope, rather than claiming to validate the full expansion.
3. Add/run checks for type-specific lift/settling, mini geometry, tray support and capacity, quota completion, a final catch just before expiry, timer expiry, pause/background/resume, retry, and saved progress surviving failure.
4. Verify lava warning/active/rest transitions, zone edge falloff, zero force outside zones, steady drift, delayed initial activation, repeat timing, pause/retry behavior and ceiling containment. Compare visible zone bounds/directions with physical effects. Verify fish warning, crossing contact, one nudge per pass, protected catches, restart repeatability and pause behavior, including the moving-peg combination.
5. Render short/tall phones, all three level pages, both help pages, the frost failure screen, all biomes, current states and fish cues. Review text fit, marker readability, ring contrast, tray openings and reduced-motion legibility.
6. Play every new stage with touch and tilt, without an automated controller deciding the route. Judge whether a player understands what changed, can recover from misses and wants another attempt. Tune both ice deadlines, current transfer timing and the fish/peg crossing from those observations.
7. Profile the eight-ring scene on lower-powered phones. It currently has up to 256 ring contact segments, twice the four-ring repair scene. Profile CPU physics, GPU shader cost and frame pacing before deciding whether it is acceptable.
8. Bump the release version/build numbers, then create and install fresh packages. The previously delivered 0.2.4 files contain none of these expansion changes or the later flat-ring repair.

The implementation is saved locally for continued changes. No cloud build, purchase, merge or publication was triggered.
