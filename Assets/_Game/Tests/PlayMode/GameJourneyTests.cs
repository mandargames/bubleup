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
                game.StartLevel(level); FloatingRing selected = null;
                for (int step = 0; step < 15000 && game.Playing; step++)
                {
                    if (selected == null || selected.Captured) selected = game.Rings.Where(x => !x.Captured).OrderByDescending(x => x.Body.position.y).FirstOrDefault();
                    if (selected == null) break;
                    var pos = selected.Body.position; var velocity = selected.Body.linearVelocity;
                    int peg = Enumerable.Range(0, game.Level.pegs.Length).Where(i => game.Occupied(i) < game.Level.pegs[i].capacity)
                        .OrderBy(i => Mathf.Abs(game.PegTip(i).x - pos.x)).First();
                    var target = game.PegTip(peg);
                    float targetX = target.x;
                    if (game.Level.baffles.Length > 0 && pos.y < .5f && Mathf.Abs(targetX) < 1.25f) targetX = targetX < 0f ? -1.5f : 1.5f;
                    game.Sensor.SetVirtualTilt(Mathf.Clamp(-2.8f * (pos.x - targetX) - 1.3f * velocity.x, -1f, 1f));
                    if (pos.y < target.y - .13f && velocity.y < 1.6f) game.Pump(pos.x <= 0f);
                    yield return new WaitForFixedUpdate();
                }
                Debug.Log("Control audit: " + game.Level.title + ": " + game.Caught + "/" + game.Rings.Count + ", " + game.Elapsed.ToString("0.0") + " seconds, " + game.Pumps + " pumps.");
                Assert.That(game.Screen, Is.EqualTo(GameScreen.Complete), game.Level.title + " should be winnable through actual controls.");
                Assert.That(game.Progress.Data.Record(game.Level.id).stars, Is.GreaterThan(0));
                Assert.IsTrue(File.Exists(GameSession.SavePathOverride));
            }
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
