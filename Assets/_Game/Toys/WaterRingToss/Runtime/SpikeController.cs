using PocketToys.Core.Input;
using PocketToys.Core.Services;
using UnityEngine;
using DeviceInput = UnityEngine.Input;

namespace PocketToys.WaterRingToss
{
    public sealed class SpikeController : MonoBehaviour
    {
        public WaterRingTossConfig config;
        public RingBody Ring { get; private set; }
        public SensorInputService Sensor { get; private set; }
        public FeedbackService Feedback { get; private set; }
        public int PumpCount { get; private set; }
        public float Elapsed { get; private set; }
        public bool Paused { get; private set; }
        public Camera GameCamera { get; private set; }
        public Canvas GameCanvas { get; private set; }
        SpikeView view;
        float nextLeft, nextRight;
        IAudioService audioFeedback;
        IHapticsService haptics;

        void Start()
        {
            if (config == null || !config.IsValid(out _))
            {
                Debug.LogError("Assign a valid WaterRingTossConfig to the spike controller.");
                enabled = false;
                return;
            }
            Application.targetFrameRate = 60;
            Sensor = gameObject.AddComponent<SensorInputService>();
            Feedback = gameObject.AddComponent<FeedbackService>();
            audioFeedback = Feedback;
            haptics = Feedback;
            view = gameObject.AddComponent<SpikeView>();
            Ring = view.BuildTank(config, Sensor);
            GameCamera = view.GameCamera;
            Ring.Landed += OnLanded;
            Ring.Contact += () => audioFeedback.Contact();
            GameCanvas = gameObject.AddComponent<SpikeHud>().Build(this);
        }

        void Update()
        {
            if (Ring == null) return;
            if (DeviceInput.GetKeyDown(KeyCode.Q)) Pump(true);
            if (DeviceInput.GetKeyDown(KeyCode.E)) Pump(false);
            if (DeviceInput.GetKeyDown(KeyCode.R)) Restart();
            if (DeviceInput.GetKeyDown(KeyCode.C)) Sensor.Calibrate();
            if (DeviceInput.GetKeyDown(KeyCode.Escape)) TogglePause();
            if (!Paused && !Ring.IsCaptured) Elapsed += Time.deltaTime;
        }

        public void Pump(bool left)
        {
            if (Ring == null || Paused || Ring.IsCaptured) return;
            if (Time.time < (left ? nextLeft : nextRight)) return;
            if (left) nextLeft = Time.time + config.pumpCooldown;
            else nextRight = Time.time + config.pumpCooldown;
            PumpCount++;
            Ring.ApplyPump(left);
            audioFeedback.Pump();
            haptics.LightTap();
            view.Pulse(left);
        }

        public void Restart()
        {
            if (Ring == null) return;
            Paused = false;
            PumpCount = 0;
            Elapsed = 0f;
            nextLeft = nextRight = 0f;
            Sensor.SetVirtualTilt(0f);
            Ring.ResetRing();
            view.ResetPulses();
        }

        public void TogglePause() { SetPaused(!Paused); }
        void SetPaused(bool paused)
        {
            Paused = paused;
            if (Ring != null) Ring.SetPaused(paused);
            if (Sensor != null) Sensor.SetVirtualTilt(0f);
        }
        void OnApplicationPause(bool paused) { if (paused) SetPaused(true); }
        void OnApplicationFocus(bool focused) { if (!focused) SetPaused(true); }
        void OnLanded() { audioFeedback.Success(); }
    }
}
