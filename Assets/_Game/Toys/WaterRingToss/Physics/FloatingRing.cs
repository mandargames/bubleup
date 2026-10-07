using System;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class FloatingRing : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }
        public Transform Visual;
        public bool Captured { get; private set; }
        public int PegIndex { get; private set; } = -1;
        public event Action Contact;
        GameSession game;
        Vector2 previous;
        Vector3 savedVelocity, catchStart;
        bool paused;
        float seed, settle, lastContact;
        int slot;

        public void Initialize(GameSession session, int index, Vector2 start)
        {
            game = session; seed = index * 2.37f;
            Body = gameObject.AddComponent<Rigidbody>();
            Body.useGravity = false;
            Body.linearDamping = game.Level.damping;
            Body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.position = new Vector3(start.x, start.y, -.75f);
            transform.position = Body.position;
            previous = start;
            // A shallow capsule preserves clear separation between floating rings without fragile mesh collisions.
            var collider = gameObject.AddComponent<CapsuleCollider>();
            collider.direction = 0; collider.radius = .19f; collider.height = .78f;
        }

        public void Pump(bool left)
        {
            if (Captured || paused) return;
            float nozzle = left ? -1.55f : 1.55f;
            float dx = Body.position.x - nozzle;
            float influence = Mathf.Exp(-dx * dx / 3.8f);
            float strength = (left ? game.Level.leftStrength : game.Level.rightStrength) * influence;
            float lift = strength * Mathf.Lerp(1f, .72f, Mathf.InverseLerp(-2.4f, 3.6f, Body.position.y));
            Body.linearVelocity = Vector3.ClampMagnitude(Body.linearVelocity + new Vector3((left ? 1f : -1f) * lift * .24f, lift, 0f), 7f);
        }

        public void Pause(bool value)
        {
            if (paused == value) return;
            paused = value;
            if (Captured) return;
            if (value) savedVelocity = Body.linearVelocity;
            Body.isKinematic = value;
            if (!value) Body.linearVelocity = savedVelocity;
        }

        void FixedUpdate()
        {
            if (game == null || paused) return;
            if (Captured)
            {
                settle += Time.fixedDeltaTime;
                var peg = game.Level.pegs[PegIndex];
                var target = game.PegTip(PegIndex);
                target.y -= peg.length - .18f - slot * .17f;
                Body.MovePosition(Vector3.Lerp(catchStart, new Vector3(target.x, target.y, -.75f), Mathf.SmoothStep(0f, 1f, settle / .36f)));
                return;
            }
            var current = (Vector2)Body.position;
            if (Body.linearVelocity.y < 0f && Body.linearVelocity.magnitude < 5.2f)
            {
                for (int i = 0; i < game.Level.pegs.Length; i++)
                {
                    var tip = game.PegTip(i);
                    if (previous.y < tip.y || current.y > tip.y) continue;
                    float t = (previous.y - tip.y) / Mathf.Max(.0001f, previous.y - current.y);
                    if (Mathf.Abs(Mathf.Lerp(previous.x, current.x, t) - tip.x) > .235f) continue;
                    if (!game.TryOccupy(i, out slot)) continue;
                    Captured = true; PegIndex = i; catchStart = Body.position;
                    Body.linearVelocity = Vector3.zero; Body.isKinematic = true;
                    GetComponent<Collider>().enabled = false;
                    game.OnRingCaptured(this);
                    break;
                }
            }
            previous = current;
            if (Captured) return;
            float flutter = Mathf.Sin(game.Elapsed * 2.1f + seed) * .085f;
            Body.AddForce(new Vector3(game.Sensor.Tilt.x * 5.5f + flutter, -game.Level.settling, 0f), ForceMode.Acceleration);
            Body.linearVelocity = Vector3.ClampMagnitude(Body.linearVelocity, 7f);
        }

        void LateUpdate()
        {
            if (Visual == null || game == null) return;
            float sway = Captured || game.Settings.reduceMotion ? 0f : Mathf.Sin(game.Elapsed * 2.6f + seed) * 12f;
            float lean = Captured ? 0f : -Body.linearVelocity.x * 7f;
            Visual.localRotation = Quaternion.Slerp(Visual.localRotation, Quaternion.Euler(58f + sway, 0f, lean), Time.deltaTime * 7f);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (game == null || !game.Playing || collision.relativeVelocity.magnitude < .65f || Time.time - lastContact < .18f) return;
            lastContact = Time.time; Contact?.Invoke();
        }
    }
}
