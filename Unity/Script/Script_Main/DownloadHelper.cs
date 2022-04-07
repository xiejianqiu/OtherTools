using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Threading;

public class DownloadHelper
{
    public delegate void DelegateDownloadFinish(bool bSuccess);
    public delegate void DelegateDownloadFaileHandler(string filePath, string error);
    public float AlreadyDownloadSize { get { return (m_alreadyDownloadSize +m_downloadingSize) /1024f; } }


    private long m_alreadyDownloadSize = 0;
    static private long m_downloadingSize = 0;

    private string[] m_curUrlArray;
    private string[] m_curFilePathArray;

    private DelegateDownloadFinish m_curDelFun;
    private DelegateDownloadFaileHandler m_DelegateDownloadFaileHandler;
    //private int m_curDownloadSize = 0;
    private bool m_bIsRemote = false;
    private bool m_bStop = false;
    public static DownloadHelper StartDownload(MonoBehaviour monoBehavior, string url, bool bRemote, string fileSavePath, DelegateDownloadFinish delFun = null)
    {
        DownloadHelper helper = new DownloadHelper(url, bRemote, fileSavePath, delFun);
        monoBehavior.StartCoroutine(helper.DownloadFile(monoBehavior));
        return helper;
    }

    public static DownloadHelper StartDownload(MonoBehaviour monoBehavior, List<string> urlList, bool bRemote, List<string> fileSavePathList, DelegateDownloadFinish delFun = null, DelegateDownloadFaileHandler delFun2= null)
    {
        DownloadHelper helper = new DownloadHelper(urlList, bRemote, fileSavePathList, delFun, delFun2);
        monoBehavior.StartCoroutine(helper.DownloadFile(monoBehavior));
        return helper;
    }

    public void Stop()
    {
        m_bStop = true;
    }

    private DownloadHelper(string url, bool bRemote, string fileSavePath, DelegateDownloadFinish delFun)
    {
        m_curUrlArray = new string[1];
        m_curFilePathArray = new string[1];
        m_curFilePathArray[0] = fileSavePath;
        m_bIsRemote = bRemote;

        m_curUrlArray[0] = url;


        m_curDelFun = delFun;
        m_bStop = false;
    }

    private DownloadHelper(List<string> urlList, bool bRemote, List<string> fileSavePathList, DelegateDownloadFinish delFun, DelegateDownloadFaileHandler delFun2)
    {
        m_curUrlArray = new string[urlList.Count];
        m_bIsRemote = bRemote;
        for (int i = 0; i < urlList.Count; i++)
        {
            m_curUrlArray[i] = urlList[i];
        }

        m_curFilePathArray = new string[fileSavePathList.Count];
        for (int i = 0; i < fileSavePathList.Count; i++)
        {
            m_curFilePathArray[i] = fileSavePathList[i];
        }

        m_curDelFun = delFun;
        m_DelegateDownloadFaileHandler = delFun2;
        m_bStop = false;
    }

    private IEnumerator DownloadFile(MonoBehaviour parent)
    {
        if (null == m_curUrlArray || null == m_curFilePathArray)
        {
            if (null != m_curDelFun) m_curDelFun(false);
            yield break;
        }

        if (m_curUrlArray.Length != m_curFilePathArray.Length)
        {
            if (null != m_curDelFun) m_curDelFun(false);
            yield break;
        }
        if (m_curUrlArray.Length > 10) {
            Queue<string> queue = new Queue<string>(m_curUrlArray);
            Queue<string> fileSavePathQueue = new Queue<string>(m_curFilePathArray);
            List<string> errorList = new List<string>();
            int nCurNumOfDownload = 0;
            int nMaxDownload = 10;
            while (true) {
                if (m_bStop)
                {
                    yield break;
                }
                if (nCurNumOfDownload >= nMaxDownload) {
                    yield return new WaitForEndOfFrame();
                }
                if (queue.Count > 0)
                {
                    var url = queue.Dequeue();
                    var savePath = fileSavePathQueue.Dequeue();
                    Interlocked.Increment(ref nCurNumOfDownload);
                    parent.StartCoroutine(GetCDNWWW(url, value =>
                    {
                        Interlocked.Decrement(ref nCurNumOfDownload);
                        var wwwCurFile = value;
                        if (!string.IsNullOrEmpty(wwwCurFile.error))
                        {
                            errorList.Add(url);
                            Debug.LogError($"### DownloadFileError: {url} {wwwCurFile.error}");
                            if (null != m_DelegateDownloadFaileHandler) m_DelegateDownloadFaileHandler(wwwCurFile.url, wwwCurFile.error);
                        }
                        else
                        {
                            try
                            {
                                if (File.Exists(savePath))
                                {
                                    File.Delete(savePath);
                                }
                                GfxUtils.CheckTargetPath(savePath);
                                FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate);
                                fs.Write(wwwCurFile.downloadHandler.data, 0, wwwCurFile.downloadHandler.data.Length);
                                fs.Close();
                                m_alreadyDownloadSize += wwwCurFile.downloadHandler.data.Length;
                                if (savePath.Contains("972afe5c3861538901ce554291893904ac64c2c9.ab"))
                                {
                                    m_alreadyDownloadSize -= 2000000;
                                }
                            }
                            catch
                            {
                                //Debug.LogError("aaaaaaaaa   " + m_curUrlArray[i] +"   " + m_curFilePathArray[i] + "   " + e.Message);
                                if (null != m_curDelFun) m_curDelFun(false);
                                m_bStop = true;
                            }

                        }
                    }
                    , m_bIsRemote));
                }
                else 
                {
                    if (nCurNumOfDownload <= 0)
                    {
                        if (errorList.Count > 0)
                        {
                            if (null != m_curDelFun) m_curDelFun(false);
                        }
                        else 
                        { 
                            if (null != m_curDelFun) m_curDelFun(!m_bStop);
                        }
                        break;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (int i = 0; i < m_curUrlArray.Length; i++)
            {
                if (m_bStop)
                {
                    yield break;
                }

                UnityWebRequest wwwCurFile = null;
                yield return parent.StartCoroutine(GetCDNWWW(m_curUrlArray[i], value => wwwCurFile = value, m_bIsRemote));
                if (!string.IsNullOrEmpty(wwwCurFile.error))
                {
                    Debug.LogError($"### DownloadFileError: {m_curUrlArray[i]} {wwwCurFile.error}");
                    //Debug.LogError("DownloadFile   " + wwwCurFile.error  + "  " + m_curUrlArray[i]);
                    if (null != m_curDelFun) m_curDelFun(false);
                    if (null != m_DelegateDownloadFaileHandler) m_DelegateDownloadFaileHandler(wwwCurFile.url, wwwCurFile.error);
                    yield break;
                }
                else
                {
                    try
                    {
                        if (File.Exists(m_curFilePathArray[i]))
                        {
                            File.Delete(m_curFilePathArray[i]);
                        }
                        GfxUtils.CheckTargetPath(m_curFilePathArray[i]);
                        FileStream fs = new FileStream(m_curFilePathArray[i], FileMode.OpenOrCreate);
                        fs.Write(wwwCurFile.downloadHandler.data, 0, wwwCurFile.downloadHandler.data.Length);
                        fs.Close();
                        m_alreadyDownloadSize += wwwCurFile.downloadHandler.data.Length;
                        if (m_curFilePathArray[i].Contains("972afe5c3861538901ce554291893904ac64c2c9.ab"))
                        {
                            m_alreadyDownloadSize -= 2000000;
                        }
                    }
                    catch
                    {
                        //Debug.LogError("aaaaaaaaa   " + m_curUrlArray[i] +"   " + m_curFilePathArray[i] + "   " + e.Message);
                        if (null != m_curDelFun) m_curDelFun(false);
                        m_bStop = true;
                    }

                }
            }

            if (null != m_curDelFun) m_curDelFun(!m_bStop);
        }
    }

    public static string AddTimestampToUrl(string url)
    {
        return url + "?" + System.DateTime.Now.Ticks.ToString();
    }

    public static IEnumerator GetCDNWWW(string url, System.Action<UnityWebRequest> retWWW, bool bRemote = true,bool bByChannel = false)
    {
        m_downloadingSize = 0;

        string firstUrl = url;
        if (bRemote)
        {
            if (bByChannel)
            {
                firstUrl = URLConfig.Instance.GetURLCDNByChannel(AddTimestampToUrl(url));
            }
            else
            {
                firstUrl = URLConfig.Instance.GetURLCDN(AddTimestampToUrl(url));
            }
        }
        UnityWebRequest wwwOrg = UnityWebRequest.Get(firstUrl);
        //wwwOrg.timeout = 5;
        //Debug.Log("request url first: " + firstUrl);
        yield return wwwOrg.SendWebRequest();
        //yield return new WaitForSeconds(0.5f);

        bool bSucc = false;

        while(true)
        {
            m_downloadingSize = wwwOrg.downloadHandler.data.Length;
            if (wwwOrg.isDone)
            {
                if(string.IsNullOrEmpty(wwwOrg.error))
                {
                    bSucc = true;
                    m_downloadingSize = 0;
                    retWWW(wwwOrg);
                    break;
                }
                
            }


            if (!string.IsNullOrEmpty(wwwOrg.error))
            {
                Debug.LogError($"### request url error: {firstUrl} {wwwOrg.error}");
                break;
            }

            yield return new WaitForEndOfFrame();
        }
        
        if(bSucc)
        {
            yield break;
        }

        firstUrl = url;
        if(bRemote)
        {
            if (bByChannel)
            {
                firstUrl = URLConfig.Instance.GetURLCDNSecByChannel(AddTimestampToUrl(url));
            }
            else
            {
                firstUrl = URLConfig.Instance.GetURLCDNSec(AddTimestampToUrl(url));
            }
        }

        UnityWebRequest secWWW = UnityWebRequest.Get(firstUrl);
        //secWWW.timeout = 5;
        //Debug.Log("request url second: " + firstUrl);
        secWWW.SendWebRequest();
        while (true)
        {
            m_downloadingSize = secWWW.downloadHandler.data.Length;

            if (secWWW.isDone)
            {
                if (string.IsNullOrEmpty(secWWW.error))
                {
                    bSucc = true;
                    m_downloadingSize = 0;
                    retWWW(secWWW);
                    break;
                }
                
            }


            if (!string.IsNullOrEmpty(secWWW.error))
            {
                Debug.LogError("### request url second: " + firstUrl);
                retWWW(secWWW);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
    