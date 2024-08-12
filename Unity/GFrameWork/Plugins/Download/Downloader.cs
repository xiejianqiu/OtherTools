using ILRuntime.Mono.Cecil.Cil;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework
{
    public enum DL_Status
    {
        IDLE,
        WAIT_FOR_DOWNLOAD,
        IN_DOWNLOADING,
        EXCEPTION,
        END
    }
    public partial class Downloader
    {
        static private Dictionary<string, bool> tmpPathDict;
        static Dictionary<string, List<MyUnityWebRequest>> busyReqDict;
        static int iCntOfDL = 0;
        static Downloader()
        {
            tmpPathDict = new Dictionary<string, bool>();
            busyReqDict = new Dictionary<string, List<MyUnityWebRequest>>();
            idleLoaderQueue = new Queue<Downloader>();
            busyLoaderLst = new List<Downloader>();
            endLoaderLst = new Queue<Downloader>();
            reqQueue = new Queue<MyUnityWebRequest>();
        }
        static public bool IsUseUnityWebRequest = true;
        static public bool IsThreadAlive = false;
        static public bool IsUseThread = false;
        private MyUnityWebRequest request;
        UnityWebRequest wwwOrg;
        private HttpClient client;
        string tmpPath;
        private DL_Status status;
        private bool isHttpError { get { return 200 != this.responseCode; } }
        private long responseCode;
        private void Start(MyUnityWebRequest tRequest)
        {
            responseCode = 200;
            this.request = tRequest;
            this.SetStatus(DL_Status.WAIT_FOR_DOWNLOAD);
        }
        private void End()
        {
            Recle(this);
            this.SetStatus(DL_Status.IDLE);
            this.request = null;
        }
        bool IsBusy()
        {
            return status != DL_Status.IDLE;
        }
        bool IsIdle()
        {
            return status == DL_Status.IDLE;
        }
        void SetStatus(DL_Status tStatus)
        {
            this.status = tStatus;
        }
        void UpdateLoaderStatus()
        {
            
            if (this.status == DL_Status.END)
            {
                this.ProcessResult();
                return;
            }
            if (this.status == DL_Status.IN_DOWNLOADING)
            {
                if (IsUseUnityWebRequest)
                {
                    if (null != wwwOrg)
                    {
                        if (this.status == DL_Status.EXCEPTION)
                        {
                            responseCode = -2;
                            this.SetStatus(DL_Status.END);
                        }
                        else if (wwwOrg.isDone)
                        {
                            responseCode = wwwOrg.responseCode;
                            this.SetStatus(DL_Status.END);
                        }
                        else
                        {
                            if (File.Exists(request.savePath))
                            {
                                this.SetStatus(DL_Status.END);
                            }
                        }
                        
                    }
                }
                return;
            }
            if (this.status == DL_Status.WAIT_FOR_DOWNLOAD)
            {
                BeginDownload();
                return;
            }
        }
        /// <summary>
        /// 开始下载
        /// </summary>
        void BeginDownload()
        {
            SetStatus(DL_Status.IN_DOWNLOADING);
            if (CacheBusyReq(request))
                return;
            if (IsUseUnityWebRequest)
            {
                DownlaodByUnityWebRequest();
            }
            else
            {
                DownloadByHttpClient();
            }
        }
        string GetTmpPath()
        { 
            //return request.savePath + $".{iCntOfDL++}.tmp";
            return request.savePath + $".tmp";
        }
        static public bool IsInDownload(string url)
        {
            return busyReqDict.ContainsKey(url);
        }
        void DownlaodByUnityWebRequest()
        {
            var firstUrl = request.url;
            var savePath = request.savePath;
            tmpPath = this.GetTmpPath(); ;
            if (!File.Exists(savePath) && !tmpPathDict.ContainsKey(tmpPath))
            {
                wwwOrg = UnityWebRequest.Get(firstUrl);
                tmpPathDict.Add(tmpPath, true);
                try
                {
                    wwwOrg.downloadHandler = new DownloadHandlerFile(tmpPath);
                }
                catch (Exception e)
                {
                    this.SetStatus(DL_Status.EXCEPTION);
                }
                wwwOrg.SendWebRequest();
            }
        }
        async void DownloadByHttpClient()
        {
            if (null == client)
            {
                client = new HttpClient();
            }
            var firstUrl = request.url;
            var savePath = request.savePath;
            tmpPath = this.GetTmpPath();
            
            if (!File.Exists(savePath) && !tmpPathDict.ContainsKey(tmpPath))
            {
                tmpPathDict.Add(tmpPath, true);
                var response = await client.GetAsync(firstUrl);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsByteArrayAsync();
                
                File.WriteAllBytes(tmpPath, responseBody);
                responseCode = (long)response.StatusCode;
                this.SetStatus(DL_Status.END);
            }
            else
            {
                if (File.Exists(savePath))
                {
                    Debug.LogError($"### {firstUrl},该文件已下载");
                    this.SetStatus(DL_Status.END);
                }
                else
                {
                    Debug.LogError($"### {firstUrl},下载异常");
                }
            }
        }
        /// <summary>
        /// 下载结束，处理资源
        /// </summary>
        public void ProcessResult()
        {
            bool IsReDownload = false;
            var savePath = request.savePath;
            var crc = request.crc;
            if (!isHttpError)
            {
                try
                {
                    if (!File.Exists(savePath))
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
                                Debug.LogError($"### ProcessResult {tmpPath}, 重复下载");
                                File.Delete(tmpPath);
                            }
                        }
                        else
                        {
                            IsReDownload = true;
                            responseCode = -1;
                        }
                    }
                    else
                    {//表示之前已有request下载完了
                        File.Delete(tmpPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    IsReDownload = true;
                }
            }
            else
            {
                try
                {
                    if (isHttpError&& File.Exists(tmpPath))
                    {
                        IsReDownload = true;
                        File.Delete(tmpPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    IsReDownload = true;
                }
            }
            tmpPathDict.Remove(tmpPath);

            #region 测试下载次数
            //IsReDownload = false;
            //responseCode = -1;
            //Debug.LogError($" {tmpPath}  {this.request.url}");
            //if(File.Exists(savePath))
            //{ 
            //    File.Delete(savePath);
            //}
            #endregion

            if (isHttpError && !IsReDownload)
            {
                if (404 != responseCode)
                {
                    if (this.request.AddCntAndCheck())
                    {
                        IsReDownload = true;
                    }
                    else
                    {
                        var tip = string.Format(UpdateTips.GetTipByID(414), $" [00ff00]tip:{responseCode}\n{request.url}[-]");
                        GfxLogicBrige.Instance.ShowTips(tip);
                        GfxLogicBrige.Instance.ShowTipsForScriptMain(tip);
                    }
                }
                else
                {
                    if (EnvUtils.IsDEVELOPMENT_BUILD() || EnvUtils.IsUNITY_ANDROID_TEMP())
                    {
                        Debug.LogError($"{responseCode} {request.url}");
                    }
                }
            }
            if (isHttpError || IsReDownload)
            { 
                Debug.LogError($"code:{responseCode} pripority:{request.GetPriority()}, {request.url}");
            }
            if (!OnReqFinish(isHttpError, IsReDownload, request.url))
            {
                request.OnDLFinish(isHttpError, IsReDownload);
            }

            this.End();
        }
        static private bool OnReqFinish(bool isHttpError, bool IsReDownload, string url)
        {

            if (busyReqDict.TryGetValue(url, out var lst))
            {
                foreach (var req in lst)
                {
                    req.OnDLFinish(isHttpError, IsReDownload);
                }
                busyReqDict.Remove(url);
                return true;
            }
            return false;
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
    }
}
