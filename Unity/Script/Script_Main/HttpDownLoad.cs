using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public partial class HttpDownLoad:IDisposable
{
    public enum DownloadStep
    {
        None,
        Begin,
        Progress,
        Fail,
        Finish,
        End,
    }
    public float progress { get; private set; }

    public bool isDone { get; private set; }

    private bool isStop;
    private MonoBehaviour myHost;
    static private int MAX_WRITE_SIZE = 1024;
    public void Start(MonoBehaviour host, string url, string filePath, Action<bool, string> callBack, Action<float> OnProgressChange, Action<long, long, long> OnDownloadInfo = null) {
        this.myHost = host;
        host.StartCoroutine(StartNewCo(url, filePath, callBack, OnProgressChange, OnDownloadInfo));
    }

    private IEnumerator StartCo(string url, string filePath, Action<bool, string> callBack, Action<float> OnProgressChange, Action<long,long,long> OnDownloadInfo = null)
    {
        this.isStop = false;
        Debug.Log($"### download {url}");
        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        long fileLength = 0;
        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fileLength = fs.Length;
            //if (fileLength > MAX_WRITE_SIZE)
            //{
            //    fileLength -= MAX_WRITE_SIZE;
            //}
            //else {
            //    fileLength = 0;
            //}
            Debug.Log($"## BEGIN DOWNLOAD: {fileLength} {totalLength}");
            if (fileLength < totalLength)
            {
                fs.Seek(fileLength, SeekOrigin.Begin);

                var request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                request.SendWebRequest();

                var writedSize = 0;
                long tickSpan = 10000000 / 5;
                long lastTicks = DateTime.Now.Ticks;
                long lastSecondTicks = 0;
                long sizePerSecond = 0;
                while (!request.isDone || fileLength < totalLength)
                {
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        if (null != callBack)
                        {
                            callBack(false, request.error);
                        }
                        yield break;
                    }
                    if (isStop) break;
                    
                    yield return new WaitForEndOfFrame();
                    var buff = request.downloadHandler.data;
                    if (buff != null)
                    {
                        while (true)
                        {
                            var length = buff.Length - writedSize;
                            if (length > MAX_WRITE_SIZE) {
                                length = MAX_WRITE_SIZE;
                            }
                            if (length > 0)
                            {
                                fs.Write(buff, writedSize, length);
                                writedSize += length;
                                fileLength += length;
                                sizePerSecond += length;
                            }
                            else 
                            {
                                break;
                            }
                            if (fileLength == totalLength)
                            {
                                progress = 1f;
                            }
                            else
                            {
                                progress = fileLength / (float)totalLength;
                            }
                            if (DateTime.Now.Ticks - lastSecondTicks >= 10000000)
                            {
                                if (null != OnDownloadInfo)
                                {
                                    OnDownloadInfo(fileLength, totalLength, sizePerSecond);
                                    sizePerSecond = 0;
                                }
                                //yield return new WaitForEndOfFrame();
                                lastSecondTicks = DateTime.Now.Ticks;
                            }
                            else if (DateTime.Now.Ticks - lastTicks >= tickSpan)
                            {
                                if (null != OnProgressChange)
                                {
                                    OnProgressChange(progress);
                                }

                                //yield return new WaitForEndOfFrame();
                                lastTicks = DateTime.Now.Ticks;
                            }
                        }
                    }
                    //--资源下载到100M是重新建立连接，防止打资源把内存撑爆
                    if (fileLength != totalLength && writedSize > (1 << 20) * 100)
                    {
                        fs.Flush(true);
                        fs.Close();
                        fs.Dispose();
                        request.Dispose();
                        request = null;
                        isStop = true;
                        System.GC.Collect();
                        Debug.Log($"## Trt Redownload: {fileLength}/{totalLength} isStop:{isStop}");
                        new HttpDownLoad().Start(this.myHost, url, filePath, callBack, OnProgressChange, OnDownloadInfo);
                        yield break;
                    }
                }
                progress = 1f;
            }
            else
            {
                progress = 1f;
            }

            fs.Close();
            fs.Dispose();
        }
        Debug.Log($"## FINISH DOWNLOAD: {fileLength} {totalLength} isStop:{isStop}");
        if (fileLength == totalLength)
        {
            isDone = true;
            if (callBack != null)
            {
                callBack(true,"");
            }
        }
        else {
            {
                callBack(false, "");
            }
        }
    }
    private IEnumerator StartNewCo(string url, string filePath, Action<bool, string> callBack, Action<float> OnProgressChange, Action<long, long, long> OnDownloadInfo = null)
    {
        this.isStop = false;
        Debug.Log($"## BEGIN DOWNLOAD: totalLength: {totalLength} url:{url}");
        var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerFile(filePath);
        request.SendWebRequest();

        ulong lastdownloadBytes = 0;
        while (!request.isDone)
        {
            if (!string.IsNullOrEmpty(request.error))
            {
                if (null != callBack)
                {
                    callBack(false, request.error);
                }
                yield break;
            }
            if (isStop) break;
            progress = request.downloadProgress;
            if (DateTime.Now.Ticks - lastSecondTicks >= 10000000)
            {
                if (null != OnDownloadInfo)
                {
                    OnDownloadInfo((long)request.downloadedBytes, totalLength, (long)(request.downloadedBytes - lastdownloadBytes));
                    lastdownloadBytes = request.downloadedBytes;
                }
                lastSecondTicks = DateTime.Now.Ticks;
            }
            else if (DateTime.Now.Ticks - lastTicks >= tickSpan)
            {
                if (null != OnProgressChange)
                {
                    OnProgressChange(progress);
                }

                lastTicks = DateTime.Now.Ticks;
            }
            if (request.downloadProgress >= 1) {
                isStop = true;
            }
            yield return new WaitForEndOfFrame();
        }
        isDone = request.downloadProgress >= 1f;
        if (callBack != null)
        {
            callBack(isDone, "");
        }
    }

    public void Stop()
    {
        isStop = true;
    }

    public void Dispose()
    {
        this.myHost = null;
    }
}