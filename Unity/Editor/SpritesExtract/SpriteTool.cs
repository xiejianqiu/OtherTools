using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteTool : EditorWindow
{
    [MenuItem("Tools/图标提前工具")]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<SpriteTool>(false, "Atlas Finder", true).Show();
    }
    void OnGUI()
    {
        if (GUILayout.Button("修改格式"))
        {
            var srcTex = Selection.activeObject as Texture2D;
            if (null == srcTex)
            {
                EditorUtility.DisplayDialog("提示", "请选择要操作的图片", "确定");
                return;
            }
            var srcTexPath = AssetDatabase.GetAssetPath(srcTex);
            var srcTexImporter = AssetImporter.GetAtPath(srcTexPath) as TextureImporter;
            if (SpriteImportMode.Multiple != srcTexImporter.spriteImportMode)
            {
                srcTexImporter.spriteImportMode = SpriteImportMode.Multiple;
            }
            srcTexImporter.alphaIsTransparency = true;
            srcTexImporter.alphaSource = TextureImporterAlphaSource.FromGrayScale;
            srcTexImporter.SaveAndReimport();
        }
        if (GUILayout.Button("开始提前")) {
            ExtractSlice();
        }
    }
    void ExtractSlice()
    {
        var srcTex = Selection.activeObject as Texture2D;
        if (null == srcTex)
        {
            EditorUtility.DisplayDialog("提示","请选择要操作的图片","确定");
            return;
        }

        var srcTexPath = AssetDatabase.GetAssetPath(srcTex);
        var srcTexImporter = AssetImporter.GetAtPath(srcTexPath) as TextureImporter;
        if (SpriteImportMode.Multiple != srcTexImporter.spriteImportMode)
        {
            Debug.Log($"Sprite Mode不是Multiple: {srcTexPath}");
            return;
        }

        var srcTexDirPath = Path.GetDirectoryName(srcTexPath);
        //Debug.Log($"dirPath: {srcTexDirPath}");

        var outDirPath = $"{srcTexDirPath}/{srcTex.name}";
        if (!Directory.Exists(outDirPath))
            AssetDatabase.CreateFolder(srcTexDirPath, srcTex.name);

        var srcTempRT = RenderTexture.GetTemporary(srcTex.width, srcTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(srcTex, srcTempRT);
        var oldActiveRT = RenderTexture.active;
        RenderTexture.active = srcTempRT;

        foreach (SpriteMetaData metaData in srcTexImporter.spritesheet) //遍历小图集
        {
            var rect = metaData.rect;
            Debug.Log($"{srcTex.format}");
            var spriteTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            Debug.Log($"{metaData.name}: rect:{metaData.rect}, border:{metaData.border}");

            var readRect = new Rect(rect.x, srcTempRT.height - (rect.y + rect.height), rect.width, rect.height); //读取是从RenderTexture, (0, 0)为左上角
            spriteTex.ReadPixels(readRect, 0, 0); //写出为Texture2D, (0, 0)为左下角
            spriteTex.Apply();

            var pngBytes = spriteTex.EncodeToPNG();
            var spriteOutPath = $"{outDirPath}/{metaData.name}.png";
            File.WriteAllBytes(spriteOutPath, pngBytes);

            AssetDatabase.ImportAsset(spriteOutPath, ImportAssetOptions.ForceUpdate);
            //修改设置
            var spriteTexImporter = AssetImporter.GetAtPath(spriteOutPath) as TextureImporter;
            if (spriteTexImporter)
            {
                spriteTexImporter.textureType = TextureImporterType.Sprite;
                spriteTexImporter.mipmapEnabled = false;
                spriteTexImporter.alphaIsTransparency = true;

                var setting = new TextureImporterSettings();
                spriteTexImporter.ReadTextureSettings(setting);
                setting.spriteGenerateFallbackPhysicsShape = false;
                setting.spriteBorder = metaData.border; //原来的9宫格设置不能丢了
                spriteTexImporter.SetTextureSettings(setting);

                spriteTexImporter.SaveAndReimport();
                //EditorUtility.SetDirty(texImporter);
            }
        }
        //AssetDatabase.Refresh();

        RenderTexture.active = oldActiveRT;
        RenderTexture.ReleaseTemporary(srcTempRT);
        Debug.Log("finish");
    }
}
