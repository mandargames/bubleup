#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PocketToys.WaterRingToss.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PocketToys.Tests
{
    public sealed class GameJourneyTests
    {
        GameObject root; GameSession game; string saveFolder;
        [UnitySetUp]
        public IEnumerator Setup()
        {
            PlayModeWindow.SetCustomRenderingResolution(720, 1280, "Pocket Toys Portrait");
            saveFolder = Path.Combine(Path.GetTempPath(), "PocketToysJourney-" + Guid.NewGuid());
            GameSession.SavePathOverride = Path.Combine(saveFolder, "save.json");
            root = new GameObject("Journey test"); game = root.AddComponent<GameSession>();
            game.campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>("Assets/_Game/Toys/WaterRingToss/Levels/FiveAdventures.asset");
            yield return null;
            game.Settings.music = false; game.Settings.sound = false; game.Settings.haptics = false; game.Settings.motion = false;
            game.SaveSettings(); yield return null;
            Assert.That(game.Rings[0].Body.position.x, Is.EqualTo(game.Level.rings[0].x).Within(.02f));
            Assert.That(game.Rings[0].Body.position.z, Is.EqualTo(-.75f).Within(.02f));
        }
        [UnityTearDown]
        public IEnumerator Teardown()
        {
            Time.timeScale = 1f; Object.Destroy(root); GameSession.SavePathOverride = null;
            yield return null;
            if (Directory.Exists(saveFolder)) Directory.Delete(saveFolder, true);
        }

        [UnityTest]
        public IEnumerator MenusPauseRestartAndSettingsKeepStateConsistent()
        {
            Assert.IsTrue(game.Ready); Assert.That(game.Screen, Is.EqualTo(GameScreen.Home));
            Assert.IsFalse(game.Unlocked(1)); game.StartLevel(1); Assert.That(game.Screen, Is.EqualTo(GameScreen.Home));
            game.StartLevel(0); game.Pump(true); yield return new WaitForFixedUpdate();
            Assert.That(game.Pumps, Is.EqualTo(1));
            game.OpenSettings(); var position = game.Rings[0].Body.position;
            yield return new WaitForSeconds(.15f); Assert.That(game.Rings[0].Body.position, Is.EqualTo(position));
            game.Settings.sensitivity = 3.1f; game.CloseSettings();
            Assert.That(game.Screen, Is.EqualTo(GameScreen.Paused)); game.Resume();
            yield return new WaitForFixedUpdate(); game.Restart();
            Assert.That(game.Pumps, Is.Zero); Assert.That(game.Caught, Is.Zero); Assert.That(game.Elapsed, Is.EqualTo(0f).Within(.1f));
            Assert.That(game.Sensor.sensitivity, Is.EqualTo(3.1f));
        }

        [UnityTest]
        public IEnumerator AllFiveLevelsAreWinnableThroughPumpAndTilt()
        {
            Time.timeScale = 5f;
            for (int level = 0; level < 5; level++)
            {
                game.StartLevel(level); FloatingRing selected = null; bool aboveTip = false; float stalled = 0f, reverseUntil = 0f, reverseDirection = 0f;
                for (int step = 0; step < 15000 && game.Playing; step++)
                {
                    if (selected == null || selected.Captured) { selected = game.Rings.Where(x => !x.Captured).OrderByDescending(x => x.Body.position.y).FirstOrDefault(); aboveTip = false; stalled = 0f; reverseUntil = 0f; }
                    if (selected == null) { game.Sensor.SetVirtualTilt(0f); yield return new WaitForFixedUpdate(); continue; }
                    var pos = selected.Body.position; var velocity = selected.Body.linearVelocity;
                    stalled = velocity.magnitude < .2f ? stalled + Time.fixedDeltaTime : 0f;
                    int peg = Enumerable.Range(0, game.Level.pegs.Length).Where(i => game.Occupied(i) < game.Level.pegs[i].capacity)
                        .OrderByDescending(i => game.Level.pegs[i].capacity).ThenBy(i => Mathf.Abs(game.PegTip(i).x - pos.x)).First();
                    var target = game.PegTip(peg);
                    if (selected.Threaded) { peg = selected.PegIndex; target = game.PegTip(peg); }
                    bool fullPeg = selected.Threaded && game.Occupied(peg) >= game.Level.pegs[peg].capacity;
                    if (pos.y > target.y + .45f) aboveTip = true;
                    if (pos.y < target.y - .35f && !selected.Threaded) aboveTip = false;
                    float targetX = target.x;
                    if (!aboveTip && !selected.Threaded)
                    {
                        float side = Mathf.Abs(target.x) > 1.25f ? -Mathf.Sign(target.x) : Mathf.Abs(target.x) > .5f ? Mathf.Sign(target.x) : pos.x < target.x ? -1f : 1f;
                        float offset = Mathf.Abs(target.x) > 1.25f ? (pos.y < target.y - game.Level.pegs[peg].length + .28f ? 1.1f : .48f) : .98f;
                        targetX = Mathf.Clamp(target.x + side * offset, -2.16f, 2.16f);
                    }
                    if (game.Level.baffles.Length > 0 && pos.y < .5f && Mathf.Abs(targetX) < 1.25f) targetX = targetX < 0f ? -1.5f : 1.5f;
                    game.Sensor.SetVirtualTilt(Mathf.Clamp(-2.8f * (pos.x - targetX) - 1.3f * velocity.x, -1f, 1f));
                    if (game.Elapsed < reverseUntil) game.Sensor.SetVirtualTilt(reverseDirection);
                    else if ((selected.Threaded || LocalPlayerAudit.HasLiftClearance(game, pos)) && (!aboveTip || fullPeg) && (!selected.Threaded || fullPeg) && (selected.Threaded || pos.y < target.y - game.Level.pegs[peg].length - .6f || pos.y > target.y - game.Level.pegs[peg].length + .15f || Mathf.Abs(pos.x - targetX) < .08f) && pos.y < target.y + .5f && velocity.y < 1.6f)
                    { game.Pump(pos.x <= 0f); stalled = 0f; }
                    else if (stalled > 1.5f)
                    {
                        if (selected.Threaded || LocalPlayerAudit.HasLiftClearance(game, pos)) game.Pump(pos.x <= 0f);
                        else if (!LocalPlayerAudit.LiftLooseRing(game, selected))
                        {
                            reverseDirection = pos.x < targetX ? -1f : 1f;
                            reverseUntil = game.Elapsed + .65f;
                        }
                        stalled = 0f;
                    }
                    yield return new WaitForFixedUpdate();
                }
                Debug.Log("Control audit: " + game.Level.title + ": " + game.Caught + "/" + game.Rings.Count + ", " + game.Elapsed.ToString("0.0") + " seconds, " + game.Pumps + " pumps.");
                if (game.Playing) foreach (var ring in game.Rings) Debug.Log("Unfinished ring: position=" + ring.Body.position + " velocity=" + ring.Body.linearVelocity + " threaded=" + ring.Threaded + " captured=" + ring.Captured);
                Assert.That(game.Screen, Is.EqualTo(GameScreen.Complete), game.Level.title + " should be winnable through actual controls.");
                Assert.That(game.Progress.Data.Record(game.Level.id).stars, Is.GreaterThan(0));
                Assert.IsTrue(File.Exists(GameSession.SavePathOverride));
            }
        }

        [UnityTest]
        public IEnumerator PegStemBlocksASideImpactBelowItsTip()
        {
            game.StartLevel(0); var ring = game.Rings[0];
            ring.Body.position = new Vector3(-.85f, .35f, -.75f);
            ring.Body.linearVelocity = new Vector3(6, 0, 0);
            yield return new WaitForSeconds(.18f);
            Assert.That(ring.Body.position.x, Is.LessThan(-.25f), "The visible peg stem must block the ring's rim.");
            Assert.IsFalse(ring.Captured);
        }

        [UnityTest]
        public IEnumerator LandingShelfBlocksAnUpwardRing()
        {
            game.StartLevel(0); var ring = game.Rings[0];
            float shelfY = game.Level.pegs[0].tip.y - game.Level.pegs[0].length;
            ring.Body.position = new Vector3(0, shelfY - .75f, -.75f);
            ring.Body.linearVelocity = Vector3.up * 6f;
            yield return new WaitForSeconds(.2f);
            Assert.That(ring.Body.position.y, Is.LessThan(shelfY - .15f), "A pump cannot push a ring through the bottom of the shelf.");
            Assert.IsFalse(ring.Captured);
        }

        [UnityTest]
        public IEnumerator RingCanRiseBesideCompactPegSupport()
        {
            game.StartLevel(0); var ring = game.Rings[0];
            float shelfY = game.Level.pegs[0].tip.y - game.Level.pegs[0].length;
            foreach (float side in new[] { -1f, 1f })
            {
                ring.Body.position = new Vector3(side * .74f, shelfY - .55f, -.75f);
                ring.Body.linearVelocity = Vector3.up * 6f;
                yield return new WaitForSeconds(.2f);
                Assert.That(ring.Body.position.y, Is.GreaterThan(shelfY + .25f), "The side route must be clear without pushing through a support.");
                Assert.IsFalse(ring.Captured);
            }
        }

        [UnityTest]
        public IEnumerator RingSlidesUnderGravityThenPhysicallyStacks()
        {
            game.Progress.Data.Record(game.campaign.levels[0].id).stars = 3;
            game.StartLevel(1);
            var first = game.Rings[0]; var second = game.Rings[1]; var tip = game.PegTip(0);
            second.Body.position = new Vector3(2f, -1.85f, -.75f);
            first.Body.position = new Vector3(tip.x, tip.y + .6f, -.75f);
            first.Body.linearVelocity = Vector3.down;
            for (int i = 0; i < 150 && !first.Threaded; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(first.Threaded, "The open ring must pass over the peg's tip.");
            Assert.IsFalse(first.Captured, "Crossing the tip alone must not score a catch.");
            Assert.IsFalse(first.Body.isKinematic, "The ring must retain mass while sliding down the peg.");
            float threadedY = first.Body.position.y;
            yield return new WaitForSeconds(.18f);
            Assert.That(first.Body.position.y, Is.LessThan(threadedY - .05f));
            for (int i = 0; i < 200 && !first.Captured; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(first.Captured, "The ring must settle on the shelf before scoring.");
            Assert.IsTrue(first.Body.isKinematic, "A landed ring must lock onto the peg.");
            Assert.IsTrue(first.GetComponentsInChildren<Collider>().All(c => c.enabled));
            second.Body.position = new Vector3(tip.x, tip.y + .65f, -.75f);
            second.Body.linearVelocity = Vector3.down;
            for (int i = 0; i < 250 && !second.Captured; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(second.Captured, "The second ring must land on the first ring's colliders.");
            Assert.That(second.Body.position.y - first.Body.position.y, Is.InRange(.16f, .34f));
            yield return new WaitForSeconds(.4f);
            Assert.That(game.Screen, Is.EqualTo(GameScreen.Complete));
        }

        [UnityTest]
        public IEnumerator SeatedRingStaysLockedThroughPumpsSteeringAndPause()
        {
            game.Progress.Data.Record(game.campaign.levels[0].id).stars = 3; game.StartLevel(1);
            var ring = game.Rings[0]; var tip = game.PegTip(0);
            game.Rings[1].Body.position = new Vector3(2f, -1.85f, -.75f);
            ring.Body.position = new Vector3(tip.x, tip.y + .6f, -.75f); ring.Body.linearVelocity = Vector3.down;
            for (int i = 0; i < 200 && !ring.Captured; i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(ring.Captured); Assert.IsTrue(ring.Body.isKinematic);
            Assert.That(game.Caught, Is.EqualTo(1)); Assert.That(game.Occupied(0), Is.EqualTo(1));
            game.Pause(); var restingPosition = ring.Body.position;
            yield return new WaitForSeconds(.2f);
            Assert.That(ring.Body.position, Is.EqualTo(restingPosition));
            game.Resume();
            for (int i = 0; i < 8; i++)
            {
                game.Sensor.SetVirtualTilt(i < 4 ? -1f : 1f);
                game.Pump(true); game.Pump(false);
                yield return new WaitForSeconds(.2f);
                Assert.That(Vector3.Distance(ring.Body.position, restingPosition), Is.LessThan(.005f));
                Assert.IsTrue(ring.Captured); Assert.IsTrue(ring.Body.isKinematic);
                Assert.That(game.Caught, Is.EqualTo(1)); Assert.That(game.Occupied(0), Is.EqualTo(1));
            }
            Assert.IsTrue(ring.GetComponentsInChildren<Collider>().All(c => c.enabled));
            game.Restart();
            Assert.That(game.Caught, Is.Zero); Assert.That(game.Occupied(0), Is.Zero);
            Assert.IsTrue(game.Rings.All(r => !r.Captured && !r.Body.isKinematic));
        }

        [UnityTest]
        public IEnumerator LockedRingFollowsMovingPeg()
        {
            for (int i = 0; i < 4; i++) game.Progress.Data.Record(game.campaign.levels[i].id).stars = 3;
            game.StartLevel(4);
            var ring = game.Rings[0]; var tip = game.PegTip(1);
            ring.Body.position = new Vector3(tip.x, tip.y + .6f, -.75f);
            ring.Body.linearVelocity = Vector3.down;
            for (int i = 0; i < 250 && !ring.Captured; i++)
            {
                game.Sensor.SetVirtualTilt(Mathf.Clamp(-2.8f * (ring.Body.position.x - game.PegTip(1).x) - 1.3f * ring.Body.linearVelocity.x, -1f, 1f));
                yield return new WaitForFixedUpdate();
            }
            Assert.IsTrue(ring.Captured); Assert.That(ring.PegIndex, Is.EqualTo(1));
            Vector2 offset = (Vector2)ring.Body.position - game.PegTip(1);
            float startX = ring.Body.position.x;
            for (int i = 0; i < 8; i++)
            {
                game.Pump(true); game.Pump(false);
                yield return new WaitForSeconds(.2f);
                Assert.That(Vector2.Distance((Vector2)ring.Body.position - game.PegTip(1), offset), Is.LessThan(.02f));
                Assert.IsTrue(ring.Captured); Assert.IsTrue(ring.Body.isKinematic);
            }
            Assert.That(Mathf.Abs(ring.Body.position.x - startX), Is.GreaterThan(.03f));
        }

        [UnityTest]
        public IEnumerator SensitivitySettingSoftensVirtualSteering()
        {
            game.Settings.sensitivity = .8f; game.SaveSettings();
            game.Sensor.SetVirtualTilt(1f);
            yield return new WaitForSeconds(.6f);
            float gentle = game.Sensor.Tilt.x;
            game.Settings.sensitivity = 2.5f; game.SaveSettings();
            game.Sensor.SetVirtualTilt(1f);
            yield return new WaitForSeconds(.6f);
            Assert.That(gentle, Is.InRange(.2f, .4f));
            Assert.That(game.Sensor.Tilt.x, Is.GreaterThan(gentle * 2f));
        }

        [UnityTest]
        public IEnumerator BothPumpsLiftWithoutSidewaysShove()
        {
            foreach (bool left in new[] { true, false })
            {
                game.StartLevel(0); var ring = game.Rings[0];
                float x = left ? -ToyPresentation.NozzleX : ToyPresentation.NozzleX;
                ring.Body.position = new Vector3(x, -1.85f, -.75f);
                ring.Body.linearVelocity = Vector3.zero;
                game.Pump(left);
                yield return new WaitForSeconds(.35f);
                Assert.That(ring.Body.position.y, Is.GreaterThan(-1.15f), "Each pump must still provide useful lift.");
                Assert.That(Mathf.Abs(ring.Body.position.x - x), Is.LessThan(.03f), "Neither pump should push a resting ring sideways.");
                Assert.That(Mathf.Abs(ring.Body.linearVelocity.x), Is.LessThan(.05f));
            }
        }

        [UnityTest]
        public IEnumerator SteeringAndPumpsReachBothOuterCorners()
        {
            foreach (float side in new[] { -1f, 1f })
            {
                game.StartLevel(0); var ring = game.Rings[0];
                game.Sensor.SetVirtualTilt(side);
                for (int step = 0; step < 250 && ring.Body.position.x * side < 2.92f; step++)
                    yield return new WaitForFixedUpdate();
                Assert.That(ring.Body.position.x * side, Is.GreaterThan(2.9f), "Steering must reach the expanded outer lane.");
                for (int tap = 0; tap < 10; tap++)
                {
                    game.Pump(side < 0);
                    yield return new WaitForSeconds(.25f);
                }
                Assert.That(ring.Body.position.x * side, Is.GreaterThan(2.9f), "Pumping must not drag a ring back toward the center.");
                Assert.That(ring.Body.position.y, Is.GreaterThan(ToyPresentation.TankTop - .6f), "The corner jet must reach the top of the expanded tank.");
                Assert.That(Mathf.Abs(ring.Body.position.x), Is.LessThan(ToyPresentation.TankHalfWidth - .3f));
                Assert.That(ring.Body.position.y, Is.LessThan(ToyPresentation.TankTop - .15f));
                game.Sensor.SetVirtualTilt(-side);
                yield return new WaitForSeconds(1.1f);
                Assert.That(ring.Body.position.x * side, Is.LessThan(2.5f), "A ring must be able to steer back out of a corner.");
            }
        }

        [UnityTest]
        public IEnumerator WaterJetBuildsMomentumAndPausePreservesIt()
        {
            game.StartLevel(0); var ring = game.Rings[0];
            game.Pump(true);
            Assert.That(ring.Body.linearVelocity.magnitude, Is.LessThan(.01f), "Pumping queues water force instead of replacing velocity.");
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            float initialLift = ring.Body.linearVelocity.y;
            Assert.That(initialLift, Is.GreaterThan(0f));
            game.Pause(); Vector3 position = ring.Body.position;
            yield return new WaitForSeconds(.2f);
            Assert.That(ring.Body.position, Is.EqualTo(position));
            game.Resume(); yield return new WaitForSeconds(.12f);
            Assert.That(ring.Body.linearVelocity.y, Is.GreaterThan(initialLift));
            yield return new WaitForSeconds(.1f);
            ring.Body.position = new Vector3(-1.8f, 1.8f, -.75f); ring.Body.linearVelocity = Vector3.zero;
            yield return new WaitForSeconds(.25f);
            Assert.That(ring.Body.linearVelocity.y, Is.LessThan(0f), "Water drag and weight should bring an unpumped ring back down.");
        }

        [UnityTest]
        public IEnumerator RingToRingImpactTransfersMomentum()
        {
            game.Progress.Data.Record(game.campaign.levels[0].id).stars = 3; game.StartLevel(1);
            var first = game.Rings[0]; var second = game.Rings[1];
            first.Body.position = new Vector3(-1.2f, 2.8f, -.75f);
            second.Body.position = new Vector3(0, 2.8f, -.75f);
            first.Body.linearVelocity = Vector3.right * 6f; second.Body.linearVelocity = Vector3.zero;
            yield return new WaitForSeconds(.2f);
            Assert.That(second.Body.linearVelocity.x, Is.GreaterThan(.5f));
            Assert.That(first.Body.position.x, Is.LessThan(second.Body.position.x - .4f), "Rings must not tunnel through each other.");
        }

        [UnityTest]
        public IEnumerator CaptureUiPreviewsAndValidateButtons()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.Ignore("Graphics required.");
            game.Settings.reduceMotion = true; game.SaveSettings();
            yield return null;
            Capture("home");
            game.StartLevel(0); yield return new WaitForSeconds(.1f); Capture("level-01");
            var left = game.Hud.GetComponentsInChildren<PumpPress>().First();
            left.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
            Assert.That(game.Pumps, Is.EqualTo(1));
            game.Pause(); yield return null; Capture("pause");
            game.SetScreen(GameScreen.Levels); yield return null; Capture("levels");
            game.OpenSettings(); yield return null; Capture("settings");
            game.SetScreen(GameScreen.Collection); yield return null; Capture("collection");
            for (int i = 0; i < 4; i++) game.Progress.Data.Record(game.campaign.levels[i].id).stars = 3;
            game.StartLevel(4); yield return new WaitForSeconds(.1f); Capture("level-05");
            Debug.Log("Preview ring position: " + game.Rings[0].Body.position + " visual: " + game.Rings[0].Visual.position);
            Assert.That(game.Hud.GetComponentsInChildren<Slider>().Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TallPortraitKeepsToyControlsAligned()
        {
            PlayModeWindow.SetCustomRenderingResolution(360, 800, "Tall portrait");
            game.StartLevel(0); game.Settings.reduceMotion = true;
            yield return null; yield return null;
            Capture("tall-portrait", 360, 800);
            foreach (var button in game.Hud.GetComponentsInChildren<PumpPress>())
            {
                var rect = button.GetComponent<RectTransform>();
                var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                var screen = game.Presentation.Camera.WorldToScreenPoint(rect.position);
                Assert.That(screen.x, Is.InRange(0f, 360f)); Assert.That(screen.y, Is.InRange(0f, 800f));
            }
        }

        void Capture(string name, int width = 720, int height = 1280)
        {
            var camera = game.Presentation.Camera; var old = camera.targetTexture; var active = RenderTexture.active;
            var target = new RenderTexture(width, height, 24) { antiAliasing = 4 };
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.aspect = (float)width / height;
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
                Directory.CreateDirectory("Logs/Previews"); File.WriteAllBytes("Logs/Previews/" + name + ".png", texture.EncodeToPNG());
            }
            finally { camera.targetTexture = old; RenderTexture.active = active; Object.Destroy(target); Object.Destroy(texture); }
        }
    }
}
#endif
