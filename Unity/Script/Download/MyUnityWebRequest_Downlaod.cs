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
            return $"NDL:{mDownlaodDict.Count}-Ireq:{MyUnityWebRequest.idleRequestQueue.Count}";
        }
        public static string AddTimestampToUrl(string url)
        {
            return url + "?" + System.DateTime.Now.Ticks.ToString();
        }
        static public void Get(string url, string savePath, string crc, Action<string, bool> OnCallBack, REQ_PRIORITY priority = REQ_PRIORITY.NORMAL,bool IsAutoDL = false)
        {
            if (File.Exists(savePath))
            {
                OnCallBack?.Invoke(savePath, false);
            }
            else
            {
                string myURL = AddTimestampToUrl(url);
                //string myURL = url;
                if (!mDownlaodDict.ContainsKey(myURL))
                {
                    mDownlaodDict.Add(myURL, idleRequestQueue.Count > 0 ? idleRequestQueue.Dequeue() : new MyUnityWebRequest());
                    mDownlaodDict[myURL].IsAutoDL = false;    
                    mDownlaodDict[myURL].nTryDL= 0;
                }
                if (!mDownlaodDict[myURL].IsAutoDL && IsAutoDL)
                {
                    mDownlaodDict[myURL].IsAutoDL = true;
                }
                mDownlaodDict[myURL].AddCallBack(OnCallBack);
                mDownlaodDict[myURL].SetPriority(priority);
                mDownlaodDict[myURL].SetFrame(Time.frameCount);
                mDownlaodDict[myURL].SetFileInfo(myURL, crc, savePath);
            }
        }
        static private List<MyUnityWebRequest> requestLst;
        static public bool IsUseLoader = true;
        static public void Update()
        {
            if (null == requestLst)
            {
                requestLst = new List<MyUnityWebRequest>();
            }
            ProfilerUtil.BeginSample("MyUnityWebRequest.Update");
            requestLst.Clear();
            var iter = mDownlaodDict.GetEnumerator();
            while (iter.MoveNext())
            {
                requestLst.Add(iter.Current.Value);
            }
            if (requestLst.Count > 0)
            {
                requestLst.Sort(Sort);
                for (int index = 0; index < requestLst.Count; index++)
                {
                    var request = requestLst[index];
                    mDownlaodDict.Remove(request.url);
                    if (IsUseLoader)
                    {
                        Downloader.Add(request);
                    }
                    else
                    {
                        DownloadThread.Add(request);
                    }
                }
                requestLst.Clear();
            }
            AutoDownloadRes.Inst.AutoDownCDNRes();
            Downloader.Update();
            ProfilerUtil.EndSample();
        }
        static private int Sort(MyUnityWebRequest rq1, MyUnityWebRequest rq2)
        {
            if (null != rq1 && null != rq2)
            {
                var numOfRq1 = (long)rq1.priority * 1000000000 + rq1.frame;
                var numOfRq2 = (long)rq2.priority * 1000000000 + rq2.frame;
                return numOfRq1 > numOfRq2 ? -1 : 1;
            }
            return 0;

        }
    }
}
