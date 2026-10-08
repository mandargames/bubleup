using System.Collections.Generic;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class ToyPresentation : MonoBehaviour
    {
        public const float LandingWidth = .58f;
        public const float PegStemRadius = .055f;
        public const float TankHalfWidth = 2.8f, TankBottom = -2.45f, TankTop = 4.75f, NozzleX = 1.95f;
        public Camera Camera { get; private set; }
        public PhysicsMaterial ContactMaterial { get; private set; }
        public float PlayfieldTop => TankTop;
        #if UNITY_EDITOR
        public static Rect? SafeAreaOverride;
        #endif
        public static Rect SafeScreenRect
        {
            get
            {
                #if UNITY_EDITOR
                if (SafeAreaOverride.HasValue) return SafeAreaOverride.Value;
                #endif
                return Screen.safeArea;
            }
        }
        public Transform LeftButton => pumpButtons[0];
        public Transform RightButton => pumpButtons[1];
        GameSession game;
        Transform levelRoot;
        readonly List<Object> resources = new List<Object>();
        readonly List<Object> levelResources = new List<Object>();
        readonly List<Rigidbody> pegBodies = new List<Rigidbody>();
        readonly Transform[] pumpButtons = new Transform[2];
        readonly float[] pumpPulse = new float[2];
        readonly List<Particle> particles = new List<Particle>();
        readonly List<Transform> ambientBubbles = new List<Transform>();
        readonly List<Transform> showcaseRings = new List<Transform>();
        Material body, trim, metal, pearl, bubble, water, glass, dark, backdrop, fishMaterial;
        Transform backdropPlane;
        Transform fishVisual, fishWarning;
        Material[] ringMaterials;
        Mesh ringMesh, markMesh;
        static readonly int[] CurrentBoundsIds = { Shader.PropertyToID("_CurrentBounds0"), Shader.PropertyToID("_CurrentBounds1"), Shader.PropertyToID("_CurrentBounds2"), Shader.PropertyToID("_CurrentBounds3") };
        static readonly int[] CurrentFlowIds = { Shader.PropertyToID("_CurrentFlow0"), Shader.PropertyToID("_CurrentFlow1"), Shader.PropertyToID("_CurrentFlow2"), Shader.PropertyToID("_CurrentFlow3") };
        int particleCursor;
        sealed class Particle { public Transform transform; public Vector3 velocity; public float age, life, size; public bool celebration; }
        public static Color Color(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }

        Material Surface(string name, string hex, float gloss = .55f, float metallic = 0f)
        {
            var material = new Material(game.campaign.surfaceMaterial) { name = name, color = Color(hex) };
            material.SetFloat("_Glossiness", gloss); material.SetFloat("_Metallic", metallic);
            resources.Add(material); return material;
        }

        public void Initialize(GameSession session)
        {
            game = session;
            ContactMaterial = new PhysicsMaterial("Wet toy contacts") { dynamicFriction = .06f, staticFriction = .08f, bounciness = .10f, frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum };
            resources.Add(ContactMaterial);
            var cameraObject = new GameObject("Studio camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false); cameraObject.transform.position = new Vector3(0f, 0f, -20f);
            Camera = cameraObject.GetComponent<Camera>(); Camera.orthographic = true; Camera.orthographicSize = 8f;
            Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = Color("#101F2B");
            Camera.allowHDR = false; Camera.nearClipPlane = .1f; cameraObject.tag = "MainCamera";
            Light("Large softbox", Quaternion.Euler(25f, -32f, 0f), .86f, "#FFF6E9");
            Light("Cool fill", Quaternion.Euler(-20f, 145f, 0f), .62f, "#92E9ED");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color("#ADC7CE") * .58f;
            body = Surface("Lagoon enamel", "#8AD6C4", .72f);
            trim = Surface("Tank seal", "#16777D", .55f);
            metal = Surface("Champagne brushed metal", "#E5CC91", .78f, .28f);
            pearl = Surface("Warm porcelain", "#FFF2CC", .78f);
            dark = Surface("Nozzle vents", "#23414D", .22f);
            bubble = Surface("Bubble glints", "#BFEFF3", .9f, .12f);
            ringMaterials = new Material[4];
            for (int i = 0; i < ringMaterials.Length; i++)
            {
                var profile = RingProfile.For((RingKind)i);
                ringMaterials[i] = Surface(profile.Name + " ring", profile.Color, .46f, 0f);
                ringMaterials[i].EnableKeyword("_EMISSION");
                ringMaterials[i].SetColor("_EmissionColor", Color(profile.Color) * .12f);
            }
            fishMaterial = Surface("Apricot reef fish", "#FFAD52", .42f);
            water = new Material(game.campaign.waterMaterial); resources.Add(water);
            water.mainTexture = Resources.Load<Texture2D>("CoralLagoon");
            water.SetVector("_TankArea", new Vector4(-TankHalfWidth, TankBottom, TankHalfWidth * 2f, TankTop - TankBottom));
            glass = new Material(game.campaign.glassMaterial); resources.Add(glass);
            ringMesh = ToyGeometry.MoldedRing(FloatingRing.InnerRadius, FloatingRing.OuterRadius, FloatingRing.Thickness, .012f); resources.Add(ringMesh);
            markMesh = ToyGeometry.SoftDisc(.03f, .004f); resources.Add(markMesh);
            backdrop = new Material(Resources.Load<Shader>("OceanBackdrop")); resources.Add(backdrop);
            // CreatePrimitive(Quad) implicitly needs MeshCollider, which is stripped
            // from iPhone players. This decorative surface needs no physics component.
            var backdropObject = new GameObject("Ocean atmosphere", typeof(MeshFilter), typeof(MeshRenderer));
            backdropPlane = backdropObject.transform; backdropPlane.SetParent(transform, false);
            backdropPlane.localPosition = new Vector3(0, 0, 4); backdropPlane.localScale = new Vector3(30, 30, 1);
            var backdropMesh = ToyGeometry.BackdropQuad(); resources.Add(backdropMesh);
            backdropObject.GetComponent<MeshFilter>().sharedMesh = backdropMesh;
            backdropObject.GetComponent<MeshRenderer>().sharedMaterial = backdrop;

            // Layered enclosure: shell, metallic lip, dark rubber seal and recessed water chamber.
            float tankHeight = TankTop - TankBottom, tankCenter = (TankTop + TankBottom) * .5f;
            Box("Soft shadow", transform, new Vector3(.05f, .155f, 1.4f), new Vector3(6.52f, 10.58f, .8f), .48f, dark, false);
            Box("Enamel casing", transform, new Vector3(0f, .275f, .8f), new Vector3(6.4f, 10.45f, .8f), .48f, body, false);
            Box("Recess lip", transform, new Vector3(0f, tankCenter, .19f), new Vector3(TankHalfWidth * 2 + .46f, tankHeight + .46f, .4f), .26f, metal, false);
            Box("Rubber seal", transform, new Vector3(0f, tankCenter, -.04f), new Vector3(TankHalfWidth * 2 + .28f, tankHeight + .28f, .26f), .19f, trim, false);
            Box("Water chamber", transform, new Vector3(0f, tankCenter, -.21f), new Vector3(TankHalfWidth * 2, tankHeight, .12f), .05f, water, false);
            Box("Left glass glint", transform, new Vector3(-TankHalfWidth + .08f, tankCenter + .3f, -.45f), new Vector3(.035f, tankHeight - 1.18f, .035f), .015f, pearl, false);
            Box("Top glass glint", transform, new Vector3(-.5f, TankTop - .06f, -.45f), new Vector3(4.2f, .025f, .025f), .01f, pearl, false);
            // Subtle sheen uses a transparent shader and never obscures gameplay objects.
            Box("Acrylic reflection", transform, new Vector3(0f, tankCenter, -1.3f), new Vector3(TankHalfWidth * 2 - .07f, tankHeight - .09f, .02f), .009f, glass, false);
            // Molded details make the lower console feel like a physical collectible.
            Box("Console inset", transform, new Vector3(0, -3.64f, .30f), new Vector3(5.94f, 1.66f, .16f), .10f, trim, false);
            Box("Console face", transform, new Vector3(0, -3.60f, .19f), new Vector3(5.86f, 1.57f, .14f), .09f, body, false);
            CreateAmbientBubbles();
            for (int i = 0; i < 4; i++)
            {
                var ring = new GameObject("Display ring " + i, typeof(MeshFilter), typeof(MeshRenderer));
                ring.transform.SetParent(transform, false);
                ring.GetComponent<MeshFilter>().sharedMesh = ringMesh;
                ring.GetComponent<MeshRenderer>().sharedMaterial = ringMaterials[(i + 1) % ringMaterials.Length];
                showcaseRings.Add(ring.transform);
            }
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -NozzleX : NozzleX;
                Disc("Pump socket", transform, new Vector3(x, -3.65f, .09f), .72f, .15f, trim);
                Disc("Metal pump lip", transform, new Vector3(x, -3.65f, -.04f), .65f, .08f, metal);
                pumpButtons[side] = Disc("Pump diaphragm", transform, new Vector3(x, -3.65f, -.18f), .57f, .22f, side == 0 ? ringMaterials[(int)RingKind.Standard] : ringMaterials[(int)RingKind.Mini]);
                for (int screw = 0; screw < 2; screw++)
                {
                    var pos = new Vector3(side == 0 ? -2.93f : 2.93f, screw == 0 ? 5.15f : -4.39f, .26f);
                    var disc = Disc("Casing screw", transform, pos, .065f, .022f, metal);
                    var slot = Box("Screw slot", transform, pos + Vector3.back * .023f, new Vector3(.075f, .013f, .015f), .006f, dark, false);
                }
            }
            for (int i = 0; i < 72; i++)
            {
                var sphere = Primitive("Water particle", PrimitiveType.Sphere, transform, Vector3.zero, Vector3.one * .04f, bubble);
                sphere.gameObject.SetActive(false); particles.Add(new Particle { transform = sphere });
            }
        }

        void CreateAmbientBubbles()
        {
            var mesh = ToyGeometry.Torus(1f, .035f); resources.Add(mesh);
            for (int i = 0; i < 20; i++)
            {
                var go = new GameObject("Drifting bubble " + i, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = bubble;
                float size = .025f + (i % 4) * .014f;
                go.transform.localScale = Vector3.one * size;
                ambientBubbles.Add(go.transform);
            }
        }

        void Light(string name, Quaternion rotation, float intensity, string hex)
        {
            var go = new GameObject(name, typeof(Light)); go.transform.SetParent(transform, false); go.transform.rotation = rotation;
            var light = go.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = intensity; light.color = Color(hex);
        }

        Transform Box(string name, Transform parent, Vector3 position, Vector3 size, float radius, Material material, bool level)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var mesh = size.x > 5f && size.z > .2f ? ToyGeometry.MoldedPanel(size, radius) : ToyGeometry.RoundedBox(size, radius); (level ? levelResources : resources).Add(mesh);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }
        Transform Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial = material; return go.transform;
        }
        Transform Disc(string name, Transform parent, Vector3 position, float radius, float depth, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var mesh = ToyGeometry.SoftDisc(radius, depth);
            bool isLevel = levelRoot != null && (parent == levelRoot || parent.IsChildOf(levelRoot));
            (isLevel ? levelResources : resources).Add(mesh);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        public void SetShell(int index)
        {
            if (body != null) body.color = Color(index == 1 ? "#EEAF89" : index == 2 ? "#ABA0DB" : "#8AD6C4");
        }

        public void LoadLevel()
        {
            if (levelRoot != null) { levelRoot.gameObject.SetActive(false); Destroy(levelRoot.gameObject); }
            foreach (var resource in levelResources) Destroy(resource); levelResources.Clear(); pegBodies.Clear();
            foreach (var particle in particles) { particle.life = 0f; particle.transform.gameObject.SetActive(false); }
            levelRoot = new GameObject("Level - " + game.Level.title).transform; levelRoot.SetParent(transform, false);
            fishVisual = fishWarning = null;
            float tankHeight = TankTop - TankBottom, tankCenter = (TankTop + TankBottom) * .5f;
            Wall("Left boundary", new Vector3(-TankHalfWidth - .125f, tankCenter, -.15f), new Vector3(.25f, tankHeight + .4f, 2f));
            Wall("Right boundary", new Vector3(TankHalfWidth + .125f, tankCenter, -.15f), new Vector3(.25f, tankHeight + .4f, 2f));
            Wall("Chamber floor", new Vector3(0f, TankBottom - .1f, -.15f), new Vector3(TankHalfWidth * 2 + .5f, .2f, 2f));
            Wall("Chamber ceiling", new Vector3(0f, TankTop + .1f, -.15f), new Vector3(TankHalfWidth * 2 + .5f, .2f, 2f));
            for (int i = 0; i < game.Level.pegs.Length; i++)
            {
                var definition = game.Level.pegs[i];
                var peg = new GameObject("Peg " + (i + 1)).transform; peg.SetParent(levelRoot, false);
                peg.localPosition = new Vector3(definition.tip.x, definition.tip.y, -.75f);
                peg.gameObject.AddComponent<PegSurface>().Index = i;
                var pegBody = peg.gameObject.AddComponent<Rigidbody>(); pegBody.isKinematic = true;
                pegBody.interpolation = RigidbodyInterpolation.Interpolate; pegBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                pegBodies.Add(pegBody);
                var stemContact = peg.gameObject.AddComponent<CapsuleCollider>();
                stemContact.center = new Vector3(0, -definition.length * .5f, 0);
                stemContact.radius = PegStemRadius; stemContact.height = definition.length + PegStemRadius * 2f;
                stemContact.sharedMaterial = ContactMaterial; stemContact.contactOffset = .003f;
                Primitive("Stem", PrimitiveType.Cylinder, peg, new Vector3(0f, -definition.length * .5f, 0f), new Vector3(.11f, definition.length * .5f, .11f), pearl);
                Primitive("Rounded tip", PrimitiveType.Sphere, peg, Vector3.zero, Vector3.one * .12f, pearl);
                // A compact support catches the ring's lower rim while leaving room
                // to pump past either side. Keep its visible and physical widths equal.
                var landingSize = new Vector3(LandingWidth, .10f, .7f);
                var landing = Box("Landing shelf", peg, new Vector3(0f, -definition.length, 0f), landingSize, .045f, metal, true);
                var landingContact = landing.gameObject.AddComponent<BoxCollider>(); landingContact.size = landingSize;
                landingContact.sharedMaterial = ContactMaterial; landingContact.contactOffset = .003f;
                for (int slot = 0; slot < definition.capacity; slot++)
                    Disc("Capacity mark", peg, new Vector3(-.12f * (definition.capacity - 1) + slot * .24f, -definition.length - .2f, .04f), .042f, .015f, pearl);
            }
            for (int i = 0; i < (game.Level.collectors?.Length ?? 0); i++) CreateCollector(i);
            if (game.Level.fishTraffic) CreateFish();
            foreach (var baffle in game.Level.baffles)
            {
                var shelf = Box("Water baffle", levelRoot, new Vector3(baffle.center.x, baffle.center.y, -.75f), new Vector3(baffle.size.x, baffle.size.y, .5f), .08f, metal, true);
                shelf.localRotation = Quaternion.Euler(0f, 0f, baffle.angle);
                var contact = shelf.gameObject.AddComponent<BoxCollider>(); contact.size = new Vector3(baffle.size.x, baffle.size.y, .5f); contact.sharedMaterial = ContactMaterial;
            }
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -NozzleX : NozzleX;
                Box("Jet housing", levelRoot, new Vector3(x, -2.3f, -.32f), new Vector3(.59f, .25f, .4f), .11f, trim, true);
                for (int j = 0; j < 3; j++)
                    Box("Jet grille", levelRoot, new Vector3(x - .16f + j * .16f, -2.17f, -.42f), new Vector3(.06f, .04f, .18f), .018f, pearl, true);
            }
            for (int i = 0; i < game.Level.rings.Length; i++)
            {
                var go = new GameObject("Floating ring " + (i + 1)); go.transform.SetParent(levelRoot, false);
                var ring = go.AddComponent<FloatingRing>();
                var visual = new GameObject("Flat molded ring", typeof(MeshFilter), typeof(MeshRenderer)); visual.transform.SetParent(go.transform, false);
                var kind = game.Level.RingType(i); var profile = RingProfile.For(kind);
                visual.GetComponent<MeshFilter>().sharedMesh = ringMesh; visual.GetComponent<MeshRenderer>().sharedMaterial = ringMaterials[(int)kind];
                for (int mark = 0; mark < profile.Marks; mark++)
                {
                    float x = (mark - (profile.Marks - 1) * .5f) * .078f;
                    var dot = new GameObject("Weight mark " + (mark + 1), typeof(MeshFilter), typeof(MeshRenderer)); dot.transform.SetParent(visual.transform, false);
                    dot.transform.localPosition = new Vector3(x, -Mathf.Sqrt(.32f * .32f - x * x), -FloatingRing.Thickness * .5f - .002f);
                    dot.GetComponent<MeshFilter>().sharedMesh = markMesh; dot.GetComponent<MeshRenderer>().sharedMaterial = dark;
                }
                ring.Visual = visual.transform;
                ring.Initialize(game, i, game.Level.rings[i]); game.Register(ring);
            }
        }

        void CreateCollector(int index)
        {
            var definition = game.Level.collectors[index];
            var tray = new GameObject("Collector " + (index + 1)).transform; tray.SetParent(levelRoot, false);
            tray.localPosition = new Vector3(definition.center.x, definition.center.y, -.75f);
            tray.gameObject.AddComponent<CollectorSurface>().Index = index;
            for (int part = 0; part < 3; part++)
            {
                var size = part == 0 ? new Vector3(definition.width, .08f, .64f) : new Vector3(.08f, definition.height, .64f);
                var position = part == 0 ? new Vector3(0, -definition.height * .5f, 0) : new Vector3((part == 1 ? -1 : 1) * definition.width * .5f, 0, 0);
                var edge = Box(part == 0 ? "Collector floor" : "Collector side", tray, position, size, .025f, metal, true);
                var contact = edge.gameObject.AddComponent<BoxCollider>(); contact.size = size; contact.sharedMaterial = ContactMaterial; contact.contactOffset = .003f;
            }
            for (int mark = 0; mark < definition.capacity; mark++)
                Disc("Collector capacity", tray, new Vector3((mark - (definition.capacity - 1) * .5f) * .16f, -definition.height * .5f - .15f, -.1f), .033f, .01f, pearl);
        }

        void CreateFish()
        {
            fishVisual = new GameObject("Reef visitor").transform; fishVisual.SetParent(levelRoot, false);
            Primitive("Fish body", PrimitiveType.Sphere, fishVisual, Vector3.zero, new Vector3(.62f, .3f, .18f), fishMaterial);
            var upper = Primitive("Upper tail", PrimitiveType.Sphere, fishVisual, new Vector3(-.34f, .075f, .015f), new Vector3(.26f, .13f, .08f), fishMaterial);
            upper.localRotation = Quaternion.Euler(0, 0, -30);
            var lower = Primitive("Lower tail", PrimitiveType.Sphere, fishVisual, new Vector3(-.34f, -.075f, .015f), new Vector3(.26f, .13f, .08f), fishMaterial);
            lower.localRotation = Quaternion.Euler(0, 0, 30);
            Primitive("Fish eye", PrimitiveType.Sphere, fishVisual, new Vector3(.18f, .05f, -.08f), Vector3.one * .095f, pearl);
            Primitive("Fish pupil", PrimitiveType.Sphere, fishVisual, new Vector3(.2f, .05f, -.127f), Vector3.one * .045f, dark);
            fishWarning = Disc("Fish entry cue", levelRoot, Vector3.zero, .13f, .025f, fishMaterial);
            fishVisual.gameObject.SetActive(false); fishWarning.gameObject.SetActive(false);
        }

        BoxCollider Wall(string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name); go.transform.SetParent(levelRoot, false); go.transform.localPosition = center;
            var contact = go.AddComponent<BoxCollider>(); contact.size = size; contact.sharedMaterial = ContactMaterial; return contact;
        }
        void Emit(Vector3 position, Vector3 velocity, float size, float life, bool celebration)
        {
            var particle = particles[particleCursor++ % particles.Count];
            particle.transform.gameObject.SetActive(true); particle.transform.localPosition = position;
            particle.velocity = velocity; particle.age = 0f; particle.life = life; particle.size = size; particle.celebration = celebration;
        }
        public void Pump(bool left)
        {
            pumpPulse[left ? 0 : 1] = 1f;
            if (game.Settings.reduceMotion) return;
            float x = left ? -NozzleX : NozzleX;
            for (int i = 0; i < 14; i++)
            {
                float fan = Mathf.Sin(i * 9.13f);
                Emit(new Vector3(x + fan * .17f, -2.12f, -.4f), new Vector3(fan * .12f, 1.9f + i * .10f, 0f), .025f + i % 4 * .012f, .8f + i * .04f, false);
            }
        }
        public void Celebrate(Vector3 position)
        {
            if (game.Settings.reduceMotion) return;
            for (int i = 0; i < 14; i++)
            {
                float angle = i * Mathf.PI * 2f / 14f;
                Emit(position + Vector3.back * .1f, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * .9f, .065f, .65f, true);
            }
        }
        void Update()
        {
            if (game == null) return;
            bool home = game.Screen == GameScreen.Home;
            var safe = SafeScreenRect;
            // Frame the whole authored toy without changing physics or stretching art
            // when the screen, safe insets, menus or steering mode change.
            float uiScale = Screen.width / 720f;
            float minY = -5.85f, maxY = 5.55f;
            float availableBottom = safe.yMin + 16f * uiScale;
            float availableTop = safe.yMax - 156f * uiScale;
            float fraction = Mathf.Max(.2f, (availableTop - availableBottom) / Screen.height);
            float halfHeight = Mathf.Max(3.32f / (Camera.aspect * Mathf.Max(.2f, safe.width / Screen.width)), (maxY - minY) / (2f * fraction));
            Camera.orthographicSize = home ? Mathf.Max(11.2f, 4.6f / Camera.aspect) : halfHeight;
            float viewportCenter = (availableTop + availableBottom) / (2f * Screen.height);
            float cameraY = home ? -.25f : (minY + maxY) * .5f - (viewportCenter - .5f) * halfHeight * 2f;
            float cameraX = home ? 0f : (.5f - safe.center.x / Screen.width) * halfHeight * 2f * Camera.aspect;
            Camera.transform.position = new Vector3(cameraX, cameraY, -20);
            backdropPlane.localPosition = new Vector3(0, Camera.transform.position.y, 4);
            backdropPlane.localScale = new Vector3(Camera.orthographicSize * Camera.aspect * 2.1f, Camera.orthographicSize * 2.1f, 1);
            water.SetFloat("_Motion", game.Settings.reduceMotion ? 0f : 1f);
            water.SetFloat("_Biome", home ? 0f : (float)game.Level.biome);
            water.SetFloat("_Frost", home ? 0f : game.Environment.Frost);
            water.SetFloat("_WaterTime", home ? Time.time : game.Elapsed);
            water.SetFloat("_VentSide", game.Environment.ThermalSide);
            water.SetFloat("_VentLift", !home && game.Environment.ThermalActive ? 1f : 0f);
            water.SetFloat("_VentWarning", !home && game.Environment.ThermalWarning ? 1f : 0f);
            for (int i = 0; i < CurrentDefinition.MaximumPerLevel; i++)
            {
                bool visible = !home && game.Level.HasCurrents && i < game.Level.currents.Length;
                if (!visible) { water.SetVector(CurrentFlowIds[i], Vector4.zero); continue; }
                var current = game.Level.currents[i];
                var direction = current.acceleration.normalized;
                water.SetVector(CurrentBoundsIds[i], new Vector4(current.center.x, current.center.y, current.size.x * .5f, current.size.y * .5f));
                water.SetVector(CurrentFlowIds[i], new Vector4(direction.x, direction.y, (float)current.StateAt(game.Elapsed), 1f));
            }
            if (fishVisual != null)
            {
                fishVisual.gameObject.SetActive(!home && game.Environment.FishActive);
                fishWarning.gameObject.SetActive(!home && game.Environment.FishWarning);
                var position = game.Environment.FishPosition;
                fishVisual.localPosition = new Vector3(position.x, position.y, -.95f);
                fishVisual.localScale = new Vector3(game.Environment.FishDirection, 1, 1);
                fishWarning.localPosition = new Vector3(-game.Environment.FishDirection * (TankHalfWidth - .16f), position.y, -1f);
            }
            backdrop.SetFloat("_Motion", game.Settings.reduceMotion ? 0f : 1f);
            float drift = game.Settings.reduceMotion ? 0 : Time.time * .16f;
            for (int i = 0; i < showcaseRings.Count; i++)
            {
                var ring = showcaseRings[i]; ring.gameObject.SetActive(home);
                ring.localPosition = new Vector3(Mathf.Sin(i * 2.1f + .5f) * 1.55f, 1.85f - i * .72f + Mathf.Sin(drift * 2 + i) * .09f, -.8f);
                ring.localRotation = Quaternion.Euler(28 + i * 9, Mathf.Sin(drift + i) * 14, i * 37);
            }
            for (int i = 0; i < ambientBubbles.Count; i++)
            {
                float y = Mathf.Repeat(i * .713f + drift * (1 + i % 3 * .22f), PlayfieldTop - TankBottom - .6f) + TankBottom + .3f;
                float x = Mathf.Sin(i * 15.73f) * (TankHalfWidth - .3f) + Mathf.Sin(y * 1.8f + i) * .055f;
                ambientBubbles[i].localPosition = new Vector3(x, y, -.52f);
            }
            for (int i = 0; i < 2; i++)
            {
                pumpPulse[i] = Mathf.MoveTowards(pumpPulse[i], 0f, Time.unscaledDeltaTime * 6f);
                pumpButtons[i].localPosition = new Vector3(i == 0 ? -NozzleX : NozzleX, -3.65f, -.18f + pumpPulse[i] * .12f);
                pumpButtons[i].localScale = Vector3.one * (1f - pumpPulse[i] * .045f);
            }
            bool animate = game.Playing || game.Screen == GameScreen.Complete;
            foreach (var particle in particles)
            {
                if (particle.life <= 0f || !animate) continue;
                particle.age += Time.deltaTime;
                if (particle.age >= particle.life) { particle.life = 0f; particle.transform.gameObject.SetActive(false); continue; }
                particle.transform.localPosition += particle.velocity * Time.deltaTime;
                float envelope = 1f - particle.age / particle.life;
                particle.transform.localScale = Vector3.one * particle.size * Mathf.Min(1f, envelope * 3f);
            }
        }
        void FixedUpdate()
        {
            if (game == null || !game.Playing) return;
            for (int i = 0; i < pegBodies.Count; i++)
            {
                var tip = game.PegTip(i); pegBodies[i].MovePosition(new Vector3(tip.x, tip.y, -.75f));
            }
        }
        void OnDestroy()
        {
            foreach (var resource in resources) if (resource != null) Destroy(resource);
            foreach (var resource in levelResources) if (resource != null) Destroy(resource);
        }
    }
}
