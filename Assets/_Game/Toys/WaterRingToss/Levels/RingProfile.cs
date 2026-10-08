namespace PocketToys.WaterRingToss.Game
{
    public enum RingKind { Standard, Light, Heavy, Mini }
    public enum TankBiome { Lagoon, Ice, Lava }

    [System.Serializable]
    public sealed class RingBehaviourDefinition { public RingKind kind; }

    // Explicit water response matters here: multiplying every force by mass alone
    // would leave all rings with the same acceleration. These are prototype values.
    public readonly struct RingProfile
    {
        public readonly string Name, Color;
        public readonly float Scale, Mass, Lift, Settling, Steering, Damping, Nudge;
        public readonly int Marks;
        RingProfile(string name, string color, float scale, float mass, float lift, float settling, float steering, float damping, float nudge, int marks)
        { Name = name; Color = color; Scale = scale; Mass = mass; Lift = lift; Settling = settling; Steering = steering; Damping = damping; Nudge = nudge; Marks = marks; }

        public static RingProfile For(RingKind kind)
        {
            switch (kind)
            {
                case RingKind.Light: return new RingProfile("Light", "#65E8BE", 1f, .026f, 1.2f, .75f, 1.15f, 1.05f, 1.15f, 1);
                case RingKind.Heavy: return new RingProfile("Heavy", "#B9A0FF", 1f, .05f, .82f, 1.25f, .85f, .95f, .75f, 3);
                case RingKind.Mini: return new RingProfile("Mini", "#FFCE52", .68f, .015f, 1.3f, .8f, 1.3f, 1.3f, 1.25f, 1);
                default: return new RingProfile("Standard", "#FF6B59", 1f, .035f, 1f, 1f, 1f, 1f, 1f, 2);
            }
        }
    }
}
