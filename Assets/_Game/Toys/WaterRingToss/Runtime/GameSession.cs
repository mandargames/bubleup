using System;
using System.Collections.Generic;
using System.IO;
using PocketToys.Core.Input;
using PocketToys.Core.Progression;
using PocketToys.Core.Services;
using UnityEngine;
using DeviceInput = UnityEngine.Input;

namespace PocketToys.WaterRingToss.Game
{
    public enum GameScreen { Home, Playing, Paused, Levels, Settings, Complete, Collection, Help, ConfirmRestart, Controls, Frozen }

    public sealed class GameSession : MonoBehaviour
    {
        public CampaignDefinition campaign;
        public GameScreen Screen { get; private set; } = GameScreen.Home;
        public LevelDefinition Level => campaign.levels[LevelIndex];
        public GameSettings Settings => Progress.Data.settings;
        public LocalProgress Progress { get; private set; }
        public LocalAnalytics Analytics { get; private set; }
        public SensorInputService Sensor { get; private set; }
        public ToyAudio Audio { get; private set; }
        public ToyPresentation Presentation { get; private set; }
        public GameHud Hud { get; private set; }
        public LevelEnvironment Environment { get; private set; }
        public List<FloatingRing> Rings { get; } = new List<FloatingRing>();
        public int LevelIndex { get; private set; }
        public int Pumps { get; private set; }
        public int Caught { get; private set; }
        public int EarnedStars { get; private set; }
        public float Elapsed { get; private set; }
        public bool Playing => Screen == GameScreen.Playing;
        public bool Ready { get; private set; }
        public int TargetCount => Level.TargetCount;
        public string Notice { get; private set; }
        public event Action Changed;
        int[] occupied, collected;
        float nextLeft, nextRight, completionRest;
        GameScreen settingsReturn = GameScreen.Home;
        GameScreen helpReturn = GameScreen.Paused;
        bool hasAttempt;
        public static string SavePathOverride;

        void Start()
        {
            if (campaign == null || !campaign.Validate(out _)) { Debug.LogError("A valid campaign is required."); enabled = false; return; }
            Application.targetFrameRate = 60;
            Progress = new LocalProgress(SavePathOverride ?? Path.Combine(Application.persistentDataPath, "pocket-toys-v1.json"));
            Analytics = new LocalAnalytics(Path.Combine(Application.persistentDataPath, "local-diagnostics.jsonl"));
            Sensor = gameObject.AddComponent<SensorInputService>();
            Sensor.smoothingSeconds = .16f;
            Audio = gameObject.AddComponent<ToyAudio>();
            Environment = gameObject.AddComponent<LevelEnvironment>();
            Presentation = gameObject.AddComponent<ToyPresentation>();
            Presentation.Initialize(this);
            ApplySettings();
            LevelIndex = Mathf.Max(0, Array.FindIndex(campaign.levels, x => x.id == Progress.Data.lastLevel));
            if (!Unlocked(LevelIndex)) LevelIndex = 0;
            CreateLevel();
            Hud = gameObject.AddComponent<GameHud>();
            Hud.Initialize(this);
            Ready = true;
            Analytics.Track("app_session_start", Level.id);
            Debug.Log("Pocket Toys game ready: " + campaign.levels.Length + " authored levels.");
        }

        public bool Unlocked(int index)
        {
            if (index < 0 || index >= campaign.levels.Length) return false;
            if (index == 0) return true;
            var previous = Progress.Data.levels.Find(x => x.id == campaign.levels[index - 1].id);
            return previous != null && previous.stars > 0 && Progress.Data.TotalStars >= campaign.levels[index].starsRequired;
        }

        public void StartLevel(int index)
        {
            if (!Unlocked(index)) return;
            if (hasAttempt && Caught < TargetCount) Analytics.Track("level_abandoned", Level.id, Elapsed, Pumps);
            LevelIndex = index;
            Elapsed = 0f; Pumps = 0; Caught = 0; EarnedStars = 0;
            nextLeft = nextRight = completionRest = 0f;
            Sensor.SetVirtualTilt(0f);
            CreateLevel();
            hasAttempt = true;
            Progress.Data.lastLevel = Level.id;
            Progress.Data.Record(Level.id).attempts++;
            Progress.Save();
            SetScreen(GameScreen.Playing);
            Analytics.Track("level_started", Level.id, 0f, Progress.Data.Record(Level.id).attempts);
        }

        void CreateLevel()
        {
            Rings.Clear(); occupied = new int[Level.pegs.Length]; collected = new int[Level.collectors?.Length ?? 0];
            Environment.ResetFor(this);
            Presentation.LoadLevel();
            foreach (var ring in Rings) ring.Pause(!Playing);
        }

        public void Register(FloatingRing ring) { Rings.Add(ring); ring.Contact += Audio.Contact; }
        public Vector2 PegTip(int index)
        {
            var peg = Level.pegs[index];
            return peg.tip + Vector2.right * (Mathf.Sin(Elapsed * peg.speed) * peg.movement);
        }
        public int Occupied(int index) => occupied[index];
        public int Collected(int index) => collected[index];
        public bool TryCollect(int index)
        {
            if (!Playing || Caught >= TargetCount || collected[index] >= Level.collectors[index].capacity) return false;
            collected[index]++; return true;
        }
        public bool TryOccupy(int index, out int slot)
        {
            slot = occupied[index];
            if (!Playing || Caught >= TargetCount || slot >= Level.pegs[index].capacity) return false;
            occupied[index]++; return true;
        }

        public void OnRingCaptured(FloatingRing ring)
        {
            Caught++; Audio.Catch(Caught); Presentation.Celebrate(ring.transform.position);
        }

        void CompleteLevel()
        {
            EarnedStars = LocalProgress.EvaluateStars(Elapsed, Pumps, Level.silverSeconds, Level.goldSeconds, Level.goldPumps);
            Progress.Complete(Level.id, EarnedStars, Elapsed, Pumps);
            Progress.Data.tutorialSeen = true;
            Progress.Save();
            Analytics.Track("level_completed", Level.id, Elapsed, EarnedStars);
            Audio.Success(); hasAttempt = false;
            SetScreen(GameScreen.Complete);
        }

        public void Pump(bool left)
        {
            if (!Playing || Caught >= TargetCount || Time.time < (left ? nextLeft : nextRight)) return;
            if (left) nextLeft = Time.time + .18f; else nextRight = Time.time + .18f;
            Pumps++;
            foreach (var ring in Rings) ring.Pump(left);
            Audio.Pump(left); Presentation.Pump(left);
        }

        public void SetScreen(GameScreen screen)
        {
            Screen = screen;
            Sensor.SetVirtualTilt(0f);
            foreach (var ring in Rings) ring.Pause(screen != GameScreen.Playing);
            Changed?.Invoke();
        }
        public void Home() { SetScreen(GameScreen.Home); }
        public void Pause() { if (Playing) SetScreen(GameScreen.Paused); }
        public void Resume() { if (hasAttempt) SetScreen(GameScreen.Playing); }
        public void Restart() { StartLevel(LevelIndex); }
        public void RequestRestart() { if (hasAttempt) SetScreen(GameScreen.ConfirmRestart); }
        public void OpenHelp() { helpReturn = Playing ? GameScreen.Paused : Screen; SetScreen(GameScreen.Help); }
        public void CloseHelp() { SetScreen(helpReturn); }
        public void OpenSettings() { settingsReturn = Playing ? GameScreen.Paused : Screen; SetScreen(GameScreen.Settings); }
        public void CloseSettings() { SaveSettings(); SetScreen(settingsReturn); }
        public void Calibrate()
        {
            Sensor.Calibrate();
            Settings.neutralX = Sensor.Neutral.x; Settings.neutralY = Sensor.Neutral.y; Settings.calibrated = Sensor.UseSensor;
            SaveSettings(); Notice = Sensor.UseSensor ? "Neutral position saved." : "Touch controls are already centered.";
            Analytics.Track("sensor_calibrated", Level.id, Settings.sensitivity);
            Changed?.Invoke();
        }
        public void SaveSettings() { ApplySettings(); Progress.Save(); }
        public void ApplySettings()
        {
            Sensor.sensitivity = Settings.sensitivity;
            if (Sensor.UseSensor != (Settings.motion && Sensor.SensorAvailable)) Sensor.SetSensorMode(Settings.motion);
            if (Sensor.UseSensor && Settings.calibrated) Sensor.RestoreCalibration(new Vector2(Settings.neutralX, Settings.neutralY));
            Audio.Configure(Settings.sound, Settings.music, Settings.haptics);
            Analytics.Enabled = Settings.localDiagnostics;
            Presentation.SetShell(Settings.shell);
        }
        public void SelectShell(int index)
        {
            int required = index == 1 ? 5 : index == 2 ? 10 : 0;
            if (Progress.Data.TotalStars < required) return;
            Settings.shell = index; SaveSettings(); Changed?.Invoke();
        }

        void Update()
        {
            if (!Ready) return;
            if (Playing)
            {
                // A completed objective wins before the celebration delay can consume
                // the last fraction of the ice timer. Pausing never advances Elapsed.
                if (Caught >= TargetCount)
                {
                    completionRest += Time.deltaTime;
                    if (completionRest >= .3f) CompleteLevel();
                }
                else
                {
                    Elapsed += Time.deltaTime;
                    if (Level.freezeSeconds > 0f && Elapsed >= Level.freezeSeconds)
                    {
                        Elapsed = Level.freezeSeconds; hasAttempt = false;
                        Analytics.Track("level_frozen", Level.id, Elapsed, Caught);
                        SetScreen(GameScreen.Frozen);
                    }
                    else
                    {
                        if (DeviceInput.GetKeyDown(KeyCode.Q) || DeviceInput.GetKeyDown(KeyCode.Space)) Pump(true);
                        if (DeviceInput.GetKeyDown(KeyCode.E)) Pump(false);
                        if (DeviceInput.GetKeyDown(KeyCode.R)) RequestRestart();
                        if (DeviceInput.GetKeyDown(KeyCode.C)) Calibrate();
                    }
                }
            }
            if (DeviceInput.GetKeyDown(KeyCode.Escape))
            {
                if (Playing) Pause();
                else if (Screen == GameScreen.Paused || Screen == GameScreen.ConfirmRestart) Resume();
                else if (Screen == GameScreen.Settings) CloseSettings();
                else if (Screen == GameScreen.Controls) { SaveSettings(); SetScreen(GameScreen.Settings); }
                else if (Screen == GameScreen.Help) CloseHelp();
                else Home();
            }
        }
        void OnApplicationPause(bool paused) { if (paused && Ready) { Pause(); Progress.Save(); } }
        void OnApplicationFocus(bool focus) { if (!focus && Ready && !Application.isBatchMode) Pause(); }
        void OnApplicationQuit() { Progress?.Save(); }
    }
}
