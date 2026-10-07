using System.Collections;
using System.IO;
using NUnit.Framework;
using PocketToys.WaterRingToss;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketToys.Tests
{
    public sealed class SpikePhysicsTests
    {
        GameObject root;
        WaterRingTossConfig config;
        SpikeController game;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            config = ScriptableObject.CreateInstance<WaterRingTossConfig>();
            root = new GameObject("Test spike");
            game = root.AddComponent<SpikeController>();
            game.config = config;
            yield return null;
            game.Feedback.SoundEnabled = false;
            game.Feedback.HapticsEnabled = false;
            if (game.Paused) game.TogglePause();
            yield return new WaitForFixedUpdate();
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            Object.Destroy(root);
            Object.Destroy(config);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PumpLiftsRingAndRestartClearsAttempt()
        {
            float start = game.Ring.Body.position.y;
            game.Pump(true);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Ring.Body.linearVelocity.y, Is.GreaterThan(0f));
            Assert.That(game.Ring.Body.position.y, Is.GreaterThan(start));
            Assert.That(game.PumpCount, Is.EqualTo(1));
            game.Restart();
            Assert.That(game.PumpCount, Is.Zero);
            Assert.That(game.Ring.Body.position.x, Is.EqualTo(config.ringStart.x).Within(.001f));
            Assert.That(game.Ring.Body.linearVelocity, Is.EqualTo(Vector3.zero));
            Assert.IsFalse(game.Ring.IsCaptured);
        }

        [UnityTest]
        public IEnumerator PauseFreezesPhysicsAndRejectsPumps()
        {
            game.Pump(true);
            yield return new WaitForFixedUpdate();
            game.TogglePause();
            Vector3 position = game.Ring.Body.position;
            int pumps = game.PumpCount;
            game.Pump(false);
            yield return new WaitForSeconds(.12f);
            Assert.That(game.Ring.Body.position, Is.EqualTo(position));
            Assert.That(game.PumpCount, Is.EqualTo(pumps));
            game.TogglePause();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Ring.Body.position.y, Is.GreaterThan(position.y));
        }

        [UnityTest]
        public IEnumerator FallingRingCapturesAndRestartReleasesIt()
        {
            int captures = 0;
            game.Ring.Landed += () => captures++;
            game.Ring.Body.position = new Vector3(config.pegTip.x, config.pegTip.y + .3f, 0f);
            game.Ring.Body.linearVelocity = Vector3.down;
            yield return new WaitForSeconds(.8f);
            Assert.IsTrue(game.Ring.IsCaptured);
            Assert.That(captures, Is.EqualTo(1));
            Assert.That(game.Ring.Body.position.y, Is.EqualTo(config.pegTip.y - config.pegLength + .2f).Within(.02f));
            game.Restart();
            Assert.IsFalse(game.Ring.IsCaptured);
            Assert.IsFalse(game.Ring.Body.isKinematic);
        }

        [UnityTest]
        public IEnumerator TankContainsSettlingRing()
        {
            yield return new WaitForSeconds(1.2f);
            Assert.That(game.Ring.Body.position.y, Is.GreaterThan(-config.tankSize.y / 2f));
            Assert.That(Mathf.Abs(game.Ring.Body.position.x), Is.LessThan(config.tankSize.x / 2f));
        }

        [UnityTest]
        public IEnumerator PumpAndTiltCanWinFromStartingPosition()
        {
            // Drive only public controls: proves the actual spawn is winnable without teleporting.
            game.Sensor.SetSensorMode(false);
            for (int step = 0; step < 900 && !game.Ring.IsCaptured; step++)
            {
                var position = game.Ring.Body.position;
                var velocity = game.Ring.Body.linearVelocity;
                game.Sensor.SetVirtualTilt(Mathf.Clamp(-2f * (position.x - config.pegTip.x) - 1.2f * velocity.x, -1f, 1f));
                if (position.y < config.pegTip.y - .1f && velocity.y < 1.5f) game.Pump(position.x <= config.pegTip.x);
                yield return new WaitForFixedUpdate();
            }
            Assert.IsTrue(game.Ring.IsCaptured, "Pump and tilt controls should allow a catch from the configured spawn.");
            Assert.That(game.PumpCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator RenderPrototypePreview()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.Ignore("Preview requires a graphics device.");
            yield return null;
            var camera = game.GameCamera;
            var render = new RenderTexture(600, 1000, 24);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var pixels = new Texture2D(600, 1000, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = render;
                camera.aspect = .6f;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0f, 0f, 600f, 1000f), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory("Logs");
                File.WriteAllBytes("Logs/prototype-preview.png", pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                Object.Destroy(render);
                Object.Destroy(pixels);
            }
        }
    }
}
