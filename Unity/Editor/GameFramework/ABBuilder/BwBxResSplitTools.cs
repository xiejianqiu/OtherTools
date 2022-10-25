using GameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BwBxResSplitTools
{
    static Dictionary<string, string> _resDic;
    static float _progress;

    [MenuItem("ABSystem/Bwbx/01 Build all android AB")]
    public static void BuildAB()
    {
        ABBuildPanel.BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("ABSystem/Bwbx/02 Split upload AB")]
    public static void SplitUpLoadRes()
    {
        EditorUtility.DisplayProgressBar("In resource splitting...", "Reading...", 0.1f);
        ChangeResList();
        SplitRes(true);

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("ABSystem/Bwbx/03 Split upload AB Test")]
    public static void TestSplitUpLoadRes()
    {
        EditorUtility.DisplayProgressBar("In resource splitting...", "Reading...", 0.1f);
        ChangeResList();
        SplitRes(true,true);

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void ChangeResList()
    {
        List<string> recordResList = new List<string>(File.ReadAllLines(Application.streamingAssetsPath + "/dataExport.info"));
        for (int i = 0; i < recordResList.Count; i++)
        {
            if (!recordResList[i].Contains("."))
            {
                recordResList[i] = ChangeResName(recordResList[i]);
            }
            if (recordResList[i] == null)
            {
                recordResList.Remove(recordResList[i]);
            }
        }
        File.WriteAllLines(Application.streamingAssetsPath + "/dataExport.info", recordResList);
    }

    public static void SplitRes(bool isSplit,bool isTest = false)
    {
        //1.将记录的列表全部获取.ab名称
        List<string> recordResList = new List<string>(File.ReadAllLines(Application.streamingAssetsPath + "/dataExport.info"));
        for (int i = 0; i < recordResList.Count; i++)
        {
            recordResList[i] = HashUtil.Get(AssetBundleUtils.ConvertToABName(recordResList[i])) + ".ab";
        }

        //2.原本所有的all
        Dictionary<string, AssetBundleData> all_Dep = new Dictionary<string, AssetBundleData>();
        all_Dep = ReadDep();

        //3.递归获取所有需要保存下来的.ab
        HashSet<string> needSaveSet = new HashSet<string>();
        for (int i = 0; i < recordResList.Count; i++)
        {
            if (recordResList[i] != null && all_Dep.ContainsKey(recordResList[i]))
            {
                SaveAbSet(all_Dep, recordResList[i], needSaveSet);
            }
        }
        needSaveSet.Add("972afe5c3861538901ce554291893904ac64c2c9.ab");     //Txt文件。

        //4.生成保存下来的列表
        Dictionary<string, AssetBundleData> data_Dep = new Dictionary<string, AssetBundleData>();
        foreach (var VARIABLE in needSaveSet)
        {
            data_Dep[VARIABLE] = all_Dep[VARIABLE];
        }
        if(!isSplit || isTest)
        {
            GenerateDownLoadList(Application.streamingAssetsPath + "/AssetBundles/dep.all", data_Dep, "ABDT");
        }

        //5.编辑器模式下拷贝资源、压缩资源
        if (isSplit || isTest)
        {
            EditorUtility.DisplayProgressBar("In resource splitting...", "Copying...", 0.2f);
            //5.1上传目录
            string copyTo = Application.dataPath + "/../UpLoadRes";
            if (Directory.Exists(copyTo))
            {
                DirectoryInfo copyToDic = new DirectoryInfo(copyTo);
                FileInfo[] copyToDicList = copyToDic.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var file in copyToDicList)
                {
                    File.Delete(file.FullName);
                }
            }
            else
            {
                Directory.CreateDirectory(copyTo);
            }
            //5.2拷贝所需下载的文件
            DirectoryInfo abDir = new DirectoryInfo("Assets/StreamingAssets/AssetBundles");
            FileInfo[] files = abDir.GetFiles("*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Name.EndsWith(".meta") && files[i].Name != "AssetBundles" && files[i].Name != "dep.all")
                {
                    if (!needSaveSet.Contains(files[i].Name))
                    {
                        string pFilePath = copyTo + "/" + files[i].Name;
                        if (File.Exists(pFilePath))
                            continue;
                        File.Copy(files[i].FullName, pFilePath, true);
                    }
                }
            }

            //5.3压缩资源所有需要下载的文件
            long totalDataLength = 0;
            DirectoryInfo abZipPath = new DirectoryInfo(copyTo);
            FileInfo[] zipFiles = abZipPath.GetFiles("*.ab", SearchOption.AllDirectories);
            for (int i = 0; i < zipFiles.Length; i++)
            {
                _progress = float.Parse((float.Parse(i.ToString())/float.Parse(zipFiles.Length.ToString())).ToString("#0.00"));
                EditorUtility.DisplayProgressBar("In resource splitting...", string.Format("{0}:{1}/{2}","Zip...",i, zipFiles.Length), _progress);
                string[] zipArr = { zipFiles[i].FullName };
                ZipTools.Zip(zipArr, zipFiles[i].FullName+".zip", "123", null);
                totalDataLength += new FileInfo(zipFiles[i].FullName + ".zip").Length;
                File.Delete(zipFiles[i].FullName);
            }

            //5.4生成所有需要下载的文件列表
            Dictionary<string, AssetBundleData> update_Dep = new Dictionary<string, AssetBundleData>();
            foreach (var VARIABLE in all_Dep)
            {
                if (!needSaveSet.Contains(VARIABLE.Key))
                {
                    update_Dep[VARIABLE.Key] = all_Dep[VARIABLE.Key];
                }
            }
            GenerateDownLoadList(copyTo + "/bwbxDep.all", update_Dep, totalDataLength.ToString());
        }

        //6.打包安装包时，把非记录的资源删除掉
        if (!isSplit || isTest)
        {
            DirectoryInfo abDirDelete = new DirectoryInfo("Assets/StreamingAssets/AssetBundles");
            FileInfo[] filesDe = abDirDelete.GetFiles("*.ab", SearchOption.AllDirectories);
            for (int i = 0; i < filesDe.Length; i++)
            {
                if (!needSaveSet.Contains(filesDe[i].Name))
                {
                    File.Delete(filesDe[i].FullName);
                }
            }
        }
    }

    static void SaveAbSet(Dictionary<string, AssetBundleData> all_Dep, string abFullName, HashSet<string> set)
    {
        if (all_Dep.Keys.Contains(abFullName))
        {
            set.Add(abFullName);
            for (int j = 0; j < all_Dep[abFullName].dependencies.Length; j++)
            {
                SaveAbSet(all_Dep, all_Dep[abFullName].dependencies[j], set);
            }
        }
    }

    static Dictionary<string, AssetBundleData> ReadDep()
    {
        AssetBundlePathHelper pathResolver = new AssetBundlePathHelper();
        string depFile = string.Format("{0}/{1}", pathResolver.BundleSavePath, pathResolver.DependFileName);
        FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
        AssetBundleDataReader reader = new AssetBundleDataReader();
        reader.Read(fs);
        fs.Close();
        return reader.infoMap;    
    }

    static void GenerateDownLoadList(string path, Dictionary<string, AssetBundleData> downLoad_Dep,string dataStr)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream fs = new FileStream(path, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        sw.WriteLine(dataStr);
        foreach (var VARIABLE in downLoad_Dep)
        {
            sw.WriteLine(VARIABLE.Value.debugName);
            sw.WriteLine(VARIABLE.Value.fullName);
            sw.WriteLine(VARIABLE.Value.dependencies.Length);
            foreach (string item in VARIABLE.Value.dependencies)
            {
                sw.WriteLine(item);
            }
        }
        sw.Close();
        sw.Dispose();
        fs.Close();
        fs.Dispose();
    }

    static BwBxResSplitTools()
    {
#if UNITY_EDITOR
        _resDic = new Dictionary<string, string>();
        DirectoryInfo abDir = new DirectoryInfo("Assets/BundleData");
        FileInfo[] files = abDir.GetFiles("*.*", SearchOption.AllDirectories);

        List<string> allResList = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (!Path.GetExtension(files[i].FullName).ToLower().Equals(".meta"))
                allResList.Add(files[i].FullName.Replace('\\', '/'));
        }
        for (int i = 0; i < allResList.Count; i++)
        {
            allResList[i] = allResList[i].Remove(0, allResList[i].IndexOf("Assets"));
            string value = allResList[i];
            allResList[i] = allResList[i].Remove(allResList[i].IndexOf('.'));
            string key = allResList[i];
            _resDic[key] = value;                      //@@@此处BundleData/TPProject 资源会重叠，导致读取不完
        }
#endif
    }

    static string ChangeResName(string key)
    {
        key = key.Replace('\\', '/');
        if (_resDic.ContainsKey(key))
        {
            return _resDic[key];
        }
        return null;
    }
}
