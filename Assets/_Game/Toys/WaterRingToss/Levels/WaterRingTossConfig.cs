using UnityEngine;

namespace PocketToys.WaterRingToss
{
    [CreateAssetMenu(menuName = "Pocket Toys/Water Ring Toss Spike")]
    public sealed class WaterRingTossConfig : ScriptableObject
    {
        [Tooltip("Serialized material keeps the prototype shader available in player builds.")]
        public Material prototypeMaterial;
        [Header("Tank and content (world units)")]
        public Vector2 tankSize = new Vector2(5.2f, 6.8f);
        public Vector2 ringStart = new Vector2(-1.1f, -2.5f);
        public Vector2 pegTip = new Vector2(0f, .8f);
        public float pegLength = 1.3f;
        public float ringRadius = .38f;
        public float ringTubeRadius = .085f;
        [Header("Water feel")]
        public float settlingAcceleration = 2.2f;
        public float linearDamping = .85f;
        public float tiltAcceleration = 5f;
        public float turbulenceAcceleration = .12f;
        public float maximumSpeed = 7f;
        [Header("Pumps")]
        public Vector2 leftPump = new Vector2(-1.5f, -3f);
        public Vector2 rightPump = new Vector2(1.5f, -3f);
        public float leftStrength = 5.7f;
        public float rightStrength = 5.7f;
        public float pumpWidth = 2.6f;
        public float inwardForce = .65f;
        public float pumpCooldown = .16f;
        [Header("Forgiving catch")]
        public float captureHalfWidth = .23f;
        public float captureMaxSpeed = 4.5f;
        public float captureSettleSeconds = .25f;

        public bool IsValid(out string reason)
        {
            if (tankSize.x <= 2f || tankSize.y <= 3f || ringRadius <= 0f || ringTubeRadius <= 0f || ringTubeRadius >= ringRadius)
            { reason = "Tank and ring dimensions must be positive with a visible ring hole."; return false; }
            float margin = ringRadius + ringTubeRadius;
            if (Mathf.Abs(ringStart.x) + margin >= tankSize.x / 2f || Mathf.Abs(ringStart.y) + margin >= tankSize.y / 2f)
            { reason = "Ring start must fit inside the tank."; return false; }
            if (pegLength <= 0f || Mathf.Abs(pegTip.x) + margin >= tankSize.x / 2f || pegTip.y + margin >= tankSize.y / 2f || pegTip.y - pegLength <= -tankSize.y / 2f)
            { reason = "Peg must fit inside the tank."; return false; }
            if (pumpWidth <= 0f || leftStrength <= 0f || rightStrength <= 0f || pumpCooldown < 0f || maximumSpeed <= 0f || settlingAcceleration <= 0f || linearDamping < 0f || captureSettleSeconds <= 0f || captureMaxSpeed <= 0f || captureHalfWidth <= 0f || captureHalfWidth >= ringRadius - ringTubeRadius)
            { reason = "Force, damping and catch settings are invalid."; return false; }
            reason = string.Empty;
            return true;
        }
    }
}
