using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameFramework.MyUnityWebRequest;
using UnityEngine.Networking;
using System.IO;
using UnityEngine;
using ILRuntime.Runtime.Generated;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace GameFramework
{
    /// <summary>
    /// 下载器
    /// </summary>
    internal class DownloadThread
    {
        static private Dictionary<string, bool> tmpPathDict;
        static Queue<MyUnityWebRequest> reqQueue;
        static List<DownloadThread> coLst;
        static Dictionary<string, List<MyUnityWebRequest>> busyReqDict;
        static DownloadThread()
        {
            reqQueue = new Queue<MyUnityWebRequest>();
            tmpPathDict = new Dictionary<string, bool>();
            coLst = new List<DownloadThread>();
            busyReqDict = new Dictionary<string, List<MyUnityWebRequest>>();
        }
        /// <summary>
        /// 协程最大个数
        /// </summary>
        public static int MAX_CO;
        /// <summary>
        /// 当前协程个数
        /// </summary>
        private int Id = -1;
        private static float lastCreateCoTime = 0;
        private float aliveTime = 10f;
        static public int GetCntOfCo()
        {
            return coLst.Count;
        }
        static public string GetDbugMsg()
        {
            return $"CO:{GetCntOfCo()}/{MAX_CO}-req:{reqQueue.Count}";
        }
        static public int GetCntOfReq()
        {
            return reqQueue.Count;
        }
        void Start()
        {
            Id = GetCntOfCo();
            //Debug.LogError($"<color=orange>### StartCo:{Id}</color>");
            AssetBundleManager.Instance.StartCoroutine(DownloadCo());
        }
        static public void Add(MyUnityWebRequest req)
        {
            reqQueue.Enqueue(req);
            int nCount = reqQueue.Count - GetCntOfCo();
            while (nCount-- > 0 && GetCntOfCo() < MAX_CO)
            {
                AddCo();
            }
        }
        static private void AddCo()
        {
            var thread = new DownloadThread();
            coLst.Add(thread);
            thread.Start();
        }
        static public void RemoveCO(DownloadThread co)
        {
            coLst.Remove(co);
        }
        static private void OnReqFinish(bool isHttpError, bool IsReDownload, string url)
        {

            if (busyReqDict.TryGetValue(url, out var lst))
            {
                foreach (var req in lst)
                {
                    req.OnDLFinish(isHttpError, IsReDownload, req);
                }
                busyReqDict.Remove(url);
            }
            else
            {
                Debug.LogError($"### 出现异常了******");
            }
        }
        static private bool CacheBusyReq(MyUnityWebRequest req)
        {
            bool IsInDownloading = busyReqDict.ContainsKey(req.url);
            if (!busyReqDict.TryGetValue(req.url, out var lst))
            {
                busyReqDict[req.url] = new List<MyUnityWebRequest>();
            }
            busyReqDict[req.url].Add(req);
            return IsInDownloading;
        }

        IEnumerator DownloadCo()
        {
            var deadTime = Time.time + aliveTime;
            while (true)
            {
                if (reqQueue.Count > 0)
                {
                    var req = reqQueue.Dequeue();
                    if (CacheBusyReq(req))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    var firstUrl = req.url;
                    var savePath = req.savePath;
                    var crc = req.crc;

                    var tmpPath = savePath + $".tmp{Time.frameCount}";
                    if (!File.Exists(savePath) && !tmpPathDict.ContainsKey(tmpPath))
                    {
                        UnityWebRequest wwwOrg = UnityWebRequest.Get(firstUrl);
                        tmpPathDict.Add(tmpPath, true);
                        try
                        {
                            wwwOrg.downloadHandler = new DownloadHandlerFile(tmpPath);
                        }
                        catch (Exception e)
                        {
                            if (EnvUtils.IsUnity_Editor() || EnvUtils.IsUnity_Editor())
                            {
                                Debug.LogError(e);
                            }
                        }
                        yield return wwwOrg.SendWebRequest();
                        var isHttpError = wwwOrg.isHttpError;
                        bool IsReDownload = false;
                        if (isHttpError)
                        {
                            Debug.LogError($"### error:{wwwOrg.error} url:{wwwOrg.uri} ");
                        }
                        else
                        {
                            try
                            {
                                var curCrc = GfxUtils.GetMD5Hash(tmpPath);
                                //资源清单没有crc
                                if (string.IsNullOrEmpty(crc) || GfxUtils.GetMD5Hash(tmpPath) == crc)
                                {
                                    if (!File.Exists(savePath))
                                    {
                                        File.Move(tmpPath, savePath);
                                    }
                                    else
                                    {
                                        File.Delete(tmpPath);
                                    }
                                }
                                else
                                {
                                    IsReDownload = true;
                                    Debug.LogError($"### {tmpPath} crc不同，重新下载!{curCrc},CND:{crc}");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                IsReDownload = true;
                            }
                        }
                        if ((isHttpError || IsReDownload) && File.Exists(tmpPath))
                        {
                            File.Delete(tmpPath);
                        }
                        tmpPathDict.Remove(tmpPath);

                        OnReqFinish(isHttpError, IsReDownload, req.url);
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                { 
                    yield return new WaitForEndOfFrame();
                }
                bool IsBreak = Time.time > deadTime && GetCntOfCo() > GetCntOfReq();
                IsBreak &= (!AutoDownloadRes.Inst.IsAutoDL() || AutoDownloadRes.Inst.IsAutoDL() && GetCntOfCo() > AutoDownloadRes.Inst.MAX_REQ);
                if (IsBreak)
                {
                    break;
                }
            }
            RemoveCO(this);
            //Debug.LogError($"<color=orange>### StopCo:{Id}</color>");
        }
    }
}
