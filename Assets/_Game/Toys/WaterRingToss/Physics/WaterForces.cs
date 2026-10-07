using UnityEngine;

namespace PocketToys.WaterRingToss
{
    public static class WaterForces
    {
        public static Vector3 PumpImpulse(Vector2 position, Vector2 nozzle, float strength, float width, float inward)
        {
            if (position.y < nozzle.y - .3f) return Vector3.zero;
            float falloff = Mathf.Clamp01(1f - Mathf.Abs(position.x - nozzle.x) / Mathf.Max(.01f, width));
            float vertical = strength * falloff;
            return new Vector3(-Mathf.Sign(nozzle.x) * vertical * inward, vertical, 0f);
        }

        public static bool CanCapture(Vector2 previous, Vector2 current, Vector2 velocity, WaterRingTossConfig config)
        {
            if (velocity.y >= 0f || velocity.magnitude > config.captureMaxSpeed || previous.y < config.pegTip.y || current.y > config.pegTip.y) return false;
            float distance = previous.y - current.y;
            float fraction = distance > .00001f ? (previous.y - config.pegTip.y) / distance : 0f;
            float crossingX = Mathf.Lerp(previous.x, current.x, fraction);
            return Mathf.Abs(crossingX - config.pegTip.x) <= config.captureHalfWidth;
        }
    }
}
