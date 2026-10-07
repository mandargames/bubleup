using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketToys.WaterRingToss
{
    public sealed class SpikeHud : MonoBehaviour
    {
        SpikeController game;
        RectTransform safeRoot;
        Text status, stats, mode, pauseLabel, soundLabel, hapticLabel;
        Slider tilt;
        Font font;
        Rect lastSafe;
        Vector2 lastSize;
        readonly Color cream = new Color(.92f, .96f, .9f);
        readonly Color muted = new Color(.55f, .75f, .77f);

        public Canvas Build(SpikeController controller)
        {
            game = controller;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Spike HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = game.GameCamera;
            canvas.planeDistance = 1f;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(600f, 1000f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            safeRoot = Rect("Safe area", canvasObject.transform, Vector2.zero, Vector2.one);
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                events.transform.SetParent(transform, false);
            }
            Label("POCKET TOYS", .5f, .965f, 28, cream, 520f);
            Label("W A T E R   R I N G   T O S S", .5f, .92f, 14, muted, 520f);
            status = Label("Lift the ring. Catch the peg.", .5f, .87f, 19, cream, 550f);
            stats = Label("", .5f, .83f, 13, muted, 500f);
            var left = Button("LEFT PUMP  /  Q", .26f, .19f, 240f, 60f, new Color(.95f, .44f, .34f), null);
            left.gameObject.AddComponent<PressControl>().Pressed = () => game.Pump(true);
            var right = Button("RIGHT PUMP  /  E", .74f, .19f, 240f, 60f, new Color(.98f, .77f, .36f), null);
            right.gameObject.AddComponent<PressControl>().Pressed = () => game.Pump(false);
            mode = Label("", .5f, .133f, 12, muted, 550f);
            BuildTiltSlider();
            Button("RESTART", .13f, .051f, 130f, 38f, new Color(.12f, .31f, .36f), game.Restart);
            Button("CALIBRATE", .38f, .051f, 140f, 38f, new Color(.12f, .31f, .36f), () => game.Sensor.Calibrate());
            var pause = Button("PAUSE", .64f, .051f, 130f, 38f, new Color(.12f, .31f, .36f), game.TogglePause);
            pauseLabel = pause.GetComponentInChildren<Text>();
            Button("INPUT", .875f, .051f, 115f, 38f, new Color(.12f, .31f, .36f), () => game.Sensor.SetSensorMode(!game.Sensor.UseSensor));
            soundLabel = Button("SOUND ON", .29f, .012f, 150f, 26f, Color.clear, () => game.Feedback.SoundEnabled = !game.Feedback.SoundEnabled).GetComponentInChildren<Text>();
            hapticLabel = Button("HAPTICS ON", .71f, .012f, 160f, 26f, Color.clear, () => game.Feedback.HapticsEnabled = !game.Feedback.HapticsEnabled).GetComponentInChildren<Text>();
            UpdateSafeArea();
            return canvas;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        RectTransform FixedRect(string name, float x, float y, float width, float height)
        {
            var rect = Rect(name, safeRoot, new Vector2(x, y), new Vector2(x, y));
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        Text Label(string value, float x, float y, int size, Color color, float width)
        {
            var rect = FixedRect(value, x, y, width, 40f);
            return AddText(rect, value, size, color);
        }

        Text AddText(RectTransform rect, string value, int size, Color color)
        {
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter; text.color = color;
            text.raycastTarget = false;
            return text;
        }

        Button Button(string value, float x, float y, float width, float height, Color color, UnityEngine.Events.UnityAction action)
        {
            var rect = FixedRect(value, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.pressedColor = new Color(.65f, .8f, .8f); button.colors = colors;
            if (action != null) button.onClick.AddListener(action);
            var labelRect = Rect("Label", rect, Vector2.zero, Vector2.one);
            AddText(labelRect, value, height > 40f ? 18 : 12, height > 40f ? new Color(.1f, .22f, .26f) : cream);
            return button;
        }

        void BuildTiltSlider()
        {
            var rect = FixedRect("Virtual tilt - release to center", .5f, .095f, 440f, 28f);
            var background = rect.gameObject.AddComponent<Image>(); background.color = new Color(.18f, .4f, .46f);
            tilt = rect.gameObject.AddComponent<Slider>();
            tilt.minValue = -1f; tilt.maxValue = 1f; tilt.value = 0f;
            var handle = Rect("Handle", rect, new Vector2(.5f, 0f), new Vector2(.5f, 1f));
            handle.sizeDelta = new Vector2(28f, 0f);
            var image = handle.gameObject.AddComponent<Image>(); image.color = cream;
            tilt.handleRect = handle; tilt.targetGraphic = image;
            tilt.onValueChanged.AddListener(value => game.Sensor.SetVirtualTilt(value));
            rect.gameObject.AddComponent<SpringSlider>();
        }

        void UpdateSafeArea()
        {
            var area = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            safeRoot.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            lastSafe = area; lastSize = new Vector2(Screen.width, Screen.height);
            // Keep the whole toy visible in narrow portrait and wide desktop windows.
            game.GameCamera.orthographicSize = Mathf.Max(6.3f, (game.config.tankSize.x + .9f) / (2f * game.GameCamera.aspect));
        }

        void Update()
        {
            if (game == null || game.Ring == null) return;
            if (lastSafe != Screen.safeArea || lastSize != new Vector2(Screen.width, Screen.height)) UpdateSafeArea();
            status.text = game.Paused ? "Paused. Take your time." : game.Ring.IsCaptured ? "Nice catch!  Tap Restart to go again." : "Lift the ring. Catch the peg.";
            stats.text = $"{game.PumpCount:00} PUMPS     /     {game.Elapsed:0.0}s     /     {(game.Ring.IsCaptured ? "1" : "0")} OF 1";
            mode.text = game.Sensor.ModeLabel + "   /   A D TO LEAN   /   DRAG BELOW";
            pauseLabel.text = game.Paused ? "RESUME" : "PAUSE";
            soundLabel.text = game.Feedback.SoundEnabled ? "SOUND ON" : "SOUND OFF";
            hapticLabel.text = game.Feedback.HapticsEnabled ? "HAPTICS ON" : "HAPTICS OFF";
            tilt.interactable = !game.Sensor.UseSensor && !game.Paused;
        }
    }
}
