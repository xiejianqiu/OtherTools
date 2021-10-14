using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using LitJson;
using Shark;
using UnityEngine.Networking;
using ICSharpCode.SharpZipLib.Zip;

class UpdateFileInfo
{
    public string FileName;
    public string MD5;
    public int FileSize;
}
public class UnzipCallback : ZipTools.UnzipCallback
{
 
    public Action<bool> OnUnzipFinishEvent;
    public UnzipCallback(Action<bool> handler) {
        OnUnzipFinishEvent = handler;
    }
    public override void OnFinished(bool _result)
    {
        base.OnFinished(_result);
        if (null != OnUnzipFinishEvent) {
            OnUnzipFinishEvent(_result);
        }
    }
    public override void OnPostUnzip(ZipEntry _entry)
    {
        base.OnPostUnzip(_entry);
    }
    public override bool OnPreUnzip(ZipEntry _entry)
    {
        return base.OnPreUnzip(_entry);
    }
}
class ResDownload
{
    private UpdateUILogic m_UILogic = null;

    string m_stringLocalVersion = "1.0.0.0";
    string m_stringAppVersion = "1.0.0.0";
    string m_stringRemoteVersion = "1.0.0.0";
    UpdateStep m_curUpdateStep = UpdateStep.NONE;
    private string m_AppStoreId = "1517775932";

    DownloadHelper m_DownloadVersionFile = null;
    DownloadHelper m_dataFileDownloader = null;
    List<UpdateFileInfo> m_lstDownloadFiles = new List<UpdateFileInfo>();
    List<UpdateFileInfo> m_lstUpdateFiles = new List<UpdateFileInfo>();

    float m_fDownloadSize = 0;
    bool m_bRestart = false;
    bool m_bNewbieOptimize = false;
    bool m_bMinApk = false; //是否为小包
    const string m_NameResTable = "972afe5c3861538901ce554291893904ac64c2c9.ab";

    public UpdateStep CurUpdateStep { get { return m_curUpdateStep; } }
    public float TotolDownloadSize { get { return m_fDownloadSize; } }
    public float AlreadyDownloadSize { get { return m_dataFileDownloader == null?0: m_dataFileDownloader.AlreadyDownloadSize; } }

    public ResDownload(UpdateUILogic vUILogic)
    {
        m_UILogic = vUILogic;
    }
     public IEnumerator CheckResVersion()
    {
        SetTips(UpdateTips.GetTipByID(0));
        m_UILogic.SetProgressTips3();
        //m_UILogic.SetProgressTips3(UpdateTips.GetTipByID(100));
        SetProgress(0.5f);
        bool bHasLocalVersion = false;
        string localVersionFilePath = PathConfig.Instance.LocalVersionFilePath;
        if (File.Exists(localVersionFilePath))
        {
            string versionTxt = GfxUtils.GetFileString(localVersionFilePath);
            string[] strVec = versionTxt.Split('\n');
            if(strVec.Length>0)
            {
                m_stringLocalVersion = strVec[0];
                bHasLocalVersion = true;
            }
            Debug.Log($"### localVer:{versionTxt}");
        }

        UnityWebRequest wwwAppVersion = UnityWebRequest.Get(PathConfig.Instance.GetAppDataURL(PathConfig.Instance.VersionFileName));
        yield return wwwAppVersion.SendWebRequest();

        if (string.IsNullOrEmpty(wwwAppVersion.error))
        {
            string[] strVec = wwwAppVersion.downloadHandler.text.Split('\n');
            m_stringAppVersion = strVec[0];
            GfxLogicBrige.Instance.SetCodeVersion (m_stringAppVersion);
            GfxLogicBrige.Instance.SetClientVersion(m_stringLocalVersion);
            ChanConnector.AppVersionCheck(m_stringAppVersion);
            if (strVec.Length >= 3)
            {
                m_bMinApk = strVec[2].Equals("1");
            }
            Debug.Log($"### appVer:{wwwAppVersion.downloadHandler.text}");
        }
        if (!m_bMinApk && (IsNewResVersion(m_stringLocalVersion, m_stringAppVersion) || IsNewDllVersion(m_stringLocalVersion, m_stringAppVersion)))
        {
            GfxUtils.DeleteFolder(PathConfig.Instance.PersistenPath);            
            m_stringLocalVersion = m_stringAppVersion;
#if UNITY_ANDROID && !UNITY_EDITOR
            
            if(bHasLocalVersion)
            {
                if (File.Exists(localVersionFilePath))
                {
                    File.Delete(localVersionFilePath);
                }    
                PushNotificationConnector.restartApp();
            }
#endif
        }
        UnityWebRequest wwwRemote = null;
        SetProgress(1);
        ChanConnector.ResourceServerVisit("null", PathConfig.Instance.VersionFileName);     //访问资源服务器请求
        yield return m_UILogic.StartCoroutine(DownloadHelper.GetCDNWWW(PathConfig.Instance.VersionFileName, value => wwwRemote = value));
        if (!string.IsNullOrEmpty(wwwRemote.error))
        {
            OnCheckVersion(false);
            //Debug.LogError(wwwRemote.error);
        }
        else
        {
            Debug.Log($"### remoteVer:{wwwRemote.downloadHandler.text}");
            string[] strVec = wwwRemote.downloadHandler.text.Split('\n');
            m_stringRemoteVersion = strVec[0];
            if (strVec.Length > 1)
            {
                m_AppStoreId = strVec[1];
            }
            if (strVec.Length > 3)
            {
                m_bNewbieOptimize = strVec[3].Equals("1");                
            }
            if (File.Exists(PathConfig.Instance.LocalVersionFilePath)) {
                File.Delete(PathConfig.Instance.LocalVersionFilePath);
            }
            if (Directory.Exists(PathConfig.Instance.PersistenPath)) {
                Directory.Delete(PathConfig.Instance.PersistenPath, true);
            }
            if (m_bMinApk && !File.Exists(PathConfig.Instance.LocalVersionFilePath))
            {
                m_UILogic.ShowMessageBox(UpdateTips.GetTipByID(33), () => { CheckUpdateBasePatch(); });
            }
            else
            {
                OnCheckVersion(true);
            }
        }
    }
    //小包需要先下载基础补丁包

    HttpDownLoad httpDownload;
    public void StopDownloadBasePatch() {
        if (null != httpDownload) {
            httpDownload.Stop();
        }
    }
    private void CheckUpdateBasePatch()
    {
        OnDownloadBasePatchFinish(true, "");
        return;
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(200));
        m_curUpdateStep = UpdateStep.DownBaseRes;
        Debug.Log($"### Begin Download File [{PathConfig.Instance.BatchPatch}] To {PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch} {System.DateTime.Now}");
        ChanConnector.SwitchStrictMode(0);
        httpDownload = new HttpDownLoad();
        string firstUrl = URLConfig.Instance.GetURLCDN(DownloadHelper.AddTimestampToUrl(PathConfig.Instance.BatchPatch));
        m_UILogic.StartCoroutine(httpDownload.Start(
            firstUrl, 
            PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch,
            OnDownloadBasePatchFinish, 
            (f) =>
        {
            m_UILogic.SetProgress(f);
        }));
    }
    private void OnDownloadBasePatchFinish(bool bSucc, string errorTip) {
        httpDownload = null;
        Debug.Log($"### Download File Finish.{bSucc} {System.DateTime.Now}");
        if (bSucc)
        {
            m_curUpdateStep = UpdateStep.UnZipBaseRes;
            Debug.Log($"### Begin Extract File. {System.DateTime.Now}");
            Debug.Log($"### Unzip File {PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch} To {PathConfig.Instance.PersistenPath}");
            m_UILogic.SetProgressTips(UpdateTips.GetTipByID(300));
            float lastProgress = 0f;
            m_UILogic.StartCoroutine( MyZipTool.UnZipFileCo(PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch, PathConfig.Instance.PersistenPath, string.Empty, (f,result)=> {
                if (result == MyZipTool.UnZipStep.Finish)
                {
                    File.Move(PathConfig.Instance.PersistenPath + PathConfig.Instance.VersionFileName, PathConfig.Instance.LocalVersionFilePath);
                    Debug.Log($"### Extract File Finish. {System.DateTime.Now}");
                    m_UILogic.StartCoroutine(CheckResVersion());
                    //if (File.Exists(PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch))
                    //{
                    //    File.Delete(PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch);
                    //}
                    m_UILogic.SetProgress(1f);
                    ChanConnector.SwitchStrictMode(1);
                }
                else if (result == MyZipTool.UnZipStep.Progress)
                {
                    if (f > lastProgress)
                    {
                        GfxLogicBrige.GfxEventManager.FireEvent(GfxEventType.Gfx_RefreshUpdateUIProgress, lastProgress, f);
                        //m_UILogic.SetProgress(f);
                        lastProgress = f;
                    }
                }
                else if (result == MyZipTool.UnZipStep.Fail)
                {
                    Debug.Log($"### Extract File Fail. {System.DateTime.Now}");
                    m_UILogic.SetProgress(1f);
                    m_UILogic.SetProgressTips(UpdateTips.GetTipByID(301));
                    if (File.Exists(PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch))
                    {
                        File.Delete(PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.BatchPatch);
                    }
                    ChanConnector.SwitchStrictMode(1);
                    m_UILogic.ShowMessageBox(UpdateTips.GetTipByID(302), () => { CheckUpdateBasePatch(); });
                }
            }));
            
        }
        else 
        {
            if (!string.IsNullOrEmpty(errorTip))
            {
                string error = string.Format(UpdateTips.GetTipByID(34), errorTip);
                m_UILogic.ShowMessageBox(error, () => { CheckUpdateBasePatch(); });
            }
            else
            {
                m_UILogic.SetProgressTips(UpdateTips.GetTipByID(201));
            }
        }
    }
    bool IsNewResVersion(string oldVersion,string newVersion)
    {
        return VersionCheck(oldVersion,newVersion,3);
    }
    bool IsNewDllVersion(string oldVersion, string newVersion)
    {
        return VersionCheck(oldVersion, newVersion, 2)|| VersionCheck(oldVersion, newVersion, 1)|| VersionCheck(oldVersion, newVersion, 0);
    }

    bool IsResGreaterThanVersion(string oldVersion, string newVersion)
    {
        return VersionGreaterThan(oldVersion, newVersion, 3);
    }

    bool IsDllGreaterThanVersion(string oldVersion, string newVersion)
    {
        return VersionGreaterThan(oldVersion, newVersion, 2) || VersionGreaterThan(oldVersion, newVersion, 1) || VersionGreaterThan(oldVersion, newVersion, 0);
    }
    
    bool IsDownloadApp(string oldVersion, string newVersion)
    {
        return VersionCheck(oldVersion,newVersion,1);
    }

    bool VersionCheck(string oldVersion, string newVersion,int nIdx)
    {

        bool bRet = false;

        string[] vecOld = oldVersion.Split('.');
        string[] vecNew = newVersion.Split('.');
        try
        {
            if (nIdx < vecOld.Length && nIdx < vecNew.Length)
            {
                if (int.Parse(vecNew[nIdx]) > int.Parse(vecOld[nIdx]))
                {
                    bRet = true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            var msg = string.Format("oldVer:{0}, newVer:{1}, idx:{2}", oldVersion, newVersion, nIdx);
            Debug.LogError(msg);
        }
        return bRet;
    }

    bool VersionGreaterThan(string oldVersion, string newVersion, int nIdx)
    {

        bool bRet = false;

        string[] vecOld = oldVersion.Split('.');
        string[] vecNew = newVersion.Split('.');

        if (nIdx < vecOld.Length && nIdx < vecNew.Length)
        {
            if (int.Parse(vecNew[nIdx]) < int.Parse(vecOld[nIdx]))
            {
                bRet = true;
            }
        }

        return bRet;
    }

    void OnCheckVersion(bool bSucc)
    {        
        if (bSucc)
        {
            GameStatics.DeviceActivationMonitor((int)EDeviceActivationStep.CheckVersion);
            bool ShowPorgress = false;
            if (IsNewDllVersion(m_stringLocalVersion, m_stringRemoteVersion))
            {
#if UNITY_ANDROID
                m_bRestart = true;
                GfxLogicBrige.Instance.GameVersion = m_stringRemoteVersion;
                m_DownloadVersionFile = DownloadHelper.StartDownload(m_UILogic, PathConfig.Instance.UpdateInfoFileName, true, PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.UpdateInfoFileName, OnDownloadUpdateInfoFile);
#elif UNITY_IOS
                if (IsGoToAppStore(m_stringLocalVersion, m_stringRemoteVersion))
                {
                    m_UILogic.ShowMessageBox(UpdateTips.GetTipByID(30), () => { Application.OpenURL("itms-apps://itunes.apple.com/app/id" + m_AppStoreId); });
                }
                else
                {
                    m_bRestart = true;
                    GfxLogicBrige.Instance.GameVersion = m_stringRemoteVersion;
                    m_DownloadVersionFile = DownloadHelper.StartDownload(m_UILogic, PathConfig.Instance.UpdateInfoFileName, true, PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.UpdateInfoFileName, OnDownloadUpdateInfoFile);
                }
                ShowPorgress = true;  
#endif
            }
            else if (IsNewResVersion(m_stringLocalVersion,m_stringRemoteVersion))
            { 
                GfxLogicBrige.Instance.GameVersion = m_stringRemoteVersion;
                m_DownloadVersionFile = DownloadHelper.StartDownload(m_UILogic, PathConfig.Instance.UpdateInfoFileName, true, PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.UpdateInfoFileName, OnDownloadUpdateInfoFile);
                ShowPorgress = true;
            }
            else if (IsDllGreaterThanVersion(m_stringLocalVersion, m_stringRemoteVersion))
            {
                GfxLogicBrige.Instance.GameVersion = m_stringLocalVersion;
                UpdateFinish(UpdateResult.Success);
                ShowPorgress = false;
            }
            else if (IsResGreaterThanVersion(m_stringLocalVersion, m_stringRemoteVersion))
            {
                GfxLogicBrige.Instance.GameVersion = m_stringLocalVersion;
                UpdateFinish(UpdateResult.Success);
                ShowPorgress = false;
            }
            else
            {
                GfxLogicBrige.Instance.GameVersion = m_stringLocalVersion;
                UpdateFinish(UpdateResult.Success);
                ShowPorgress = true;
            }
#if UNITY_IOS
            m_UILogic.m_ProgressSlider.gameObject.SetActive(true);
            m_UILogic.m_LblTips.gameObject.SetActive(true);
            m_UILogic.m_LblTips_2.gameObject.SetActive(true);
            m_UILogic.m_LblTips_3.gameObject.SetActive(true);
            //m_UILogic.m_ProgressSlider_2.gameObject.SetActive(true);
#endif
        }
        else
        {
            GameStatics.DeviceActivationMonitor((int)EDeviceActivationStep.CheckVersionFail);
            UpdateFinish(UpdateResult.GetVersionFail);
        }
    }


    bool IsGoToAppStore(string oldVersion, string newVersion)
    {
        if (VersionCheck(oldVersion, newVersion, 2))            //ios不区分代码与资源，只更新最后一位，保留改变倒数第二位用于前去AppStore更新包
            return true;
        return false;
    }

    void OnDownloadUpdateInfoFile(bool bSucc)
    {
        if (bSucc)
        {
            m_curUpdateStep = UpdateStep.CompareRes;

            string localUpdateInfoPath = PathConfig.Instance.PersistenPath + PathConfig.Instance.UpdateInfoFileName;
            if (!File.Exists(localUpdateInfoPath))
            {
                m_DownloadVersionFile = DownloadHelper.StartDownload(m_UILogic, PathConfig.Instance.GetAppDataURL(PathConfig.Instance.UpdateInfoFileName), false, localUpdateInfoPath, CompaireUpdateInfo);
            }
            else
            {
                CompaireUpdateInfo(true);
            }
        }
        else
        {
            UpdateFinish(UpdateResult.GetUpdateInfoFileFail);
        }
    }

    void CompaireUpdateInfo(bool bSucc)
    {

        if (!bSucc)
        {
            UpdateFinish(UpdateResult.Success);
        }

        m_UILogic.StartCoroutine(BeginCompairUpdateInfo());
    }
    IEnumerator BeginCompairUpdateInfo() {

        m_lstUpdateFiles.Clear();
        Dictionary<string, UpdateFileInfo> dicLocalInfo = new Dictionary<string, UpdateFileInfo>();
        Dictionary<string, UpdateFileInfo> dicRemoteInfo = new Dictionary<string, UpdateFileInfo>();
        string localUpdateInfoPath = PathConfig.Instance.PersistenPath + PathConfig.Instance.UpdateInfoFileName;
        string remoteUpdateInfoPath = PathConfig.Instance.DownLoadCachePath + PathConfig.Instance.UpdateInfoFileName;

        ReadUpdateInfo(localUpdateInfoPath, dicLocalInfo);
        if (!ReadUpdateInfo(remoteUpdateInfoPath, dicRemoteInfo))
        {
            UpdateFinish(UpdateResult.LoadRomoteFailListError);
            yield break;
        }

        var enu = dicRemoteInfo.GetEnumerator();
        int iCount = 0;
        while (enu.MoveNext())
        {
            UpdateFileInfo fileInfo = null;
            dicLocalInfo.TryGetValue(enu.Current.Key, out fileInfo);
            if (fileInfo == null || !enu.Current.Value.MD5.Equals(fileInfo.MD5))
            {
                m_lstUpdateFiles.Add(enu.Current.Value);
            }
        }

        for (int i = 0; i < m_lstUpdateFiles.Count; i++)
        {
            m_lstUpdateFiles[i].FileName = "/Res/" + m_lstUpdateFiles[i].FileName;
        }

        if (m_lstUpdateFiles.Count > 0)
        {
            if (m_bNewbieOptimize)
            {
                if (!string.IsNullOrEmpty(GfxPlayerPrefsData.LastAccount))
                {
                    m_bNewbieOptimize = false;
                }
            }

            m_lstDownloadFiles.Clear();
            m_fDownloadSize = 0;
            iCount = 0;
            int nCount = 1;
            Debug.Log($"BeginCompairUpdateInfo  Calcu Download size {m_lstUpdateFiles.Count}");
            for (int i = 0; i < m_lstUpdateFiles.Count; i++)
            {
                m_UILogic.SetProgressTxt(nCount * 1f / m_lstUpdateFiles.Count);
                string localPath = PathConfig.Instance.DownLoadCachePath + m_lstUpdateFiles[i].FileName;
                bool isFileExist = File.Exists(localPath);
                if (!isFileExist || GfxUtils.GetMD5Hash(localPath).ToLower() != m_lstUpdateFiles[i].MD5.ToLower())
                {
                    m_lstDownloadFiles.Add(m_lstUpdateFiles[i]);
                    m_fDownloadSize += m_lstUpdateFiles[i].FileSize;
                }
                //MD5计算耗时
                if (isFileExist)
                {
                    iCount++;
                    if (iCount >= 10)
                    {
                        yield return new WaitForEndOfFrame();
                        iCount = 0;
                    }
                }
                nCount += 1;
            }

            m_fDownloadSize = m_fDownloadSize / 1024f;

            if (m_bNewbieOptimize && m_fDownloadSize < 20480)
            {
                UpdateFileInfo vInfo = null;
                for (int i = 0; i < m_lstDownloadFiles.Count; i++)
                {
                    if (m_lstDownloadFiles[i].FileName.Contains(m_NameResTable))
                    {
                        vInfo = m_lstDownloadFiles[i];
                        break;
                    }
                }

                m_lstDownloadFiles.Clear();
                m_bRestart = false;
                m_fDownloadSize = 0;
                if (vInfo != null)
                {
                    m_lstDownloadFiles.Add(vInfo);
                    m_fDownloadSize = vInfo.FileSize / 1024f;
                    for (int i = m_lstUpdateFiles.Count - 1; i >= 0; i--)
                    {
                        if (!m_lstUpdateFiles[i].FileName.Contains(m_NameResTable))
                        {
                            m_lstUpdateFiles.RemoveAt(i);
                        }
                    }
                }
            }

            if (m_lstDownloadFiles.Count > 0)
            {
                AskDownload();
            }
            else
            {
                OnDownloadRes(true);
            }
        }
        else
        {
            OnDownloadRes(true);
        }

        dicLocalInfo.Clear();
        dicRemoteInfo.Clear();

    }

    void AskDownload()
    {        
        m_UILogic.SetProgressTxt(0f);
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(3));
        m_UILogic.SetProgressTips3(UpdateTips.GetTipByID(103));
        if (m_fDownloadSize > 0)
        {
            string strTips = string.Format(UpdateTips.GetTipByID(4), m_fDownloadSize / 1024);
            m_UILogic.ShowMessageBox(strTips, () => { BeginDownload(); });
        }
        else
        {
            BeginDownload();
        }
        
    }
    bool hasReportResDownload = false;
    public void BeginDownload()
    {
        if (m_fDownloadSize > 0 && !GfxLogicBrige.Instance.IsTiShenFu() && !hasReportResDownload)
        {
            hasReportResDownload = true;
            ChanConnector.SendKoreaInfoLog("downloadBegin");
            ChanConnector.ResourceUpdateStart(URLConfig.Instance.GetURLCDN(PathConfig.Instance.UpdateInfoFileName));          //开始更新资源服务器请求
        }
        GameStatics.DeviceActivationMonitor((int)EDeviceActivationStep.BeginUpdate);
        m_curUpdateStep = UpdateStep.DownloadRes;
        List<string> lst = new List<string>();
        for (int i = 0; i < m_lstDownloadFiles.Count; i++)
        {
            lst.Add(m_lstDownloadFiles[i].FileName);
            if(m_lstDownloadFiles[i].FileName.Equals(m_NameResTable))
            {
                m_fDownloadSize += 2048;
            }
        }
        List<string> lstSave = new List<string>();
        for (int i = 0; i < m_lstDownloadFiles.Count; i++)
        {
            lstSave.Add(PathConfig.Instance.DownLoadCachePath + m_lstDownloadFiles[i].FileName);
        }
        m_dataFileDownloader = DownloadHelper.StartDownload(m_UILogic, lst, true, lstSave, OnDownloadRes, OnDownloadFailFileHandler);

        m_UILogic.InvokeBackTextureChange();//下载时背景图每3秒切换一次
    }
    void OnDownloadFailFileHandler(string file, string error) { 
        if (m_fDownloadSize > 0 && !GfxLogicBrige.Instance.IsTiShenFu())
        {
            ChanConnector.ResourceUpdate(file, "file download fail", error);   //AssetBundle更新失败
        }
    }
    void OnDownloadRes(bool bSucc)
    {
        m_UILogic.StartCoroutine(CheckDownloadFiles(bSucc));
    }
    IEnumerator CheckDownloadFiles(bool bSucc) {

        m_curUpdateStep = UpdateStep.CheckRes;
        if (bSucc)
        {
            m_UILogic.SetProgressTxt(0.9f);
            m_UILogic.SetProgressTips(UpdateTips.GetTipByID(5));
            m_UILogic.SetProgressTips3();

            bool bError = false;
            int nCount = 1;
            int iCount = 0;
            for (int i = 0; i < m_lstDownloadFiles.Count; i++)
            {
                m_UILogic.SetProgressTxt(nCount * 1f / m_lstDownloadFiles.Count);
                string curFile = PathConfig.Instance.DownLoadCachePath + m_lstDownloadFiles[i].FileName;
                if (!File.Exists(curFile))
                {
                    bError = true;
                    break;
                }
                else
                {
                    string curMd5 = GfxUtils.GetMD5Hash(curFile);
                    if (curMd5.ToLower() != m_lstDownloadFiles[i].MD5.ToLower())
                    {
                        //Debug.LogError("OnDownloadRes MD5 dismatch!!! " + curFile + "   " + curMd5 + "   " + m_lstDownloadFiles[i].MD5);
                        bError = true;
                        break;
                    }
                    iCount++;
                    if (iCount >= 10)
                    {
                        yield return new WaitForEndOfFrame();
                        iCount = 0;
                    }
                }
                nCount += 1;
            }

            if (!bError)
            {
                CopyResToPersistenPath();
            }
            else
            {
                UpdateFinish(UpdateResult.DownloadInComplete);
            }

        }
        else
        {
            UpdateFinish(UpdateResult.DownloadFail);
        }
    }

    void CopyResToPersistenPath()
    {
        m_curUpdateStep = UpdateStep.CopyRes;
        try
        {
            for (int i = 0; i < m_lstUpdateFiles.Count; i++)
            {
                string localFilePath = PathConfig.Instance.PersistenPath +  m_lstUpdateFiles[i].FileName;
                GfxUtils.CheckTargetPath(localFilePath);
                File.Copy(PathConfig.Instance.DownLoadCachePath +  m_lstUpdateFiles[i].FileName, localFilePath, true);
            }

            File.Copy(PathConfig.Instance.DownLoadCachePath +  PathConfig.Instance.UpdateInfoFileName, PathConfig.Instance.PersistenPath +  PathConfig.Instance.UpdateInfoFileName, true);

            string localVersionFilePath = PathConfig.Instance.LocalVersionFilePath; ;
            GfxUtils.CheckTargetPath(localVersionFilePath);
            FileStream fs = new FileStream(localVersionFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(m_stringRemoteVersion.ToString());
            sw.Close();
            fs.Close();

        }
        catch
        {
            //Debug.LogError("CopyResToPersistenPath    " + e.Message);
            UpdateFinish(UpdateResult.CopyDataFileFail);
            return;
        }

        DeleteCacheFiles();
    }

    void DeleteCacheFiles()
    {
        try
        {
            GfxUtils.DeleteFolder(PathConfig.Instance.DownLoadCachePath);
        }
        catch
        {
            UpdateFinish(UpdateResult.CleanCacheFail);
            return;
        }

        m_curUpdateStep = UpdateStep.FINISH;
        UpdateFinish(UpdateResult.Success);
    }

    bool ReadUpdateInfo(string path, Dictionary<string, UpdateFileInfo> dic)
    {
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                string strLine = sr.ReadLine();
                while (!string.IsNullOrEmpty(strLine))
                {
                    JsonData jd = JsonMapper.ToObject(strLine) ;
                    UpdateFileInfo fileInfo = new UpdateFileInfo();
                    fileInfo.FileName = jd["fn"].ToString();
                    fileInfo.MD5 = jd["md"].ToString();
                    fileInfo.FileSize = 0;
                    int.TryParse(jd["size"].ToString(), out fileInfo.FileSize);
                    dic.Add(fileInfo.FileName, fileInfo);
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }

        }
        catch
        {
            //Debug.LogError(e.Message);
            return false;
        }
        return true;
    }

    void SetTips(string strTips)
    {
        m_UILogic.SetProgressTips(strTips);
    }

    void SetProgress(float fProgress)
    {
        m_UILogic.SetProgressTxt(fProgress);
    }

    IEnumerator RestartApp()
    {
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(29)+"...3");
        yield return new WaitForSeconds(1);
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(29)+ "...2");
        yield return new WaitForSeconds(1);
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(29)+ "...1");
        yield return new WaitForSeconds(1);
        PushNotificationConnector.restartApp();
    }

    void UpdateFinish(UpdateResult result)
    {
        //Debug.LogError(result.ToString());

        if (result == UpdateResult.DownloadFail)
        {
            m_UILogic.m_fTimeReTryDownload = Time.time + 5f;
            m_UILogic.SetProgressTips(UpdateTips.GetTipByID(13));
            m_UILogic.SetProgressTips3();
            m_curUpdateStep = UpdateStep.ReTryDownLoadRes;
        }
        else if(result == UpdateResult.Success)
        {
            m_UILogic.SetProgressTxt(1);

            if (m_fDownloadSize > 0 && !GfxLogicBrige.Instance.IsTiShenFu())
            {
                ChanConnector.SendKoreaInfoLog("downloadEnd");
                ChanConnector.ResourceUpdate(URLConfig.Instance.GetURLCDN(PathConfig.Instance.UpdateInfoFileName), "succ", "succ");  //AssetBundle更新成功
            }

            GameStatics.DeviceActivationMonitor((int)EDeviceActivationStep.HotUpdate);
            if (m_bRestart)
            {
                m_UILogic.StartCoroutine(RestartApp());
            }
            else
            {
                m_UILogic.BeginRequestServerLst();
            }
            return;
        }
        else
        {
            if (!GfxLogicBrige.Instance.IsTiShenFu())
            {
                if (result == UpdateResult.GetVersionFail)
                {
                     ChanConnector.ResourceUpdate(URLConfig.Instance.GetURLCDN(PathConfig.Instance.VersionFileName), $"get versioninfo fail: {URLConfig.Instance.GetURLCDN(PathConfig.Instance.VersionFileName)}", UpdateTips.GetUpdateErrorTip(result));
                }
                else if (result == UpdateResult.GetUpdateInfoFileFail)
                {
                    ChanConnector.ResourceUpdate(URLConfig.Instance.GetURLCDN(PathConfig.Instance.UpdateInfoFileName), $"get updateinfo fail: {URLConfig.Instance.GetURLCDN(PathConfig.Instance.UpdateInfoFileName)}", UpdateTips.GetUpdateErrorTip(result));
                }
                else
                {
                    ChanConnector.ResourceUpdate(result.ToString(), $"other fail {result}", UpdateTips.GetUpdateErrorTip(result));
                }
            }
            m_UILogic.SetProgressTips(UpdateTips.GetUpdateErrorTip(result));
            m_UILogic.SetProgressTips3();
        }


    }
}
