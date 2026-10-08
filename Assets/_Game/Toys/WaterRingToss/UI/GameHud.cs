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
        Text progress, timer, pumpCount, sensitivityLabel, controlsHint, homeSubtitle;
        Slider lean;
        Font font, displayFont;
        RectTransform progressPill, timerPill;
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
            scaler.referenceResolution = new Vector2(720f, 1280f); scaler.matchWidthOrHeight = .5f;
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
            if (screenRoot != null) { screenRoot.gameObject.SetActive(false); Destroy(screenRoot.gameObject); }
            screenRoot = Stretch("Screen - " + game.Screen, safeArea); fade = screenRoot.gameObject.AddComponent<CanvasGroup>();
            screenAge = 0f; builtScreen = game.Screen; progress = timer = pumpCount = null; leftPump = rightPump = null; lean = null;
            controlsHint = homeSubtitle = null;
            progressPill = timerPill = null;
            switch (game.Screen)
            {
                case GameScreen.Home: Home(); break;
                case GameScreen.Playing: Play(); break;
                case GameScreen.Paused: Pause(); break;
                case GameScreen.Levels: Levels(); break;
                case GameScreen.Settings: Settings(); break;
                case GameScreen.Complete: Complete(); break;
                case GameScreen.Collection: Collection(); break;
            }
            if (!string.IsNullOrEmpty(game.Progress.LastError)) Label("Save needs attention - " + game.Progress.LastError, .5f, .013f, 12, gold, 650f);
        }

        void Home()
        {
            Brand("SMALL WONDERS.  HAPPY HANDS.");
            Label("A little ocean.\nA lot of joy.", .5f, .824f, 54, cream, 650f, 140f, true);
            Label("Your pocket-sized escape.", .5f, .768f, 17, muted, 630f, 24f);
            homeSubtitle = Label("C O R A L   C L U B", .5f, .70f, 14, ink);
            Card(.5f, .162f, 620f, 225f);
            int count = game.Progress.Data.TotalStars;
            Label(count + " / " + game.campaign.levels.Length * 3 + " STARS  ·  FIVE LITTLE ADVENTURES", .5f, .228f, 12, gold);
            Button("LET'S PLAY  >", .5f, .178f, 554f, 72f, gold, () => game.StartLevel(game.LevelIndex));
            Button("ADVENTURES", .3f, .111f, 256f, 49f, Hex("#286675"), () => game.SetScreen(GameScreen.Levels));
            Button("TOY SHELLS", .7f, .111f, 256f, 49f, Hex("#286675"), () => game.SetScreen(GameScreen.Collection));
            Button("SETTINGS", .5f, .034f, 160f, 35f, panel, game.OpenSettings);
        }

        void Play()
        {
            var title = Label((game.LevelIndex + 1).ToString("00") + "  /  " + game.Level.title, 0f, 1f, 20, cream, 450f, 36f, true);
            title.color = ink;
            title.alignment = TextAnchor.MiddleLeft; title.rectTransform.pivot = new Vector2(0, .5f);
            title.rectTransform.anchoredPosition = new Vector2(24, -36);
            SettingsButton();
            progressPill = Rect("Ring counter capsule", .5f, .5f, 150, 34); Image(progressPill, new Color(.025f, .20f, .25f, .85f)).raycastTarget = false;
            timerPill = Rect("Time capsule", .5f, .5f, 92, 34); Image(timerPill, new Color(.025f, .20f, .25f, .85f)).raycastTarget = false;
            progress = Label("", .32f, .735f, 14, cream, 150f);
            timer = Label("", .7f, .735f, 14, cream, 92f);
            leftPump = PumpButton(true); rightPump = PumpButton(false);
            pumpCount = Label("", .5f, .296f, 12, ink, 150f);
            controlsHint = Label(game.Sensor.UseSensor ? "TILT TO\nSTEER" : "DRAG TO STEER", .5f, .09f, 11, ink, 140f, 42f);
            if (!game.Sensor.UseSensor)
            {
                lean = Slider("Lean", .5f, .05f, 430f, -1f, 1f, 0f, value => game.Sensor.SetVirtualTilt(value));
                lean.gameObject.AddComponent<CenterOnRelease>();
            }
        }
        void SettingsButton()
        {
            var rect = Rect("Game settings", 1f, 1f, 80, 80);
            rect.anchoredPosition = new Vector2(-48, -42);
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
            Text(Stretch("PUMP", rect), "PUSH", 17, ink, FontStyle.Bold);
            return rect;
        }
        Slider Slider(string name, float x, float y, float width, float min, float max, float value, Action<float> action)
        {
            var rect = Rect(name, x, y, width, 28f); Image(rect, Hex("#286170"));
            var slider = rect.gameObject.AddComponent<Slider>(); slider.minValue = min; slider.maxValue = max; slider.value = value;
            var area = Stretch("Handle area", rect); area.offsetMin = new Vector2(18f, 0f); area.offsetMax = new Vector2(-18f, 0f);
            var handle = Rect("Handle", .5f, .5f, 28f, 28f, area); var image = Image(handle, cream, false); image.sprite = circle;
            slider.handleRect = handle; slider.targetGraphic = image;
            handle.sizeDelta = new Vector2(28f, 0f);
            slider.onValueChanged.AddListener(v => action(v));
            var nav = slider.navigation; nav.mode = Navigation.Mode.None; slider.navigation = nav;
            return slider;
        }

        void Pause()
        {
            Overlay(.75f); Brand("A LITTLE BREATHER"); Card(.5f, .5f, 610f, 660f);
            Label("Take your time.", .5f, .66f, 34, cream, 540f, 60f, true);
            Label("Your rings are right where you left them.", .5f, .607f, 17, muted);
            Button("KEEP PLAYING", .5f, .55f, 510f, 74f, gold, game.Resume);
            Button("TRY AGAIN", .5f, .477f, 510f, 62f, Hex("#2B4E59"), game.Restart);
            Button("SETTINGS", .5f, .407f, 510f, 62f, Hex("#2B4E59"), game.OpenSettings);
            Button("LEVELS", .5f, .337f, 510f, 62f, Hex("#2B4E59"), () => game.SetScreen(GameScreen.Levels));
            Button("BACK TO HOME", .5f, .27f, 400f, 40f, panel, game.Home);
        }

        void Levels()
        {
            Overlay(.88f); Brand("YOUR LITTLE ADVENTURES");
            Label("Find your flow.", .5f, .855f, 36, cream, 630f, 55f, true);
            Label(game.Progress.Data.TotalStars + " of " + game.campaign.levels.Length * 3 + " stars  /  finish a level to open the next", .5f, .807f, 15, muted);
            for (int i = 0; i < game.campaign.levels.Length; i++)
            {
                int index = i; var level = game.campaign.levels[i]; float y = .714f - i * .122f;
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
                    Text(goal, "3 STARS  /  " + level.goldSeconds.ToString("0") + "s + " + level.goldPumps + " pumps or fewer", 11, gold, FontStyle.Normal, TextAnchor.MiddleLeft);
                }
                Label(unlocked ? ">" : "LOCKED", .866f, y, unlocked ? 28 : 10, muted, 70f);
                Stars(.78f, y + .019f, record?.stars ?? 0, 18f, 22f);
            }
            Button("HOME", .5f, .083f, 260f, 50f, panel, game.Home);
        }

        void Toggle(string title, string description, float y, Func<bool> value, Action<bool> set, bool interactable = true)
        {
            var row = Rect(title, .5f, y, 618f, 79f); Image(row, panel);
            Text(Rect("Title", .4f, .67f, 420f, 30f, row), title, 20, cream, FontStyle.Bold, TextAnchor.MiddleLeft);
            Text(Rect("Description", .4f, .28f, 420f, 30f, row), description, 13, muted, FontStyle.Normal, TextAnchor.MiddleLeft);
            Button(value() ? "ON" : "OFF", .85f, y, 90f, 43f, value() ? gold : Hex("#375260"), () => { set(!value()); game.SaveSettings(); Rebuild(); }, interactable);
        }
        void Settings()
        {
            Overlay(.96f); Brand("MAKE YOURSELF COMFORTABLE");
            Label("Your way to play.", .5f, .852f, 34, cream, 630f, 55f, true);
            Toggle("Sound effects", "Water, clicks and little celebrations", .768f, () => game.Settings.sound, v => game.Settings.sound = v);
            Toggle("Music", "A quiet original melody", .688f, () => game.Settings.music, v => game.Settings.music = v);
            Toggle("Haptics", "Tactile feedback on supported phones", .608f, () => game.Settings.haptics, v => game.Settings.haptics = v);
            Toggle("Phone tilt", game.Sensor.SensorAvailable ? "Turn off to use the touch slider" : "No motion sensor detected; touch stays available", .528f, () => game.Settings.motion && game.Sensor.SensorAvailable, v => game.Settings.motion = v, game.Sensor.SensorAvailable);
            Toggle("Reduced motion", "Gentler visuals and fewer particles", .448f, () => game.Settings.reduceMotion, v => game.Settings.reduceMotion = v);
            sensitivityLabel = Label("STEERING SENSITIVITY  " + game.Settings.sensitivity.ToString("0.0"), .5f, .38f, 13, muted);
            Slider("Sensitivity", .5f, .343f, 510f, .8f, 4.5f, game.Settings.sensitivity, v => { game.Settings.sensitivity = v; game.Sensor.sensitivity = v; sensitivityLabel.text = "STEERING SENSITIVITY  " + v.ToString("0.0"); });
            Button("CALIBRATE NEUTRAL POSITION", .5f, .284f, 560f, 51f, panel, game.Calibrate);
            Label(game.Notice ?? "Hold the phone comfortably, then calibrate.", .5f, .241f, 12, muted);
            Toggle("Local diagnostics", "Optional playtest events saved only on this device", .184f, () => game.Settings.localDiagnostics, v => game.Settings.localDiagnostics = v);
            Button("DONE", .5f, .083f, 310f, 58f, gold, game.CloseSettings);
        }

        void Complete()
        {
            Overlay(.79f); Brand("A LITTLE MOMENT OF JOY"); Card(.5f, .51f, 624f, 686f);
            Stars(.5f, .728f, game.EarnedStars, 67f, 92f);
            Label(game.EarnedStars == 3 ? "Beautifully done." : "Every ring, a little win.", .5f, .644f, 33, cream, 590f, 64f, true);
            Label(game.Level.title + " complete", .5f, .596f, 18, muted);
            Label(game.Elapsed.ToString("0.0") + "s    /    " + game.Pumps + " pumps", .5f, .535f, 24, gold, 550f, 50f, true);
            Label("3 stars: " + game.Level.goldSeconds.ToString("0") + "s or less + " + game.Level.goldPumps + " pumps or fewer", .5f, .486f, 14, muted);
            Label("2 stars: finish within " + game.Level.silverSeconds.ToString("0") + "s.  Every finish earns a star.", .5f, .458f, 13, muted);
            bool last = game.LevelIndex == game.campaign.levels.Length - 1;
            Button(last ? "YOUR ADVENTURE COLLECTION" : "NEXT LITTLE ADVENTURE", .5f, .379f, 550f, 73f, gold, () => { if (last) game.SetScreen(GameScreen.Levels); else game.StartLevel(game.LevelIndex + 1); });
            Button("PLAY AGAIN", .31f, .297f, 255f, 54f, Hex("#2B4E59"), game.Restart);
            Button("HOME", .69f, .297f, 255f, 54f, Hex("#2B4E59"), game.Home);
            Label(!string.IsNullOrEmpty(game.Progress.LastError) ? "Save unavailable. See the message below." : last ? "All five adventures explored. Revisit them to earn every star." : "Progress saved on this device.", .5f, .21f, 14, muted);
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
                progress.text = "RINGS  " + game.Caught + " / " + game.Rings.Count;
                timer.text = game.Elapsed.ToString("0.0") + "s";
                pumpCount.text = game.Pumps + " PUMPS";
                PlacePump(leftPump, game.Presentation.LeftButton); PlacePump(rightPump, game.Presentation.RightButton);
                PlaceAt(progress.rectTransform, new Vector3(-2.05f, game.Presentation.PlayfieldTop - .48f, -1f));
                PlaceAt(timer.rectTransform, new Vector3(2.15f, game.Presentation.PlayfieldTop - .48f, -1f));
                PlaceAt(progressPill, new Vector3(-2.05f, game.Presentation.PlayfieldTop - .48f, -1f));
                PlaceAt(timerPill, new Vector3(2.15f, game.Presentation.PlayfieldTop - .48f, -1f));
                PlaceAt(pumpCount.rectTransform, new Vector3(0f, -3.17f, -1f));
                if (lean != null) PlaceAt(lean.GetComponent<RectTransform>(), new Vector3(0f, -4.95f, -1f));
                PlaceAt(controlsHint.rectTransform, new Vector3(0f, -4.05f, -1f));
            }
        }
        void PlacePump(RectTransform button, Transform world)
        {
            PlaceAt(button, world.position);
            float diameter = 1.32f * CameraPixelsPerWorldUnit() / Canvas.scaleFactor;
            button.sizeDelta = Vector2.one * Mathf.Max(72f, diameter);
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
