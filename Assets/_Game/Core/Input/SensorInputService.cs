using UnityEngine;
using DeviceInput = UnityEngine.Input;

namespace PocketToys.Core.Input
{
    [DefaultExecutionOrder(-100)]
    public sealed class SensorInputService : MonoBehaviour
    {
        public float sensitivity = 2.5f;
        public float deadZone = .06f;
        public float smoothingSeconds = .12f;
        public Vector2 Tilt => filter.Value;
        public Vector2 Neutral => filter.Neutral;
        public bool UseSensor { get; private set; }
        public bool SensorAvailable => SystemInfo.supportsAccelerometer;
        public string ModeLabel => UseSensor ? "PHONE TILT" : "TOUCH / KEYBOARD";
        readonly TiltFilter filter = new TiltFilter();
        Vector2 virtualTilt;
        bool calibrated;

        void Awake() { UseSensor = Application.isMobilePlatform && SensorAvailable; }

        public void SetVirtualTilt(float horizontal) { virtualTilt = new Vector2(horizontal, 0f); }

        public void SetSensorMode(bool enabled)
        {
            UseSensor = enabled && SensorAvailable;
            virtualTilt = Vector2.zero;
            Calibrate();
        }

        Vector2 ReadSample()
        {
            if (UseSensor)
            {
                var acceleration = DeviceInput.acceleration;
                return new Vector2(acceleration.x, acceleration.y);
            }
            var horizontal = virtualTilt.x;
            if (DeviceInput.GetKey(KeyCode.A) || DeviceInput.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (DeviceInput.GetKey(KeyCode.D) || DeviceInput.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            return new Vector2(Mathf.Clamp(horizontal, -1f, 1f), 0f);
        }

        public void Calibrate()
        {
            filter.Calibrate(UseSensor ? ReadSample() : Vector2.zero);
            calibrated = true;
        }

        public void RestoreCalibration(Vector2 neutral)
        {
            filter.Calibrate(neutral);
            calibrated = true;
        }

        void FixedUpdate()
        {
            // Sensor reads and filter time constants follow physics, independently of render rate.
            if (!calibrated) Calibrate();
            filter.Step(ReadSample(), UseSensor ? sensitivity : sensitivity / 2.5f, deadZone, smoothingSeconds, Time.fixedDeltaTime);
        }
    }
}
