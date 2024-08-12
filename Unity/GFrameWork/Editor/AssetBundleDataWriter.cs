using System.Collections.Generic;
using System.IO;

namespace GameFramework
{
    public class AssetBundleDataWriter
    {
        public void Save(string path, AssetTarget[] targets)
        {
            FileStream fs = new FileStream(path, FileMode.CreateNew);
            Save(fs, targets);
        }

        public void SaveDetail(string path, AssetTarget[] targets)
        {
            FileStream fs = new FileStream(path, FileMode.CreateNew);
            SaveDetail(fs, targets);
        }

        public virtual void Save(Stream stream, AssetTarget[] targets)
        {
            StreamWriter sw = new StreamWriter(stream);
            //写入文件头判断文件类型用，ABDT 意思即 Asset-Bundle-Data-Text
            sw.WriteLine("ABDT");
            Dictionary<string, int> strDic = new Dictionary<string, int>();
            for (int i = 0; i < targets.Length; i++)
            {
                AssetTarget target = targets[i];
                if (strDic.ContainsKey(target.assetPath))
                {
                    continue;
                }
                strDic.Add(target.assetPath, i);
                HashSet<AssetTarget> deps = new HashSet<AssetTarget>();
                target.GetDependencies(deps);

                //debug name
                sw.WriteLine(target.assetPath);
                //bundle name
                sw.WriteLine(target.bundleName);
                //bundle hash
                sw.WriteLine(target.bundleCrc);
                //写入依赖信息
                sw.WriteLine(deps.Count);

                List<string> lst = new List<string>();
                
                foreach (AssetTarget item in deps)
                {
                    lst.Add(item.bundleName);
                }

                lst.Sort();
                for(int idx = 0; idx < lst.Count; idx++)
                {
                    sw.WriteLine(lst[idx]);
                }
            }
            sw.Close();
        }

        void SaveDetail(Stream stream, AssetTarget[] targets)
        {
            StreamWriter sw = new StreamWriter(stream);
            //写入文件头判断文件类型用，ABDT 意思即 Asset-Bundle-Data-Text
            sw.WriteLine("ABDT");
            Dictionary<string, int> strDic = new Dictionary<string, int>();
            for (int i = 0; i < targets.Length; i++)
            {                
                AssetTarget target = targets[i];
                if (strDic.ContainsKey(target.assetPath))
                {
                    continue;
                }
                strDic.Add(target.assetPath,i);
                HashSet<AssetTarget> deps = new HashSet<AssetTarget>();
                target.GetDependencies(deps);

                //debug name
                sw.WriteLine(target.assetPath);
                //bundle name
                sw.WriteLine(target.bundleName);
                //File Name
                sw.WriteLine(target.bundleShortName);
                //hash
                sw.WriteLine(target.bundleCrc);
                //type
                sw.WriteLine((int)target.compositeType);
                //写入依赖信息
                sw.WriteLine(deps.Count);

                foreach (AssetTarget item in deps)
                {
                    sw.WriteLine(item.assetPath);
                }
            }
            sw.Close();
        }
    }
}