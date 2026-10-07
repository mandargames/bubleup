using System.IO;
using UnityEditor;
using UnityEngine;

namespace PocketToys.Editor
{
    public static class GameIcon
    {
        public static Texture2D Create()
        {
            const string path = "Assets/_Game/App/PocketToysIcon.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            var texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            for (int y = 0; y < 512; y++)
            for (int x = 0; x < 512; x++)
            {
                var p = new Vector2(x + .5f, y + .5f);
                Color c = Color.Lerp(Hex("#102330"), Hex("#234552"), (float)y / 512f);
                Blend(ref c, Hex("#081720"), Rounded(p, new Vector2(263, 252), new Vector2(350, 450), 53));
                Blend(ref c, Color.Lerp(Hex("#79B4AC"), Hex("#D1ECE0"), (float)y / 512f), Rounded(p, new Vector2(256, 265), new Vector2(350, 448), 53));
                Blend(ref c, Hex("#CDBD93"), Rounded(p, new Vector2(256, 307), new Vector2(298, 290), 26));
                Blend(ref c, Hex("#28535C"), Rounded(p, new Vector2(256, 307), new Vector2(284, 276), 22));
                Blend(ref c, Color.Lerp(Hex("#103D51"), Hex("#398D8D"), Mathf.Clamp01((y - 170f) / 280f)), Rounded(p, new Vector2(256, 307), new Vector2(266, 258), 18));
                Blend(ref c, Hex("#F6D389"), Rounded(p, new Vector2(256, 302), new Vector2(13, 145), 6));
                Blend(ref c, Hex("#D6C694"), Rounded(p, new Vector2(256, 232), new Vector2(96, 12), 6));
                float ellipse = new Vector2((p.x - 256f) / 63f, (p.y - 298f) / 31f).magnitude;
                float torus = Mathf.Abs(ellipse - .77f) - .22f;
                Blend(ref c, Color.Lerp(Hex("#C9513F"), Hex("#FFAE85"), Mathf.Clamp01((y - 275f) / 45f)), torus * 31f);
                for (int side = 0; side < 2; side++)
                {
                    Vector2 center = new Vector2(side == 0 ? 182f : 330f, 107f);
                    float r = Vector2.Distance(p, center);
                    Blend(ref c, Hex("#28545E"), r - 46f); Blend(ref c, Hex("#D1BD8E"), r - 40f);
                    Color baseColor = side == 0 ? Hex("#EA775A") : Hex("#EFC967");
                    Blend(ref c, Color.Lerp(baseColor * .82f, baseColor, Mathf.Clamp01((y - 75f) / 55f)), r - 34f);
                }
                Blend(ref c, new Color(.84f, 1f, 1f), Rounded(p, new Vector2(139, 330), new Vector2(3, 165), 1.4f), .45f);
                texture.SetPixel(x, y, c);
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Color Hex(string value) { ColorUtility.TryParseHtmlString(value, out var c); return c; }
        static void Blend(ref Color background, Color foreground, float distance, float alpha = 1f) { background = Color.Lerp(background, foreground, Mathf.Clamp01(.75f - distance) * alpha); background.a = 1f; }
        static float Rounded(Vector2 p, Vector2 center, Vector2 size, float radius)
        {
            Vector2 d = new Vector2(Mathf.Abs(p.x - center.x), Mathf.Abs(p.y - center.y)) - size * .5f + Vector2.one * radius;
            return new Vector2(Mathf.Max(0f, d.x), Mathf.Max(0f, d.y)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - radius;
        }
    }
}
