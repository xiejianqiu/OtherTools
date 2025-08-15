using GameFramework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;
using System;
using System.Text;
using Shark;
using System.Security.Cryptography;
using System.Security.Policy;

public partial class ResTool
{
    /// <summary>
    /// 客户端收集到的资源的存放类路径
    /// </summary>
	static string editor_collect_res = "collect_editor_res.txt";
    /// <summary>
    /// 不放在apk包里的资源（乐变需要的配置文件）
    /// </summary>
    static string lb_origin_cfg = "lb_origin_cfg_#platform#_.txt";
    /// <summary>
    /// 存放在apk包里的资源
    /// </summary>
    static public string lb_inapk_ab = "lb_inapk_ab.txt";
    /// <summary>
    /// 等级，用于根据等级筛选出包内资源
    /// </summary>
    static string lb_inapkres_level = "lb_inapkres_level.txt";
    /// <summary>
    /// 用于自定义需要存放到apk包里的资源
    /// </summary>
    /// <returns></returns>
    static string lb_pkg_res = "lb_pkg_res.txt";
    /// <summary>
    /// 构建成AB的小资源
    /// </summary>
    static string lb_small_res = "lb_in_pkg_small_res.txt";
    /// <summary>
    /// 存放小的ab资源名称
    /// </summary>
    static public string lb_small_res_ab = "lb_in_pkg_small_ab.txt";
    private class ERes
    {
        public string src;
        public string ab;
        public string lv;
        public string frameCnt;
    }
    private class EditorResInfo
    {
        public List<ERes> InApkRes;
    }
    private class AssetInfo
    {
        public string debugName;
        public string abName;
        public HashSet<string> deps;

        public AssetInfo(string _debugName, string _abName)
        {
            this.debugName = _debugName;
            this.abName = _abName;
            this.deps = new HashSet<string>();
        }
        public void AddDep(string _abName)
        {
            this.deps.Add(_abName);
        }
    }
    private static Dictionary<string, ERes> sEditorResDict;
    private static string dl_missing_content = string.Empty;
    private static bool IsLoadMissingContent = false;
    /// <summary>
    /// 收集apk包内资源
    /// </summary>
    /// <param name="assetPath"></param>
    public static void RecordRes(string assetPath)
    {
        if (!EnvUtils.IsCollectEditorRes() || !EnvUtils.IsUnity_Editor())
        {
            return;
        }
        if (!IsLoadMissingContent && string.IsNullOrEmpty(dl_missing_content))
        {
            dl_missing_content = GetDLMissingTxt();
            IsLoadMissingContent = true;
        }
        if (null == sEditorResDict) sEditorResDict = new Dictionary<string, ERes>();
        if (sEditorResDict.Count <= 0 && File.Exists(editor_collect_res))
        {
            var json = File.ReadAllText(editor_collect_res);
            EditorResInfo cr = JsonMapper.ToObject<EditorResInfo>(json);
            foreach (var info in cr.InApkRes)
            {
                sEditorResDict[info.src] = info;
            }
        }
        string src = assetPath.Replace("/", "\\");
        if (sEditorResDict.ContainsKey(src))
            return;
        string lv = ChanConnector.CacheRoleLevel;
        string frameCnt = Time.frameCount.ToString();
        sEditorResDict.Add(src, new ERes() { src = src, ab = GetAbName(assetPath), lv = lv, frameCnt = frameCnt });
    }
    /// <summary>
    /// 把资源路径转化为ab名称
    /// </summary>
    /// <param name="srcPath"></param>
    /// <returns></returns>
    private static string GetAbName(string srcPath)
    {
        var abname = srcPath.Substring(0, srcPath.LastIndexOf(".")).ToLower().Replace("/", "\\");
        abname = HashUtil.Get(HashUtil.ConvertToABName(abname)) + ".ab";
        return abname;
    }
    /// <summary>
    /// 将收集到的资源保持到文件中
    /// </summary>
    public static void FlushResToFile()
    {
        if (!EnvUtils.IsCollectEditorRes())
        {
            return;
        }
        if (sEditorResDict.Count <= 0)
        {
            return;
        }
        if (File.Exists(editor_collect_res))
        {
            File.Delete(editor_collect_res);
        }
        using (FileStream fs = File.Open(editor_collect_res, FileMode.OpenOrCreate))
        {
            StreamWriter writer = new StreamWriter(fs);
            JsonData jd = new JsonData();
            jd["InApkRes"] = new JsonData();
            var rootJd = jd["InApkRes"];
            foreach (var kv in sEditorResDict)
            {
                JsonData tmpJd = new JsonData();
                tmpJd["src"] = kv.Value.src;
                tmpJd["ab"] = kv.Value.ab;
                tmpJd["lv"] = kv.Value.lv;
                tmpJd["frameCnt"] = kv.Value.frameCnt;
                rootJd.Add(tmpJd);
            }
            writer.WriteLine(jd.ToJson());
            writer.Flush();
        }
    }
    /// <summary>
    /// 生成分出去的文件列表配置文件配置文件，即不放在apk包里的资源列表(所有资源去除collect_editor_res.txt中的资源)
    /// </summary>
    public static void GenerateLBCfg()
    {
        if (!File.Exists(editor_collect_res))
        {
            return;
        }
        //获取apk包内资源配置文件
        var json = File.ReadAllText(editor_collect_res);
        EditorResInfo cr = JsonMapper.ToObject<EditorResInfo>(json);
        int Lv = 0;
        if (File.Exists(lb_inapkres_level))
        {
            string content = File.ReadAllText(lb_inapkres_level);
            int.TryParse(content.Trim(), out Lv);
        }

        //获取工程内ab资源清单
        Dictionary<string, AssetInfo> abdetailDict = new Dictionary<string, AssetInfo>();//key为debugname
        ReadAbdetailInfo(abdetailDict, Application.dataPath + "/abdetail.info");

        #region 根据ab资源清单和apk包内配置文件，计算出乐变originfile配置文件
        HashSet<string> InApkResSet = new HashSet<string>();//存放的需要放在apk中的资源(debugName)
        HashSet<string> lbCdnResSet = new HashSet<string>();
        #region 强制将lb_pkg_res文件中的资源防放到apk中的资源
        string pkg_res_content = File.ReadAllText(lb_pkg_res);
        string[] resArray = pkg_res_content.Split('\n','\r');
        foreach (var res in resArray)
        {
            if (res.StartsWith("#!"))
                continue;
            string src = res.Trim();
            if (!string.IsNullOrEmpty(src))
            {
                cr.InApkRes.Add(new ERes() { src = src, lv = "0" });
            }
        }
        #endregion
        cr.InApkRes.Sort((a, b)=>{ return int.Parse(a.lv) - int.Parse(b.lv); });
        foreach (var info in cr.InApkRes)
        {
            if (int.Parse(info.lv) <= Lv)
            {
                CollectDepRes(info.src.ToLower(), abdetailDict, InApkResSet);
            }
            else
            {
                CollectDepRes(info.src.ToLower(), abdetailDict, lbCdnResSet);
            }    
        }
        #region ab文件小的添加到包体内
        string small_res_content = File.ReadAllText(lb_small_res);
        string[] smallResArray = small_res_content.Split('\n', '\r');
        foreach (var res in smallResArray)
        {
            if (res.StartsWith("#!"))
                continue;
            string src = res.Trim();
            InApkResSet.Add(res);
        }
        #endregion
        HashSet<string> InApkAbSet = new HashSet<string>();
        foreach (var src in InApkResSet)
        {
            if (abdetailDict.TryGetValue(src, out var info))
            {
                InApkAbSet.Add(info.abName);
                lbCdnResSet.Remove(src);
            }
            else
            {
                Debug.LogError($"{src} 在abdetail中未找到");
            }
        }

        HashSet<string> originSet = new HashSet<string>();//存放的时ab名称
        foreach (var debugName in lbCdnResSet)
        {
            if (abdetailDict.TryGetValue(debugName, out var info))
            {
                originSet.Add(info.abName);
            }
        }
        var iter = abdetailDict.GetEnumerator();
        while (iter.MoveNext())
        {
            if (!InApkAbSet.Contains(iter.Current.Value.abName))
            {
                originSet.Add(iter.Current.Value.abName);
                foreach (var debugName in iter.Current.Value.deps)
                {
                    if (abdetailDict.TryGetValue(debugName, out var info))
                    {
                        if (!InApkAbSet.Contains(info.abName))
                        {
                            originSet.Add(info.abName);
                        }
                    }
                    else
                    {
                        Debug.LogError($"{debugName} 在abdetail中未找到");
                    }
                }
            }
        }
        #endregion
        string[] platforms = new string[] { "android", "ios" };
        foreach (var platform in platforms)
        {
            string platform_orgin_cfg = lb_origin_cfg.Replace("#platform#", platform);
            if (File.Exists(platform_orgin_cfg))
                File.Delete(platform_orgin_cfg);

            File.WriteAllText(platform_orgin_cfg, GetString(platform, originSet));
            File.WriteAllText(lb_inapk_ab, GetString("", InApkAbSet));

            float InApkAbSetSize = CalAbSize(InApkAbSet) * 1f / 1024 / 1024;
            float originSetSize = CalAbSize(originSet) * 1f / 1024 / 1024;
            Debug.Log($"<color=green>乐变orgin配置文件生成完毕! paltform:{platform} Lv:{Lv} InApk:{InApkAbSetSize}M LB_CDN:{originSetSize}M</color>");
        }
    }
    private static long CalAbSize(HashSet<string> set)
    {
        long InApkResSize = 0;
        string PathABSaveFolder = Environment.CurrentDirectory + "/StreamingAssets/AssetBundles/";
        foreach (var abName in set)
        {
            string abfile = PathABSaveFolder + abName;
            if (File.Exists(abfile))
            {
                InApkResSize += new FileInfo(abfile).Length;
            }
            else
            {
                Debug.Log($"<color=blue>{abfile} 找不到</color>");
            }
        }
        return InApkResSize;
    }
    private static string GetString(string platfrom, HashSet<string> set)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var ab in set)
        {
            if (platfrom == "android")
            {
                builder.Append("assets/AssetBundles/");
            }
            else if (platfrom == "ios")
            {
                builder.Append("Data/Raw/AssetBundles/");
            }
            builder.AppendLine(ab);
        }
        return builder.ToString();
    }
    /// <summary>
    /// 收集ab依赖文件
    /// </summary>
    /// <param name="src"></param>
    /// <param name="abdetailDict"></param>
    /// <param name="set"></param>
    private static void CollectDepRes(string src, Dictionary<string, AssetInfo> abdetailDict, HashSet<string> set)
    {
        if (set.Contains(src))
            return;
        if (abdetailDict.TryGetValue(src, out var aInfo))
        {
            set.Add(src);
            foreach (var depSrc in aInfo.deps)
            {
                CollectDepRes(depSrc.ToLower(), abdetailDict, set);
            }
        }
        else
        {
            Debug.LogError($"<color=yellow>CollectDepRes {src} 在abdetail中未找到</color>");
        }
    }
    /// <summary>
    /// 通过abdetail获取工程内ab资源清单
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="path"></param>
    private static void ReadAbdetailInfo(Dictionary<string, AssetInfo> dic, string path,bool keyIsdebugName = true)
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
            if (keyIsdebugName)
            {
                debugName = debugName.ToLower();
            }
            string name = sr.ReadLine();
            string shortFileName = sr.ReadLine();
            string hash = sr.ReadLine();
            int typeData = Convert.ToInt32(sr.ReadLine());
            int depsCount = Convert.ToInt32(sr.ReadLine());
            string[] deps = new string[depsCount];
            string dictkey = keyIsdebugName ? debugName : name;
            if (!dic.ContainsKey(name))
            {
                dic[dictkey] = new AssetInfo(debugName, name);
            }
            for (int i = 0; i < depsCount; i++)
            {
                deps[i] = sr.ReadLine();
                dic[dictkey].deps.Add(deps[i].ToLower());
            }
        }

        sr.Close();
        fs.Close();
    }
    public static string GetDLMissingTxt()
    { 
        return File.ReadAllText("lb_downloadMissing.txt");
    }
    public static void PaseABNameToDebugName()
    {
        var downloadmissing = File.ReadAllText("lb_downloadMissing.txt");
        string[] nameArray = downloadmissing.Split(new char[] { '\n','\r'});
        Dictionary<string, AssetInfo> abdetailDict = new Dictionary<string, AssetInfo>();//key为debugname
        ReadAbdetailInfo(abdetailDict, Application.dataPath + "/abdetail.info", false);
        HashSet<string> missingSet = new HashSet<string>();
        foreach (var abName in nameArray)
        {
            string newAbName = abName.Trim();
            if (string.IsNullOrEmpty(newAbName))
            {
                continue;
            }
            if (abdetailDict.TryGetValue(newAbName, out var info))
            {
                missingSet.Add(info.debugName);
            }
            else
            {
                
                Debug.LogError($"{abName} 在abdetail中找不到");
            }
        }
        StringBuilder builder = new StringBuilder();
        foreach (var debugname in missingSet)
        {
            builder.AppendLine(debugname);
        }
        File.WriteAllText("lb_downloadMissing_debug.txt", builder.ToString());
    }
    /// <summary>
    /// 收集小于10kb的资源
    /// </summary>
    public static void CollectSmallAB()
    {
        Dictionary<string, AssetInfo> abdetailDict = new Dictionary<string, AssetInfo>();//key为debugname
        ReadAbdetailInfo(abdetailDict, Application.dataPath + "/abdetail.info");
        var ABSavePath = Environment.CurrentDirectory + "/StreamingAssets/AssetBundles/";
        long size = 30 * 1024;
        var iter = abdetailDict.GetEnumerator();
        StringBuilder builder = new StringBuilder();
        long allSize = 0;
        int nCnt = 0;
        HashSet<string> smallResSet = new HashSet<string>();
        while (iter.MoveNext())
        {
            var abName = iter.Current.Value.abName;
            var abPath = ABSavePath + abName;
            if (File.Exists(abPath))
            {
                var fileInfo = new FileInfo(abPath);
                if (fileInfo.Length < size)
                {
                    builder.AppendLine(iter.Current.Key);
                    smallResSet.Add(abName);
                    allSize += fileInfo.Length;
                    nCnt += 1;
                }
            }
        }
        File.WriteAllText(lb_small_res, builder.ToString(), Encoding.UTF8);
        builder.Clear();
        foreach (var fn in smallResSet)
        {
            builder.AppendLine(fn);
        }
        File.WriteAllText(lb_small_res_ab, builder.ToString(), Encoding.UTF8);

        Debug.LogError($"CollectSmallAB： {(allSize * 1f / 1024 / 1024)}M {nCnt}");
    }
}
