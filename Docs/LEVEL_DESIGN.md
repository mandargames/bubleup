# Five little adventures

The editable source is `Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset`. Select it in Unity's Inspector. The editor setup only creates initial content when the asset is absent; rebuilding does not replace designer edits.

| Level | Teaching purpose | Layout | Three-star target | Two-star target |
| --- | --- | --- | --- | --- |
| First Splash | Pump up, stop pumping, steer a descending ring | One ring and a central peg | 22 seconds and no more than 8 pumps | 50 seconds |
| Double Dip | Separate lift from alignment; finish a stack | Two rings on one two-slot peg | 36 seconds / 15 pumps | 70 seconds |
| Side by Side | Choose destinations and notice capacities | Three rings, offset pegs with one and two slots, slightly asymmetric pumps | 52 seconds / 23 pumps | 95 seconds |
| The Scenic Route | Read an obstacle and use the side channels | Three rings, two raised pegs and a tilted central baffle | 65 seconds / 30 pumps | 115 seconds |
| A Little Symphony | Combine steering, capacity management and timing | Four rings and three pegs; the middle peg gently moves | 80 seconds / 38 pumps | 140 seconds |

Every completion earns one star. There is no countdown failure or energy system. Seated rings lock onto their peg and cannot be dislodged by pumps or steering. Any ring may fill any available slot. Dots below a peg show its total capacity. The level completes shortly after every ring has landed and locked. Each completed level unlocks the next; replaying keeps the best stars, time and pump count independently.

Pegs and their compact shelves are solid. The shelves are narrower than a ring to leave generous upward routes beside each peg. Lift beside a shelf, steer the opening over the peg tip, then stop pumping to let the ring fall and settle. A ring crossing the tip has not scored yet; it must physically rest on the shelf or stack. Once locked, its solid colliders support subsequent rings, and it follows the peg if it moves.

The chamber spans 5.6 units horizontally and 7.2 vertically. This bounded arena retains its proportions on short and tall phones instead of extending its ceiling to fill the screen. Flat molded rings retain the earlier outside diameter with a larger opening and thinner section; peg sizes and positions remain unchanged. Vertical jets at x = ±1.95 cover the outer lanes. Pumps do not add lateral force. Tilt or touch/keyboard steering determines horizontal travel.

Gameplay shows the goal and landed-ring count. Time and pump records continue to be measured, but appear only when the player opens results after completion. Every finish receives the same main celebration. First-level contextual help and a revisitable pause-menu guide explain lift, steering and the need to let a ring settle. Restarting an active attempt requires an explicit choice; saved completions remain intact.

Stack acceptance accounts for the ring's full projected height when contact geometry prevents deep nesting. Threading is recognised whenever the shaft is inside the opening below its tip, including an off-centre entry that first touches the rim. It does not depend on crossing a narrow invisible line in one physics step. A ring must still be physically supported, settle at low speed and fit an available peg slot. No ring is teleported into a scoring position. A ring on an already full peg must be lifted off and guided to a free slot.

Proposed colour/weight types, mini-ring group play and new level lessons are recorded in [Ring design](Research/RING_DESIGN.md). They remain prototype designs; the five current levels still use identical ring behaviour and require every ring to land.

Five total stars unlock Apricot enamel; ten unlock Moonstone. These are cosmetic changes.

The automated controller has completed all five layouts through public pump and tilt controls. It uses exact physics positions, so its times are evidence of reachability rather than a human difficulty benchmark. The time/efficiency goals are provisional until fresh players test them.
