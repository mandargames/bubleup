using System;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class FloatingRing : MonoBehaviour
    {
        public const float OuterRadius = .39f, InnerRadius = .25f, Thickness = .07f, Pitch = 58f;
        // Nested rings can sit lower, but their full projected height is possible
        // when a neighboring ring shifts the contact points in a stack.
        public static readonly float StackSpacing = 2f * (OuterRadius * Mathf.Cos(Pitch * Mathf.Deg2Rad) + Thickness * .5f * Mathf.Sin(Pitch * Mathf.Deg2Rad));
        const float PumpDuration = .22f;
        public Rigidbody Body { get; private set; }
        public Transform Visual;
        public bool Captured { get; private set; }
        public int PegIndex { get; private set; } = -1;
        public bool Threaded => PegIndex >= 0;
        public int CollectorIndex { get; private set; } = -1;
        public RingKind Kind { get; private set; }
        public RingProfile Profile { get; private set; }
        public float CollisionRadius => OuterRadius * Profile.Scale;
        public float ProjectedHeight => StackSpacing * Profile.Scale;
        public event Action Contact;
        GameSession game;
        Vector2 lockedOffset;
        Vector3 savedVelocity, savedAngularVelocity, pendingImpulse;
        bool paused;
        float seed, seatedTime, lastContact, pumpRemaining, supportTime = -1f;
        int supportPeg = -1;
        int supportCollector = -1;
        float collectorSupportTime = -1f;

        public void Initialize(GameSession session, int index, Vector2 start)
        {
            game = session; seed = index * 2.37f;
            Kind = game.Level.RingType(index); Profile = RingProfile.For(Kind);
            transform.localScale = Vector3.one * Profile.Scale;
            Body = gameObject.AddComponent<Rigidbody>();
            Body.mass = Profile.Mass;
            Body.useGravity = false; // Settling is gravity minus buoyancy, applied at every height.
            Body.linearDamping = game.Level.damping * Profile.Damping;
            // Keep the stable ring angle: rolling a ring sideways on a peg can
            // wedge it because this toy deliberately constrains movement to one plane.
            Body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.solverIterations = 12; Body.solverVelocityIterations = 6;
            Body.maxLinearVelocity = Kind == RingKind.Mini ? 5.5f : 7f;
            Body.position = new Vector3(start.x, start.y, -.75f);
            Body.rotation = Quaternion.Euler(Pitch, 0, 0);
            transform.SetPositionAndRotation(Body.position, Body.rotation);
            if (Visual != null) Visual.localRotation = Quaternion.identity;

            // Two thin rounded contact rails approximate the flat band. Their outside,
            // opening and thickness match the mesh without sharp box corners that
            // can snag neighboring rings. Capsules retain continuous collision checks.
            const int segments = 16;
            float contactRadius = Thickness * .5f;
            for (int rail = 0; rail < 2; rail++)
            for (int i = 0; i < segments; i++)
            {
                float radius = rail == 0 ? InnerRadius + contactRadius : OuterRadius - contactRadius;
                float a = i * Mathf.PI * 2f / segments, b = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 from = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius;
                Vector3 to = new Vector3(Mathf.Cos(b), Mathf.Sin(b), 0) * radius;
                var part = new GameObject("Ring contact " + rail + "-" + i); part.transform.SetParent(transform, false);
                part.transform.localPosition = (from + to) * .5f;
                part.transform.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
                var collider = part.AddComponent<CapsuleCollider>();
                collider.radius = contactRadius; collider.height = Vector3.Distance(from, to) + Thickness;
                collider.sharedMaterial = game.Presentation.ContactMaterial;
                collider.contactOffset = .003f;
            }
        }

        public void Pump(bool left)
        {
            if (paused || Captured) return;
            float nozzle = left ? -ToyPresentation.NozzleX : ToyPresentation.NozzleX;
            float dx = Body.position.x - nozzle;
            float influence = Mathf.Exp(-dx * dx / 6.5f);
            float strength = (left ? game.Level.leftStrength : game.Level.rightStrength) * influence;
            float lift = strength * Profile.Lift * Mathf.Lerp(1f, .72f, Mathf.InverseLerp(-2.4f, 3.6f, Body.position.y));
            // The peg shelters threaded rings from the jet. They still lift, but a
            // light tap should not throw an otherwise settled stack across the tank.
            if (Threaded) lift *= .45f;
            // A short water jet adds momentum over several physics steps, preserving contact response.
            // Pumps lift vertically. Steering controls lateral travel, so repeated
            // red/yellow presses cannot force rings away from the outer corners.
            pendingImpulse += Vector3.up * (lift * Body.mass);
            pumpRemaining = PumpDuration;
            Body.WakeUp();
        }

        public void Nudge(Vector2 impulse)
        {
            if (paused || Captured || !game.Playing) return;
            Body.AddForce((Vector3)impulse * (Body.mass * Profile.Nudge), ForceMode.Impulse);
            seatedTime = 0f;
        }

        public void Pause(bool value)
        {
            if (paused == value) return;
            paused = value;
            if (Captured) return;
            if (value)
            {
                savedVelocity = Body.linearVelocity; savedAngularVelocity = Body.angularVelocity;
                Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                Body.isKinematic = true;
            }
            else
            {
                Body.isKinematic = false; Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Body.linearVelocity = savedVelocity; Body.angularVelocity = savedAngularVelocity;
            }
        }

        void FixedUpdate()
        {
            if (game == null || paused) return;
            if (Captured)
            {
                if (PegIndex >= 0)
                {
                    var position = game.PegTip(PegIndex) + lockedOffset;
                    Body.MovePosition(new Vector3(position.x, position.y, Body.position.z));
                }
                return;
            }
            var current = (Vector2)Body.position;
            if (game.Level.UsesCollectors && CheckCollectorCatch(current)) return;
            if (!Threaded)
            {
                for (int i = 0; i < game.Level.pegs.Length; i++)
                {
                    var tip = game.PegTip(i);
                    // Use the shaft inside the opening, not a narrow one-frame tip
                    // crossing. A rim can touch the tip, then slide into place later.
                    // Colliders enforce entry; support and settling still earn the catch.
                    if (current.y > tip.y || current.y < tip.y - game.Level.pegs[i].length + .05f) continue;
                    if (!ContainsStem(current.x - tip.x)) continue;
                    PegIndex = i; seatedTime = 0f;
                    break;
                }
            }
            if (Threaded)
            {
                var tip = game.PegTip(PegIndex); var peg = game.Level.pegs[PegIndex];
                if (current.y > tip.y + .25f || current.y < tip.y - peg.length - .1f || Mathf.Abs(current.x - tip.x) > CollisionRadius)
                { PegIndex = -1; seatedTime = 0f; }
                else
                {
                    float bottomY = tip.y - peg.length + .05f + ProjectedHeight * .5f;
                    bool supported = supportPeg == PegIndex && Time.fixedTime - supportTime < Time.fixedDeltaTime * 2.5f;
                    bool seated = supported && current.y > bottomY - .12f && current.y < bottomY + (peg.capacity - 1) * StackSpacing + .12f && ContainsStem(current.x - tip.x) && Body.linearVelocity.magnitude < .5f;
                    seatedTime = seated ? seatedTime + Time.fixedDeltaTime : 0f;
                    if (!Captured && seatedTime >= .18f && game.TryOccupy(PegIndex, out _))
                    {
                        // Lock only after a physical landing, preserving the resting position
                        // and solid colliders so later rings can stack on this one.
                        Lock(current - tip);
                        return;
                    }
                }
            }
            if (pumpRemaining > 0f)
            {
                float fraction = Mathf.Min(1f, Time.fixedDeltaTime / pumpRemaining);
                Vector3 impulse = pendingImpulse * fraction;
                pendingImpulse -= impulse; pumpRemaining = Mathf.Max(0, pumpRemaining - Time.fixedDeltaTime);
                Body.AddForce(impulse, ForceMode.Impulse);
            }
            float flutter = Mathf.Sin(game.Elapsed * 2.1f + seed) * .045f;
            Vector2 environment = game.Environment.AccelerationAt(current) * Profile.Lift;
            Body.AddForce((new Vector3(game.Sensor.Tilt.x * 4f * Profile.Steering + flutter, -game.Level.settling * Profile.Settling, 0f) + (Vector3)environment) * Body.mass, ForceMode.Force);
        }

        void Lock(Vector2 offset)
        {
            Captured = true; lockedOffset = offset;
            pendingImpulse = Vector3.zero; pumpRemaining = 0f;
            Body.linearVelocity = Vector3.zero; Body.angularVelocity = Vector3.zero;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Body.isKinematic = true; game.OnRingCaptured(this);
        }

        bool CheckCollectorCatch(Vector2 current)
        {
            bool resting = false;
            for (int i = 0; i < game.Level.collectors.Length; i++)
            {
                var collector = game.Level.collectors[i];
                float bottom = collector.center.y - collector.height * .5f;
                bool supported = supportCollector == i && Time.fixedTime - collectorSupportTime < Time.fixedDeltaTime * 2.5f;
                if (!supported || Mathf.Abs(current.x - collector.center.x) > collector.width * .5f - CollisionRadius + .04f || current.y < bottom + .03f || current.y > bottom + collector.height + .03f || Body.linearVelocity.magnitude >= .5f) continue;
                resting = true; seatedTime += Time.fixedDeltaTime;
                if (seatedTime >= .18f && game.TryCollect(i))
                { CollectorIndex = i; Lock(Vector2.zero); return true; }
                break;
            }
            if (!resting) seatedTime = 0f;
            return false;
        }

        bool ContainsStem(float offset)
        {
            // Small contact tolerance covers the collider skin and moving-peg step.
            return Mathf.Abs(offset) <= InnerRadius * Profile.Scale - ToyPresentation.PegStemRadius + .012f;
        }

        void OnCollisionStay(Collision collision) { TrackSupport(collision); }
        void TrackSupport(Collision collision)
        {
            var peg = collision.collider.GetComponentInParent<PegSurface>();
            var ring = collision.collider.GetComponentInParent<FloatingRing>();
            var collector = collision.collider.GetComponentInParent<CollectorSurface>();
            // A threaded ring can rest on another ring even when that lower ring is
            // offset beside the peg; only this ring's own threading earns its slot.
            int index = peg != null ? peg.Index : ring != null && Threaded ? PegIndex : -1;
            int collectorIndex = collector != null ? collector.Index : ring != null && ring.Captured ? ring.CollectorIndex : -1;
            if (index < 0 && collectorIndex < 0) return;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > .45f)
                {
                    if (index >= 0) { supportPeg = index; supportTime = Time.fixedTime; }
                    if (collectorIndex >= 0) { supportCollector = collectorIndex; collectorSupportTime = Time.fixedTime; }
                    break;
                }
        }
        void OnCollisionEnter(Collision collision)
        {
            TrackSupport(collision);
            if (game == null || !game.Playing || collision.relativeVelocity.magnitude < .65f || Time.time - lastContact < .18f) return;
            lastContact = Time.time; Contact?.Invoke();
        }
    }
}
