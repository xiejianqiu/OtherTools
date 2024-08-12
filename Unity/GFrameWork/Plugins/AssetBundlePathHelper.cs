using System.IO;
using UnityEngine;

namespace GameFramework
{ 
    /// <summary>
    /// AB 打包及运行时路径解决器
    /// </summary>
    public class AssetBundlePathHelper
    {
        public static AssetBundlePathHelper instance;

        public AssetBundlePathHelper()
        {
            instance = this;
        }

        /// <summary>
        /// AB 保存的路径相对于 Assets/StreamingAssets 的名字
        /// </summary>
        public virtual string BundleSaveDirName { get { return "AssetBundles"; } }

        public static string NewBundleSavePath = string.Empty;
        /// <summary>
        /// AB 保存的路径
        /// </summary>
        public string BundleSavePath { get {
                if (!string.IsNullOrEmpty(NewBundleSavePath))
                {
                    return NewBundleSavePath;
                }
                return "Assets/StreamingAssets/" + BundleSaveDirName; 
        } }
        /// <summary>
        /// AB打包的原文件HashCode要保存到的路径，下次可供增量打包
        /// </summary>
        public virtual string HashCacheSaveFile { get { return "Assets/AssetBundles/cache.txt"; } }
        /// <summary>
        /// 在编辑器模型下将 abName 转为 Assets/... 路径
        /// 这样就可以不用打包直接用了
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public virtual string GetEditorModePath(string abName)
        {
            //将 Assets.AA.BB.prefab 转为 Assets/AA/BB.prefab
            abName = abName.Replace(".", "/");
            int last = abName.LastIndexOf("/");

            if (last == -1)
                return abName;

            string path = string.Format("{0}.{1}", abName.Substring(0, last), abName.Substring(last + 1));
            return path;
        }


        /// <summary>
        /// 获取 AB 源文件路径（打包进安装包的）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="forWWW"></param>
        /// <returns></returns>
        public virtual string GetBundleSourceFile(string path, bool forWWW = true)
        {
            string filePath = null;
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        if (forWWW)
                            filePath = string.Format("file://{0}/StreamingAssets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
                        else
                            filePath = string.Format("{0}/StreamingAssets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        if (forWWW)
                            filePath = string.Format("jar:file://{0}!/assets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
                        else
                            filePath = string.Format("{0}!assets/{1}/{2}", Application.dataPath, BundleSaveDirName, path);
                    }
                    break;
                default:
                    {
                        if(forWWW)
                        {
                            filePath = string.Format("file://{0}/{1}/{2}", Application.streamingAssetsPath, BundleSaveDirName, path);
                        }
                        else
                        {
                            filePath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, BundleSaveDirName, path);
                        }                        
                    }
                    break;
            }
            return filePath;
        }

        /// <summary>
        /// AB 依赖信息文件名
        /// </summary>
        public virtual string DependFileName { get { return "dep.all"; } }
        /// <summary>
        /// 包内资源清单
        /// </summary>
        public virtual string ClientResName {
            get
            {
                return "client.txt";
            }
        }

        /// <summary>
        /// 用于缓存AB的目录，要求可写
        /// </summary>
        public string BundleCacheDir = "";

        /// <summary>
        /// AB在CDN上的存放地址
        /// </summary>
        public string WEBGL_AB_CDN = "";
        public string GetABUrl(string strURL)
        {
            return WEBGL_AB_CDN + strURL;
        }
    }
}