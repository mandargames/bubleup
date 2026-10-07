using System.Collections.Generic;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public sealed class ToyPresentation : MonoBehaviour
    {
        public Camera Camera { get; private set; }
        public Transform LeftButton => pumpButtons[0];
        public Transform RightButton => pumpButtons[1];
        GameSession game;
        Transform levelRoot;
        readonly List<Object> resources = new List<Object>();
        readonly List<Object> levelResources = new List<Object>();
        readonly List<Transform> pegs = new List<Transform>();
        readonly Transform[] pumpButtons = new Transform[2];
        readonly float[] pumpPulse = new float[2];
        readonly List<Particle> particles = new List<Particle>();
        Material body, trim, metal, pearl, bubble, water, glass, dark;
        Material[] ringMaterials;
        Mesh ringMesh;
        int particleCursor;
        static readonly string[] RingColors = { "#F36B49", "#FFCF59", "#BC91DE", "#9AE3CC", "#F291B4" };
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
            var cameraObject = new GameObject("Studio camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false); cameraObject.transform.position = new Vector3(0f, 0f, -20f);
            Camera = cameraObject.GetComponent<Camera>(); Camera.orthographic = true; Camera.orthographicSize = 7f;
            Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = Color("#101F2B");
            Camera.allowHDR = false; Camera.nearClipPlane = .1f; cameraObject.tag = "MainCamera";
            Light("Large softbox", Quaternion.Euler(25f, -32f, 0f), .72f, "#FFF3DF");
            Light("Cool fill", Quaternion.Euler(-20f, 145f, 0f), .45f, "#92D9ED");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color("#8498A5") * .52f;
            body = Surface("Lagoon enamel", "#BFE7DC", .58f);
            trim = Surface("Tank seal", "#255C63", .42f);
            metal = Surface("Champagne brushed metal", "#C6B994", .66f, .58f);
            pearl = Surface("Warm porcelain", "#F2E7CA", .7f);
            dark = Surface("Nozzle vents", "#23414D", .22f);
            bubble = Surface("Bubble glints", "#BFEFF3", .9f, .12f);
            ringMaterials = new Material[RingColors.Length];
            for (int i = 0; i < ringMaterials.Length; i++) ringMaterials[i] = Surface("Ring color " + i, RingColors[i], .68f, .08f);
            water = new Material(game.campaign.waterMaterial); resources.Add(water);
            glass = new Material(game.campaign.glassMaterial); resources.Add(glass);
            ringMesh = ToyGeometry.Torus(.305f, .086f); resources.Add(ringMesh);

            // Layered enclosure: shell, metallic lip, dark rubber seal and recessed water chamber.
            Box("Soft shadow", transform, new Vector3(.10f, -.30f, 1.4f), new Vector3(6.38f, 9.73f, .8f), .38f, dark, false);
            Box("Enamel casing", transform, new Vector3(0f, -.15f, .8f), new Vector3(6.2f, 9.6f, .8f), .38f, body, false);
            Box("Recess lip", transform, new Vector3(0f, .65f, .19f), new Vector3(5.66f, 6.59f, .4f), .19f, metal, false);
            Box("Rubber seal", transform, new Vector3(0f, .65f, -.04f), new Vector3(5.48f, 6.41f, .26f), .12f, trim, false);
            Box("Water chamber", transform, new Vector3(0f, .65f, -.21f), new Vector3(5.2f, 6.13f, .12f), .05f, water, false);
            Box("Left glass glint", transform, new Vector3(-2.52f, .95f, -.45f), new Vector3(.035f, 4.95f, .035f), .015f, pearl, false);
            Box("Top glass glint", transform, new Vector3(-.5f, 3.66f, -.45f), new Vector3(3.6f, .025f, .025f), .01f, pearl, false);
            // Subtle sheen uses a transparent shader and never obscures gameplay objects.
            Box("Acrylic reflection", transform, new Vector3(0f, .65f, -1.3f), new Vector3(5.13f, 6.04f, .02f), .009f, glass, false);
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -1.55f : 1.55f;
                Disc("Pump socket", transform, new Vector3(x, -3.65f, .09f), .72f, .15f, trim);
                Disc("Metal pump lip", transform, new Vector3(x, -3.65f, -.04f), .65f, .08f, metal);
                pumpButtons[side] = Disc("Pump diaphragm", transform, new Vector3(x, -3.65f, -.18f), .57f, .16f, side == 0 ? ringMaterials[0] : ringMaterials[1]);
                for (int screw = 0; screw < 2; screw++)
                {
                    var pos = new Vector3(side == 0 ? -2.72f : 2.72f, screw == 0 ? 4.07f : -4.39f, .26f);
                    Disc("Casing screw", transform, pos, .065f, .022f, metal);
                    Box("Screw slot", transform, pos + Vector3.back * .023f, new Vector3(.075f, .013f, .015f), .006f, dark, false);
                }
            }
            for (int i = 0; i < 72; i++)
            {
                var sphere = Primitive("Water particle", PrimitiveType.Sphere, transform, Vector3.zero, Vector3.one * .04f, bubble);
                sphere.gameObject.SetActive(false); particles.Add(new Particle { transform = sphere });
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
            var mesh = ToyGeometry.RoundedBox(size, radius); (level ? levelResources : resources).Add(mesh);
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
            if (body != null) body.color = Color(index == 1 ? "#EBC5AE" : index == 2 ? "#B8B9DE" : "#BFE7DC");
        }

        public void LoadLevel()
        {
            if (levelRoot != null) { levelRoot.gameObject.SetActive(false); Destroy(levelRoot.gameObject); }
            foreach (var resource in levelResources) Destroy(resource); levelResources.Clear(); pegs.Clear();
            foreach (var particle in particles) { particle.life = 0f; particle.transform.gameObject.SetActive(false); }
            levelRoot = new GameObject("Level - " + game.Level.title).transform; levelRoot.SetParent(transform, false);
            Wall("Left boundary", new Vector3(-2.72f, .65f, -.15f), new Vector3(.25f, 6.5f, 2f));
            Wall("Right boundary", new Vector3(2.72f, .65f, -.15f), new Vector3(.25f, 6.5f, 2f));
            Wall("Chamber floor", new Vector3(0f, -2.55f, -.15f), new Vector3(5.5f, .2f, 2f));
            Wall("Chamber ceiling", new Vector3(0f, 3.83f, -.15f), new Vector3(5.5f, .2f, 2f));
            for (int i = 0; i < game.Level.pegs.Length; i++)
            {
                var definition = game.Level.pegs[i];
                var peg = new GameObject("Peg " + (i + 1)).transform; peg.SetParent(levelRoot, false);
                peg.localPosition = new Vector3(definition.tip.x, definition.tip.y, -.75f); pegs.Add(peg);
                Primitive("Stem", PrimitiveType.Cylinder, peg, new Vector3(0f, -definition.length * .5f, 0f), new Vector3(.11f, definition.length * .5f, .11f), pearl);
                Primitive("Rounded tip", PrimitiveType.Sphere, peg, Vector3.zero, Vector3.one * .12f, pearl);
                Box("Landing shelf", peg, new Vector3(0f, -definition.length, 0f), new Vector3(.95f, .10f, .7f), .045f, metal, true);
                for (int slot = 0; slot < definition.capacity; slot++)
                    Disc("Capacity mark", peg, new Vector3(-.12f * (definition.capacity - 1) + slot * .24f, -definition.length - .2f, .04f), .042f, .015f, pearl);
            }
            foreach (var baffle in game.Level.baffles)
            {
                var shelf = Box("Water baffle", levelRoot, new Vector3(baffle.center.x, baffle.center.y, -.75f), new Vector3(baffle.size.x, baffle.size.y, .5f), .08f, metal, true);
                shelf.localRotation = Quaternion.Euler(0f, 0f, baffle.angle);
                shelf.gameObject.AddComponent<BoxCollider>().size = new Vector3(baffle.size.x, baffle.size.y, .9f);
            }
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -1.55f : 1.55f;
                Box("Jet housing", levelRoot, new Vector3(x, -2.3f, -.32f), new Vector3(.59f, .25f, .4f), .11f, trim, true);
                for (int j = 0; j < 3; j++)
                    Box("Jet grille", levelRoot, new Vector3(x - .16f + j * .16f, -2.17f, -.42f), new Vector3(.06f, .04f, .18f), .018f, pearl, true);
            }
            for (int i = 0; i < game.Level.rings.Length; i++)
            {
                var go = new GameObject("Floating ring " + (i + 1)); go.transform.SetParent(levelRoot, false);
                var ring = go.AddComponent<FloatingRing>();
                var visual = new GameObject("Enamel torus", typeof(MeshFilter), typeof(MeshRenderer)); visual.transform.SetParent(go.transform, false);
                visual.GetComponent<MeshFilter>().sharedMesh = ringMesh; visual.GetComponent<MeshRenderer>().sharedMaterial = ringMaterials[i % ringMaterials.Length];
                ring.Visual = visual.transform; ring.Visual.localRotation = Quaternion.Euler(58f, 0f, 0f);
                ring.Initialize(game, i, game.Level.rings[i]); game.Register(ring);
            }
        }

        void Wall(string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name); go.transform.SetParent(levelRoot, false); go.transform.localPosition = center;
            go.AddComponent<BoxCollider>().size = size;
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
            float x = left ? -1.55f : 1.55f;
            for (int i = 0; i < 14; i++)
            {
                float fan = Mathf.Sin(i * 9.13f);
                Emit(new Vector3(x + fan * .17f, -2.12f, -.4f), new Vector3(fan * .22f + (left ? .15f : -.15f), 1.9f + i * .10f, 0f), .025f + i % 4 * .012f, .8f + i * .04f, false);
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
            Camera.orthographicSize = Mathf.Max(7f, 3.5f / Camera.aspect);
            water.SetFloat("_Motion", game.Settings.reduceMotion ? 0f : 1f);
            for (int i = 0; i < pegs.Count; i++) { var tip = game.PegTip(i); pegs[i].localPosition = new Vector3(tip.x, tip.y, -.75f); }
            for (int i = 0; i < 2; i++)
            {
                pumpPulse[i] = Mathf.MoveTowards(pumpPulse[i], 0f, Time.unscaledDeltaTime * 6f);
                pumpButtons[i].localPosition = new Vector3(i == 0 ? -1.55f : 1.55f, -3.65f, -.18f + pumpPulse[i] * .12f);
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
        void OnDestroy()
        {
            foreach (var resource in resources) if (resource != null) Destroy(resource);
            foreach (var resource in levelResources) if (resource != null) Destroy(resource);
        }
    }
}
