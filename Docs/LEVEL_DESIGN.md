# Five little adventures

The editable source is `Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset`. Select it in Unity's Inspector. The editor setup only creates initial content when the asset is absent; rebuilding does not replace designer edits.

| Level | Teaching purpose | Layout | Three-star target | Two-star target |
| --- | --- | --- | --- | --- |
| First Splash | Pump up, stop pumping, steer a descending ring | One ring and a central peg | 22 seconds and no more than 8 pumps | 50 seconds |
| Double Dip | Separate lift from alignment; finish a stack | Two rings on one two-slot peg | 36 seconds / 15 pumps | 70 seconds |
| Side by Side | Choose destinations and notice capacities | Three rings, offset pegs with one and two slots, slightly asymmetric pumps | 52 seconds / 23 pumps | 95 seconds |
| The Scenic Route | Read an obstacle and use the side channels | Three rings, two raised pegs and a tilted central baffle | 65 seconds / 30 pumps | 115 seconds |
| A Little Symphony | Combine steering, capacity management and timing | Four rings and three pegs; the middle peg gently moves | 80 seconds / 38 pumps | 140 seconds |

Every completion earns one star. There is no countdown failure or energy system. Rings remain caught once seated. Any ring may fill any available slot. Dots below a peg show its total capacity. Each completed level unlocks the next; replaying keeps the best stars, time and pump count independently.

Five total stars unlock Apricot enamel; ten unlock Moonstone. These are cosmetic changes.

The automated controller has completed all five layouts through public pump and tilt controls. It uses exact physics positions, so its times are evidence of reachability rather than a human difficulty benchmark. The time/efficiency goals are provisional until fresh players test them.
