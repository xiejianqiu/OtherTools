using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public partial class HttpDownLoad
{
    public static UnityWebRequest myDataRequest { get; set; }
    public static float sprogress = 0f;
    public static string mFilePath;
    public static Action<bool, string> mCallBack;
    public static Action<float> mOnProgressChange;
    public static Action<long, long, long> mOnDownloadNetSpdChange;
    public static bool CanReceiveData = false;
    public static long fileLength = 0;
    public static long totalLength = 0;
    public static FileStream saveFileStream = null;
    private static byte[] DATA_BUFFER;
    private static int BUFFER_DATA_SIZE = 0;
    public static Thread CurDownloadThread = null;
    private static bool NeedResetData = true;
    private static float lastDownloadProgress = 0;
    private static string fileUrl;
    private static MonoBehaviour sHost;
    /// <summary>
    /// 用于控制线程结束
    /// </summary>
    public static bool CanStopThread = false;
    static public void CleanData()
    {
        Debug.Log("##  CleanData");
        DATA_BUFFER = null;
        BUFFER_DATA_SIZE = 0;
        fileLength = 0;
        CanReceiveData = false;
        CanStopThread = false;
        CurDownloadThread = null;
        NeedResetData = true;
        if (null != saveFileStream)
        {
            saveFileStream.Flush(true);
            saveFileStream.Close();
            saveFileStream.Dispose();
        }
        if (null != myDataRequest)
        {
            myDataRequest.Dispose();
            myDataRequest = null;
        }
    }
    static public void CreateWR(MonoBehaviour host, string url, string filePath, Action<bool, string> callBack, Action<float> OnProgressChange, Action<long, long, long> OnDownloadInfo = null)
    {
        sHost = host;
        fileUrl = url;
        mFilePath = filePath;
        mCallBack = callBack;
        mOnProgressChange = OnProgressChange;
        mOnDownloadNetSpdChange = OnDownloadInfo;
        host.StartCoroutine(CreateWebRequest(url));
        host.StartCoroutine(UpdateCo());
    }
    static private IEnumerator CreateWebRequest(string url)
    {
        var dirPath = Path.GetDirectoryName(mFilePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        saveFileStream = new FileStream(mFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        fileLength = saveFileStream.Length;
        if (fileLength > MAX_WRITE_SIZE)
        {
            fileLength -= MAX_WRITE_SIZE;
        }
        else
        {
            fileLength = 0;
        }
        saveFileStream.Seek(fileLength, SeekOrigin.Begin);


        myDataRequest = UnityWebRequest.Get(url);
        myDataRequest.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
        myDataRequest.SendWebRequest();

        yield return myDataRequest;
        if (!string.IsNullOrEmpty(myDataRequest.error))
        {
            Debug.LogError($"{myDataRequest.error} {url}");
            if (null != mCallBack)
            {
                mCallBack(false, myDataRequest.error);
            }
            yield break;
        }
        WRITED_DATA_SIZE = 0;
        CanReceiveData = true;
        UpdateBuffer(myDataRequest.downloadHandler.data);
        lastDownloadProgress = myDataRequest.downloadProgress;
        DATA_BUFFER = new byte[MAX_WRITE_SIZE];
        Debug.Log($"### Start download thread, dataBuffer.Length:{DATA_BUFFER.Length} {mFilePath}/{totalLength}");
        CurDownloadThread = new Thread(ProessNetData);
        CurDownloadThread.Start();
    }
    static void UpdateBuffer(byte[] data) {
        if (WRITED_DATA_SIZE < data.Length) {
            int newSize = data.Length - WRITED_DATA_SIZE;
            if (newSize > DATA_BUFFER.Length) {
                newSize = DATA_BUFFER.Length;
            }
            if (newSize > 0)
            {
                try
                {
                    lock (DATA_BUFFER)
                    {
                        Array.Copy(data, WRITED_DATA_SIZE, DATA_BUFFER, 0, newSize);
                        BUFFER_DATA_SIZE = newSize;
                        NeedResetData = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"### UpdateBuffer newSize:{newSize}, {e.ToString()}");
                }
            }
        }
    }
    static IEnumerator UpdateCo()
    {
        if (!CanReceiveData) {
            yield return null;
        }
        while (true)
        {
            if (CanReceiveData)
            {

                if (WRITED_DATA_SIZE > (1 << 20) * 100)
                {
                    CleanData();
                    HttpDownLoad.CreateWR(sHost, fileUrl, mFilePath, mCallBack, mOnProgressChange, mOnDownloadNetSpdChange);
                    break;
                }
                if (!string.IsNullOrEmpty(myDataRequest.error))
                {
                    if (null != mCallBack)
                    {
                        mCallBack(false, myDataRequest.error);
                    }
                    CanStopThread = true;
                    yield break;
                }
                else
                {
                    if (fileLength == totalLength)
                    {
                        if (mCallBack != null)
                        {
                            mCallBack(true, "");
                        }
                        CanStopThread = true;
                        yield break;
                    }
                    if (NeedResetData)
                    {
                        if (myDataRequest.downloadProgress - lastDownloadProgress > float.Epsilon)
                        {
                            UpdateBuffer(myDataRequest.downloadHandler.data);
                        }
                    }
                }
                if (!CurDownloadThread.IsAlive)
                {
                    if (null != mCallBack)
                    {
                        mCallBack(false, "### download thread stop");
                    }
                    yield break;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private static int WRITED_DATA_SIZE = 0;
    private static long tickSpan = 10000000 / 5;
    private static long lastTicks = 0;
    private static long lastSecondTicks = 0;
    private static long sizePerSecond = 0;
    private static void ProessNetData()
    {
        var filePath = mFilePath;
        var OnProgressChange = mOnProgressChange;
        var OnDownloadNetSpdChange = mOnDownloadNetSpdChange;
        while (!CanStopThread)
        {
            //lock (dataBuffer) 
            {
                if (DATA_BUFFER != null && BUFFER_DATA_SIZE > 0)
                {
                    try
                    {
                        lock (DATA_BUFFER)
                        {
                            saveFileStream.Write(DATA_BUFFER, 0, BUFFER_DATA_SIZE);
                            WRITED_DATA_SIZE += BUFFER_DATA_SIZE;
                            fileLength += BUFFER_DATA_SIZE;
                            sizePerSecond += BUFFER_DATA_SIZE;
                            NeedResetData = true;
                            BUFFER_DATA_SIZE = 0;
                        }
                        if (WRITED_DATA_SIZE > (1 << 20) * 100)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"### ProessNetData length:{BUFFER_DATA_SIZE} WRITED_DATA_SIZE:{WRITED_DATA_SIZE} FS.Lengh:{saveFileStream.Length}, {e.ToString()}");
                        CanStopThread = true;
                    }
                    if (fileLength == totalLength)
                    {
                        sprogress = 1f;
                    }
                    else
                    {
                        sprogress = fileLength / (float)totalLength;
                    }
                    if (DateTime.Now.Ticks - lastSecondTicks >= 10000000)
                    {
                        if (null != OnDownloadNetSpdChange)
                        {
                            OnDownloadNetSpdChange(fileLength, totalLength, sizePerSecond);
                            sizePerSecond = 0;
                        }
                        lastSecondTicks = DateTime.Now.Ticks;
                    }
                    else if (DateTime.Now.Ticks - lastTicks >= tickSpan)
                    {
                        if (null != OnProgressChange)
                        {
                            OnProgressChange(sprogress);
                        }
                        lastTicks = DateTime.Now.Ticks;
                    }
                    else 
                    { 
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
    }
}
