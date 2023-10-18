using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace GameFramework
{
    /// <summary>
    /// 用于边玩边下载CDN上资源
    /// 1.获取未下载的资源
    /// 2.删除本地过时资源
    /// 3.获取CDN上资源配置文件（资源顺序文件）
    /// 3.边玩边下载剩余资源
    /// </summary>
    public sealed class AutoDownloadRes
    {
        private bool IsInit = false;
        static public AutoDownloadRes _inst;
        /// <summary>
        /// 存放需要下载的资源
        /// </summary>
        Queue<AssetBundleData> waitForDLQueue;
        /// <summary>
        /// 最大并发下载个数
        /// </summary>
        public int MAX_REQ = 5;
        private int NumOfBusyReq = 0;
        /// <summary>
        /// 剩余并发个数
        /// </summary>
        /// <returns></returns>
        public int GetNumOfIdleReq()
        {
            return MAX_REQ - NumOfBusyReq;
        }
        /// <summary>
        /// 当前并发个数
        /// </summary>
        /// <returns></returns>
        public int GetReqInUse()
        {
            return this.NumOfBusyReq;
        }
        /// <summary>
        /// 获取边玩边下资源个数
        /// </summary>
        /// <returns></returns>
        public int GetCoutOfALRes()
        {

            return this.waitForDLQueue.Count;
        }
        public void Incerease()
        {
            NumOfBusyReq += 1;
        }
        public void Decrease()
        {
            NumOfBusyReq -= 1;
        }
        private int NumOfDL = 0;
        public int GetNumOfDL()
        {
            return NumOfDL;
        }
        static AutoDownloadRes()
        {
            _inst = new AutoDownloadRes();
        }
        public AutoDownloadRes()
        {
            waitForDLQueue = new Queue<AssetBundleData>();
        }
        static public AutoDownloadRes Inst
        {
            get { return _inst; }
        }
        public bool IsAutoDL()
        {
            return DownloadThread.MAX_CO > 0 && null != waitForDLQueue && waitForDLQueue.Count > 0;
        }
        int nTry = 0;
        public void DownResCfg(string cfgUrl)
        {
            if (IsInit)
                return;
            UnityEngine.Debug.Log($"###  DownResCfg:{cfgUrl} MAX_REQ:{MAX_REQ}");
            if (MAX_REQ <= 0)
            {
                return;
            }
            if (nTry > 3)
            {
                return;
            }
            nTry += 1;
            var cacheDir = AssetBundleManager.Instance.pathResolver.BundleCacheDir;
            var savePath = cacheDir + "/clientrescfg.txt";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            Action<string, bool> OnCallBack = (a, b) =>
            {
                if (!b)
                {
                    var thread = new Thread(() =>
                    {
                        InitCfg(savePath);
                    });
                    thread.Priority = System.Threading.ThreadPriority.Lowest;
                    thread.Start();
                }
                else
                {
                    DownResCfg(cfgUrl);
                }
            };
            MyUnityWebRequest.Get(cfgUrl, savePath, string.Empty, OnCallBack, MyUnityWebRequest.REQ_PRIORITY.HIGHT);

        }
        Stopwatch dog = new Stopwatch();
        long tickSpan = TimeSpan.TicksPerSecond / 25;
        bool CheckAndSleep()
        {
            if (dog.ElapsedTicks > tickSpan)
            {
                Thread.Sleep(10);
                dog.Reset();
                return true;
            }
            return false;
        }
        private void InitCfg(string filePath)
        {
            var cacheDir = AssetBundleManager.Instance.pathResolver.BundleCacheDir;
            var infoMap = AssetBundleManager.Instance.depInfoReader.infoMap;
            #region 统计需要下载的资源
            var lines = File.ReadAllLines(filePath);
            StringBuilder builder = new StringBuilder();
            ProfilerUtil.BeginSample("InitCfg 1");
            for (int tindex = 0; tindex < lines.Length; tindex++)
            {
                CheckAndSleep();
                var line = lines[tindex];
                var bundleName = line.TrimEnd('\n').TrimEnd('\r');
                if (infoMap.TryGetValue(bundleName, out var info))
                {
                    if (AssetBundleManager.Instance.IsPackAsset(info.fullNameWithHash))
                    {
                        continue;
                    }
                    builder.Clear();
                    builder.Append(cacheDir);
                    builder.Append("/");
                    builder.Append(info.fullNameWithHash);
                    var savePath = builder.ToString();
                    if (File.Exists(savePath))
                    {
                        continue;
                    }
                    waitForDLQueue.Enqueue(info);

                }
            }
            ProfilerUtil.EndSample();
            ProfilerUtil.BeginSample("InitCfg 2");
            #endregion
            #region 删除过时资源
            var iter = infoMap.GetEnumerator();
            HashSet<string> vaildResDict = new HashSet<string>();
            while (iter.MoveNext())
            {
                CheckAndSleep();
                var info = iter.Current.Value;
                if (vaildResDict.Contains(info.fullNameWithHash))
                {
                    continue;
                }
                vaildResDict.Add(info.fullNameWithHash);
            }
            ProfilerUtil.EndSample();
            ProfilerUtil.BeginSample("InitCfg 3-1");
            var localABFiles = Directory.GetFiles(cacheDir, "*.ab", SearchOption.AllDirectories);
            ProfilerUtil.EndSample();
            List<string> dirtyResLst = new List<string>();
            ProfilerUtil.BeginSample("InitCfg 3-2");
            foreach (var file in localABFiles)
            {
                CheckAndSleep();
                var fileName = Path.GetFileName(file);
                if (!vaildResDict.Contains(fileName))
                {
                    dirtyResLst.Add(file);
                }
            }
            ProfilerUtil.EndSample();
            ProfilerUtil.BeginSample("InitCfg 4");

            var curVerDep = URLConfig.Instance.GetDebFileNameWhiVer(GfxLogicBrige.Instance.GameVersion);
            var depFiles = Directory.GetFiles(cacheDir, "*.all", SearchOption.AllDirectories);
            foreach (var f in depFiles)
            {
                if (!f.EndsWith(curVerDep))
                {
                    dirtyResLst.Add(f);
                }
            }
            //可能正在下载过程中的文件会被删除
            //var tmpFiles = Directory.GetFiles(cacheDir, "*.tmp", SearchOption.AllDirectories);
            //dirtyResLst.AddRange(tmpFiles);
            foreach (var file in dirtyResLst)
            {
                try
                {
                    CheckAndSleep();
                    File.Delete(file);

                }
                catch (Exception e)
                {
                    if (EnvUtils.IsUnity_Editor() || EnvUtils.IsDEVELOPMENT_BUILD())
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
            #endregion
            ProfilerUtil.EndSample();
            IsInit = true;
        }
        bool OpenDebug = false;
        private void OnDownFinish(string savePath, bool IsError)
        {
            NumOfDL += 1;
            Decrease();
            if (OpenDebug)
            {
                UnityEngine.Debug.LogError($"<color=green>fn {NumOfBusyReq} {savePath} {UnityEngine.Time.time}</color>");
            }
        }
        StringBuilder builder;
        /// <summary>
        /// 开始自动下载资源
        /// </summary>
        public void AutoDownCDNRes()
        {
            if (!IsInit || !IsAutoDL())
                return;
            if (null == builder)
            {
                builder = new StringBuilder();
            }
            
            if ((this.GetNumOfIdleReq() > 0 && (CanDLNextResByCo() || CanDLNextResByLoader())))
            {
                var info = waitForDLQueue.Dequeue();
                var cacheDir = AssetBundleManager.Instance.pathResolver.BundleCacheDir;
                string abUrl = AssetBundleManager.Instance.pathResolver.GetABUrl(info.fullNameWithHash);
                builder.Append(cacheDir);
                builder.Append("/");
                builder.Append(info.fullNameWithHash);
                MyUnityWebRequest.Get(abUrl, builder.ToString(), info.crc, OnDownFinish, MyUnityWebRequest.REQ_PRIORITY.BELOW_NORMAL, IsAutoDL:true);
                builder.Clear();
                if (OpenDebug)
                {
                    UnityEngine.Debug.LogError($"<color=yellow>ad {NumOfBusyReq} {abUrl} {UnityEngine.Time.time}</color>");
                }
                this.Incerease();
            }
        }
        public bool CanDLNextResByCo()
        {
            return !MyUnityWebRequest.IsUseLoader && DownloadThread.GetCntOfCo() <= MAX_REQ;
        }
        public bool CanDLNextResByLoader()
        {
            return MyUnityWebRequest.IsUseLoader && Downloader.GetBusyLoaderCount() <= MAX_REQ;
        }

        public string GetDebugMsg()
        {

            if (MyUnityWebRequest.IsUseLoader)
            {
                return $"{Downloader.GetDbugMsg()} AT:{GetReqInUse()}/{MAX_REQ}-{GetNumOfDL()}/{GetCoutOfALRes()}";

            }
            else
            {
                return $"{DownloadThread.GetDbugMsg()} AT:{GetReqInUse()}/{MAX_REQ}-{GetNumOfDL()}/{GetCoutOfALRes()}";
            }
        }
    }
}