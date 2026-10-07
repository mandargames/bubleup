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
                game.StartLevel(level); FloatingRing selected = null;
                yield return new WaitForSeconds(.25f); Capture(game, "level-" + (level + 1).ToString("00"));
                float deadline = Time.realtimeSinceStartup + 90f;
                while (game.Playing && Time.realtimeSinceStartup < deadline && failure == null)
                {
                    if (selected == null || selected.Captured)
                    {
                        selected = null;
                        foreach (var ring in game.Rings)
                            if (!ring.Captured && (selected == null || ring.Body.position.y > selected.Body.position.y)) selected = ring;
                    }
                    if (selected == null) break;
                    var pos = selected.Body.position; var velocity = selected.Body.linearVelocity;
                    int peg = -1; float distance = float.MaxValue;
                    for (int i = 0; i < game.Level.pegs.Length; i++)
                    {
                        float candidate = Mathf.Abs(game.PegTip(i).x - pos.x);
                        if (game.Occupied(i) < game.Level.pegs[i].capacity && candidate < distance) { distance = candidate; peg = i; }
                    }
                    if (peg < 0) break;
                    var target = game.PegTip(peg); float targetX = target.x;
                    if (game.Level.baffles.Length > 0 && pos.y < .5f && Mathf.Abs(targetX) < 1.25f) targetX = targetX < 0f ? -1.5f : 1.5f;
                    game.Sensor.SetVirtualTilt(Mathf.Clamp(-2.8f * (pos.x - targetX) - 1.3f * velocity.x, -1f, 1f));
                    if (pos.y < target.y - .13f && velocity.y < 1.6f) game.Pump(pos.x <= 0f);
                    frames.Add(Time.unscaledDeltaTime * 1000f);
                    yield return null;
                }
                if (game.Screen != GameScreen.Complete) { failure = failure ?? "Could not complete " + game.Level.title; break; }
                complete++; yield return new WaitForSeconds(.8f); Capture(game, "complete-" + (level + 1).ToString("00"));
            }
            game.SetScreen(GameScreen.Levels); yield return new WaitForSeconds(.3f); Capture(game, "levels");
            game.SetScreen(GameScreen.Settings); yield return new WaitForSeconds(.3f); Capture(game, "settings");
            game.SetScreen(GameScreen.Collection); yield return new WaitForSeconds(.3f); Capture(game, "collection");
            Finish(complete, failure);
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
