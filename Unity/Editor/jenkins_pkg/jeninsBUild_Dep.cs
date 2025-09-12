using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Jenkins.JenkinsBuild;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        /// <summary>
        /// 用于读取dep.all
        /// </summary>
        public class ABDepInfo
        {
            public static  Dictionary<string,ABDepInfo> sDepDict;
            public string debugName;
            public string name;
            public string bundleCrc;
            public int depsCount;
            public List<string> deps;
            public string bundleNameWithCrc;

            public ABDepInfo(string debugName, string name, string bundleCrc, int depsCount)
            {
                this.debugName = debugName;
                this.name = name;
                this.bundleCrc = bundleCrc;
                this.depsCount = depsCount;
                deps = new List<string>(depsCount);
                this.bundleNameWithCrc = $"{this.name.Substring(0, this.name.IndexOf("."))}_{this.bundleCrc}.ab";
            }

            public void AddDep(string dep)
            {
                deps.Add(dep);
            }
            static public void Add(ABDepInfo depInfo)
            {
                if (null == sDepDict)
                {
                    sDepDict = new Dictionary<string, ABDepInfo>();
                }
                try
                {
                    sDepDict.Add(depInfo.name, depInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"###ABDepInfo {depInfo.name}");
                }
            }
            public static void Clear()
            {
                if (null != sDepDict)
                {
                    sDepDict.Clear();
                    sDepDict = null;
                }
            }
        }
        static Dictionary<string, ABDepInfo> ReadDep(string depFile)
        {
            if (null != ABDepInfo.sDepDict && ABDepInfo.sDepDict.Count > 0)
            {
                return ABDepInfo.sDepDict;
            }
            ABDepInfo.Clear();
            using (var fs = File.OpenRead(depFile))
            {
                StreamReader sr = new StreamReader(fs);
                char[] fileHeadChars = new char[6];
                sr.Read(fileHeadChars, 0, fileHeadChars.Length);
                //读取文件头判断文件类型，ABDT 意思即 Asset-Bundle-Data-Text
                if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D' || fileHeadChars[3] != 'T')
                    return null;
                while (true)
                {
                    string debugName = sr.ReadLine();
                    if (string.IsNullOrEmpty(debugName))
                        break;
                    string name = sr.ReadLine();
                    string bundleCrc = sr.ReadLine();
                    int depsCount = Convert.ToInt32(sr.ReadLine());
                    ABDepInfo depInfo = new ABDepInfo(debugName,name,bundleCrc,depsCount);
                    for (int i = 0; i < depsCount; i++)
                    {
                        depInfo.AddDep(sr.ReadLine());
                    }
                    ABDepInfo.Add(depInfo);
                }

                sr.Close();
                return ABDepInfo.sDepDict;
            }
        }
        static void SaveToFile(Dictionary<string, ABDepInfo> infoMap,string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("ABDT");
            var iter = infoMap.GetEnumerator();
            while (iter.MoveNext())
            {
                var info = iter.Current.Value;
                builder.AppendLine(info.debugName);
                builder.AppendLine(info.name);
                builder.AppendLine(info.bundleCrc);
                builder.AppendLine(info.depsCount.ToString());
                foreach (var dep in info.deps)
                {
                    builder.AppendLine(dep);
                }
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, builder.ToString());
        }
       
        [MenuItem("jenkins/表数据和代码/生成新版本dep")]
        static public void NewDepMenu()
        {
            GenerateNewDep();
        }
        /// <summary>
        /// 基于项目中的dep.all,生成新的dep.all.(表数据ab和dllabhash值变化)
        /// </summary>
        static public Dictionary<string, ABDepInfo> GenerateNewDep(string tmpABPath = "AssetBundles")
        {
            var infoMap = ReadDep(PathABSaveFolder + ABDepFile);
            if (Directory.Exists(tmpABPath))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(Environment.CurrentDirectory + "/" + PathABSaveFolder + "/AssetBundles");
                AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                var files = Directory.GetFiles(tmpABPath, "*.ab", SearchOption.AllDirectories);
                StringBuilder builder = new StringBuilder();
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (infoMap.TryGetValue(fileInfo.Name, out var abInfo))
                    {
                        //var newBundleCrc = manifest.GetAssetBundleHash(abInfo.name).ToString();
                        var newBundleCrc = GfxUtils.GetMD5Hash(file);
                        if (newBundleCrc != abInfo.bundleCrc)
                        {
                            builder.AppendLine($"{abInfo.name} {abInfo.bundleCrc} => {newBundleCrc}");
                            abInfo.bundleCrc = newBundleCrc;
                        }
                    }
                }
                Debug.Log(builder.ToString());
                ab.Unload(true);
                var filePath = $"{tmpABPath}/dep.all";
                SaveToFile(infoMap, filePath);
            }
            return infoMap;
        }
    }
}
