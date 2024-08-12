using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace GameFramework
{
    public partial class MyUnityWebRequest
    {
        public static int MAX_REQUEST = 100;
        public bool IsAutoDL = false;
        /// <summary>
        /// 下载失败重试参数
        /// </summary>
        private int nTryDL = 0;
        /// <summary>
        /// 判断是否可以重试下载失败的资源
        /// </summary>
        /// <returns></returns>
        public bool AddCntAndCheck()
        {
            return ++nTryDL <= 3;
        }

        /// <summary>
        /// 获取排队下载的资源个数
        /// </summary>
        /// <returns></returns>
        public static string GetCountOfNowDL()
        {
            return $"NDL:{RequestQueue.Count}-Ireq:{MyUnityWebRequest.idleRequestQueue.Count}";
        }
        static string _timestamp = string.Empty;
        static string STimeStamp
        {
            get
            {
                if (string.IsNullOrEmpty(_timestamp))
                {
                    _timestamp = "?" + System.DateTime.Now.Ticks.ToString();
                }
                return _timestamp;
            }
        }
        public static string AddTimestampToUrl(string url)
        {
            return url + STimeStamp;
        }
        static public void Get(string url, string savePath, string crc, Action<string, bool> OnCallBack, REQ_PRIORITY priority = REQ_PRIORITY.NORMAL,bool IsAutoDL = false)
        {
            if (IsAutoDL && Downloader.IsInDownload(url))
            {
                OnCallBack?.Invoke(savePath, true);
                return;
            }
            if (File.Exists(savePath))
            {
                OnCallBack?.Invoke(savePath, false);
            }
            else
            {
                string myURL = AddTimestampToUrl(url);
                var request = idleRequestQueue.Count > 0 ? idleRequestQueue.Dequeue() : new MyUnityWebRequest();
                request.Reset();
                request.SetFileInfo(myURL, crc, savePath);
                request.AddCallBack(OnCallBack);
                request.SetPriority(priority);
                request.SetFrame(Time.frameCount);
                request.IsAutoDL = IsAutoDL;
                RequestQueue.Enqueue(request);
            }
        }
        static private List<MyUnityWebRequest> tmpLst;
        static public bool IsUseLoader = true;
        static public void Update()
        {
            if (null == tmpLst)
            {
                tmpLst = new List<MyUnityWebRequest>();
            }
            ProfilerUtil.BeginSample("MyUnityWebRequest.Update");
            tmpLst.Clear();
            while (RequestQueue.Count > 0)
            {
                tmpLst.Add(RequestQueue.Dequeue());
            }
            if (tmpLst.Count > 0)
            {
                tmpLst.Sort(Sort);
                for (int index = 0; index < tmpLst.Count; index++)
                {
                    var request = tmpLst[index];
                    if (IsUseLoader)
                    {
                        Downloader.Add(request);
                    }
                    else
                    {
                        DownloadThread.Add(request);
                    }
                }
            }
            AutoDownloadRes.Inst.AutoDownCDNRes();
            Downloader.Update();
            ProfilerUtil.EndSample();
        }
        static private int Sort(MyUnityWebRequest rq1, MyUnityWebRequest rq2)
        {
            if (null != rq1 && null != rq2)
            {
                var numOfRq1 = (long)rq1.priority * 1000000000;
                var numOfRq2 = (long)rq2.priority * 1000000000;
                return numOfRq1 > numOfRq2 ? -1 : 1;
            }
            return 0;

        }
    }
}
