using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using LitJson;
using Shark;
using UnityEngine.Networking;
using ICSharpCode.SharpZipLib.Zip;

public class Patch {
    public string name;
    public string md5;
    public string size;
}
public class PatchCfg {
    public string version;
    public Patch[] patchs;
}
partial class ResDownload
{
    PatchCfg m_PatchCfg;
    public long TotalPatchSize = 0;
    private bool HaveTipSpace = false;
    IEnumerator CheckAndDownloadPatch()
    {
        yield return m_UILogic.StartCoroutine(DownPatchCfg());
        List<Patch> dlLst = new List<Patch>();
        yield return m_UILogic.StartCoroutine(CalAndGetDownloadPatch(dlLst));
        if (!HaveTipSpace && TotalPatchSize > 0)
        {
            var totalLength = TotalPatchSize;
            string strTips = string.Format(UpdateTips.GetTipByID(33), totalLength * 1f / 1024 / 1024);

            //磁盘空间大小检查
            var needSize = totalLength * 2 - ChanConnector.getAvaliableStorageSize();
            Debug.LogError($"ShowDownloadResZipTip AvaliableStorageSize:{ChanConnector.getAvaliableStorageSize()}, needSize:{needSize}");
            if (needSize > 0)
            {
                strTips += $"[ff0000]{string.Format(UpdateTips.GetTipByID(305), totalLength * 2 * 1f / 1024 / 1024)}[-]";
            }
            ChanConnector.SendGameInfoLog("basezip_donwload", "basezip_donwload", "basezip_donwload_show_tips", "basezip_donwload_show_tips");
            basePatchLength = totalLength;
            m_UILogic.ShowMessageBox(strTips, () => {
                if (dlLst.Count > 0)
                {
                    m_UILogic.StartCoroutine(DownMultiBatch(dlLst));
                }
                else
                {
                    m_UILogic.StartCoroutine(UnzipPatch(m_PatchCfg));
                }
            });
        }
        else
        {
            if (dlLst.Count > 0)
            {
                yield return new WaitForEndOfFrame();
                m_UILogic.StartCoroutine(DownMultiBatch(dlLst));
            }
            else
            {
                yield return new WaitForEndOfFrame();
                m_UILogic.StartCoroutine(UnzipPatch(m_PatchCfg));
            }
        }
    }
    /// <summary>
    /// 下载基础包配置文件
    /// </summary>
    /// <returns></returns>
    public IEnumerator DownPatchCfg() {
        if (null != m_PatchCfg) {
            yield break;
        }
        string firstUrl = URLConfig.Instance.GetURLCDN(DownloadHelper.AddTimestampToUrl(PathConfig.Instance.MultiPatchCfg));
        var request = UnityWebRequest.Get(firstUrl);
        yield return request.SendWebRequest();
        string patchCfgTxt = request.downloadHandler.text;
        if (string.IsNullOrEmpty(request.error))
        {
            if (EnvUtils.IsDEVELOPMENT_BUILD() || EnvUtils.IsUnity_Editor())
            {
                Debug.Log($"DownBatchCfg: {patchCfgTxt}");
            }
            try
            {
                JsonData jd = JsonMapper.ToObject(patchCfgTxt);
                if (null != jd)
                {
                    m_PatchCfg = JsonMapper.ToObject<PatchCfg>(jd.ToJson());
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        else
        {
            Debug.LogError($"### {request.error}");
        }
    }
    /// <summary>
    /// 根据本地已下载的补丁包，计算出哪些补丁包需要重新下载
    /// </summary>
    /// <param name="lst"></param>
    IEnumerator CalAndGetDownloadPatch(List<Patch> lst) {
        TotalPatchSize = 0;
        for (int index = 0; index < m_PatchCfg.patchs.Length; index++)
        {
            m_UILogic.SetProgress(index * 1f / m_PatchCfg.patchs.Length);
            var patch = m_PatchCfg.patchs[index];
            var cachePatch = PathConfig.Instance.DownLoadCachePath + "/" + patch.name;
            if (File.Exists(cachePatch))
            {
                if (GfxUtils.GetMD5Hash(cachePatch).Equals(patch.md5))
                {
                    continue;
                }
                else
                {
                    File.Delete(cachePatch);
                }
            }
            yield return new WaitForEndOfFrame();
            if (long.TryParse(patch.size, out var size))
            {
                TotalPatchSize += size;
            }
            else 
            {
                Debug.LogError($"{patch.name} Size[{patch.size}] invaild.");
            }
            lst.Add(patch);
        }
    }
    /// <summary>
    /// 开始下载补丁包
    /// </summary>
    IEnumerator DownMultiBatch(List<Patch> lst) {
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(200));
        m_curUpdateStep = UpdateStep.DownBaseRes;
        bool downloadPatchFail = false;
        string errorTip = string.Empty;
        yield return new HttpDownLoad().Start(
                this.m_UILogic,
                lst,
                PathConfig.Instance.DownLoadCachePath,
                (result, error)=> {
                    if (!result) {
                        downloadPatchFail = true;
                        if (!string.IsNullOrEmpty(error))
                        {
                            errorTip = string.Format(UpdateTips.GetTipByID(34), error);
                        }
                        else
                        {
                            errorTip = UpdateTips.GetTipByID(201);
                        }
                    }
                },
                (f)=> {
                    m_UILogic.SetProgress(f);
                },
                (downloadedSize, allSize, sizePerSec) => {
                    string netTip = string.Empty;
                    if (sizePerSec > 1024 * 1024)
                    {
                        netTip = string.Format(UpdateTips.GetTipByID(111), downloadedSize * 1f / 1024 / 1024, allSize * 1f / 1024 / 1024, sizePerSec / 1024 / 1024, GfxLogicBrige.Instance.GameVersion);
                    }
                    else
                    {
                        netTip = string.Format(UpdateTips.GetTipByID(110), downloadedSize * 1f / 1024 / 1024, allSize * 1f / 1024 / 1024, sizePerSec / 1024, GfxLogicBrige.Instance.GameVersion);
                    }
                    m_UILogic.SetProgressTips3(netTip);
                }
            );
        m_UILogic.SetProgressTips3();
        yield return new WaitForEndOfFrame();
        if (downloadPatchFail)
        {
            m_UILogic.ShowMessageBox(errorTip, () => { 
                m_UILogic.StartCoroutine(CheckAndDownloadPatch()); 
            });
        }
        else
        {
            m_UILogic.StartCoroutine(CheckAndDownloadPatch());
        }
    }
    IEnumerator UnzipPatch(PatchCfg patchCfg) {
        List<Patch> lst = new List<Patch>();
        foreach (var patch in patchCfg.patchs) {
            lst.Add(patch);
        }
        m_UILogic.SetProgressTips(UpdateTips.GetTipByID(300));
        yield return null;
        yield return m_UILogic.StartCoroutine(MyZipTool.UnZipFileCo(lst, PathConfig.Instance.PersistenPath, OnProgressChange: (f, result, msg) =>
        {
            if (CurUnZipStep != result)
            {
                CurUnZipStep = result;
            }
            if (result == MyZipTool.UnZipStep.Finish)
            {
                Debug.Log($"### Extract File Finish. {System.DateTime.Now}");
            }
            else if (result == MyZipTool.UnZipStep.Progress)
            {
                this.m_UILogic.SetProgressTxt(f);
            }
            else if (result == MyZipTool.UnZipStep.Fail)
            {
                errorMsg = msg;
                if (errorMsg.Contains("Disk full"))
                {
                    m_UILogic.SetProgress(1f);
                    m_UILogic.SetProgressTips(UpdateTips.GetTipByID(304));
                    m_UILogic.ShowMessageBox(UpdateTips.GetTipByID(304), () =>
                    {
                    });
                }
                else
                {
                    m_UILogic.SetProgress(1f);
                    m_UILogic.SetProgressTips(UpdateTips.GetTipByID(301));
                    m_UILogic.ShowMessageBox(UpdateTips.GetTipByID(302), () =>
                    {
                        CheckUpdateBasePatch();
                    });
                }
            }
        }, OnUnzipSpdChange: (sizePerSec) =>
        {
            string errorTip = string.Format(UpdateTips.GetTipByID(303), sizePerSec * 1f / 1024 / 1024);
            m_UILogic.SetProgressTips3(errorTip);
        }));

        File.Move(PathConfig.Instance.PersistenPath + PathConfig.Instance.VersionFileName, PathConfig.Instance.LocalVersionFilePath);
        if (Directory.Exists(PathConfig.Instance.DownLoadCachePath))
        {
            Directory.Delete(PathConfig.Instance.DownLoadCachePath, true);
        }
        this.SetProgress(1f);
        ReCheckResVersion();
    }
}
