using System.Collections.Generic;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    // All hazard clocks advance only in active gameplay. One fish at a time keeps
    // crossings readable; a local seed varies routes without perturbing game RNG.
    public sealed class LevelEnvironment : MonoBehaviour
    {
        public const float FishWarningSeconds = 1.4f;
        public bool FishWarning { get; private set; }
        public bool FishActive { get; private set; }
        public Vector2 FishPosition { get; private set; }
        public int FishDirection { get; private set; } = 1;
        GameSession game;
        System.Random random;
        float fishClock, nextFish, warningLeft;
        readonly HashSet<FloatingRing> nudged = new HashSet<FloatingRing>();

        public void ResetFor(GameSession session)
        {
            game = session; random = new System.Random(game.Level.fishSeed);
            FishWarning = FishActive = false; fishClock = warningLeft = 0f;
            nextFish = 3f; nudged.Clear(); FishPosition = new Vector2(-3.5f, 2f);
        }
        public float Frost => game.Level.freezeSeconds > 0 ? Mathf.Clamp01(game.Elapsed / game.Level.freezeSeconds) : 0f;
        public int ThermalSide => Mathf.FloorToInt(game.Elapsed / Mathf.Max(5f, game.Level.thermalPeriod)) % 2;
        public float ThermalPhase => Mathf.Repeat(game.Elapsed, Mathf.Max(5f, game.Level.thermalPeriod));
        public bool ThermalWarning => game.Level.biome == TankBiome.Lava && ThermalPhase < 1.5f;
        public bool ThermalActive => game.Level.biome == TankBiome.Lava && ThermalPhase >= 1.5f && ThermalPhase < 4f;

        public Vector2 AccelerationAt(Vector2 position)
        {
            if (!game.Playing || !ThermalActive) return Vector2.zero;
            float x = ThermalSide == 0 ? -ToyPresentation.NozzleX : ToyPresentation.NozzleX;
            float influence = Mathf.Exp(-Mathf.Pow((position.x - x) / .65f, 2f));
            float ceilingFade = 1f - Mathf.InverseLerp(3.7f, ToyPresentation.TankTop, position.y);
            return Vector2.up * (game.Level.thermalStrength * influence * ceilingFade);
        }

        public string Status
        {
            get
            {
                if (game.Level.biome == TankBiome.Ice) return "FREEZES IN " + Mathf.CeilToInt(Mathf.Max(0f, game.Level.freezeSeconds - game.Elapsed)) + "s";
                if (game.Level.biome == TankBiome.Lava)
                {
                    string side = ThermalSide == 0 ? "LEFT" : "RIGHT";
                    if (ThermalWarning) return side + " VENT RISING IN " + Mathf.CeilToInt(1.5f - ThermalPhase) + "s";
                    if (ThermalActive) return side + " CURRENT RISING";
                    return "CURRENT RESTING · " + (ThermalSide == 0 ? "RIGHT" : "LEFT") + " VENT NEXT";
                }
                if (game.Level.fishTraffic)
                {
                    if (FishWarning) return "FISH ENTERING FROM " + (FishDirection > 0 ? "LEFT" : "RIGHT");
                    if (FishActive) return "FISH CROSSING " + (FishDirection > 0 ? "LEFT TO RIGHT" : "RIGHT TO LEFT");
                    return "CLEAR WATER · WATCH THE EDGES";
                }
                return null;
            }
        }

        void FixedUpdate()
        {
            if (game == null || !game.Playing || game.Caught >= game.TargetCount || !game.Level.fishTraffic) return;
            float step = Time.fixedDeltaTime; fishClock += step;
            if (FishWarning)
            {
                warningLeft -= step;
                if (warningLeft <= 0f) { FishWarning = false; FishActive = true; }
                return;
            }
            if (!FishActive)
            {
                if (fishClock < nextFish) return;
                FishDirection = random.Next(2) == 0 ? 1 : -1;
                FishPosition = new Vector2(-FishDirection * 3.45f, game.Level.fishLanes[random.Next(game.Level.fishLanes.Length)]);
                FishWarning = true; warningLeft = FishWarningSeconds; nudged.Clear();
                return;
            }
            Vector2 before = FishPosition;
            FishPosition += Vector2.right * (FishDirection * game.Level.fishSpeed * step);
            foreach (var ring in game.Rings)
            {
                if (ring.Captured || nudged.Contains(ring)) continue;
                Vector2 position = ring.Body.position;
                float nearestX = Mathf.Clamp(position.x, Mathf.Min(before.x, FishPosition.x), Mathf.Max(before.x, FishPosition.x));
                float dx = (position.x - nearestX) / (ring.CollisionRadius + .35f);
                float dy = (position.y - FishPosition.y) / (ring.ProjectedHeight * .5f + .17f);
                if (dx * dx + dy * dy > 1f) continue;
                ring.Nudge(new Vector2(FishDirection * .7f, .25f)); nudged.Add(ring);
            }
            if (Mathf.Abs(FishPosition.x) > 3.5f)
            {
                FishActive = false;
                nextFish = fishClock + game.Level.fishInterval * (.85f + (float)random.NextDouble() * .3f);
            }
        }
    }
}
