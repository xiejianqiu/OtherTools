using GameFramework;
using JGGame;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        /// <summary>
        /// 补丁存放位置
        /// </summary>
        public static string PatchBuilderFolder = Application.dataPath + "/../PatchBuild";
        public static string PkgBuildFolder = Application.dataPath + "/../PkgBuild";
        public static string PathABSaveFolder
        {
            get
            {
                if (EnvUtils.IsEncryAB())
                {
                    return "StreamingAssets/AssetBundlesEncry";
                }
                return "StreamingAssets/AssetBundles";
            }
        }
        public static string ABDetailFile = "/abdetail.info";
        public static string ABDepFile = "/dep.all";
        public static string NewABDepFile{
            get {
                if (mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash))
                {
                    string resVer = mPargmaTypeCheck.GetValue(PargmaType.ResVer);
                    return $"/dep_{resVer}.all";
                }
                return ABDepFile;
            }
        }
        public static string VerAbdetailDir
        {
            get {
                string tarDir = "PatchHistory/windows/";
                if (EnvUtils.IsUNITY_ANDROID())
                {
                    tarDir = "PatchHistory/android/";
                }
                else if (EnvUtils.IsUNITY_IOS())
                {
                    tarDir = "PatchHistory/ios/";
                }
                else if (EnvUtils.IsUNITY_WEBGL())
                {
                    tarDir = "PatchHistory/webgl/";
                }
                else if (EnvUtils.IsUNITY_STANDALONE() || EnvUtils.IsUNITY_STANDALONE_WIN())
                {
                    tarDir = "PatchHistory/windows/";
                }
                return Application.dataPath + "/../" + tarDir;
            }

        }
        /// <summary>
        /// 上次更新的UpdateInfo
        /// </summary>
        public static string LastUpdateInfo {
            get {
                string tarDir = "PatchHistory/windowsVersion/";
                if (EnvUtils.IsUNITY_ANDROID())
                {
                    tarDir = "PatchHistory/androidVersion/";
                }
                else if (EnvUtils.IsUNITY_IOS())
                {
                    tarDir = "PatchHistory/iosVersion/";
                }
                else if (EnvUtils.IsUNITY_WEBGL())
                {
                    tarDir = "PatchHistory/webglVersion/";
                }
                else if (EnvUtils.IsUNITY_STANDALONE() || EnvUtils.IsUNITY_STANDALONE_WIN())
                {
                    tarDir = "PatchHistory/windowsVersion/";
                }
                string path = Application.dataPath + "/../" + tarDir +  PathConfig.Instance.UpdateInfoFileName;
                return path;
            }
        }
        static string PatchAbSavePath
        {
            get {
                if (PathConfig.Instance.UseVerPath)
                {
                    return PatchBuilderFolder + PathConfig.Instance.ResFolderPath + $"/{mPargmaTypeCheck.GetValue(PargmaType.ResVer)}/";
                }
                else
                {
                    return PatchBuilderFolder + PathConfig.Instance.ResFolderPath + "/";
                }
            }
        }
        /// <summary>
        /// ab名称带有hash值
        /// </summary>
        [MenuItem("jenkins/patch/BuildPatchWithHashName")]
        public static void BuildPatchWithHashName()
        {
            var AbdetailVer =  mPargmaTypeCheck.GetValue(PargmaType.AbdetailVer);
            //AbdetailVer = "1_baseVer";
            var oldAbdetail = VerAbdetailDir + AbdetailVer + ABDetailFile;
            var newAbdetail = Application.dataPath + ABDetailFile;
            Dictionary<string, string> fromDic = new Dictionary<string, string>();
            Dictionary<string, string> toDic = new Dictionary<string, string>();

            ReadVersionInfo(fromDic, oldAbdetail);
            ReadVersionInfo(toDic, newAbdetail);


            Dictionary<string, int> updateDic = new Dictionary<string, int>();

            if (Directory.Exists(PatchBuilderFolder))
            {
                Directory.Delete(PatchBuilderFolder, true);
            }
            Directory.CreateDirectory(PatchBuilderFolder);
            var tmpABBuildSavePath = "AssetBundles";
            string PatchInfoFile = PatchBuilderFolder +  PathConfig.Instance.UpdateInfoFileName;
            using (FileStream fs = new FileStream(PatchInfoFile, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs);
                var enu = toDic.GetEnumerator();
                
                Directory.CreateDirectory(PatchAbSavePath);

                BuildMode builMode = BuildMode.BuildAll;
                HashSet<string> abSet = new HashSet<string>();
                Enum.TryParse<BuildMode>(mPargmaTypeCheck.GetValue(PargmaType.BuildMode), out builMode);
                if (builMode != BuildMode.BuildAll)
                {
                    if (Directory.Exists(tmpABBuildSavePath))
                    {
                        if (EnvUtils.IsEncryAB())
                        {
                            var tmpABBuildSavePathEncry = tmpABBuildSavePath + "Encry";
                            EncryUtil.EncryDir(tmpABBuildSavePath, tmpABBuildSavePathEncry);
                            tmpABBuildSavePath = tmpABBuildSavePathEncry;
                        }
                        var infoMap = GenerateNewDep(tmpABBuildSavePath);
                        var files = Directory.GetFiles(tmpABBuildSavePath, "*.ab", SearchOption.AllDirectories);
                        foreach (var filePath in files)
                        {
                            var fileInfo = new FileInfo(filePath);
                            JsonData jd = new JsonData();
                            var depInfo = infoMap[fileInfo.Name];
                            var newabName = $"{depInfo.name.Substring(0, depInfo.name.IndexOf("."))}_{depInfo.bundleCrc}.ab";
                            jd["fn"] = newabName;
                            jd["md"] = GfxUtils.GetMD5Hash(filePath);
                            jd["size"] = fileInfo.Length;
                            //强设一下表格资源的文件大小
                            if (jd["fn"].Equals("972afe5c3861538901ce554291893904ac64c2c9.ab"))
                            {
                                jd["size"] = 200000;
                            }
                            sw.WriteLine(jd.ToJson());

                            File.Copy(filePath, PatchAbSavePath + newabName);
                            abSet.Add(fileInfo.Name);
                        }
                    }
                    sw.Flush();
                }

                int nCount = toDic.Count;
                int index = 0;
                try
                {
                    while (enu.MoveNext())
                    {
                        index++;
                        if (abSet.Contains(enu.Current.Key))
                            continue;
                        EditorUtility.DisplayCancelableProgressBar("提示", $"正在校验资源{enu.Current.Key}...", index * 1f / nCount);
                        string crcOld = null;
                        if (fromDic.TryGetValue(enu.Current.Key, out crcOld))
                        {
                            if (string.Equals(crcOld, enu.Current.Value))
                            {
                                continue;
                            }
                        }

                        string filePath = PathABSaveFolder + "/" + enu.Current.Key;
                        if (!File.Exists(filePath))
                        {
                            Debug.Log($"### 文件不存在：{filePath}");
                            continue;
                        }

                        var newabName = $"{enu.Current.Key.Substring(0, enu.Current.Key.IndexOf("."))}_{enu.Current.Value}.ab";
                        updateDic.Add(newabName, 0);
                        JsonData jd = new JsonData();
                        jd["fn"] = newabName;
                        jd["md"] = GfxUtils.GetMD5Hash(filePath);
                        jd["size"] = new FileInfo(filePath).Length;

                        //强设一下表格资源的文件大小
                        if (jd["fn"].Equals("972afe5c3861538901ce554291893904ac64c2c9.ab"))
                        {
                            jd["size"] = 200000;
                        }
                        File.Copy(filePath, PatchAbSavePath + newabName);
                        sw.WriteLine(jd.ToJson());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
                

                #region 检查和上次更出去的资源差异，用于记录已上传到CDN的资源信息
                List<JsonData> lstLastUpdateFiles = new List<JsonData>();
                lstLastUpdateFiles = GetLastUpdateFiles();
                for (int i = 0; i < lstLastUpdateFiles.Count; i++)
                {
                    string strFileName = lstLastUpdateFiles[i]["fn"].ToString();
                    if (!updateDic.ContainsKey(strFileName) && !strFileName.EndsWith(".dll") && !strFileName.EndsWith(".all"))
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
                                File.Copy(filePath, PatchAbSavePath + strFileName);
                                onlineCrc = newCrc;
                                size = new FileInfo(filePath).Length;
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
                        sw.WriteLine(jd.ToJson());
                    }
                }
                #endregion

                var saveABdepFile = PatchAbSavePath + NewABDepFile;
                if (builMode == BuildMode.BuildAll)
                {
                    File.Copy(PathABSaveFolder + ABDepFile, saveABdepFile);
                }
                else
                {
                    File.Copy(tmpABBuildSavePath + ABDepFile, saveABdepFile);
                }
                JsonData jdDep = new JsonData();
                jdDep["fn"] = "dep.all";
                jdDep["md"] = GfxUtils.GetMD5Hash(saveABdepFile);
                jdDep["size"] = new FileInfo(saveABdepFile).Length;
                sw.WriteLine(jdDep.ToJson());
                sw.Flush();
                File.Copy(Application.streamingAssetsPath + PathConfig.Instance.VersionFileName, PatchBuilderFolder + PathConfig.Instance.VersionFileName);
            }
            Debug.Log("#### 补丁生成完毕!!!");
        }
        /// <summary>
        /// ab名称不带hash值
        /// </summary>
        [MenuItem("jenkins/patch/BuildPatchFile")]
        public static void BuildPatchFile()
        {

            var AbdetailVer = mPargmaTypeCheck.GetValue(PargmaType.AbdetailVer);
            //AbdetailVer = "1_baseVer";
            var oldAbdetail = VerAbdetailDir + AbdetailVer + ABDetailFile;
            var newAbdetail = Application.dataPath + ABDetailFile;
            Dictionary<string, string> fromDic = new Dictionary<string, string>();
            Dictionary<string, string> toDic = new Dictionary<string, string>();

            ReadVersionInfo(fromDic, oldAbdetail);
            ReadVersionInfo(toDic, newAbdetail);

            Dictionary<string, int> updateDic = new Dictionary<string, int>();

            string PatchInfoFile = PatchBuilderFolder + PathConfig.m_UpdateInfoFileName;
            if (Directory.Exists(PatchBuilderFolder))
            {
                Directory.Delete(PatchBuilderFolder, true);
            }
            Directory.CreateDirectory(PatchBuilderFolder);
            Directory.CreateDirectory(PatchAbSavePath);
            var tmpABBuildSavePath = "AssetBundles";
            using (FileStream fs = new FileStream(PatchInfoFile, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs);

                BuildMode builMode = BuildMode.BuildAll;
                HashSet<string> abSet = new HashSet<string>();
                Enum.TryParse<BuildMode>(mPargmaTypeCheck.GetValue(PargmaType.BuildMode), out builMode);
                if (builMode != BuildMode.BuildAll)
                {
                    if (Directory.Exists(tmpABBuildSavePath))
                    {
                        if (EnvUtils.IsEncryAB())
                        {
                            var tmpABBuildSavePathEncry = tmpABBuildSavePath + "Encry";
                            EncryUtil.EncryDir(tmpABBuildSavePath, tmpABBuildSavePathEncry);
                            tmpABBuildSavePath = tmpABBuildSavePathEncry;
                        }
                        var files = Directory.GetFiles(tmpABBuildSavePath, "*.ab", SearchOption.AllDirectories);
                        foreach (var filePath in files)
                        {
                            var fileInfo = new FileInfo(filePath);
                            JsonData jd = new JsonData();
                            jd["fn"] = fileInfo.Name;
                            jd["md"] = GfxUtils.GetMD5Hash(filePath);
                            jd["size"] = fileInfo.Length;

                            //强设一下表格资源的文件大小
                            if (jd["fn"].Equals("972afe5c3861538901ce554291893904ac64c2c9.ab"))
                            {
                                jd["size"] = 200000;
                            }
                            sw.WriteLine(jd.ToJson());
                            File.Copy(filePath, PatchAbSavePath + fileInfo.Name);
                            abSet.Add(fileInfo.Name);
                        }
                        GenerateNewDep(tmpABBuildSavePath);
                    }
                    sw.Flush();
                }
                var enu = toDic.GetEnumerator();
                while (enu.MoveNext())
                {
                    if (abSet.Contains(enu.Current.Key))
                        continue;
                    string crcOld = null;
                    if (fromDic.TryGetValue(enu.Current.Key, out crcOld))
                    {
                        if (string.Equals(crcOld, enu.Current.Value))
                        {
                            continue;
                        }
                    }

                    string filePath = PathABSaveFolder + "/" + enu.Current.Key;
                    if (!File.Exists(filePath))
                    {
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

                    File.Copy(filePath, PatchAbSavePath + enu.Current.Key);

                    sw.WriteLine(jd.ToJson());
                }
                


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
                                File.Copy(filePath, PatchAbSavePath + strFileName);
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

                        //File.Copy(filePath, PatchAbSavePath + lstLastUpdateFiles[i]);

                        sw.WriteLine(jd.ToJson());
                    }
                }
                if (builMode == BuildMode.BuildAll)
                {
                    File.Copy(PathABSaveFolder + ABDepFile, PatchAbSavePath + ABDepFile);
                }
                else
                {
                    File.Copy(tmpABBuildSavePath + ABDepFile, PatchAbSavePath + ABDepFile);
                }
                JsonData jdDep = new JsonData();
                jdDep["fn"] = NewABDepFile.TrimStart('/');
                jdDep["md"] = GfxUtils.GetMD5Hash(PatchAbSavePath + ABDepFile);
                jdDep["size"] = new FileInfo(PatchAbSavePath + ABDepFile).Length;
                sw.WriteLine(jdDep.ToJson());
                updateDic.Add("dep.all", 0);
                File.Copy(Application.streamingAssetsPath + PathConfig.Instance.VersionFileName, PatchBuilderFolder + PathConfig.Instance.VersionFileName);
                sw.Close();
                fs.Close();
            }
            Debug.Log("#### 补丁生成完毕!!!");
        }

        /// <summary>
        /// 读取abdetail
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="path"></param>
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
                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = sr.ReadLine();
                }
                if (!dic.ContainsKey(name))
                    dic.Add(name, hash);
            }

            sr.Close();
            fs.Close();
        }
        /// <summary>
        /// 获取上次更新出去的资源信息
        /// </summary>
        /// <returns></returns>
        public static List<JsonData> GetLastUpdateFiles()
        {
            List<JsonData> lstFile = new List<JsonData>();
            //if (File.Exists(LastUpdateInfo))
            //{
            //    using (FileStream fs = new FileStream(LastUpdateInfo, FileMode.Open))
            //    {
            //        StreamReader sr = new StreamReader(fs);
            //        string strLine = sr.ReadLine();
            //        while (!string.IsNullOrEmpty(strLine))
            //        {
            //            JsonData jd = JsonMapper.ToObject(strLine);
            //            lstFile.Add(jd);
            //            strLine = sr.ReadLine();
            //        }
            //        sr.Close();
            //    }
            //}
            //else
            //{
            //    Debug.LogError($"{LastUpdateInfo} 不存在");
            //}
            return lstFile;
        }
        /// <summary>
        /// 包内资源逻辑
        /// </summary>
        /// <param name="abSavePath"></param>
        public static void ExtractPatchRes(string abSavePath, PkgType pkgType= PkgType.AllRes, bool IsABNameWithHash = false)
        {
            //if (pkgType == PkgType.AllRes && !IsABNameWithHash)
            //{
            //    return;
            //}
            if (Directory.Exists(abSavePath))
            {
                Directory.Delete(abSavePath, true);
            }
            if (!Directory.Exists(abSavePath))
            {
                Directory.CreateDirectory(abSavePath);
            }
            Debug.LogError($"### ExtractPatchRes {abSavePath}");
            var infoMap = ReadDep(PathABSaveFolder + ABDepFile);

            Dictionary<string, string> paDict = new Dictionary<string, string>();
            if (pkgType == PkgType.HalfRes)
            {
                var allLines = File.ReadLines(ResTool.lb_inapk_ab);
                foreach (var line in allLines)
                {
                    if (infoMap.TryGetValue(line.Trim(), out var depInfo))
                    {
                        paDict.Add(depInfo.name, IsABNameWithHash ? depInfo.bundleNameWithCrc : depInfo.name);
                    }
                }
            }
            else if(pkgType == PkgType.AllRes)
            {
                var iter = infoMap.GetEnumerator();
                while (iter.MoveNext())
                {
                    var depInfo = iter.Current.Value;
                    paDict.Add(depInfo.name, IsABNameWithHash ? depInfo.bundleNameWithCrc : depInfo.name);
                }
            }
            int cntOfAB = 0;
            if (paDict.Count > 0)
            {
                var abFiles = Directory.GetFiles(PathABSaveFolder, "*.ab", SearchOption.AllDirectories);
                foreach (var filePath in abFiles)
                {
                    var fileInfo = new FileInfo(filePath);
                    cntOfAB += 1;
                    if (paDict.ContainsKey(fileInfo.Name))
                    {
                        string newFIlePath = $"{abSavePath}/{paDict[fileInfo.Name]}";
                        if (!File.Exists(newFIlePath))
                        {
                            File.Copy(filePath, newFIlePath);
                        }
                    }
                }
                var depFiles = Directory.GetFiles(PathABSaveFolder, "*.all", SearchOption.AllDirectories);
                foreach (var filePath in depFiles)
                {
                    string newFIlePath = $"{abSavePath}{NewABDepFile}";
                    if (!File.Exists(newFIlePath))
                    {
                        File.Copy(filePath, newFIlePath);
                    }
                }
            }
           
            var files = Directory.GetFiles(abSavePath, "*.ab", SearchOption.AllDirectories);
            AssetBundlePathHelper pathResolver = new AssetBundlePathHelper();
            var cntOfPackAsset = 0;
            List<BaseFileInfo> lst = new List<BaseFileInfo>();
            foreach (var file in files)
            {
                lst.Add(new BaseFileInfo(file));
                cntOfPackAsset += 1;
            }
            if (File.Exists($"{abSavePath}{NewABDepFile}"))
            {
                lst.Add(new BaseFileInfo($"{abSavePath}{NewABDepFile}"));
            }
            File.WriteAllText($"{abSavePath}/{pathResolver.ClientResName}", JsonMapper.ToJson(lst.ToArray()), Encoding.UTF8);
            
            Debug.LogError($"PackRes:{cntOfPackAsset}/{paDict.Count}, allAB:{cntOfAB}");
            GenerateUpdateInfo(IsABNameWithHash, abSavePath);
        }

        [MenuItem("jenkins/TestExtractPatchRes")]
        public static void TestExtractPatchRes()
        {
            var abPath = "qq_debug/Yzz_Data/StreamingAssets/AssetBundles/";
            if (Directory.Exists(abPath))
            {
                Directory.Delete(abPath, true);
            }
            Directory.CreateDirectory(abPath);
            mPargmaTypeCheck.SetValue(PargmaType.ABNameWithHash, "true");
            mPargmaTypeCheck.SetValue(PargmaType.ResVer, "1.0.0.1");
            ExtractPatchRes(abPath, PkgType.HalfRes, mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
        }
        [MenuItem("jenkins/patch/TestGenerateUpdateInfo")]
        public static void TestGenerateUpdateInfo()
        {
            mPargmaTypeCheck.SetValue(PargmaType.ABNameWithHash, "true");
            mPargmaTypeCheck.SetValue(PargmaType.ResVer, "1.0.0.1");
            DefaultGenerateUpdateInfo(mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
        }
        [MenuItem("jenkins/ClearProgressBar")]
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
        public static void DefaultGenerateUpdateInfo(bool IsABNameWithHash = false)
        {
            GenerateUpdateInfo(IsABNameWithHash, PathABSaveFolder);
        }
        public static void GenerateUpdateInfo(bool IsABNameWithHash, string abPath)
        {
            bool isUnityAbSavePath = abPath == PathABSaveFolder;
            var updateInfoTxt = abPath + "/../UpdateInfo.txt";
            if (File.Exists(updateInfoTxt))
            {
                File.Delete(updateInfoTxt);
            }
            using (FileStream f = File.OpenWrite(updateInfoTxt))
            {
                StreamWriter writer = new StreamWriter(f);
                //var infoMap = ReadDep(abPath + "/" + (isUnityAbSavePath ? ABDepFile : NewABDepFile));
                var infoMap = ReadDep(PathABSaveFolder + ABDepFile);
                var iter = infoMap.GetEnumerator();
                int index = 0;
                JsonData jd = new JsonData();
                while (iter.MoveNext())
                {
                    var depInfo = iter.Current.Value;
                    var filePath = abPath + "/" + (!isUnityAbSavePath && IsABNameWithHash ? depInfo.bundleNameWithCrc : depInfo.name);
                    if (!File.Exists(filePath))
                    {
                        if (isUnityAbSavePath)
                        {
                            Debug.LogWarning($"Not Find {filePath}");
                        }
                        continue;
                    }
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (!Application.isBatchMode) EditorUtility.DisplayProgressBar("提示", $"正在计算md5...", index++ * 1f / infoMap.Count);
                    var name = fileInfo.Name;
                    jd["fn"] = IsABNameWithHash ? depInfo.bundleNameWithCrc : depInfo.name;
                    jd["md"] = GfxUtils.GetMD5Hash(filePath);
                    jd["size"] = fileInfo.Length;
                    writer.WriteLine(jd.ToJson());
                }

                var depFiles = Directory.GetFiles(abPath, "*.all", SearchOption.AllDirectories);
                foreach (var filePath in depFiles)
                {
                    JsonData jdDep = new JsonData();
                    jdDep["fn"] = Path.GetFileName(filePath);
                    jdDep["md"] = GfxUtils.GetMD5Hash(filePath);
                    jdDep["size"] = new FileInfo(filePath).Length;
                    writer.WriteLine(jdDep.ToJson());
                }
                writer.Flush();
                if (!Application.isBatchMode) EditorUtility.ClearProgressBar();
            }
        }
    }
}
