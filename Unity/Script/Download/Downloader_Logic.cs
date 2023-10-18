using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace GameFramework
{
    public partial class Downloader:IDisposable
    {
        static public int MAX_LOADER = 10;
        /// <summary>
        /// 空闲loader
        /// </summary>
        static Queue<Downloader> idleLoaderQueue;
        /// <summary>
        /// 繁忙laoder
        /// </summary>
        static List<Downloader> busyLoaderLst;
        /// <summary>
        /// 用于存放下载结束后的loader
        /// </summary>
        static Queue<Downloader> endLoaderLst;
        /// <summary>
        /// 需要下载的请求
        /// </summary>
        static Queue<MyUnityWebRequest> reqQueue;
        static public string GetDbugMsg()
        {
            return $"loader:{GetBusyLoaderCount()}-{GetLoaderCount()}/{MAX_LOADER}-req:{reqQueue.Count}/{idleLoaderQueue.Count}";
        }
        static public void Add(MyUnityWebRequest tRequest)
        {
            reqQueue.Enqueue(tRequest);
            int nCount = reqQueue.Count - idleLoaderQueue.Count;
            while (nCount-- > 0 && GetLoaderCount() < MAX_LOADER || GetLoaderCount() < AutoDownloadRes.Inst.MAX_REQ)
            {
                AddLoader();
            }
        }
        static void AddLoader()
        {
            idleLoaderQueue.Enqueue(new Downloader());
        }
        /// <summary>
        /// 下载结束，回收laoder
        /// </summary>
        /// <param name="loader"></param>
        static void Recle(Downloader loader)
        {
            endLoaderLst.Enqueue(loader);
        }
        static public int GetLoaderCount()
        {
            return idleLoaderQueue.Count + busyLoaderLst.Count + endLoaderLst.Count;
        }
        static public int GetBusyLoaderCount()
        {
            return busyLoaderLst.Count;
        }
        static void StartLoader(MyUnityWebRequest tRequest)
        {
            var loader = idleLoaderQueue.Dequeue();
            loader.Start(tRequest);
            busyLoaderLst.Add(loader);
        }
        static public void Update()
        {
            if (!IsUseUnityWebRequest && IsUseThread)
            {
                string exception = string.Empty;
                if (!IsThreadAlive)
                {
                    IsThreadAlive = true;
                    ThreadPool.QueueUserWorkItem((a) =>
                    {
                        while (IsThreadAlive)
                        {
                            try
                            {
                                foreach (var loader in busyLoaderLst)
                                {
                                    if (loader.status == DL_Status.WAIT_FOR_DOWNLOAD)
                                    {
                                        loader.UpdateLoaderStatus();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                exception = e.ToString();
                                IsThreadAlive = false;
                            }
                            Thread.Sleep(10);

                        }
                    });
                }
                if (!IsThreadAlive && !string.IsNullOrEmpty(exception))
                {
                    Debug.LogError(exception);
                }
                foreach (var loader in busyLoaderLst)
                {
                    if (loader.status != DL_Status.WAIT_FOR_DOWNLOAD)
                    {
                        loader.UpdateLoaderStatus();
                    }
                }

                if (reqQueue.Count > 0 && idleLoaderQueue.Count > 0)
                {
                    StartLoader(reqQueue.Dequeue());
                }
                while (endLoaderLst.Count > 0)
                {
                    var loader = endLoaderLst.Dequeue();
                    if (busyLoaderLst.Remove(loader))
                    {
                        idleLoaderQueue.Enqueue(loader);
                    }
                }
                return;
            }
            CheckAndUpdateLoaderStatus();
        }
        static private void CheckAndUpdateLoaderStatus()
        {
            foreach (var loader in busyLoaderLst)
            {
                loader.UpdateLoaderStatus();
            }

            if (reqQueue.Count > 0 && idleLoaderQueue.Count > 0)
            {
                StartLoader(reqQueue.Dequeue());
            }
            while (endLoaderLst.Count > 0)
            {
                var loader = endLoaderLst.Dequeue();
                if (busyLoaderLst.Remove(loader))
                {
                    idleLoaderQueue.Enqueue(loader);
                }
                else
                {
                    Debug.LogError("########### Loader管理异常 ############");
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
