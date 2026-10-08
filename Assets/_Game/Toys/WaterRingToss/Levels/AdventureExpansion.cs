#if UNITY_EDITOR
using System;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    // One-time authoring recipe. The serialized campaign remains the editable source.
    // Setup uses this only when creating a missing asset; existing edits are retained.
    public static class AdventureExpansion
    {
        public static LevelDefinition[] AppendTo(LevelDefinition[] introduction)
        {
            var additions = Create();
            var result = new LevelDefinition[introduction.Length + additions.Length];
            Array.Copy(introduction, result, introduction.Length);
            Array.Copy(additions, 0, result, introduction.Length, additions.Length);
            return result;
        }
        static LevelDefinition[] Create() => new[]
        {
            new LevelDefinition
            {
                id = "water_06",
                title = "Feather and Pebble",
                chapter = "NEW CURRENTS",
                lesson = "Mint rises readily. Violet settles sooner.",
                hint = "One mark is light; three marks are heavy. Try a short pump.",
                rings = new[] { new Vector2(-0.6f, -1.85f), new Vector2(0.6f, -1.85f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Light }, new RingBehaviourDefinition { kind = RingKind.Heavy } },
                pegs = new[] { new PegDefinition { tip = new Vector2(-1.05f, 1.75f), length = 1.6f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(1.05f, 0.9f), length = 1.4f, capacity = 1, movement = 0f, speed = 0.6f } },
                baffles = Array.Empty<BaffleDefinition>(),
                collectors = Array.Empty<CollectorDefinition>(),
                requiredCatches = 0,
                biome = TankBiome.Lagoon,
                freezeSeconds = 0f,
                fishTraffic = false,
                fishSeed = 176,
                fishInterval = 7f,
                fishSpeed = 1.25f,
                fishLanes = new[] { 1.5f, 2.5f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 100f,
                goldSeconds = 50f,
                goldPumps = 24,
                starsRequired = 5
            },
            new LevelDefinition
            {
                id = "water_07",
                title = "Bubble Garden",
                chapter = "NEW CURRENTS",
                lesson = "Guide a group. Collect any six of eight.",
                hint = "Pump up the middle, then steer down into either open tray.",
                rings = new[] { new Vector2(-0.975f, -1.55f), new Vector2(-0.325f, -1.55f), new Vector2(0.325f, -1.55f), new Vector2(0.975f, -1.55f), new Vector2(-0.975f, -2.2f), new Vector2(-0.325f, -2.2f), new Vector2(0.325f, -2.2f), new Vector2(0.975f, -2.2f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini }, new RingBehaviourDefinition { kind = RingKind.Mini } },
                pegs = Array.Empty<PegDefinition>(),
                baffles = Array.Empty<BaffleDefinition>(),
                collectors = new[] { new CollectorDefinition { center = new Vector2(-1.7f, 0.65f), width = 1.35f, height = 0.9f, capacity = 4 }, new CollectorDefinition { center = new Vector2(1.7f, 0.65f), width = 1.35f, height = 0.9f, capacity = 4 } },
                requiredCatches = 6,
                biome = TankBiome.Lagoon,
                freezeSeconds = 0f,
                fishTraffic = false,
                fishSeed = 177,
                fishInterval = 7f,
                fishSpeed = 1.25f,
                fishLanes = new[] { 1.5f, 2.5f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 110f,
                goldSeconds = 55f,
                goldPumps = 22,
                starsRequired = 6
            },
            new LevelDefinition
            {
                id = "water_08",
                title = "First Frost",
                chapter = "NEW CURRENTS",
                lesson = "Land both rings before the tank freezes.",
                hint = "You have 60 seconds. Open settings to pause the countdown.",
                rings = new[] { new Vector2(-1.2f, -1.85f), new Vector2(1.2f, -1.85f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Standard }, new RingBehaviourDefinition { kind = RingKind.Standard } },
                pegs = new[] { new PegDefinition { tip = new Vector2(-1.05f, 1.1f), length = 1.45f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(1.05f, 1.5f), length = 1.7f, capacity = 1, movement = 0f, speed = 0.6f } },
                baffles = Array.Empty<BaffleDefinition>(),
                collectors = Array.Empty<CollectorDefinition>(),
                requiredCatches = 0,
                biome = TankBiome.Ice,
                freezeSeconds = 60f,
                fishTraffic = false,
                fishSeed = 178,
                fishInterval = 7f,
                fishSpeed = 1.25f,
                fishLanes = new[] { 1.5f, 2.5f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 50f,
                goldSeconds = 35f,
                goldPumps = 16,
                starsRequired = 7
            },
            new LevelDefinition
            {
                id = "water_09",
                title = "Lava Lift",
                chapter = "NEW CURRENTS",
                lesson = "Ride a rising current. Settle while it rests.",
                hint = "The glowing vent warns before it rises. Left and right take turns.",
                rings = new[] { new Vector2(-0.9f, -1.85f), new Vector2(0.9f, -1.85f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Standard }, new RingBehaviourDefinition { kind = RingKind.Standard } },
                pegs = new[] { new PegDefinition { tip = new Vector2(-1.1f, 1.8f), length = 1.6f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(1.1f, 1.8f), length = 1.6f, capacity = 1, movement = 0f, speed = 0.6f } },
                baffles = Array.Empty<BaffleDefinition>(),
                collectors = Array.Empty<CollectorDefinition>(),
                requiredCatches = 0,
                biome = TankBiome.Lava,
                freezeSeconds = 0f,
                fishTraffic = false,
                fishSeed = 179,
                fishInterval = 7f,
                fishSpeed = 1.25f,
                fishLanes = new[] { 1.5f, 2.5f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 110f,
                goldSeconds = 55f,
                goldPumps = 22,
                starsRequired = 8
            },
            new LevelDefinition
            {
                id = "water_10",
                title = "Passing Company",
                chapter = "NEW CURRENTS",
                lesson = "Watch the edge. Let the fish pass.",
                hint = "A fish can nudge a loose ring. Landed rings stay safe.",
                rings = new[] { new Vector2(-1.2f, -1.85f), new Vector2(1.2f, -1.85f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Standard }, new RingBehaviourDefinition { kind = RingKind.Standard } },
                pegs = new[] { new PegDefinition { tip = new Vector2(-1.1f, 1.15f), length = 1.5f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(1.1f, 1.8f), length = 1.8f, capacity = 1, movement = 0f, speed = 0.6f } },
                baffles = Array.Empty<BaffleDefinition>(),
                collectors = Array.Empty<CollectorDefinition>(),
                requiredCatches = 0,
                biome = TankBiome.Lagoon,
                freezeSeconds = 0f,
                fishTraffic = true,
                fishSeed = 180,
                fishInterval = 7f,
                fishSpeed = 1.25f,
                fishLanes = new[] { 1.35f, 2.4f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 110f,
                goldSeconds = 55f,
                goldPumps = 22,
                starsRequired = 9
            },
            new LevelDefinition
            {
                id = "water_11",
                title = "Reef Rhythm",
                chapter = "NEW CURRENTS",
                lesson = "Three weights. Choose your landing order.",
                hint = "Light rings drift longer; heavy rings resist a fish nudge.",
                rings = new[] { new Vector2(-1.6f, -1.85f), new Vector2(0f, -1.85f), new Vector2(1.6f, -1.85f) },
                ringBehaviours = new[] { new RingBehaviourDefinition { kind = RingKind.Light }, new RingBehaviourDefinition { kind = RingKind.Standard }, new RingBehaviourDefinition { kind = RingKind.Heavy } },
                pegs = new[] { new PegDefinition { tip = new Vector2(-1.4f, 0.95f), length = 1.45f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(0f, 1.85f), length = 1.55f, capacity = 1, movement = 0f, speed = 0.6f }, new PegDefinition { tip = new Vector2(1.4f, 0.95f), length = 1.45f, capacity = 1, movement = 0f, speed = 0.6f } },
                baffles = new[] { new BaffleDefinition { center = new Vector2(0f, -0.35f), size = new Vector2(0.9f, 0.16f), angle = 0f } },
                collectors = Array.Empty<CollectorDefinition>(),
                requiredCatches = 0,
                biome = TankBiome.Lagoon,
                freezeSeconds = 0f,
                fishTraffic = true,
                fishSeed = 181,
                fishInterval = 7f,
                fishSpeed = 1.2f,
                fishLanes = new[] { 0.65f, 2.35f },
                thermalPeriod = 7.5f,
                thermalStrength = 4.8f,
                leftStrength = 5.8f,
                rightStrength = 5.8f,
                settling = 2.1f,
                damping = 0.78f,
                silverSeconds = 150f,
                goldSeconds = 80f,
                goldPumps = 34,
                starsRequired = 10
            }
        };
    }
}
#endif
