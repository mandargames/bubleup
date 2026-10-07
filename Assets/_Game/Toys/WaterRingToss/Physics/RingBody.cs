using System;
using UnityEngine;
using PocketToys.Core.Input;

namespace PocketToys.WaterRingToss
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RingBody : MonoBehaviour
    {
        public event Action Landed;
        public event Action Contact;
        public Rigidbody Body { get; private set; }
        public bool IsCaptured { get; private set; }
        public bool IsPaused { get; private set; }
        WaterRingTossConfig config;
        SensorInputService input;
        Vector2 previous;
        Vector3 captureStart, savedVelocity;
        float settleElapsed, simulationTime, lastContact;

        public void Initialize(WaterRingTossConfig settings, SensorInputService sensor)
        {
            config = settings;
            input = sensor;
            Body = GetComponent<Rigidbody>();
            Body.useGravity = false;
            Body.mass = 1f;
            Body.linearDamping = settings.linearDamping;
            Body.angularDamping = 3f;
            Body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            ResetRing();
        }

        public void ResetRing()
        {
            IsCaptured = false;
            IsPaused = false;
            Body.isKinematic = false;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.position = new Vector3(config.ringStart.x, config.ringStart.y, 0f);
            transform.position = Body.position;
            previous = config.ringStart;
            simulationTime = 0f;
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;
            IsPaused = paused;
            if (IsCaptured) return;
            if (paused) savedVelocity = Body.linearVelocity;
            Body.isKinematic = paused;
            if (!paused) Body.linearVelocity = savedVelocity;
        }

        public void ApplyPump(bool left)
        {
            if (IsCaptured || IsPaused) return;
            var nozzle = left ? config.leftPump : config.rightPump;
            Body.AddForce(WaterForces.PumpImpulse(Body.position, nozzle, left ? config.leftStrength : config.rightStrength, config.pumpWidth, config.inwardForce), ForceMode.VelocityChange);
        }

        void FixedUpdate()
        {
            if (config == null || IsPaused) return;
            if (IsCaptured)
            {
                settleElapsed += Time.fixedDeltaTime;
                var destination = new Vector3(config.pegTip.x, config.pegTip.y - config.pegLength + .2f, -.08f);
                Body.MovePosition(Vector3.Lerp(captureStart, destination, Mathf.SmoothStep(0f, 1f, settleElapsed / config.captureSettleSeconds)));
                return;
            }
            var current = (Vector2)Body.position;
            if (WaterForces.CanCapture(previous, current, Body.linearVelocity, config))
            {
                IsCaptured = true;
                captureStart = Body.position;
                settleElapsed = 0f;
                Body.linearVelocity = Vector3.zero;
                Body.isKinematic = true;
                Landed?.Invoke();
                return;
            }
            previous = current;
            simulationTime += Time.fixedDeltaTime;
            float drift = Mathf.Sin(simulationTime * 2.3f) * config.turbulenceAcceleration;
            Body.AddForce(new Vector3(input.Tilt.x * config.tiltAcceleration + drift, -config.settlingAcceleration, 0f), ForceMode.Acceleration);
            Body.linearVelocity = Vector3.ClampMagnitude(Body.linearVelocity, config.maximumSpeed);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < .5f || Time.time - lastContact < .12f) return;
            lastContact = Time.time;
            Contact?.Invoke();
        }
    }
}
