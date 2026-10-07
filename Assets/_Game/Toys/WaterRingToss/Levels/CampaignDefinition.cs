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
    public sealed class LevelDefinition
    {
        public string id, title, chapter, lesson, hint;
        public Vector2[] rings;
        public PegDefinition[] pegs;
        public BaffleDefinition[] baffles = Array.Empty<BaffleDefinition>();
        public float leftStrength = 5.8f, rightStrength = 5.8f;
        public float settling = 2.1f, damping = .78f;
        public float silverSeconds = 60f, goldSeconds = 30f;
        public int goldPumps = 14;
        public int starsRequired;

        public bool Validate(out string reason)
        {
            if (string.IsNullOrEmpty(id) || rings == null || rings.Length == 0 || pegs == null || pegs.Length == 0)
            { reason = "Every level needs an ID, rings and pegs."; return false; }
            int slots = 0;
            foreach (var peg in pegs)
            {
                if (peg.capacity < 1 || peg.length < .3f + peg.capacity * .16f || Mathf.Abs(peg.tip.x) + peg.movement > 2.05f || peg.tip.y > 3.2f || peg.tip.y - peg.length < -2.5f)
                { reason = "Peg slots or position are invalid."; return false; }
                slots += peg.capacity;
            }
            foreach (var ring in rings)
                if (Mathf.Abs(ring.x) > 2.2f || ring.y < -2.45f || ring.y > 3.25f)
                { reason = "Ring spawn is outside the chamber."; return false; }
            if (slots != rings.Length || goldSeconds <= 0f || silverSeconds < goldSeconds || goldPumps < 1 || leftStrength <= 0f || rightStrength <= 0f || damping < 0f || settling <= 0f)
            { reason = "Capacity, forces or star thresholds are invalid."; return false; }
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
                if (level == null || !level.Validate(out reason)) { reason = "Invalid level content."; return false; }
                if (!ids.Add(level.id)) { reason = "Duplicate level ID: " + level.id; return false; }
            }
            reason = null; return true;
        }
    }
}
