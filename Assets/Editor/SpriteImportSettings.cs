using UnityEditor;
using UnityEngine;

// Auto-imports every texture under Resources/Art as a 2D Sprite so the
// code-driven game can Resources.Load<Sprite>(...) without manual setup.
public class SpriteImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string p = assetPath.Replace('\\', '/');
        if (!p.Contains("/Resources/Art/")) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 256f;
        ti.filterMode          = FilterMode.Bilinear;
        ti.mipmapEnabled       = false;
        ti.alphaIsTransparency = true;
        ti.wrapMode            = TextureWrapMode.Clamp;
        ti.maxTextureSize      = 2048;
        ti.textureCompression  = TextureImporterCompression.Compressed;

        // 9-slice borders for stretchable buttons so rounded caps don't distort.
        string file = System.IO.Path.GetFileNameWithoutExtension(p);
        if (file == "btn_blue_long" || file == "btn_blue_wide")
        {
            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteBorder = new Vector4(72, 48, 72, 48);
            ti.SetTextureSettings(s);
        }
    }
}
