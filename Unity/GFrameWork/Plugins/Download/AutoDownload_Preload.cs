using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameFramework
{
    public sealed partial class AutoDownloadRes
    {
        /// <summary>
        /// 存放预先下载资源
        /// </summary>
        Queue<AssetBundleData> preloadQueue;
        /// <summary>
        /// 在后台缓慢下载资源
        /// </summary>
        /// <param name="assetpath"></param>
        public void DLInBackground(string assetpath)
        {
            if (!EnvUtils.IsEDITOR_CDN_ABMOD())
                return;
            if (null == preloadQueue)
            {
                preloadQueue = new Queue<AssetBundleData>();
            }
            var abName = HashUtil.Get(HashUtil.ConvertToABName(assetpath)) + ".ab";
            var infoMap = AssetBundleManager.Instance.depInfoReader.infoMap;
            if (infoMap.TryGetValue(abName, out var abdata))
            {
                if (!preloadQueue.Contains(abdata))
                {
                    preloadQueue.Enqueue(abdata);
                    if (null != abdata.dependencies && abdata.dependencies.Length > 0)
                    {
                        foreach (var depAbName in abdata.dependencies)
                        {
                            this.DLInBackgroundByAbName(depAbName);
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.Log($"### DLInBackground  {assetpath}=>{abName},在资源清单中找不到");
            }
        }
        private void DLInBackgroundByAbName(string abName)
        {
            var infoMap = AssetBundleManager.Instance.depInfoReader.infoMap;
            if (infoMap.TryGetValue(abName, out var abdata))
            {
                if (!preloadQueue.Contains(abdata))
                {
                    preloadQueue.Enqueue(abdata);
                    if (null != abdata.dependencies && abdata.dependencies.Length > 0)
                    {
                        foreach (var depAbName in abdata.dependencies)
                        {
                            this.DLInBackgroundByAbName(depAbName);
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.Log($"### DLInBackgroundByAbName  {abName},在资源清单中找不到");
            }

        }
    }
}
