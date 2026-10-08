using System;
using PocketToys.Core.Progression;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class GameHud : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public RectTransform Root { get; private set; }
        GameSession game;
        RectTransform safeArea, screenRoot, leftPump, rightPump;
        Text progress, sensitivityLabel, controlsHint, homeSubtitle, lesson, environmentStatus;
        Slider lean;
        Font font, displayFont;
        bool showResults;
        int levelPage;
        const int LevelsPerPage = 5;
        Sprite rounded, circle, starSprite;
        Texture2D roundedTexture, circleTexture, starTexture;
        CanvasGroup fade;
        float screenAge;
        GameScreen builtScreen;
        static Color Hex(string hex) => ToyPresentation.Color(hex);
        readonly Color ink = Hex("#123D49"), cream = Hex("#FFF7E4"), muted = Hex("#A6CFD0"), gold = Hex("#FFD17A"), panel = Hex("#174B58");

        public void Initialize(GameSession session)
        {
            game = session; font = Resources.Load<Font>("Lato") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            displayFont = Resources.Load<Font>("LilitaOne") ?? font;
            roundedTexture = ShapeTexture(false); circleTexture = ShapeTexture(true);
            rounded = Sprite.Create(roundedTexture, new Rect(0, 0, 64, 64), Vector2.one * .5f, 100f, 0, SpriteMeshType.FullRect, Vector4.one * 20f);
            circle = Sprite.Create(circleTexture, new Rect(0, 0, 64, 64), Vector2.one * .5f);
            starTexture = MakeStarTexture(); starSprite = Sprite.Create(starTexture, new Rect(0, 0, 128, 128), Vector2.one * .5f);
            var canvasObject = new GameObject("Pocket Toys interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas = canvasObject.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = game.Presentation.Camera; Canvas.planeDistance = 1f;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f); scaler.matchWidthOrHeight = 0f;
            Root = canvasObject.GetComponent<RectTransform>();
            safeArea = Stretch("Safe area", Root);
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("UI events", typeof(EventSystem), typeof(StandaloneInputModule)); events.transform.SetParent(transform, false);
            }
            game.Changed += Rebuild; Canvas.willRenderCanvases += RefreshWorldAnchors; Rebuild();
        }

        static Texture2D ShapeTexture(bool disk)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = disk ? "Circle UI" : "Rounded UI", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                var p = new Vector2(Mathf.Abs(x - 31.5f), Mathf.Abs(y - 31.5f));
                float distance = disk ? p.magnitude - 30f : new Vector2(Mathf.Max(0f, p.x - 13f), Mathf.Max(0f, p.y - 13f)).magnitude - 18f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(.5f - distance)));
            }
            texture.Apply(); return texture;
        }

        static Texture2D MakeStarTexture()
        {
            var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false) { name = "Star UI", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5f;
                points[i] = new Vector2(64f, 64f) + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? 60f : 27f);
            }
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float coverage = 0f;
                for (int sample = 0; sample < 4; sample++)
                {
                    float px = x + (sample % 2 + .5f) * .5f, py = y + (sample / 2 + .5f) * .5f;
                    bool inside = false;
                    for (int i = 0, j = 9; i < 10; j = i++)
                        if ((points[i].y > py) != (points[j].y > py) && px < (points[j].x - points[i].x) * (py - points[i].y) / (points[j].y - points[i].y) + points[i].x) inside = !inside;
                    if (inside) coverage += .25f;
                }
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, coverage));
            }
            texture.Apply(); return texture;
        }

        RectTransform Stretch(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        RectTransform Rect(string name, float x, float y, float width, float height, Transform parent = null)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent ?? screenRoot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        Image Image(RectTransform rect, Color color, bool round = true)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = color;
            if (round) { image.sprite = rounded; image.type = UnityEngine.UI.Image.Type.Sliced; }
            return image;
        }
        Text Text(RectTransform rect, string value, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var text = rect.gameObject.AddComponent<Text>(); text.font = style == FontStyle.Bold && size >= 23 ? displayFont : font; text.text = value; text.fontSize = size; text.color = color;
            text.fontStyle = text.font == displayFont ? FontStyle.Normal : style; text.alignment = alignment; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; return text;
        }
        Text Label(string value, float x, float y, int size, Color color, float width = 630f, float height = 42f, bool bold = false)
            => Text(Rect(value, x, y, width, height), value, size, color, bold ? FontStyle.Bold : FontStyle.Normal);
        Button Button(string label, float x, float y, float width, float height, Color color, Action action, bool enabled = true)
        {
            height = Mathf.Max(88f, height);
            var shadow = Rect(label + " shadow", x, y, width, height);
            shadow.anchoredPosition = new Vector2(0, -5);
            Image(shadow, new Color(.01f, .045f, .065f, .36f)).raycastTarget = false;
            var rect = Rect(label, x, y, width, height); var image = Image(rect, color);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.interactable = enabled;
            var colors = button.colors; colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f); colors.pressedColor = new Color(.75f, .85f, .86f); colors.disabledColor = new Color(.6f, .6f, .6f, .5f); button.colors = colors;
            var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav;
            button.onClick.AddListener(() => { game.Audio.Click(); action(); });
            var textRect = Stretch("Label", rect); textRect.offsetMin = new Vector2(12f, 0f); textRect.offsetMax = new Vector2(-12f, 0f);
            Text(textRect, label, height >= 64f ? 24 : 16, color == gold || color == cream ? ink : cream, FontStyle.Bold);
            return button;
        }
        void Overlay(float opacity)
        {
            var rect = Stretch("Backdrop", screenRoot); Image(rect, new Color(.035f, .085f, .12f, opacity), false);
        }
        void Card(float x, float y, float width, float height)
        {
            Image(Rect("Card rim", x, y, width + 2f, height + 2f), Hex("#39717A")).raycastTarget = false;
            Image(Rect("Card", x, y, width, height), panel).raycastTarget = false;
        }
        void Stars(float x, float y, int count, float size = 32f, float spacing = 42f)
        {
            for (int i = 0; i < 3; i++)
            {
                var rect = Rect("Star " + i, x, y, size, size); rect.anchoredPosition = new Vector2((i - 1) * spacing, 0f);
                var star = Image(rect, i < count ? gold : Hex("#526C75"), false); star.sprite = starSprite; star.raycastTarget = false;
            }
        }
        void Brand(string subtitle)
        {
            Label("P O C K E T   T O Y S", .5f, .953f, 19, cream, 620f, 40f, true);
            Label(subtitle, .5f, .920f, 11, muted);
            var line = Rect("Brand accent", .5f, .894f, 34, 2); Image(line, gold, false).raycastTarget = false;
        }

        void Rebuild()
        {
            bool sameScreen = screenRoot != null && builtScreen == game.Screen;
            if (!sameScreen && game.Screen == GameScreen.Levels) levelPage = game.LevelIndex / LevelsPerPage;
            if (screenRoot != null) { screenRoot.gameObject.SetActive(false); Destroy(screenRoot.gameObject); }
            screenRoot = Stretch("Screen - " + game.Screen, safeArea); fade = screenRoot.gameObject.AddComponent<CanvasGroup>();
            if (builtScreen != game.Screen) showResults = false;
            screenAge = sameScreen ? 10f : 0f; builtScreen = game.Screen; progress = null; leftPump = rightPump = null; lean = null;
            controlsHint = homeSubtitle = lesson = environmentStatus = null;
            switch (game.Screen)
            {
                case GameScreen.Home: Home(); break;
                case GameScreen.Playing: Play(); break;
                case GameScreen.Paused: Pause(); break;
                case GameScreen.Levels: Levels(); break;
                case GameScreen.Settings: Settings(); break;
                case GameScreen.Complete: Complete(); break;
                case GameScreen.Collection: Collection(); break;
                case GameScreen.Help: Help(); break;
                case GameScreen.ConfirmRestart: ConfirmRestart(); break;
                case GameScreen.Controls: Controls(); break;
                case GameScreen.Frozen: Frozen(); break;
            }
            if (!string.IsNullOrEmpty(game.Progress.LastError)) Label("Save needs attention - " + game.Progress.LastError, .5f, .013f, 12, gold, 650f);
        }

        void Home()
        {
            Brand("SMALL WONDERS.  HAPPY HANDS.");
            Label("A little ocean.\nA lot of joy.", .5f, .824f, 54, cream, 650f, 140f, true);
            Label("Your pocket-sized escape.", .5f, .768f, 17, muted, 630f, 24f);
            homeSubtitle = Label("C O R A L   C L U B", .5f, .70f, 14, ink);
            Card(.5f, .177f, 620f, 300f);
            Label(game.campaign.levels.Length + " ADVENTURES · CALM PLAY + TIMED CHALLENGES", .5f, .28f, 14, gold);
            Button("LET'S PLAY  >", .5f, .215f, 554f, 88f, gold, () => game.StartLevel(game.LevelIndex));
            Button("ADVENTURES", .3f, .13f, 256f, 88f, Hex("#286675"), () => game.SetScreen(GameScreen.Levels));
            Button("TOY SHELLS", .7f, .13f, 256f, 88f, Hex("#286675"), () => game.SetScreen(GameScreen.Collection));
            Button("SETTINGS", .5f, .042f, 220f, 88f, panel, game.OpenSettings);
        }

        void Play()
        {
            var title = Label((game.LevelIndex + 1).ToString("00") + "  /  " + game.Level.title, 0f, 1f, 30, cream, 530f, 40f, true);
            title.alignment = TextAnchor.MiddleLeft; title.rectTransform.pivot = new Vector2(0, .5f);
            title.rectTransform.anchoredPosition = new Vector2(24, -30);
            SettingsButton();
            progress = Label("", 0, 1, 24, gold, 520f, 32f);
            progress.alignment = TextAnchor.MiddleLeft; progress.rectTransform.pivot = new Vector2(0, .5f);
            progress.rectTransform.anchoredPosition = new Vector2(24, -67);
            lesson = Label("", 0, 1, 24, muted, 635f, 54f);
            lesson.alignment = TextAnchor.MiddleLeft; lesson.rectTransform.pivot = new Vector2(0, .5f);
            lesson.rectTransform.anchoredPosition = new Vector2(24, -117);
            if (game.Level.biome != TankBiome.Lagoon || game.Level.fishTraffic)
            {
                var status = Rect("Environment status", .5f, .5f, 480f, 52f);
                Image(status, panel).raycastTarget = false;
                environmentStatus = Text(Stretch("Status text", status), "", 22, cream);
            }
            leftPump = PumpButton(true); rightPump = PumpButton(false);
            controlsHint = Label(game.Sensor.UseSensor ? "TILT TO\nSTEER" : "PUMP TO\nLIFT", .5f, .09f, 24, ink, 152f, 64f, true);
            if (!game.Sensor.UseSensor)
            {
                lean = Slider("Lean", .5f, .05f, 430f, -1f, 1f, 0f, value => game.Sensor.SetVirtualTilt(value));
                lean.gameObject.AddComponent<CenterOnRelease>();
                var caption = Text(Rect("Steering label", .5f, .5f, 400, 30, lean.transform), "SLIDE TO STEER", 22, cream);
                caption.rectTransform.anchoredPosition = new Vector2(0, -38);
            }
        }
        void SettingsButton()
        {
            var rect = Rect("Game settings", 1f, 1f, 96, 96);
            rect.anchoredPosition = new Vector2(-54, -54);
            Image(rect, new Color(1, 1, 1, .001f), false);
            var image = Image(Rect("Settings surface", .5f, .5f, 64, 64, rect), panel);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav;
            button.onClick.AddListener(() => { game.Audio.Click(); game.Pause(); });
            // A drawn settings icon avoids relying on platform font symbol coverage.
            for (int i = 0; i < 3; i++)
            {
                var line = Rect("Settings line", .5f, .5f, 30, 2, rect); line.anchoredPosition = new Vector2(0, (1 - i) * 10);
                Image(line, cream, false).raycastTarget = false;
                var knob = Rect("Settings knob", .5f, .5f, 8, 8, rect); knob.anchoredPosition = new Vector2(i == 1 ? 7 : -7, (1 - i) * 10);
                var dot = Image(knob, cream, false); dot.sprite = circle; dot.raycastTarget = false;
            }
        }
        RectTransform PumpButton(bool left)
        {
            var rect = Rect(left ? "Left pump" : "Right pump", .5f, .5f, 114f, 114f);
            var image = Image(rect, new Color(1f, 1f, 1f, .001f), false); image.sprite = circle;
            rect.gameObject.AddComponent<PumpPress>().Action = () => game.Pump(left);
            Text(Stretch("PUMP", rect), "PUSH", 22, ink, FontStyle.Bold);
            return rect;
        }
        Slider Slider(string name, float x, float y, float width, float min, float max, float value, Action<float> action)
        {
            var rect = Rect(name, x, y, width, 88f); Image(rect, new Color(1, 1, 1, .001f), false);
            var track = Rect("Track", .5f, .5f, width, 18f, rect); Image(track, Hex("#397B87")).raycastTarget = false;
            var slider = rect.gameObject.AddComponent<Slider>(); slider.minValue = min; slider.maxValue = max; slider.value = value;
            var area = Stretch("Handle area", rect); area.offsetMin = new Vector2(22f, 24f); area.offsetMax = new Vector2(-22f, -24f);
            var handle = Rect("Handle", .5f, .5f, 40f, 40f, area); var image = Image(handle, cream, false); image.sprite = circle;
            slider.handleRect = handle; slider.targetGraphic = image;
            handle.sizeDelta = new Vector2(40f, 0f);
            slider.onValueChanged.AddListener(v => action(v));
            var nav = slider.navigation; nav.mode = Navigation.Mode.None; slider.navigation = nav;
            return slider;
        }

        void Pause()
        {
            Overlay(.9f); Brand("A LITTLE BREATHER");
            Label("Take your time.", .5f, .82f, 38, cream, 540f, 60f, true);
            Label(game.Level.freezeSeconds > 0 ? "Countdown paused. Your rings stay here." : "Your rings are right where you left them.", .5f, .765f, 20, muted);
            Button("KEEP PLAYING", .5f, .66f, 550f, 96f, gold, game.Resume);
            Button("HOW TO PLAY", .5f, .555f, 550f, 88f, panel, game.OpenHelp);
            Button("SETTINGS", .5f, .45f, 550f, 88f, panel, game.OpenSettings);
            Button("RESTART LEVEL", .5f, .345f, 550f, 88f, panel, game.RequestRestart);
            Button("LEVELS", .5f, .24f, 550f, 88f, panel, () => game.SetScreen(GameScreen.Levels));
            Button("BACK TO HOME", .5f, .135f, 550f, 88f, panel, game.Home);
        }

        void Help()
        {
            Overlay(.96f); Brand("HOW TO PLAY");
            Label("Lift. Steer. Let it settle.", .5f, .81f, 36, cream, 650, 70, true);
            Label("1   Tap a pump to lift nearby rings.\nLeft pump lifts on the left; right on the right.", .5f, .66f, 23, cream, 600, 110);
            Label(game.Sensor.UseSensor ? "2   Gently tilt to steer left or right.\nFind a comfortable position in Settings,\nthen tap Calibrate." : "2   Slide the steering control left or right.\nRelease to stop steering. You can pump,\nthen steer with the same finger.", .5f, .47f, 23, cream, 600, 150);
            string goal = game.Level.UsesCollectors ? "3   Let rings settle inside either tray.\nCollect " + game.TargetCount + " of " + game.Rings.Count + " to finish." : "3   Let rings fall over the peg tips.\nThey count when settled in an available slot.";
            string timing = game.Level.freezeSeconds > 0 ? "Finish before " + game.Level.freezeSeconds.ToString("0") + "s. Pause stops the countdown." : "This level has no time limit.";
            Label(goal + "\n" + timing, .5f, .285f, 22, cream, 620, 120);
            Label("Mint: light (1 mark) · Coral: standard (2)\nViolet: heavy (3) · Small gold: agile mini", .5f, .177f, 18, muted, 620, 52);
            Button("GOT IT", .5f, .1f, 430, 96, gold, game.CloseHelp);
        }

        void Frozen()
        {
            Overlay(.94f); Brand("ICE CHALLENGE");
            Label("The tank froze.", .5f, .72f, 42, cream, 620, 80, true);
            Label(game.Caught + " / " + game.TargetCount + " rings landed this attempt.\nYour earned stars and completed levels are safe.", .5f, .6f, 23, muted, 620, 110);
            Button("TRY AGAIN", .5f, .43f, 550, 96, gold, game.Restart);
            Button("CHOOSE A LEVEL", .5f, .31f, 550, 88, panel, () => game.SetScreen(GameScreen.Levels));
            Button("HOME", .5f, .19f, 550, 88, panel, game.Home);
        }

        void ConfirmRestart()
        {
            Overlay(.95f); Brand("RESTART LEVEL");
            Label("Start this level again?", .5f, .64f, 36, cream, 620, 80, true);
            Label("This attempt will reset.\nYour completed levels and earned stars stay saved.", .5f, .53f, 23, muted, 620, 90);
            Button("KEEP THIS ATTEMPT", .5f, .38f, 550, 96, gold, game.Resume);
            Button("RESTART", .5f, .26f, 550, 88, panel, game.Restart);
        }

        void Levels()
        {
            Overlay(.88f); Brand("YOUR LITTLE ADVENTURES");
            Label("Find your flow.", .5f, .855f, 36, cream, 630f, 55f, true);
            int pages = Mathf.CeilToInt(game.campaign.levels.Length / (float)LevelsPerPage);
            levelPage = Mathf.Clamp(levelPage, 0, pages - 1);
            Label("Complete each goal to open the next · " + (levelPage + 1) + " / " + pages, .5f, .807f, 20, muted);
            int first = levelPage * LevelsPerPage;
            for (int i = first; i < Mathf.Min(first + LevelsPerPage, game.campaign.levels.Length); i++)
            {
                int index = i; var level = game.campaign.levels[i]; float y = .714f - (i - first) * .122f;
                bool unlocked = game.Unlocked(i); var record = game.Progress.Data.levels.Find(x => x.id == level.id);
                var button = Button("", .5f, y, 626f, 136f, unlocked ? panel : Hex("#153641"), () => game.StartLevel(index), unlocked);
                var rowColors = button.colors; rowColors.disabledColor = UnityEngine.Color.white; button.colors = rowColors;
                var badge = Rect("Adventure badge", .095f, .5f, 64, 64, button.transform);
                var badgeImage = Image(badge, unlocked ? Hex("#286A71") : Hex("#224851"), false); badgeImage.sprite = circle; badgeImage.raycastTarget = false;
                var number = Rect("Number", .095f, .5f, 60f, 54f, button.transform); Text(number, (i + 1).ToString("00"), 29, unlocked ? gold : muted, FontStyle.Bold);
                var title = Rect("Title", .48f, .69f, 380f, 38f, button.transform); Text(title, level.title, 23, unlocked ? cream : muted, FontStyle.Bold, TextAnchor.MiddleLeft);
                var hint = Rect("Lesson", .48f, .39f, 380f, 44f, button.transform); Text(hint, unlocked ? level.lesson : "Complete " + game.campaign.levels[i - 1].title, 14, muted, FontStyle.Normal, TextAnchor.MiddleLeft);
                if (record != null && record.stars > 0)
                {
                    var best = Rect("Best", .48f, .15f, 380f, 22f, button.transform); Text(best, "BEST  " + record.bestSeconds.ToString("0.0") + "s  /  " + record.bestPumps + " pumps", 11, gold, FontStyle.Normal, TextAnchor.MiddleLeft);
                }
                else if (unlocked)
                {
                    var goal = Rect("Star goal", .48f, .15f, 380f, 22f, button.transform);
                    Text(goal, level.freezeSeconds > 0 ? "TIMED ICE CHALLENGE · " + level.freezeSeconds.ToString("0") + "s" : level.UsesCollectors ? "COLLECT " + level.TargetCount + " OF " + level.rings.Length : "NO TIME LIMIT", 13, gold, FontStyle.Normal, TextAnchor.MiddleLeft);
                }
                Label(unlocked ? ">" : "LOCKED", .866f, y, unlocked ? 28 : 10, muted, 70f);
                Stars(.78f, y + .019f, record?.stars ?? 0, 18f, 22f);
            }
            Button("< PREV", .21f, .083f, 180f, 88f, panel, () => { levelPage--; Rebuild(); }, levelPage > 0);
            Button("HOME", .5f, .083f, 170f, 88f, panel, game.Home);
            Button("NEXT >", .79f, .083f, 180f, 88f, panel, () => { levelPage++; Rebuild(); }, levelPage + 1 < pages);
        }

        void Toggle(string title, string description, float y, Func<bool> value, Action<bool> set, bool interactable = true)
        {
            var row = Rect(title, .5f, y, 618f, 100f); Image(row, panel);
            Text(Rect("Title", .4f, .67f, 420f, 30f, row), title, 20, cream, FontStyle.Bold, TextAnchor.MiddleLeft);
            Text(Rect("Description", .4f, .28f, 420f, 42f, row), description, 17, muted, FontStyle.Normal, TextAnchor.MiddleLeft);
            Button(value() ? "ON" : "OFF", .85f, y, 90f, 88f, value() ? gold : Hex("#375260"), () => { set(!value()); game.SaveSettings(); Rebuild(); }, interactable);
        }
        void Settings()
        {
            Overlay(.96f); Brand("MAKE YOURSELF COMFORTABLE");
            Label("Your way to play.", .5f, .852f, 34, cream, 630f, 55f, true);
            Toggle("Sound effects", "Bubbles, landings and soft clicks", .75f, () => game.Settings.sound, v => game.Settings.sound = v);
            Toggle("Music", "A quiet original melody", .64f, () => game.Settings.music, v => game.Settings.music = v);
            Toggle("Haptics", "Gentle taps on supported phones", .53f, () => game.Settings.haptics, v => game.Settings.haptics = v);
            Toggle("Reduced motion", "Fewer decorative bubbles and effects", .42f, () => game.Settings.reduceMotion, v => game.Settings.reduceMotion = v);
            Button("CONTROLS & CALIBRATION", .5f, .30f, 618, 88, panel, () => game.SetScreen(GameScreen.Controls));
            Toggle("Local diagnostics", "Optional playtest log; stays on this device", .18f, () => game.Settings.localDiagnostics, v => game.Settings.localDiagnostics = v);
            Button("DONE", .5f, .07f, 310f, 88f, gold, game.CloseSettings);
        }

        void Controls()
        {
            Overlay(.96f); Brand("CONTROLS");
            Label("Find your balance.", .5f, .84f, 36, cream, 630, 60, true);
            Toggle("Phone tilt", game.Sensor.SensorAvailable ? "Off: use the on-screen steering slider" : "No motion sensor; touch steering is active", .72f, () => game.Settings.motion && game.Sensor.SensorAvailable, v => game.Settings.motion = v, game.Sensor.SensorAvailable);
            Label(game.Sensor.UseSensor ? "Tilt gently to steer. Hold your phone\ncomfortably, then calibrate to center it." : "Slide to steer, release to center.\nYou can play with the phone resting on a table.", .5f, .57f, 23, cream, 620, 100);
            sensitivityLabel = Label("STEERING SENSITIVITY  " + game.Settings.sensitivity.ToString("0.0"), .5f, .45f, 20, muted);
            Slider("Sensitivity", .5f, .38f, 510, .8f, 4.5f, game.Settings.sensitivity, v => { game.Settings.sensitivity = v; game.Sensor.sensitivity = v; sensitivityLabel.text = "STEERING SENSITIVITY  " + v.ToString("0.0"); });
            Button("CALIBRATE NEUTRAL POSITION", .5f, .25f, 600, 88, panel, game.Calibrate, game.Sensor.UseSensor);
            Label(game.Notice ?? (game.Sensor.UseSensor ? "Calibration changes steering, not your progress." : "Touch steering needs no calibration."), .5f, .16f, 18, muted);
            Button("DONE", .5f, .07f, 310, 88, gold, () => { game.SaveSettings(); game.SetScreen(GameScreen.Settings); });
        }

        void Complete()
        {
            Overlay(.9f); Brand("A LITTLE MOMENT OF JOY");
            Label("Every ring. Nicely done.", .5f, .79f, 38, cream, 640, 70, true);
            Label(game.Level.title + " complete", .5f, .725f, 23, muted);
            if (showResults)
            {
                Stars(.5f, .635f, game.EarnedStars, 44, 65);
                Label(game.Elapsed.ToString("0.0") + "s   /   " + game.Pumps + (game.Pumps == 1 ? " pump" : " pumps"), .5f, .57f, 24, gold);
                Label("3 stars: within " + game.Level.goldSeconds.ToString("0") + "s and " + game.Level.goldPumps + " pumps\n2 stars: within " + game.Level.silverSeconds.ToString("0") + "s. Every finish earns a star.", .5f, .505f, 19, muted, 620, 70);
            }
            else Label("All rings landed.\nTake a breath, or try the next little challenge.", .5f, .59f, 24, cream, 620, 120);
            Button(showResults ? "HIDE RESULTS" : "VIEW RESULTS & STARS", .5f, .41f, 550, 88, panel, () => { showResults = !showResults; Rebuild(); });
            bool last = game.LevelIndex == game.campaign.levels.Length - 1;
            Button(last ? "YOUR ADVENTURES" : "NEXT ADVENTURE", .5f, .29f, 550f, 96f, gold, () => { if (last) game.SetScreen(GameScreen.Levels); else game.StartLevel(game.LevelIndex + 1); });
            Button("PLAY AGAIN", .31f, .18f, 255f, 88f, panel, game.Restart);
            Button("HOME", .69f, .18f, 255f, 88f, panel, game.Home);
            Label(!string.IsNullOrEmpty(game.Progress.LastError) ? "Save unavailable. See the message below." : "Progress saved on this device.", .5f, .08f, 18, muted);
        }

        void Collection()
        {
            Overlay(.91f); Brand("YOUR TOY, YOUR COLOUR");
            Label("Small treasures.", .5f, .855f, 36, cream, 630f, 55f, true);
            Label("Earn stars to unlock new enamel shells.", .5f, .80f, 16, muted);
            string[] names = { "Sea Glass", "Apricot", "Moonstone" }; string[] colors = { "#8AD6C4", "#EEAF89", "#ABA0DB" };
            for (int i = 0; i < 3; i++)
            {
                int choice = i, required = i == 1 ? 5 : i == 2 ? 10 : 0; float y = .67f - i * .178f;
                Card(.5f, y, 620f, 184f);
                var swatch = Rect("Enamel swatch", .18f, y, 86f, 105f); Image(swatch, Hex(colors[i]));
                var window = Rect("Miniature window", .5f, .61f, 67, 64, swatch);
                var reef = window.gameObject.AddComponent<RawImage>(); reef.texture = Resources.Load<Texture2D>("CoralLagoon"); reef.raycastTarget = false;
                for (int pump = 0; pump < 2; pump++)
                {
                    var mini = Image(Rect("Mini pump", pump == 0 ? .27f : .73f, .16f, 16, 16, swatch), pump == 0 ? Hex("#FF866B") : gold, false);
                    mini.sprite = circle; mini.raycastTarget = false;
                }
                Label(names[i], .48f, y + .033f, 25, cream, 290f, 40f, true);
                Label(required == 0 ? "Yours from the start" : required + " stars to unlock", .48f, y - .013f, 14, muted, 290f);
                bool unlocked = game.Progress.Data.TotalStars >= required;
                Button(game.Settings.shell == i ? "IN USE" : unlocked ? "USE" : "LOCKED", .81f, y, 116f, 48f, game.Settings.shell == i ? gold : Hex("#36515D"), () => game.SelectShell(choice), unlocked);
            }
            Button("HOME", .5f, .105f, 310f, 58f, gold, game.Home);
        }

        void LateUpdate()
        {
            if (game == null || !game.Ready) return;
            var safe = ToyPresentation.SafeScreenRect;
            safeArea.anchorMin = new Vector2(safe.xMin / UnityEngine.Screen.width, safe.yMin / UnityEngine.Screen.height);
            safeArea.anchorMax = new Vector2(safe.xMax / UnityEngine.Screen.width, safe.yMax / UnityEngine.Screen.height);
            screenAge += Time.unscaledDeltaTime;
            float delay = builtScreen == GameScreen.Complete ? .55f : 0f;
            fade.alpha = game.Settings.reduceMotion ? 1f : Mathf.Clamp01((screenAge - delay) * 7f);
            fade.blocksRaycasts = fade.alpha > .9f;
        }
        void RefreshWorldAnchors()
        {
            if (game == null || !game.Ready || screenRoot == null) return;
            if (homeSubtitle != null) PlaceAt(homeSubtitle.rectTransform, new Vector3(0f, 5.25f, -1f));
            if (game.Playing)
            {
                progress.text = game.Caught + " / " + game.TargetCount + (game.Level.UsesCollectors ? " RINGS COLLECTED" : " RINGS LANDED");
                lesson.text = !game.Progress.Data.tutorialSeen && game.LevelIndex == 0
                    ? game.Pumps == 0 ? "Tap a pump to lift the ring. No need to hurry."
                    : game.Sensor.UseSensor ? "Tilt to guide it above the peg, then let it fall." : "Slide to guide it above the peg, then let it fall."
                    : game.Level.lesson;
                if (environmentStatus != null)
                {
                    environmentStatus.text = game.Environment.Status;
                    PlaceAt((RectTransform)environmentStatus.transform.parent, new Vector3(0f, 4.36f, -1f));
                }
                PlacePump(leftPump, game.Presentation.LeftButton); PlacePump(rightPump, game.Presentation.RightButton);
                if (lean != null) PlaceAt(lean.GetComponent<RectTransform>(), new Vector3(0f, -5.18f, -1f));
                PlaceAt(controlsHint.rectTransform, new Vector3(0f, -3.7f, -1f));
            }
        }
        void PlacePump(RectTransform button, Transform world)
        {
            PlaceAt(button, world.position);
            float diameter = 1.32f * CameraPixelsPerWorldUnit() / Canvas.scaleFactor;
            button.sizeDelta = Vector2.one * Mathf.Max(96f, diameter);
        }
        float CameraPixelsPerWorldUnit() => game.Presentation.Camera.pixelHeight / (game.Presentation.Camera.orthographicSize * 2f);
        void PlaceAt(RectTransform element, Vector3 position)
        {
            Vector2 screen = game.Presentation.Camera.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screenRoot, screen, Canvas.worldCamera, out var local);
            element.anchorMin = element.anchorMax = new Vector2(.5f, .5f);
            element.anchoredPosition = local;
        }
        void OnDestroy()
        {
            Canvas.willRenderCanvases -= RefreshWorldAnchors;
            if (game != null) game.Changed -= Rebuild;
            Destroy(rounded); Destroy(circle); Destroy(starSprite); Destroy(roundedTexture); Destroy(circleTexture); Destroy(starTexture);
        }
    }
}
