using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AtlasRGBASpliter : Editor
{
    static string prjDataPath = Application.dataPath.Replace("\\", "/");
    static string atlasBaseShaderName = "Unlit/Transparent Colored";
    static string atlasExtendShaderName = "Unlit/Transparent Merge";
    [MenuItem("Tools/SplitAtlas")]
    public static void SplitAtlas()
    {
        string testPath = (prjDataPath + "/BundleData/TPProject/").Replace("\\", "/");
        SplitAtlasTexture(testPath);
    }

    [MenuItem("Tools/ChangeAtlasShader")]
    public static void ChangeAtlasShader()
    {
        string testPath = (prjDataPath + "/BundleData/TPProject/").Replace("\\", "/");
        ChangeShader(testPath);
    }

    public static void SplitAtlas(string path)
    {
        //SplitAtlasTexture(path);
    }

    public static void SplitAtlasTexture(string dirPath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        if (!dirInfo.Exists)
        {
            return;
        }
        List<string> fileList = GetTextureList(dirInfo);
        foreach (string file in fileList)
        {
            ProcessMaterial(file);
        }
    }

    public static void ChangeShader(string dirPath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        if (!dirInfo.Exists)
        {
            return;
        }
        List<string> fileList = GetAllMaterials(dirInfo);
        foreach (string file in fileList)
        {
            ChangeMaterialShader(file);
        }
    }

    private static List<string> GetAllMaterials(DirectoryInfo dirInfo)
    {
        List<string> fileList = new List<string>();
        FileInfo[] allFile = dirInfo.GetFiles();
        foreach (FileInfo fi in allFile)
        {
            if (fi.Extension.ToLower() == ".mat")
            {
                fileList.Add(fi.FullName.Replace("\\", "/").Replace(prjDataPath, "Assets"));
            }
        }
        DirectoryInfo[] allDirInfos = dirInfo.GetDirectories();
        foreach (DirectoryInfo di in allDirInfos)
        {
            fileList.AddRange(GetAllMaterials(di));
        }
        return fileList;
    }

    private static List<string> GetTextureList(DirectoryInfo dirInfo)
    {
        List<string> fileList = new List<string>();
        FileInfo[] allFile = dirInfo.GetFiles();
        foreach (FileInfo fi in allFile)
        {
            if (fi.Extension.ToLower() == ".png")
            {
                if (fi.Name.ToLower().EndsWith("_alpha.png") || fi.Name.ToLower().EndsWith("_color.png"))
                {
                    continue;
                }

                fileList.Add(fi.FullName.Replace("\\", "/").Replace(prjDataPath, "Assets"));
            }
        }
        DirectoryInfo[] allDirInfos = dirInfo.GetDirectories();
        foreach (DirectoryInfo di in allDirInfos)
        {
            fileList.AddRange(GetTextureList(di));
        }
        return fileList;
    }

    private static bool CheckHasAlpha(Texture2D texture)
    {
        if (texture == null)
            return false;
        string texpath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(texpath))
            return false;

        TextureImporter textureImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
        if (textureImporter == null)
            return false;
        return textureImporter.DoesSourceTextureHaveAlpha();
    }
    private static bool SetTextureReadable(string texturePath, bool isReadable)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        textureImporter.isReadable = isReadable;
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        return true;
    }
    public static void WriteTextureToFile(Texture2D texture, string texPath)
    {
        if (null == texture || string .IsNullOrEmpty(texPath))
            return;
        File.WriteAllBytes(prjDataPath.Replace("Assets", "") + texPath, texture.EncodeToPNG());
    }
    public static void SetTextureImportParameter(string texPath)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (null == textureImporter)
            return;
        textureImporter.textureType = TextureImporterType.Default;
        textureImporter.mipmapEnabled = false;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.filterMode = FilterMode.Trilinear;
        textureImporter.anisoLevel = 5;
        textureImporter.SetPlatformTextureSettings("Default", 2048, TextureImporterFormat.AutomaticCompressed);
        textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGB4, 98, false);
        textureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGB4, 98, false);
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }
    private static void ProcessMaterial(string matPath)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(matPath);
        if (texture == null)
            return;
        try
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            if (!texturePath.EndsWith(".png"))
                throw new System.Exception("texturePath is not png.");

            if (!SetTextureReadable(texturePath, true))
                throw new System.Exception("SetTextureReadable(texturePath, true) failed.");

            int textWidth = texture.width >= texture.height ? texture.width : texture.height;
            Texture2D colorTex = new Texture2D(textWidth, textWidth, TextureFormat.RGB24, false);
            Texture2D alphaTex = new Texture2D(textWidth, textWidth, TextureFormat.RGB24, false);
            if (!SplitTexture(texture, ref colorTex, ref alphaTex))
                throw new System.Exception("SplitTexture failed.");
            
            string colorTexPath = texturePath.Replace(".png", "_color.png");
            string alphaTexPath = texturePath.Replace(".png", "_alpha.png");
            WriteTextureToFile(colorTex, colorTexPath);
            WriteTextureToFile(alphaTex, alphaTexPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            SetTextureImportParameter(colorTexPath);
            SetTextureImportParameter(alphaTexPath);
            
            Object.DestroyImmediate(colorTex, true);
            Object.DestroyImmediate(alphaTex, true);

            if (!SetTextureReadable(texturePath, false))
                throw new System.Exception("SetTextureReadable(texturePath, false) failed.");
        }
        catch(System.Exception ex)
        {
            Debug.LogFormat("ProcessMaterial Error: {0}", ex.Message);
        }
        Resources.UnloadUnusedAssets();
    }

    private static bool SplitTexture(Texture2D sourceTex, ref Texture2D colorTex, ref Texture2D alphaTex)
    {
        if (sourceTex == null || colorTex == null || alphaTex == null)
            return false;
        if (colorTex.width != colorTex.height ||
            alphaTex.width != alphaTex.height ||
            colorTex.width != alphaTex.width ||
            sourceTex.width > colorTex.width ||
            sourceTex.height > colorTex.width)
            return false;
        Color32[] sourcePixels = sourceTex.GetPixels32();
        Color32[] colorPixels = new Color32[colorTex.width * colorTex.height];
        Color32[] alphaPixels = new Color32[alphaTex.width * alphaTex.height];
        if (sourcePixels == null || colorPixels == null || alphaPixels == null)
            return false;
        for(int i = 0; i < colorTex.height; ++i)
        {
            for (int j = 0; j < colorTex.width; ++j)
            {
                int sorIdx = i * sourceTex.width + j;
                int tarIdx = i * colorTex.width + j;
                if (i >= sourceTex.height || j >= sourceTex.width)
                {
                    colorPixels[tarIdx] = Color.black;
                    alphaPixels[tarIdx] = Color.white;
                }
                else
                {
                    //copy color
                    colorPixels[tarIdx].r = sourcePixels[sorIdx].r;
                    colorPixels[tarIdx].g = sourcePixels[sorIdx].g;
                    colorPixels[tarIdx].b = sourcePixels[sorIdx].b;
                    colorPixels[tarIdx].a = 0;

                    //copy alpha
                    alphaPixels[tarIdx].r = sourcePixels[sorIdx].a;
                    alphaPixels[tarIdx].g = 0;
                    alphaPixels[tarIdx].b = 0;
                    alphaPixels[tarIdx].a = 0;
                }
            }
        }
        colorTex.SetPixels32(colorPixels);
        alphaTex.SetPixels32(alphaPixels);
        colorTex.Apply();
        alphaTex.Apply();
        return true;
    }

    private static void ChangeMaterialShader(string matPath)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
            return;
        try
        {
            Shader baseShader = mat.shader;
            Shader replaceShader = Shader.Find(atlasExtendShaderName);
            if (baseShader == null || replaceShader == null || baseShader.name != atlasBaseShaderName)
                throw new System.Exception("baseShader.name != atlasBaseShaderName.");

            Texture2D texture = mat.GetTexture("_MainTex") as Texture2D;
            string texturePath = AssetDatabase.GetAssetPath(texture);
            if (!texturePath.EndsWith(".png"))
                throw new System.Exception("texturePath is not png.");

            string colorTexPath = texturePath.Replace(".png", "_color.png");
            string alphaTexPath = texturePath.Replace(".png", "_alpha.png");

            Texture2D colorReplaceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(colorTexPath);
            Texture2D alphaReplaceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaTexPath);
            if (colorReplaceTex == null || alphaReplaceTex == null)
                throw new System.Exception("colorReplaceTex == null || alphaReplaceTex == null.");

            mat.shader = replaceShader;
            mat.SetTexture("_MainTex", colorReplaceTex);
            mat.SetTexture("_Mask", alphaReplaceTex);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("ProcessMaterial Error: {0}", ex.Message);
        }
        Resources.UnloadUnusedAssets();
    }
}
