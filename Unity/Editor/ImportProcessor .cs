using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ImportProcessor : AssetPostprocessor
{
    private void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;

        if (assetPath.StartsWith("Assets/ResMS/Model")||assetPath.StartsWith("Assets/ResMS/Animation"))
        {


            modelImporter.importMaterials = false;
            if (assetPath.StartsWith("Assets/ResMS/Model/Player"))
            {
                modelImporter.isReadable = true;
            }
            else
            {
            modelImporter.isReadable = false;
            }
            
            modelImporter.animationType = ModelImporterAnimationType.Legacy;
            modelImporter.addCollider = false;
        }
    }
    private void OnPostprocessModel(GameObject model)
    {
        if (null == model) return;

        Renderer[] renders = model.GetComponentsInChildren<Renderer>();
        if (null == renders) return;
        foreach (Renderer render in renders)
        {
            if (render.sharedMaterials.Length > 0 && render.sharedMaterials[0].shader.name.Equals("Standard"))
            {
                render.sharedMaterials = new Material[render.sharedMaterials.Length];
            }
        }
    }
    private void OnPreprocessTexture()
    {
        TextureImporter txImporter = assetImporter as TextureImporter;
        string path1 = "Assets/Scene";
        string path2 = "Assets/ResMS/Cubicmap/MipMapCubeMap";
        if (assetPath.StartsWith(path2))
        {
            txImporter.mipmapEnabled = true;
        }
        else if (assetPath.StartsWith(path1))
        {

        }
        else
        {
            txImporter.mipmapEnabled = false;
        }
    }
    private void OnPostprocessTexture(Texture2D texture)
    {
        TextureImporter txImporter = assetImporter as TextureImporter;
       //txImporter.maxTextureSize = 1024;

       if (assetPath.StartsWith("Assets/BundleData/TPProject") && !assetPath.EndsWith("_alpha.png") && !assetPath.EndsWith("_color.png"))
        {
            if (texture.width != texture.height)
            {
                Debug.LogError("UI Atlas texture must be power of 2!  path is " + assetPath);
                AssetDatabase.DeleteAsset(assetPath);
                return;
            }
            TextureImporterPlatformSettings PlatSetting = new TextureImporterPlatformSettings();
            PlatSetting.format = TextureImporterFormat.ETC2_RGBA8;
            PlatSetting.overridden = true;
            PlatSetting.name = "Android";
            PlatSetting.textureCompression = 0;
            
            TextureImporterPlatformSettings PlatSetting2 = new TextureImporterPlatformSettings();
            PlatSetting2.format = TextureImporterFormat.ASTC_RGBA_4x4;
            PlatSetting2.overridden = true;
            PlatSetting2.name = "iPhone";
            PlatSetting2.textureCompression = 0;
            txImporter.SetPlatformTextureSettings(PlatSetting);
            txImporter.SetPlatformTextureSettings(PlatSetting2);

            TextureImporterPlatformSettings PlatSetting3 = new TextureImporterPlatformSettings();
            PlatSetting.format = TextureImporterFormat.DXT1;
            PlatSetting.overridden = true;
            PlatSetting.name = "WebGL";
            PlatSetting.textureCompression = 0;
        }

        if (assetPath.StartsWith("Assets/ResMS/UI/UITexture") || assetPath.StartsWith("Assets/BundleData/Texture")|| assetPath.StartsWith("Assets/ResMS/UI/Texture"))
        {
            bool bSet = false;
            if(assetPath.EndsWith("_alpha.png")|| assetPath.EndsWith("_color.png"))
            {
                bSet = false;
            }
            if (assetPath.StartsWith("Assets/BundleData/Texture/Player")|| assetPath.StartsWith("Assets/BundleData/Texture/Scene")|| assetPath.StartsWith("Assets/ResMS/UI/NPotTexture"))
            {
                bSet = false;
            }
            if (bSet)
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                txImporter.ReadTextureSettings(settings);
                if (txImporter.DoesSourceTextureHaveAlpha())
                {
                    settings.alphaIsTransparency = true;
                }
                settings.npotScale = TextureImporterNPOTScale.ToNearest;
                settings.textureType = TextureImporterType.Default;
                settings.mipmapEnabled = false;
                settings.readable = false;
                txImporter.SetTextureSettings(settings);
            }
        }
        if (assetPath.StartsWith("Assets/ResMS/UI/Atlas/Material"))
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            txImporter.ReadTextureSettings(settings);

            if (txImporter.DoesSourceTextureHaveAlpha())
            {
                settings.alphaIsTransparency = true;
            }
            txImporter.textureCompression = TextureImporterCompression.Uncompressed; 
            settings.npotScale = TextureImporterNPOTScale.None;
            txImporter.SetTextureSettings(settings);
        }
    }

    private void OnPreprocessAudio()
    {
        AudioImporter adAudioImporter = assetImporter as AudioImporter;
        if (assetPath.StartsWith("Assets/BundleData/Music/SceneBG"))
        {
            adAudioImporter.defaultSampleSettings = new AudioImporterSampleSettings()
            {
                loadType = AudioClipLoadType.Streaming,
                compressionFormat = AudioCompressionFormat.Vorbis,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
                quality =0.8f
            };
        }
    }


}
