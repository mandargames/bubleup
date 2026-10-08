using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    [Serializable]
    public sealed class PegDefinition
    {
        public Vector2 tip;
        public float length = 1.5f;
        public int capacity = 1;
        public float movement;
        public float speed = .6f;
    }

    [Serializable]
    public sealed class BaffleDefinition
    {
        public Vector2 center;
        public Vector2 size;
        public float angle;
    }

    [Serializable]
    public sealed class CollectorDefinition
    {
        public Vector2 center;
        public float width = 1.3f, height = .8f;
        public int capacity = 4;
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        public string id, title, chapter, lesson, hint;
        public Vector2[] rings;
        public RingBehaviourDefinition[] ringBehaviours = Array.Empty<RingBehaviourDefinition>();
        public PegDefinition[] pegs;
        public BaffleDefinition[] baffles = Array.Empty<BaffleDefinition>();
        public CollectorDefinition[] collectors = Array.Empty<CollectorDefinition>();
        public int requiredCatches;
        public TankBiome biome;
        public float freezeSeconds;
        public bool fishTraffic;
        public int fishSeed = 17;
        public float fishInterval = 7f, fishSpeed = 1.25f;
        public float[] fishLanes = { 1.5f, 2.5f };
        public float thermalPeriod = 7f, thermalStrength = 2.5f;
        public float leftStrength = 5.8f, rightStrength = 5.8f;
        public float settling = 2.1f, damping = .78f;
        public float silverSeconds = 60f, goldSeconds = 30f;
        public int goldPumps = 14;
        public int starsRequired;
        public int TargetCount => requiredCatches > 0 ? requiredCatches : rings.Length;
        public bool UsesCollectors => collectors != null && collectors.Length > 0;
        public RingKind RingType(int index) => ringBehaviours != null && ringBehaviours.Length == rings.Length ? ringBehaviours[index].kind : RingKind.Standard;

        public bool Validate(out string reason)
        {
            if (string.IsNullOrEmpty(id) || rings == null || rings.Length == 0 || pegs == null || (pegs.Length == 0 && !UsesCollectors))
            { reason = "Every level needs an ID, rings and a catch destination."; return false; }
            if (ringBehaviours != null && ringBehaviours.Length > 0 && ringBehaviours.Length != rings.Length)
            { reason = "Each ring needs exactly one behaviour, or all behaviours must be omitted for standard rings."; return false; }
            if (ringBehaviours != null) foreach (var behaviour in ringBehaviours)
                if (behaviour == null || !Enum.IsDefined(typeof(RingKind), behaviour.kind)) { reason = "Invalid ring behaviour."; return false; }
            int slots = 0;
            foreach (var peg in pegs)
            {
                if (peg == null || peg.capacity < 1 || peg.length < .3f + peg.capacity * .16f || Mathf.Abs(peg.tip.x) + peg.movement > 2.05f || peg.tip.y > 3.2f || peg.tip.y - peg.length < -2.5f)
                { reason = "Peg slots or position are invalid."; return false; }
                slots += peg.capacity;
            }
            if (UsesCollectors) foreach (var collector in collectors)
            {
                if (collector == null || collector.capacity < 1 || collector.width < 1f || collector.height < .5f || Mathf.Abs(collector.center.x) + collector.width * .5f > 2.65f || collector.center.y - collector.height * .5f < -2.25f || collector.center.y + collector.height * .5f > 3.2f)
                { reason = "Collector geometry or capacity is invalid."; return false; }
                slots += collector.capacity;
            }
            foreach (var ring in rings)
                if (Mathf.Abs(ring.x) > 2.2f || ring.y < -2.45f || ring.y > 3.25f)
                { reason = "Ring spawn is outside the chamber."; return false; }
            if (requiredCatches < 0 || TargetCount < 1 || TargetCount > rings.Length || slots < TargetCount || goldSeconds <= 0f || silverSeconds < goldSeconds || goldPumps < 1 || leftStrength <= 0f || rightStrength <= 0f || damping < 0f || settling <= 0f || starsRequired < 0)
            { reason = "Capacity, forces or star thresholds are invalid."; return false; }
            if (!Enum.IsDefined(typeof(TankBiome), biome) || (biome == TankBiome.Ice ? freezeSeconds < 10f : freezeSeconds != 0f) || (biome == TankBiome.Lava && (thermalPeriod < 5f || thermalStrength <= 0f || fishTraffic)))
            { reason = "Invalid environment: ice needs a timer, lava needs a readable cycle and cannot contain reef fish."; return false; }
            if (fishTraffic)
            {
                if (fishInterval < 3f || fishSpeed < .5f || fishSpeed > 2f || fishLanes == null || fishLanes.Length == 0)
                { reason = "Fish need visible, bounded crossing routes."; return false; }
                foreach (float lane in fishLanes) if (lane < -.8f || lane > 3.8f) { reason = "Fish lane is outside the clear play area."; return false; }
            }
            reason = null; return true;
        }
    }

    [CreateAssetMenu(menuName = "Pocket Toys/Campaign")]
    public sealed class CampaignDefinition : ScriptableObject
    {
        public Material surfaceMaterial;
        public Material waterMaterial;
        public Material glassMaterial;
        public LevelDefinition[] levels;

        public bool Validate(out string reason)
        {
            var ids = new HashSet<string>();
            if (levels == null || levels.Length == 0) { reason = "No levels."; return false; }
            foreach (var level in levels)
            {
                if (level == null) { reason = "Missing level content."; return false; }
                if (!level.Validate(out reason)) { reason = level.id + ": " + reason; return false; }
                if (!ids.Add(level.id)) { reason = "Duplicate level ID: " + level.id; return false; }
            }
            reason = null; return true;
        }
    }
}
