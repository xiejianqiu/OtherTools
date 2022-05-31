using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// 用于查找项目中UITexture资源的使用情况
/// </summary>
public class FindAllUITextureTool : EditorWindow
{
    /// <summary>
    /// 用于存放获取的文件路径
    /// </summary>
    static string FOEDERPATH;
    static string RESULTPATH = "Assets/Editor/UITextureTools/Result";
    static string AllTextureInPrefabInfoFileName = "AllTextureInPrefabInfo.txt";
    static string CompareAllTextureFileName = "CompareAllTextureInfo.txt";

    //该方法目前会漏掉一些文件，暂时不用
    ///// <summary>
    ///// 查找所有被Prefab使用了的UITexture信息
    ///// </summary>
    //[UnityEditor.@MenuItem("Tools/FindAllUITextureFormPrefab/1、AllTextureInPrefabInfo")]
    //static void AllTextureInPrefabInfo()
    //{
    //    FOEDERPATH = "Assets/BundleData/UI";   
        
    //    if (!Directory.Exists(FOEDERPATH))
    //    {
    //        UnityEngine.Debug.LogError("文件夹不存在！path：Assets/BundleData/UI");
    //        return;
    //    }

    //    FindAllTextureInPrefabInfo();
    //}

    /// <summary>
    /// 比较所有UITexture信息
    /// </summary>
    [UnityEditor.@MenuItem("Tools/FindAllUITextureFormPrefab")]//"Tools/FindAllUITextureFormPrefab/2、CompareAllTexture"
    static void CompareAllTexture()
    {
        FOEDERPATH = "Assets/ResMS/UI/UITexture";
        
        if (!Directory.Exists(FOEDERPATH))
        {
            UnityEngine.Debug.LogError("文件夹不存在！path：Assets/ResMS/UI/UITexture");
            return;
        }

        CompareAllTextureInfo();
    }

    /// <summary>
    /// 输出所有被预设引用了的UITexture信息
    /// </summary>
    static void FindAllTextureInPrefabInfo()
    {
        DirectoryInfo direction = new DirectoryInfo(FOEDERPATH);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

        string BasePath = direction.FullName.Remove(direction.FullName.Length - FOEDERPATH.Length, FOEDERPATH.Length);
        string savePath = Path.Combine(BasePath, RESULTPATH.Replace('/', '\\'));

        CleatTextInfo(savePath, AllTextureInPrefabInfoFileName);//清理之前的text信息

        List<string> referenceList = new List<string>();
        StringBuilder infoStr = new StringBuilder();
        //读取文件信息
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".prefab"))
            {
                continue;//跳过.meta文件
            }
            string unityFilePath = files[i].FullName.Remove(0, BasePath.Length).Replace('\\', '/');

            infoStr.Clear();
            referenceList.Clear();
            referenceList = FindAllTextureInfoFromPrefab(unityFilePath, "Assets\\BundleData\\UI", i + 1, files.Length);

            string info = "预设名：" + files[i].Name + "          " + "预设路径：" + unityFilePath.Remove(unityFilePath.Length - files[i].Name.Length - 1, files[i].Name.Length + 1) +
                "          使用UITexture数：" + referenceList.Count + "          此Prefab所使用的UITexture资源：";
            infoStr.Append(info);
            for (int h = 0; h < referenceList.Count; h++)
            {
                infoStr.Append(referenceList[h]);
                if (h != referenceList.Count - 1)
                {
                    infoStr.Append("，");
                }
            }
            CreateOrOPenFile(savePath, AllTextureInPrefabInfoFileName, infoStr.ToString());
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 输出所有的UItexture信息
    /// </summary>
    static void CompareAllTextureInfo()
    {
        DirectoryInfo direction = new DirectoryInfo(FOEDERPATH);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

        string BasePath = direction.FullName.Remove(direction.FullName.Length - FOEDERPATH.Length, FOEDERPATH.Length);
        string savePath = Path.Combine(BasePath, RESULTPATH.Replace('/', '\\'));

        CleatTextInfo(savePath, CompareAllTextureFileName);//清理之前的text信息

        List<string> referenceList = new List<string>();
        StringBuilder infoStr = new StringBuilder();
        //读取文件信息
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta"))
            {
                continue;//跳过.meta文件
            }
            string unityFilePath = files[i].FullName.Remove(0, BasePath.Length).Replace('\\', '/');

            infoStr.Clear();
            referenceList.Clear();
            referenceList = FindPrefabReference(unityFilePath, "Assets\\BundleData\\UI", i + 1, files.Length);

            string info = "文件名：" + files[i].Name + "          " + "文件路径：" + unityFilePath.Remove(unityFilePath.Length - files[i].Name.Length - 1, files[i].Name.Length + 1) +
                "          引用数：" + referenceList.Count + "          使用此资源的Prefab：";
            infoStr.Append(info);
            for (int h = 0; h < referenceList.Count; h++)
            {
                infoStr.Append(referenceList[h]);
                if (h != referenceList.Count - 1)
                {
                    infoStr.Append("，");
                }
            }
            CreateOrOPenFile(savePath, CompareAllTextureFileName, infoStr.ToString());
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static List<string> FindAllTextureInfoFromPrefab(string assetPath, string forderName, int totalI, int totalLength)
    {
        List<string> prefabList = new List<string>();

        string[] names = AssetDatabase.GetDependencies(new string[] { assetPath });  //依赖的东东

        int i = 0;
        foreach (string name in names)
        {
            i++;
            //UnityEngine.Debug.LogError(name);
            if (name.Contains("Assets/ResMS/UI/UITexture"))
            {
                //UnityEngine.Debug.LogError("0000000："+name);
                if (name.EndsWith(".png"))
                {
                    //UnityEngine.Debug.LogError("1111111111：" + name);
                    prefabList.Add(name);
                    break;
                }
            }
            ShowProgress(names.Length, i, totalI, totalLength);
        }               

        return prefabList;
    }

    static List<string> FindPrefabReference(string curPathName,string forderName,int totalI,int totalLength)
    {
        List<string> prefabList = new List<string>();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab t:Scene", new string[] { forderName });

        int i = 0;
        foreach (string guid in allGuids)
        {
            i++;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] names = AssetDatabase.GetDependencies(new string[] { assetPath });  //依赖的东东
            foreach (string name in names)
            {
                if (name.Equals(curPathName))
                {
                    //Debug.Log("Refer:" + assetPath);
                    if (assetPath.EndsWith(".prefab"))
                    {
                        prefabList.Add(assetPath);
                        break;
                    }
                }
            }
            ShowProgress(allGuids.Length, i, totalI, totalLength);           
        }       

        return prefabList;
    }

    public static void ShowProgress(int total, int cur, int totalI,int totalLength)
    {
        EditorUtility.DisplayProgressBar("查找引用", string.Format("Finding {0}/{1}({2}/{3}), 正在查找...", totalI, totalLength, cur, total), (float)totalI / totalLength);
    }

    /// <summary>
    /// 打开文件并写入
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="name">文件名</param>
    /// <param name="info">写入内容</param>
    static void CreateOrOPenFile(string path, string name, string info)
    {
        StreamWriter sw = null;
        FileInfo fi = new FileInfo(path + "//" + name);
        OpenTxt(sw, fi, info);
    }

    /// <summary>
    /// 供CreateOrOPenFile方法调用,对现有txt文件写入或没有目标txt时创建该txt文件并写入
    /// </summary>
    /// <param name="sw"></param>
    /// <param name="fi"></param>
    /// <param name="info"></param>
    static void OpenTxt(StreamWriter sw, FileInfo fi, string info)
    {
        if (!fi.Exists)
        {
            sw = fi.CreateText();
        }
        else
        {
            sw = fi.AppendText();
        }
        sw.WriteLine(info, Encoding.ASCII);

        sw.Close();
        sw.Dispose();
    }

    static void CleatTextInfo(string sPath, string sName)
    {
        StreamWriter sw = new StreamWriter(sPath + "//" + sName, false, Encoding.UTF8);
        sw.Close();
        sw.Dispose();
    }
}
