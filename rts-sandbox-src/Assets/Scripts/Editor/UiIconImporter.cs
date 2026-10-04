using UnityEditor;
using UnityEngine;

/// <summary>
/// Import settings for every picture of the interface. The icons are drawn
/// large (128-256 px) and shown small, so they keep their mip maps for a clean
/// downscale, are never compressed (compression smears thin outlines) and treat
/// alpha as transparency so the edges do not get a dark halo.
///
/// Readable, so that the HUD can make a grey copy for cooldowns at run time
/// instead of storing a second picture of every skill.
///
/// Lives in an importer rather than in each .meta so that a new icon dropped
/// into the folder comes out right without anyone touching the inspector.
/// </summary>
public class UiIconImporter : AssetPostprocessor
{
    private const string IconsFolder = "Assets/UI/Icons/";

    /// <summary>Raise when the settings below change: Unity then reimports the icons.</summary>
    public override uint GetVersion() => 2;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(IconsFolder))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Trilinear;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 512;

        // The HUD makes the grey cooldown version of a skill picture itself,
        // which needs the pixels on the CPU side.
        importer.isReadable = true;
    }
}
