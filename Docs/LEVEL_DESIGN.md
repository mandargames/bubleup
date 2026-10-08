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

The chamber spans 6.8 units horizontally and 7.2 vertically, with a wider camera view and unchanged ring size. Vertical jets at x = ±2.15 cover the outer lanes; pumps do not add lateral force. Tilt or touch/keyboard steering determines horizontal travel. Peg layouts retain their authored positions, leaving more room around them.

Five total stars unlock Apricot enamel; ten unlock Moonstone. These are cosmetic changes.

The automated controller has completed all five layouts through public pump and tilt controls. It uses exact physics positions, so its times are evidence of reachability rather than a human difficulty benchmark. The time/efficiency goals are provisional until fresh players test them.
