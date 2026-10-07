using UnityEngine;

namespace PocketToys.Core.Input
{
    /// <summary>Frame-rate independent filtering shared by sensor and simulated input.</summary>
    public sealed class TiltFilter
    {
        public Vector2 Value { get; private set; }
        public Vector2 Neutral { get; private set; }

        public void Calibrate(Vector2 sample)
        {
            Neutral = sample;
            Value = Vector2.zero;
        }

        public Vector2 Step(Vector2 sample, float sensitivity, float deadZone, float smoothingSeconds, float deltaTime)
        {
            var target = Vector2.ClampMagnitude((sample - Neutral) * Mathf.Max(0f, sensitivity), 1f);
            var magnitude = target.magnitude;
            deadZone = Mathf.Clamp(deadZone, 0f, .95f);
            target = magnitude <= deadZone ? Vector2.zero : target.normalized * ((magnitude - deadZone) / (1f - deadZone));
            var blend = smoothingSeconds <= 0f ? 1f : 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / smoothingSeconds);
            Value = Vector2.Lerp(Value, target, blend);
            return Value;
        }
    }
}
