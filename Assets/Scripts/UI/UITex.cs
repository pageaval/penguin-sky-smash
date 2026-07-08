using System.Collections.Generic;
using UnityEngine;

namespace PSS
{
    /// Generates crisp rounded-rectangle sprites at runtime (9-sliced) so panels,
    /// pills and bars look clean without shipping extra art. Cached by parameters.
    public static class UITex
    {
        static readonly Dictionary<string, Sprite> cache = new();

        public static Sprite Rounded(int radius, Color fill, Color border = default, int borderW = 0)
        {
            radius = Mathf.Max(2, radius);
            string key = $"{radius}_{ColorKey(fill)}_{ColorKey(border)}_{borderW}";
            if (cache.TryGetValue(key, out var s)) return s;

            int r = radius;
            int size = 2 * r + 8;                 // 8px stretchable middle for 9-slice
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x, r, size - 1 - r);
                    float cy = Mathf.Clamp(y, r, size - 1 - r);
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float a = Mathf.Clamp01(r + 0.5f - d);        // 1 inside, 0 outside, AA edge
                    Color c = fill;
                    if (borderW > 0 && d >= r - borderW) c = border;
                    c.a *= a;
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            cache[key] = sprite;
            return sprite;
        }

        static string ColorKey(Color c) => $"{c.r:F2}{c.g:F2}{c.b:F2}{c.a:F2}";
    }
}
