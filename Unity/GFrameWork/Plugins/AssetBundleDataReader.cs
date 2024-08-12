using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameFramework
{
    public class AssetBundleData
    {
        public string fullName;
        public string debugName;
        public string fullNameWithHash;
        public string crc;
        public string[] dependencies;
        public bool isAnalyzed;
        public AssetBundleData[] dependList;
    }

    /// <summary>
    /// 文本文件格式说明
    /// *固定一行字符串ABDT
    /// 循环 { AssetBundleData
    ///     *名字(string)
    ///     *短名字(string)
    ///     *Hash值(string)
    ///     *类型(AssetBundleExportType)
    ///     *依赖文件个数M(int)
    ///     循环 M {
    ///         *依赖的AB文件名(string)
    ///     }
    /// }
    /// </summary>
    public class AssetBundleDataReader
    {
        public Dictionary<string, AssetBundleData> infoMap = new Dictionary<string, AssetBundleData>();

        protected Dictionary<string, string> shortName2FullName = new Dictionary<string, string>();

        public virtual void Read(Stream fs)
        {
            StreamReader sr = new StreamReader(fs);
            char[] fileHeadChars = new char[6];
            sr.Read(fileHeadChars, 0, fileHeadChars.Length);
            //读取文件头判断文件类型，ABDT 意思即 Asset-Bundle-Data-Text
            if (fileHeadChars[0] != 'A' || fileHeadChars[1] != 'B' || fileHeadChars[2] != 'D' || fileHeadChars[3] != 'T')
                return;
            StringBuilder builder = new StringBuilder();
            string bundleHashName = string.Empty;
            while (true)
            {
                string debugName = sr.ReadLine();
                if (string.IsNullOrEmpty(debugName))
                    break;

                string name = sr.ReadLine();
                string bundleCrc = sr.ReadLine();
                int depsCount = Convert.ToInt32(sr.ReadLine());
                string[] deps = new string[depsCount];

                //if (!shortName2FullName.ContainsKey(shortFileName))
                //    shortName2FullName.Add(shortFileName, name);
                for (int i = 0; i < depsCount; i++)
                {
                    deps[i] = sr.ReadLine();
                }
                builder.Remove(0, builder.Length);
                builder.Append(name.Substring(0, name.IndexOf(".")));
                builder.Append("_");
                builder.Append(bundleCrc);
                builder.Append(".ab");
                bundleHashName = builder.ToString();
                AssetBundleData info = new AssetBundleData();
                info.debugName = debugName;
                info.crc = bundleCrc;
                info.fullName = name;
                info.fullNameWithHash = bundleHashName;
                info.dependencies = deps;
                infoMap[name] = info;
            }

            sr.Close();
        }

        /// <summary>
        /// 分析生成依赖树
        /// </summary>
        public void Analyze()
        {
            var e = infoMap.GetEnumerator();
            while (e.MoveNext())
            {
                Analyze(e.Current.Value);
            }
            e.Dispose();
        }

        void Analyze(AssetBundleData abd)
        {
            if (!abd.isAnalyzed)
            {
                abd.isAnalyzed = true;
                abd.dependList = new AssetBundleData[abd.dependencies.Length];
                for (int i = 0; i < abd.dependencies.Length; i++)
                {
                    AssetBundleData dep = this.GetAssetBundleInfo(abd.dependencies[i]);
                    abd.dependList[i] = dep;
                    this.Analyze(dep);
                }
            }
        }

        public string GetFullName(string shortName)
        {
            string fullName = null;
            shortName2FullName.TryGetValue(shortName, out fullName);
            return fullName;
        }

        public AssetBundleData GetAssetBundleInfoByShortName(string shortName)
        {
            string fullName = GetFullName(shortName);
            if (fullName != null && infoMap.ContainsKey(fullName))
                return infoMap[fullName];
            return null;
        }

        public AssetBundleData GetAssetBundleInfo(string fullName)
        {
            if (fullName != null)
            {
                if (infoMap.ContainsKey(fullName))
                    return infoMap[fullName];
            }
            return null;
        }
    }
}