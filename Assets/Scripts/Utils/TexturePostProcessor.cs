using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TexturePostProcessor : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        if (assetPath.Contains("Art/"))
        { 
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.mipmapEnabled = false;
            textureImporter.alphaIsTransparency = true;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.isReadable = true;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
        }
    }
}
