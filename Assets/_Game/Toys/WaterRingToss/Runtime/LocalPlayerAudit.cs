using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    /// <summary>Opt-in command-line QA, isolated from the player's real saves. Inactive in ordinary play.</summary>
    public sealed class LocalPlayerAudit : MonoBehaviour
    {
        [Serializable] sealed class Result
        {
            public string platform, graphics, version;
            public bool passed;
            public int completedLevels, frames;
            public float averageFrameMilliseconds, p95FrameMilliseconds;
            public string error;
        }
        static string output;
        readonly List<float> frames = new List<float>();
        string failure;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-pocketToysAudit");
            if (index < 0 || index + 1 >= args.Length) return;
            output = Path.GetFullPath(args[index + 1]); Directory.CreateDirectory(output);
            // A unique QA save ensures replay audits cannot affect the user's progression.
            GameSession.SavePathOverride = Path.Combine(output, "save-" + Guid.NewGuid().ToString("N") + ".json");
            new GameObject("Local player audit").AddComponent<LocalPlayerAudit>();
        }
        void OnEnable() { Application.logMessageReceived += Log; }
        void OnDisable() { Application.logMessageReceived -= Log; }
        void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) failure = message; }

        IEnumerator Start()
        {
            Application.runInBackground = true;
            GameSession game = null;
            for (int i = 0; i < 600; i++)
            {
                game = FindFirstObjectByType<GameSession>();
                if (game != null && game.Ready) break;
                yield return null;
            }
            if (game == null || !game.Ready) { Finish(0, "Game did not initialize."); yield break; }
            game.Settings.sound = false; game.Settings.music = false; game.Settings.haptics = false; game.Settings.motion = false;
            game.SaveSettings();
            yield return new WaitForSeconds(.6f); Capture(game, "home");
            int complete = 0;
            for (int level = 0; level < game.campaign.levels.Length; level++)
            {
                game.StartLevel(level); FloatingRing selected = null; bool aboveTip = false; float stalled = 0f, reverseUntil = 0f, reverseDirection = 0f;
                yield return new WaitForSeconds(.25f); Capture(game, "level-" + (level + 1).ToString("00"));
                float deadline = Time.realtimeSinceStartup + Mathf.Max(90f, game.Level.silverSeconds + 30f);
                while (game.Playing && Time.realtimeSinceStartup < deadline && failure == null)
                {
                    if (selected == null || selected.Captured)
                    {
                        selected = null; aboveTip = false; stalled = 0f; reverseUntil = 0f;
                        foreach (var ring in game.Rings)
                            if (!ring.Captured && (selected == null || ring.Body.position.y > selected.Body.position.y)) selected = ring;
                    }
                    if (selected == null) { game.Sensor.SetVirtualTilt(0f); yield return null; continue; }
                    var pos = selected.Body.position; var velocity = selected.Body.linearVelocity;
                    stalled = velocity.magnitude < .2f ? stalled + Time.deltaTime : 0f;
                    int peg = -1; float distance = float.MaxValue;
                    for (int i = 0; i < game.Level.pegs.Length; i++)
                    {
                        // Fill the larger stack before trying to separate rings onto single pegs.
                        float candidate = Mathf.Abs(game.PegTip(i).x - pos.x) - game.Level.pegs[i].capacity * 10f;
                        if (game.Occupied(i) < game.Level.pegs[i].capacity && candidate < distance) { distance = candidate; peg = i; }
                    }
                    if (peg < 0) break;
                    if (selected.Threaded) peg = selected.PegIndex;
                    bool fullPeg = selected.Threaded && game.Occupied(peg) >= game.Level.pegs[peg].capacity;
                    var target = game.PegTip(peg); float targetX = target.x;
                    if (pos.y > target.y + .45f) aboveTip = true;
                    if (pos.y < target.y - .35f && !selected.Threaded) aboveTip = false;
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
                    frames.Add(Time.unscaledDeltaTime * 1000f);
                    yield return null;
                }
                if (game.Screen != GameScreen.Complete)
                {
                    failure = failure ?? "Could not complete " + game.Level.title;
                    foreach (var ring in game.Rings) Debug.Log("Audit ring: " + ring.Body.position + " velocity=" + ring.Body.linearVelocity + " threaded=" + ring.Threaded + " captured=" + ring.Captured);
                    Capture(game, "blocked-level-" + (level + 1));
                    break;
                }
                complete++; yield return new WaitForSeconds(.8f); Capture(game, "complete-" + (level + 1).ToString("00"));
            }
            game.SetScreen(GameScreen.Levels); yield return new WaitForSeconds(.3f); Capture(game, "levels");
            game.SetScreen(GameScreen.Settings); yield return new WaitForSeconds(.3f); Capture(game, "settings");
            game.SetScreen(GameScreen.Collection); yield return new WaitForSeconds(.3f); Capture(game, "collection");
            Finish(complete, failure);
        }
        public static bool HasLiftClearance(GameSession game, Vector2 position)
        {
            // The QA driver should release pressure under shelves, but lift past
            // neighboring rings when they block its horizontal route.
            for (int i = 0; i < game.Level.pegs.Length; i++)
            {
                var tip = game.PegTip(i); float underside = tip.y - game.Level.pegs[i].length;
                float clearance = ToyPresentation.LandingWidth * .5f + FloatingRing.Radius + FloatingRing.Tube + .03f;
                if (position.y < underside - .1f && position.y > underside - .6f && Mathf.Abs(position.x - tip.x) < clearance) return false;
            }
            foreach (var baffle in game.Level.baffles)
                if (position.y < baffle.center.y && position.y > baffle.center.y - .6f && Mathf.Abs(position.x - baffle.center.x) < baffle.size.x * .5f + .4f) return false;
            return true;
        }
        public static bool LiftLooseRing(GameSession game, FloatingRing selected)
        {
            foreach (var ring in game.Rings)
                if (ring != selected && !ring.Captured && HasLiftClearance(game, ring.Body.position))
                { game.Pump(ring.Body.position.x <= 0f); return true; }
            return false;
        }
        void Capture(GameSession game, string name)
        {
            var camera = game.Presentation.Camera; var prior = camera.targetTexture; var active = RenderTexture.active;
            int width = Screen.width, height = Screen.height;
            var target = new RenderTexture(width, height, 24) { antiAliasing = 4 };
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
            }
            finally { camera.targetTexture = prior; RenderTexture.active = active; Destroy(target); Destroy(texture); }
        }
        void Finish(int completed, string error)
        {
            frames.Sort(); float sum = 0f; foreach (float frame in frames) sum += frame;
            var result = new Result { platform = Application.platform.ToString(), graphics = SystemInfo.graphicsDeviceName, version = Application.version,
                passed = completed == 5 && error == null, completedLevels = completed, frames = frames.Count,
                averageFrameMilliseconds = frames.Count == 0 ? 0f : sum / frames.Count,
                p95FrameMilliseconds = frames.Count == 0 ? 0f : frames[Mathf.Min(frames.Count - 1, Mathf.FloorToInt(frames.Count * .95f))], error = error };
            File.WriteAllText(Path.Combine(output, "report.json"), JsonUtility.ToJson(result, true));
            Application.Quit(result.passed ? 0 : 1);
        }
    }
}
