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
        static private Dictionary<string, MyUnityWebRequest> mDownlaodDict;
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
            mDownlaodDict = new Dictionary<string, MyUnityWebRequest>();
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
        public void SetFrame(long frame)
        {
            this.frame = frame;
        }
        public void OnDLFinish(bool isHttpError, bool IsReDownload, MyUnityWebRequest request)
        {
            if (IsReDownload)
            {
                mDownlaodDict[request.url] = request;
            }
            else
            {
                while (request.callBackList.Count > 0)
                {
                    var callBackFunc = request.DequeCallBack();
                    if (null != callBackFunc)
                    {
                        callBackFunc(savePath, isHttpError);
                    }
                }
                idleRequestQueue.Enqueue(request);
            }
        }

    }
}
