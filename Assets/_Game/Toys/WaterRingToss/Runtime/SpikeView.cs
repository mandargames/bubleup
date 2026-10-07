using System.Collections.Generic;
using PocketToys.Core.Input;
using UnityEngine;

namespace PocketToys.WaterRingToss
{
    public sealed class SpikeView : MonoBehaviour
    {
        public Camera GameCamera { get; private set; }
        readonly List<Object> resources = new List<Object>();
        readonly Transform[] jets = new Transform[2];
        readonly float[] pulses = new float[2];
        readonly Vector3[] nozzlePositions = new Vector3[2];
        Material template;
        static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }

        Material Material(string name, string hex)
        {
            var material = template != null ? new Material(template) : new Material(Shader.Find("Standard"));
            material.name = name;
            material.color = Hex(hex);
            material.SetFloat("_Glossiness", .42f);
            resources.Add(material);
            return material;
        }

        GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, bool collision = false)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.transform.SetParent(transform, false);
            shape.transform.localPosition = position;
            shape.transform.localScale = scale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision)
            {
                var collider = shape.GetComponent<Collider>();
                collider.enabled = false;
                Destroy(collider);
            }
            return shape;
        }

        public RingBody BuildTank(WaterRingTossConfig config, SensorInputService sensor)
        {
            template = config.prototypeMaterial;
            var cameraObject = new GameObject("Portrait Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -14f);
            GameCamera = cameraObject.GetComponent<Camera>();
            GameCamera.orthographic = true;
            GameCamera.orthographicSize = 6.3f;
            GameCamera.clearFlags = CameraClearFlags.SolidColor;
            GameCamera.backgroundColor = Hex("#102D3A");
            GameCamera.nearClipPlane = .1f;
            cameraObject.tag = "MainCamera";
            var lightObject = new GameObject("Soft key light", typeof(Light));
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(28f, -28f, 0f);
            var key = lightObject.GetComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.25f;
            RenderSettings.ambientLight = new Color(.55f, .65f, .7f);
            var shell = Material("Warm ivory plastic", "#D9E6DD");
            var water = Material("Lagoon", "#287C91");
            var rim = Material("Deep teal", "#15495A");
            var yellow = Material("Marigold peg", "#F9CC62");
            var coral = Material("Coral ring", "#FF755D");
            var mist = Material("Pump plume", "#71DCDD");
            float w = config.tankSize.x, h = config.tankSize.y;
            Shape("Toy shell", PrimitiveType.Cube, new Vector3(0f, 0f, .9f), new Vector3(w + .55f, h + .55f, .5f), shell);
            Shape("Water backdrop", PrimitiveType.Cube, new Vector3(0f, 0f, .5f), new Vector3(w, h, .2f), water);
            Shape("Left wall", PrimitiveType.Cube, new Vector3(-w / 2f - .1f, 0f, 0f), new Vector3(.2f, h, 1f), rim, true);
            Shape("Right wall", PrimitiveType.Cube, new Vector3(w / 2f + .1f, 0f, 0f), new Vector3(.2f, h, 1f), rim, true);
            Shape("Floor", PrimitiveType.Cube, new Vector3(0f, -h / 2f - .1f, 0f), new Vector3(w + .4f, .2f, 1f), rim, true);
            Shape("Ceiling", PrimitiveType.Cube, new Vector3(0f, h / 2f + .1f, 0f), new Vector3(w + .4f, .2f, 1f), rim, true);
            Shape("Peg", PrimitiveType.Cylinder, new Vector3(config.pegTip.x, config.pegTip.y - config.pegLength / 2f, .16f), new Vector3(.14f, config.pegLength / 2f, .14f), yellow);
            Shape("Peg tip", PrimitiveType.Sphere, new Vector3(config.pegTip.x, config.pegTip.y, .16f), Vector3.one * .15f, yellow);
            Shape("Peg foot", PrimitiveType.Cube, new Vector3(config.pegTip.x, config.pegTip.y - config.pegLength, .16f), new Vector3(.85f, .12f, .32f), yellow);
            for (var i = 0; i < 2; i++)
            {
                var nozzle = i == 0 ? config.leftPump : config.rightPump;
                Shape(i == 0 ? "Left nozzle" : "Right nozzle", PrimitiveType.Cylinder, new Vector3(nozzle.x, nozzle.y, .1f), new Vector3(.38f, .12f, .38f), yellow);
                nozzlePositions[i] = new Vector3(nozzle.x, nozzle.y, .24f);
                jets[i] = Shape("Jet pulse", PrimitiveType.Capsule, nozzlePositions[i], Vector3.one * .1f, mist).transform;
                jets[i].gameObject.SetActive(false);
            }
            // Authored decorative water marks; no fluid simulation or production art dependency.
            for (var i = 0; i < 14; i++)
            {
                float x = Mathf.Sin(i * 17.2f) * (w / 2f - .3f);
                float y = Mathf.Cos(i * 8.8f) * (h / 2f - .3f);
                Shape("Water bead", PrimitiveType.Sphere, new Vector3(x, y, .32f), Vector3.one * (.035f + i % 3 * .02f), mist);
            }
            var ringObject = new GameObject("Ring", typeof(MeshFilter), typeof(MeshRenderer), typeof(SphereCollider), typeof(Rigidbody), typeof(RingBody));
            ringObject.transform.SetParent(transform, false);
            var mesh = Torus(config.ringRadius, config.ringTubeRadius);
            resources.Add(mesh);
            ringObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            ringObject.GetComponent<MeshRenderer>().sharedMaterial = coral;
            ringObject.GetComponent<SphereCollider>().radius = config.ringRadius + config.ringTubeRadius;
            var ring = ringObject.GetComponent<RingBody>();
            ring.Initialize(config, sensor);
            return ring;
        }

        static Mesh Torus(float radius, float tube)
        {
            const int segments = 48, sides = 12;
            var vertices = new Vector3[(segments + 1) * (sides + 1)];
            var indices = new int[segments * sides * 6];
            int index = 0;
            for (int i = 0; i <= segments; i++)
            for (int j = 0; j <= sides; j++)
            {
                float a = i * Mathf.PI * 2f / segments, b = j * Mathf.PI * 2f / sides;
                vertices[i * (sides + 1) + j] = new Vector3((radius + tube * Mathf.Cos(b)) * Mathf.Cos(a), (radius + tube * Mathf.Cos(b)) * Mathf.Sin(a), tube * Mathf.Sin(b));
                if (i == segments || j == sides) continue;
                int n = i * (sides + 1) + j;
                indices[index++] = n; indices[index++] = n + sides + 1; indices[index++] = n + 1;
                indices[index++] = n + 1; indices[index++] = n + sides + 1; indices[index++] = n + sides + 2;
            }
            var mesh = new Mesh { name = "Prototype torus", vertices = vertices, triangles = indices };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public void Pulse(bool left) { pulses[left ? 0 : 1] = .35f; }
        public void ResetPulses()
        {
            for (int i = 0; i < 2; i++) { pulses[i] = 0f; jets[i].gameObject.SetActive(false); }
        }
        void Update()
        {
            for (int i = 0; i < 2; i++)
            {
                if (jets[i] == null) continue;
                pulses[i] = Mathf.Max(0f, pulses[i] - Time.deltaTime);
                jets[i].gameObject.SetActive(pulses[i] > 0f);
                float progress = 1f - pulses[i] / .35f;
                jets[i].localPosition = nozzlePositions[i] + Vector3.up * (.3f + progress * 1.3f);
                jets[i].localScale = new Vector3(.25f * (1f - progress), .65f, .1f);
            }
        }

        void OnDestroy() { foreach (var resource in resources) if (resource != null) Destroy(resource); }
    }
}
