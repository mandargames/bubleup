using System;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public enum CurrentState { Rest, Warning, Active }

    [Serializable]
    public sealed class CurrentDefinition
    {
        public const int MaximumPerLevel = 4;
        public string label;
        public Vector2 center, size, acceleration;
        // Zero period means a steady current. Delays are measured from attempt start.
        public float period, startDelay, warningSeconds = 1.5f, activeSeconds = 4f;

        public CurrentState StateAt(float elapsed)
        {
            if (elapsed < startDelay) return CurrentState.Rest;
            if (period <= 0f) return CurrentState.Active;
            float phase = Mathf.Repeat(elapsed - startDelay, period);
            if (phase < warningSeconds) return CurrentState.Warning;
            return phase < warningSeconds + activeSeconds ? CurrentState.Active : CurrentState.Rest;
        }

        public float WarningRemaining(float elapsed) => StateAt(elapsed) == CurrentState.Warning
            ? warningSeconds - Mathf.Repeat(elapsed - startDelay, period) : 0f;

        public Vector2 AccelerationAt(Vector2 position, float elapsed)
        {
            if (StateAt(elapsed) != CurrentState.Active || size.x <= 0f || size.y <= 0f) return Vector2.zero;
            Vector2 relative = position - center;
            float edge = Mathf.Max(Mathf.Abs(relative.x) / (size.x * .5f), Mathf.Abs(relative.y) / (size.y * .5f));
            // Fade the outer quarter; the water shader uses this exact footprint.
            return acceleration * Mathf.Clamp01((1f - edge) / .25f);
        }

        public string Direction => Mathf.Abs(acceleration.x) > Mathf.Abs(acceleration.y)
            ? (acceleration.x > 0f ? "RIGHT" : "LEFT") : (acceleration.y > 0f ? "UP" : "DOWN");

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(label) || !Finite(center.x) || !Finite(center.y) || !Finite(size.x) || !Finite(size.y)
                || !Finite(acceleration.x) || !Finite(acceleration.y) || !Finite(period) || !Finite(startDelay)
                || !Finite(warningSeconds) || !Finite(activeSeconds)) return false;
            if (size.x < .3f || size.y < .3f || Mathf.Abs(center.x) + size.x * .5f > ToyPresentation.TankHalfWidth
                || center.y - size.y * .5f < ToyPresentation.TankBottom || center.y + size.y * .5f > 3.85f
                || acceleration.sqrMagnitude < .01f || acceleration.sqrMagnitude > 36f || startDelay < 0f) return false;
            return period == 0f ? startDelay == 0f
                : period >= 5f && startDelay < period && warningSeconds >= 1.2f && activeSeconds >= 1f && warningSeconds + activeSeconds < period;
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
