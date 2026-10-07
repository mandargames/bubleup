using NUnit.Framework;
using PocketToys.Core.Input;
using PocketToys.WaterRingToss;
using UnityEngine;

namespace PocketToys.Tests
{
    public sealed class SpikeLogicTests
    {
        [Test]
        public void CalibratedPoseIsNeutralAndExtremeInputIsClamped()
        {
            var filter = new TiltFilter();
            var pose = new Vector2(.2f, -.7f);
            filter.Calibrate(pose);
            Assert.That(filter.Step(pose, 3f, .05f, 0f, .02f), Is.EqualTo(Vector2.zero));
            Assert.That(filter.Step(Vector2.one * 100f, 3f, .05f, 0f, .02f).magnitude, Is.EqualTo(1f).Within(.0001f));
        }

        [Test]
        public void FilteringMatchesAcrossSamplingRates()
        {
            var a = new TiltFilter(); var b = new TiltFilter();
            for (int i = 0; i < 60; i++) a.Step(Vector2.right, 1f, .05f, .2f, 1f / 60f);
            for (int i = 0; i < 120; i++) b.Step(Vector2.right, 1f, .05f, .2f, 1f / 120f);
            Assert.That(a.Value.x, Is.EqualTo(b.Value.x).Within(.0001f));
        }

        [Test]
        public void DeadZoneSuppressesSmallSensorJitter()
        {
            var filter = new TiltFilter();
            Assert.That(filter.Step(new Vector2(.02f, .01f), 1f, .06f, 0f, .02f), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void PumpIsLocalAndPushesInward()
        {
            var nozzle = new Vector2(-1.5f, -3f);
            var nearby = WaterForces.PumpImpulse(new Vector2(-1.5f, -2f), nozzle, 5f, 2f, .5f);
            var far = WaterForces.PumpImpulse(new Vector2(2f, -2f), nozzle, 5f, 2f, .5f);
            Assert.That(nearby.y, Is.EqualTo(5f));
            Assert.That(nearby.x, Is.GreaterThan(0f));
            Assert.That(far, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void CaptureRequiresDescendingAlignedCrossing()
        {
            var config = ScriptableObject.CreateInstance<WaterRingTossConfig>();
            try
            {
                Assert.IsTrue(WaterForces.CanCapture(new Vector2(0f, 1f), new Vector2(0f, .7f), Vector2.down, config));
                Assert.IsFalse(WaterForces.CanCapture(new Vector2(1f, 1f), new Vector2(1f, .7f), Vector2.down, config));
                Assert.IsFalse(WaterForces.CanCapture(new Vector2(0f, .7f), new Vector2(0f, 1f), Vector2.up, config));
                Assert.IsFalse(WaterForces.CanCapture(new Vector2(0f, 1f), new Vector2(0f, .7f), Vector2.down * 10f, config));
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void ConfigRejectsInvalidContent()
        {
            var config = ScriptableObject.CreateInstance<WaterRingTossConfig>();
            try
            {
                Assert.IsTrue(config.IsValid(out _));
                config.ringStart = new Vector2(100f, 0f);
                Assert.IsFalse(config.IsValid(out _));
                config.ringStart = new Vector2(-1f, -2f);
                config.pumpWidth = 0f;
                Assert.IsFalse(config.IsValid(out _));
            }
            finally { Object.DestroyImmediate(config); }
        }
    }
}
