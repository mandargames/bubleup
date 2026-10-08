using System;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class FloatingRing : MonoBehaviour
    {
        public const float Radius = .305f, Tube = .086f, Pitch = 58f, StackSpacing = .215f;
        const float PumpDuration = .22f;
        public Rigidbody Body { get; private set; }
        public Transform Visual;
        public bool Captured { get; private set; }
        public int PegIndex { get; private set; } = -1;
        public bool Threaded => PegIndex >= 0;
        public event Action Contact;
        GameSession game;
        Vector2 previous, lockedOffset;
        Vector3 savedVelocity, savedAngularVelocity, pendingImpulse;
        bool paused;
        float seed, seatedTime, lastContact, pumpRemaining, supportTime = -1f;
        int supportPeg = -1;

        public void Initialize(GameSession session, int index, Vector2 start)
        {
            game = session; seed = index * 2.37f;
            Body = gameObject.AddComponent<Rigidbody>();
            Body.mass = .035f;
            Body.useGravity = false; // Settling is gravity minus buoyancy, applied at every height.
            Body.linearDamping = game.Level.damping;
            // Keep the original stable ring angle: rolling a torus sideways on a peg can
            // wedge it because this toy deliberately constrains movement to one plane.
            Body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.solverIterations = 12; Body.solverVelocityIterations = 6;
            Body.maxLinearVelocity = 7f;
            Body.position = new Vector3(start.x, start.y, -.75f);
            Body.rotation = Quaternion.Euler(Pitch, 0, 0);
            transform.SetPositionAndRotation(Body.position, Body.rotation);
            if (Visual != null) Visual.localRotation = Quaternion.identity;
            previous = start;

            // Convex capsule segments preserve the torus's real opening and support CCD.
            // The visible mesh and collision shapes share the same rigid-body rotation.
            const int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments, b = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 from = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * Radius;
                Vector3 to = new Vector3(Mathf.Cos(b), Mathf.Sin(b), 0) * Radius;
                var part = new GameObject("Ring contact " + i); part.transform.SetParent(transform, false);
                part.transform.localPosition = (from + to) * .5f;
                part.transform.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
                var collider = part.AddComponent<CapsuleCollider>();
                collider.radius = Tube; collider.height = Vector3.Distance(from, to) + Tube * 2f;
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
            float lift = strength * Mathf.Lerp(1f, .72f, Mathf.InverseLerp(-2.4f, 3.6f, Body.position.y));
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
                previous = Body.position;
            }
        }

        void FixedUpdate()
        {
            if (game == null || paused) return;
            if (Captured)
            {
                var position = game.PegTip(PegIndex) + lockedOffset;
                Body.MovePosition(new Vector3(position.x, position.y, Body.position.z));
                return;
            }
            var current = (Vector2)Body.position;
            if (!Threaded && Body.linearVelocity.y < 0f)
            {
                for (int i = 0; i < game.Level.pegs.Length; i++)
                {
                    var tip = game.PegTip(i);
                    if (previous.y < tip.y || current.y > tip.y) continue;
                    float t = (previous.y - tip.y) / Mathf.Max(.0001f, previous.y - current.y);
                    if (Mathf.Abs(Mathf.Lerp(previous.x, current.x, t) - tip.x) > .15f) continue;
                    PegIndex = i; seatedTime = 0f;
                    break;
                }
            }
            previous = current;
            if (Threaded)
            {
                var tip = game.PegTip(PegIndex); var peg = game.Level.pegs[PegIndex];
                if (current.y > tip.y + .25f || current.y < tip.y - peg.length - .1f || Mathf.Abs(current.x - tip.x) > Radius + Tube)
                { PegIndex = -1; seatedTime = 0f; }
                else
                {
                    float bottomY = tip.y - peg.length + .05f + Radius * Mathf.Cos(Pitch * Mathf.Deg2Rad) + Tube;
                    bool supported = supportPeg == PegIndex && Time.fixedTime - supportTime < Time.fixedDeltaTime * 2.5f;
                    bool seated = supported && current.y > bottomY - .12f && current.y < bottomY + (peg.capacity - 1) * StackSpacing + .12f && Mathf.Abs(current.x - tip.x) < .19f && Body.linearVelocity.magnitude < .5f;
                    seatedTime = seated ? seatedTime + Time.fixedDeltaTime : 0f;
                    if (!Captured && seatedTime >= .18f && game.TryOccupy(PegIndex, out _))
                    {
                        Captured = true;
                        // Lock only after a physical landing, preserving the resting position
                        // and solid colliders so later rings can stack on this one.
                        lockedOffset = current - tip;
                        pendingImpulse = Vector3.zero; pumpRemaining = 0f;
                        Body.linearVelocity = Vector3.zero; Body.angularVelocity = Vector3.zero;
                        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        Body.isKinematic = true;
                        game.OnRingCaptured(this);
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
            Body.AddForce(new Vector3(game.Sensor.Tilt.x * 4f + flutter, -game.Level.settling, 0f) * Body.mass, ForceMode.Force);
        }

        void OnCollisionStay(Collision collision) { TrackSupport(collision); }
        void TrackSupport(Collision collision)
        {
            var peg = collision.collider.GetComponentInParent<PegSurface>();
            var ring = collision.collider.GetComponentInParent<FloatingRing>();
            // A threaded ring can rest on another ring even when that lower ring is
            // offset beside the peg; only this ring's own threading earns its slot.
            int index = peg != null ? peg.Index : ring != null && Threaded ? PegIndex : -1;
            if (index < 0) return;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > .45f)
                { supportPeg = index; supportTime = Time.fixedTime; break; }
        }
        void OnCollisionEnter(Collision collision)
        {
            TrackSupport(collision);
            if (game == null || !game.Playing || collision.relativeVelocity.magnitude < .65f || Time.time - lastContact < .18f) return;
            lastContact = Time.time; Contact?.Invoke();
        }
    }
}
