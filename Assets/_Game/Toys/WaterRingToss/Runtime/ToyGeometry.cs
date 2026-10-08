using System.Collections.Generic;
using UnityEngine;

namespace PocketToys.WaterRingToss.Game
{
    public static class ToyGeometry
    {
        public static Mesh BackdropQuad()
        {
            var mesh = new Mesh { name = "Ocean backdrop quad" };
            mesh.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0) };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        // Rounded rectangular extrusion with a rolled bevel, independent of its depth.
        public static Mesh MoldedPanel(Vector3 size, float radius)
        {
            const int arc = 12, layers = 9;
            const int perimeter = 4 * (arc + 1);
            float bevel = Mathf.Min(.14f, size.z * .35f);
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int layer = 0; layer < layers; layer++)
            {
                float angle = Mathf.PI * layer / (layers - 1);
                float inset = bevel * (1 - Mathf.Sin(angle));
                float z = Mathf.Cos(angle) * size.z * .5f;
                float r = Mathf.Max(.02f, radius - inset);
                for (int corner = 0; corner < 4; corner++)
                for (int step = 0; step <= arc; step++)
                {
                    float a = (corner * 90f + step * 90f / arc) * Mathf.Deg2Rad;
                    float cx = (corner == 0 || corner == 3 ? 1 : -1) * (size.x * .5f - radius);
                    float cy = (corner < 2 ? 1 : -1) * (size.y * .5f - radius);
                    var p = new Vector3(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r, z);
                    vertices.Add(p); uv.Add(new Vector2(p.x / size.x + .5f, p.y / size.y + .5f));
                }
            }
            for (int layer = 0; layer < layers - 1; layer++)
            for (int p = 0; p < perimeter; p++)
            {
                int a = layer * perimeter + p, b = layer * perimeter + (p + 1) % perimeter;
                triangles.Add(a); triangles.Add(a + perimeter); triangles.Add(b);
                triangles.Add(b); triangles.Add(a + perimeter); triangles.Add(b + perimeter);
            }
            for (int face = 0; face < 2; face++)
            {
                int center = vertices.Count, start = face == 0 ? 0 : (layers - 1) * perimeter;
                vertices.Add(new Vector3(0, 0, (face == 0 ? 1 : -1) * size.z * .5f)); uv.Add(Vector2.one * .5f);
                for (int p = 0; p < perimeter; p++)
                {
                    triangles.Add(center);
                    triangles.Add(start + (face == 0 ? p : (p + 1) % perimeter));
                    triangles.Add(start + (face == 0 ? (p + 1) % perimeter : p));
                }
            }
            var mesh = new Mesh { name = "Rolled enamel panel" };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        public static Mesh RoundedBox(Vector3 size, float radius)
        {
            const int steps = 16;
            radius = Mathf.Min(radius, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * .49f);
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            var half = size * .5f; var inner = half - Vector3.one * radius;
            var faces = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (var face in faces)
            {
                Vector3 u = Mathf.Abs(face.y) > .5f ? Vector3.right : Vector3.up;
                Vector3 v = Vector3.Cross(face, u);
                int start = vertices.Count;
                for (int y = 0; y <= steps; y++)
                for (int x = 0; x <= steps; x++)
                {
                    float extentU = Mathf.Abs(Vector3.Dot(u, half));
                    float extentV = Mathf.Abs(Vector3.Dot(v, half));
                    Vector3 p = Vector3.Scale(face, half) + u * EdgeCoordinate(x, steps, extentU, radius) + v * EdgeCoordinate(y, steps, extentV, radius);
                    Vector3 q = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                    Vector3 normal = (p - q).normalized;
                    vertices.Add(q + normal * radius); normals.Add(normal);
                    uv.Add(new Vector2(p.x / size.x + .5f, p.y / size.y + .5f));
                    if (x == steps || y == steps) continue;
                    int i = start + y * (steps + 1) + x;
                    triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + steps + 1);
                    triangles.Add(i + 1); triangles.Add(i + steps + 2); triangles.Add(i + steps + 1);
                }
            }
            var mesh = new Mesh { name = "Molded rounded surface" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            return mesh;
        }

        static float EdgeCoordinate(int i, int steps, float half, float radius)
        {
            const int edgeSteps = 4;
            if (i <= edgeSteps) return -half + radius * i / edgeSteps;
            if (i >= steps - edgeSteps) return half - radius + radius * (i - steps + edgeSteps) / edgeSteps;
            return Mathf.Lerp(-half + radius, half - radius, (float)(i - edgeSteps) / (steps - 2 * edgeSteps));
        }

        public static Mesh SoftDisc(float radius, float depth)
        {
            const int segments = 64;
            float[] profileR = { 0f, .84f, .98f, 1f, .98f, .84f, 0f };
            float[] profileZ = { .5f, .5f, .35f, 0f, -.35f, -.5f, -.5f };
            var vertices = new Vector3[profileR.Length * (segments + 1)];
            var indices = new List<int>();
            for (int p = 0; p < profileR.Length; p++)
            for (int a = 0; a <= segments; a++)
            {
                float angle = a * Mathf.PI * 2f / segments;
                int n = p * (segments + 1) + a;
                vertices[n] = new Vector3(Mathf.Cos(angle) * radius * profileR[p], Mathf.Sin(angle) * radius * profileR[p], depth * profileZ[p]);
                if (p == profileR.Length - 1 || a == segments) continue;
                indices.Add(n); indices.Add(n + segments + 1); indices.Add(n + 1);
                indices.Add(n + 1); indices.Add(n + segments + 1); indices.Add(n + segments + 2);
            }
            var mesh = new Mesh { name = "Soft molded disc", vertices = vertices, triangles = indices.ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        public static Mesh Torus(float radius, float tube)
        {
            const int segments = 64, sides = 16;
            var vertices = new Vector3[(segments + 1) * (sides + 1)];
            var normals = new Vector3[vertices.Length]; var uv = new Vector2[vertices.Length];
            var triangles = new int[segments * sides * 6]; int index = 0;
            for (int i = 0; i <= segments; i++)
            for (int j = 0; j <= sides; j++)
            {
                float a = i * Mathf.PI * 2f / segments, b = j * Mathf.PI * 2f / sides;
                int n = i * (sides + 1) + j;
                normals[n] = new Vector3(Mathf.Cos(b) * Mathf.Cos(a), Mathf.Cos(b) * Mathf.Sin(a), Mathf.Sin(b));
                vertices[n] = new Vector3(radius * Mathf.Cos(a), radius * Mathf.Sin(a), 0f) + normals[n] * tube;
                uv[n] = new Vector2((float)i / segments, (float)j / sides);
                if (i == segments || j == sides) continue;
                triangles[index++] = n; triangles[index++] = n + sides + 1; triangles[index++] = n + 1;
                triangles[index++] = n + 1; triangles[index++] = n + sides + 1; triangles[index++] = n + sides + 2;
            }
            var mesh = new Mesh { name = "Satin ring", vertices = vertices, normals = normals, uv = uv, triangles = triangles };
            mesh.RecalculateBounds(); return mesh;
        }
    }
}
