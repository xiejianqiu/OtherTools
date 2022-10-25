using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using LitJson;
using UnityEngine;

public class PatchBuilder
{

    public static string PatchBuilderFolder = Application.dataPath + "/../PatchBuild";
    public static string PathABSaveFolder = Application.dataPath + "/StreamingAssets/AssetBundles";
    public static string ABDepFile = "/dep.all";
    public static string BaseABDepFile = "/basedep";
    public static string NewABDepFile = "/newdep";
    public static string ABDetailFile = "/abdetail.info";
    public static string BaseABDetailFile = "/baseabdetail";
    public static string NewABDetailFile = "/newabdetail";
    public static string PathBaseDll = PatchBuilderFolder + "/basedll";
    public static string PathNewDll = PatchBuilderFolder + "/newdll";
    public static string PathDll = PatchBuilderFolder + "/pathdll";

    public static void BuildBaseVersion_Android()
    {
        BuildBaseVersion(BuildTarget.Android);
    }
    public static void BuildNewVersion_Android()
    {
        BuildNewVersion(BuildTarget.Android);
    }
    public static void BuildBaseVersion_iOS()
    {
        BuildBaseVersion(BuildTarget.iOS);
    }
    public static void BuildNewVersion_iOS()
    {
        BuildNewVersion(BuildTarget.iOS);
    }


    static void BuildBaseVersion(BuildTarget target)
    {
        //ABBuildPanel.BuildAssetBundles(target);

        File.Copy(PathABSaveFolder + ABDepFile, PatchBuilderFolder + BaseABDepFile);
        File.Copy(Application.dataPath + ABDetailFile, PatchBuilderFolder + BaseABDetailFile);

        //CopyDll(PathBaseDll);
    }

    static void BuildNewVersion(BuildTarget target)
    {
        //ABBuildPanel.BuildAssetBundles(target);

        File.Copy(PathABSaveFolder + ABDepFile, PatchBuilderFolder + NewABDepFile);
        File.Copy(Application.dataPath + ABDetailFile, PatchBuilderFolder + NewABDetailFile);

        //CopyDll(PathNewDll);
    }

    public static void BuildPatchFile()
    {
        Dictionary<string, string> fromDic = new Dictionary<string, string>();
        Dictionary<string, string> toDic = new Dictionary<string, string>();
        Dictionary<string, int> updateDic = new Dictionary<string, int>();

        ReadVersionInfo(fromDic, PatchBuilderFolder + BaseABDetailFile);
        ReadVersionInfo(toDic, PatchBuilderFolder + NewABDetailFile);

        string PatchInfoFile = PatchBuilderFolder + PathConfig.m_UpdateInfoFileName;

        FileStream fs = new FileStream(PatchInfoFile, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        var enu = toDic.GetEnumerator();
        while (enu.MoveNext())
        {
            string crcOld = null;
            if (fromDic.TryGetValue(enu.Current.Key, out crcOld))
            {
                if (string.Equals(crcOld, enu.Current.Value))
                {
                    continue;
                }
            }

            string filePath = PathABSaveFolder + "/" + enu.Current.Key;
            if (!File.Exists(filePath)) {
                Debug.Log($"### 文件不存在：{filePath}");
                continue;
            }
            updateDic.Add(enu.Current.Key, 0);
            
            JsonData jd = new JsonData();
            jd["fn"] = enu.Current.Key;
            jd["md"] = GfxUtils.GetMD5Hash(filePath);
            jd["size"] = new FileInfo(filePath).Length;

            //强设一下表格资源的文件大小
            if (jd["fn"].Equals("972afe5c3861538901ce554291893904ac64c2c9.ab"))
            {
                jd["size"] = 200000;
            }

            File.Copy(filePath, PatchBuilderFolder + PathConfig.Instance.ResFolderPath + "/" + enu.Current.Key);

            sw.WriteLine(jd.ToJson());
        }

        File.Copy(PathABSaveFolder + ABDepFile, PatchBuilderFolder + ABDepFile);

        List<JsonData> lstLastUpdateFiles = new List<JsonData>();
        lstLastUpdateFiles = GetLastUpdateFiles();
        for (int i = 0; i < lstLastUpdateFiles.Count; i++)
        {
            string strFileName = lstLastUpdateFiles[i]["fn"].ToString();
            if (!updateDic.ContainsKey(strFileName) && !strFileName.EndsWith(".dll") && !strFileName.Equals("dep.all"))
            {
                string filePath = PathABSaveFolder + "/" + strFileName;

                updateDic.Add(strFileName, 0);
                var onlineCrc = lstLastUpdateFiles[i]["md"].ToString();
                var size = lstLastUpdateFiles[i]["size"];
                if (toDic.TryGetValue(strFileName, out var hash))
                {
                    var newCrc = GfxUtils.GetMD5Hash(filePath);
                    if (!string.Equals(newCrc, onlineCrc))
                    {
                        File.Copy(filePath, PatchBuilderFolder + PathConfig.Instance.ResFolderPath + "/" + strFileName);
                        onlineCrc = newCrc;
                        size = new FileInfo(filePath).Length;
                        Debug.Log($"****** ######### {strFileName}");
                    }
                }
                else 
                {
                    Debug.Log($"Not Exist:########## {strFileName}");
                }
                JsonData jd = new JsonData();
                jd["fn"] = strFileName;
                jd["md"] = onlineCrc;
                jd["size"] = size;

                //File.Copy(filePath, PatchBuilderFolder + PathConfig.Instance.ResFolderPath + "/" + lstLastUpdateFiles[i]);

                sw.WriteLine(jd.ToJson());
            }
        }

        JsonData jdDep = new JsonData();
        jdDep["fn"] = "dep.all";
        jdDep["md"] = GfxUtils.GetMD5Hash(PathABSaveFolder + ABDepFile);
        jdDep["size"] = new FileInfo(PathABSaveFolder + ABDepFile).Length;
        sw.WriteLine(jdDep.ToJson());
        updateDic.Add("dep.all", 0);

        sw.Close();
        fs.Close();

#if UNITY_ANDROID
        GenDllPatch();
#endif
#if UNITY_IOS
        //GenDllPatchIOS();
#endif
    }

    public static List<JsonData> GetLastUpdateFiles()
    {
        List<JsonData> lstFile = new List<JsonData>();

#if UNITY_IOS
        string pathAndroid = Application.dataPath + "/../PatchHistory/iosVersion" + PathConfig.Instance.UpdateInfoFileName;
#else
        string pathAndroid = Application.dataPath + "/../PatchHistory/androidVersion" + PathConfig.Instance.UpdateInfoFileName;
#endif
        using (FileStream fs = new FileStream(pathAndroid, FileMode.Open))
        {
            StreamReader sr = new StreamReader(fs);
            string strLine = sr.ReadLine();
            while (!string.IsNullOrEmpty(strLine))
            {
                JsonData jd = JsonMapper.ToObject(strLine);
                lstFile.Add(jd);
                strLine = sr.ReadLine();
            }
            sr.Close();
        }

        return lstFile;
    }

    public static void ReadVersionInfo(Dictionary<string, string> dic, string path)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

        StreamReader sr = new StreamReader(fs);
        char[] fileHeadChars = new char[6];
        sr.Read(fileHeadChars, 0, fileHeadChars.Length);

        while (true)
        {
            string debugName = sr.ReadLine();
            if (string.IsNullOrEmpty(debugName))
                break;

            string name = sr.ReadLine();
            string shortFileName = sr.ReadLine();
            string hash = sr.ReadLine();
            int typeData = Convert.ToInt32(sr.ReadLine());
            int depsCount = Convert.ToInt32(sr.ReadLine());
            string[] deps = new string[depsCount];

            //if (!shortName2FullName.ContainsKey(shortFileName))
            //    shortName2FullName.Add(shortFileName, name);
            for (int i = 0; i < depsCount; i++)
            {
                deps[i] = sr.ReadLine();
            }
            if(!dic.ContainsKey(name))
                dic.Add(name, hash);
        }

        sr.Close();
        fs.Close();
    }

    static void CopyDll(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            GfxUtils.DltAllFiles(path);
        }

        List<string> lstFile = new List<string>();

        lstFile.Add(Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp.dll");
        lstFile.Add(Application.dataPath + "/Plugins/Common.dll");
        lstFile.Add(Application.dataPath + "/Plugins/GameFramework.dll");

        for (int i = 0; i < lstFile.Count; i++)
        {
            FileInfo fi = new FileInfo(lstFile[i]);
            File.Copy(lstFile[i], path + "/" + fi.Name);
        }
    }

    static Dictionary<string, string> CalcMD5ForDll(string path)
    {
        Dictionary<string, string> dicMd5 = new Dictionary<string, string>();

        List<string> lstFile = new List<string>();

        lstFile.Add(path + "/Assembly-CSharp.dll");
        lstFile.Add(path + "/Common.dll");
        lstFile.Add(path + "/GameFramework.dll");

        for (int i = 0; i < lstFile.Count; i++)
        {
            FileInfo fi = new FileInfo(lstFile[i]);

            dicMd5.Add(fi.Name, GfxUtils.GetMD5Hash(lstFile[i]));
        }

        return dicMd5;
    }

    static void GenDllPatch()
    {
        if (EnvUtils.IsIl2CPP_ARM64()) {
            return;
        }
        Dictionary<string, string> dicBaseMd5 = CalcMD5ForDll(PathBaseDll);
        Dictionary<string, string> dicNewMd5 = CalcMD5ForDll(PathNewDll);
        List<string> lstPathDll = new List<string>();

        var enu = dicBaseMd5.GetEnumerator();
        while (enu.MoveNext())
        {
            string newMd5 = null;
            if (dicNewMd5.TryGetValue(enu.Current.Key, out newMd5))
            {
                if (!enu.Current.Value.Equals(newMd5))
                {
                    lstPathDll.Add(enu.Current.Key);
                }
            }
        }

        GfxUtils.DltAllFiles(PathDll);

        for (int i = 0; i < lstPathDll.Count; i++)
        {
            File.Copy(PathNewDll + "/" + lstPathDll[i], PathDll + "/" + lstPathDll[i]);
        }

        string PatchInfoFile = PatchBuilderFolder + PathConfig.m_UpdateInfoFileName;
        FileStream fs = new FileStream(PatchInfoFile, FileMode.Append, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);

        string[] files = Directory.GetFiles(PathDll, "*.dll");
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo fi = new FileInfo(files[i]);

            JsonData jdDll = new JsonData();
            jdDll["fn"] = fi.Name;
            jdDll["md"] = GfxUtils.GetMD5Hash(files[i]);
            jdDll["size"] = fi.Length;
            sw.WriteLine(jdDll.ToJson());
        }

        sw.Close();
        fs.Close();

    }

    static void GenDllPatchIOS()
    {
        Dictionary<string, string> dicBaseMd5 = CalcMD5ForDll_IOS(PathBaseDll);
        Dictionary<string, string> dicNewMd5 = CalcMD5ForDll_IOS(PathNewDll);
        List<string> lstPathDll = new List<string>();

        var enu = dicBaseMd5.GetEnumerator();
        while (enu.MoveNext())
        {
            string newMd5 = null;
            if (dicNewMd5.TryGetValue(enu.Current.Key, out newMd5))
            {
                if (!enu.Current.Value.Equals(newMd5))          
                {
                    lstPathDll.Add(enu.Current.Key);
                }
            }
        }

        GfxUtils.DltAllFiles(PathDll);

        for (int i = 0; i < lstPathDll.Count; i++)
        {
            File.Copy(PathNewDll + "/" + lstPathDll[i], PathDll + "/" + lstPathDll[i]);
        }

        string PatchInfoFile = PatchBuilderFolder + PathConfig.m_UpdateInfoFileName;
        FileStream fs = new FileStream(PatchInfoFile, FileMode.Append, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);

        string[] files = Directory.GetFiles(PathDll, "*.bytes");
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo fi = new FileInfo(files[i]);

            JsonData jdDll = new JsonData();
            jdDll["fn"] = fi.Name;
            jdDll["md"] = GfxUtils.GetMD5Hash(files[i]);
            jdDll["size"] = fi.Length;
            sw.WriteLine(jdDll.ToJson());
        }
        sw.Close();
        fs.Close();
    }
    static Dictionary<string, string> CalcMD5ForDll_IOS(string path)
    {
        Dictionary<string, string> dicMd5 = new Dictionary<string, string>();

        List<string> lstFile = new List<string>();

        lstFile.Add(path + "/hotfix.dll.bytes");
        lstFile.Add(path + "/hotfix.pdb.bytes");

        for (int i = 0; i < lstFile.Count; i++)
        {
            FileInfo fi = new FileInfo(lstFile[i]);

            dicMd5.Add(fi.Name, GfxUtils.GetMD5Hash(lstFile[i]));
        }
        return dicMd5;
    }
    [MenuItem("ABSystem/CopyTarAbToFolder")]
    public static void CopyTarAbToFold()
    {
        string projPath = Application.dataPath.Replace("Assets", "");
        string srcPath = projPath + @"AssetBundles\AssetBundles\".Replace("\\", "/");
        string outPath = projPath + @"AssetBundles\AssetBundles_Out\".Replace("\\", "/");
        string fileListPath = projPath + @"\AssetBundles\fileList.txt".Replace("\\", "/");
        if (Directory.Exists(outPath))
        {
            Directory.Delete(outPath, true);
        }
        Directory.CreateDirectory(outPath);
        using (var stream = File.OpenRead(fileListPath))
        {
            StreamReader reader = new StreamReader(stream);
            var abs = reader.ReadToEnd().Split('\n');
            foreach (var ab in abs)
            {
                string abfile = ab.Trim();
                string srcFile = $"{srcPath}{abfile}";
                string tarFile = $"{outPath}{abfile}";
                if (File.Exists(srcFile))
                {
                    File.Copy(srcFile, tarFile);
                }
                else
                {
                    Debug.Log($"{srcFile} to {tarFile}");

                }
            }

        }

    }
}
