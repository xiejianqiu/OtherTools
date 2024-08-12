using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace GameFramework
{
    public partial class MyUnityWebRequest
    {
        static private Queue<MyUnityWebRequest> RequestQueue;
        static private Queue<MyUnityWebRequest> idleRequestQueue;
        public enum REQ_PRIORITY
        {
            BELOW_NORMAL,
            NORMAL,
            HIGHT
        }
        public string url;
        public string crc;
        public string savePath;
        private Queue<Action<string, bool>> callBackList;

        private REQ_PRIORITY priority;
        public long frame;
        static MyUnityWebRequest()
        {
            RequestQueue = new Queue<MyUnityWebRequest>();
            idleRequestQueue = new Queue<MyUnityWebRequest>();
        }
        public MyUnityWebRequest()
        {
            callBackList = new Queue<Action<string, bool>>();
        }
        public void AddCallBack(Action<string, bool> callBack)
        {
            this.callBackList.Enqueue(callBack);
        }
        public Action<string, bool> DequeCallBack()
        {
            return this.callBackList.Dequeue();
        }
        public void SetFileInfo(string url,string crc, string savePath)
        {
            this.url = url;
            this.crc = crc;
            this.savePath = savePath;
        }
        public void SetPriority(REQ_PRIORITY priority)
        {
            this.priority = priority;
        }
        public REQ_PRIORITY GetPriority()
        {
            return this.priority;
        }
        public void SetFrame(long frame)
        {
            this.frame = frame;
        }
        public void Reset()
        {
            this.url = string.Empty;
            this.crc = string.Empty;
            this.savePath = string.Empty;
            this.priority = REQ_PRIORITY.NORMAL;
            this.IsAutoDL = false;
            this.nTryDL = 0;
            this.callBackList.Clear();
        }
        public void OnDLFinish(bool isHttpError, bool IsReDownload)
        {
            if (IsReDownload)
            {
                RequestQueue.Enqueue(this);
            }
            else
            {
                while (this.callBackList.Count > 0)
                {
                    var callBackFunc = this.DequeCallBack();
                    if (null != callBackFunc)
                    {
                        callBackFunc(savePath, isHttpError);
                    }
                }
                idleRequestQueue.Enqueue(this);
            }
        }

    }
}
