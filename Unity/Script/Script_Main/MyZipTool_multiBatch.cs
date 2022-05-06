using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static partial class MyZipTool

{
    public static IEnumerator UnZipFileCo(List<Patch> patchLst, string outPath, string password = "", Action<float, UnZipStep, string> OnProgressChange = null, Action<long> OnUnzipSpdChange = null)
    {
        ZipEntry ent = null;
        int nCountOfFiles = 0;
        long allFileSize = 0;
        try
        {
            foreach (var patch in patchLst)
            {
                string zipFile = $"{PathConfig.Instance.DownLoadCachePath}/{patch.name}";
                Stream stream = File.OpenRead(zipFile);
                stream.Position = 0;
                using (ZipInputStream zipTmpStream = new ZipInputStream(stream))
                {
                    while ((ent = zipTmpStream.GetNextEntry()) != null)
                    {
                        nCountOfFiles += 1;
                        allFileSize += zipTmpStream.Length;
                        //throw new Exception("Disk full");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"### UnZipFileCo  {e.ToString()}");
            if (null != OnProgressChange)
            {
                OnProgressChange(1f, UnZipStep.Fail, e.ToString());
            }
            yield break;
        }
        Debug.Log($"### UnZipFileCo extract file count: nCountOfFiles:{nCountOfFiles} allFileSize:{allFileSize * 1.0f / 1024 / 1024}M");
        float curProgress = 0f;
        long tickSpan = 10000000 / 10;
        long lastTicks = DateTime.Now.Ticks;
        long lastSecondTicks = 0;
        long allWriteSize = 0;
        long unzipSizePerSec = 0;
        int size = 1 << 22;
        byte[] data_buffer = new byte[size];
        int iCount = 0;
        foreach (var patch in patchLst)
        {
            string fileName = string.Empty;
            string zipFile = $"{PathConfig.Instance.DownLoadCachePath}/{patch.name}";
            if (null != OnProgressChange)
            {
                OnProgressChange(curProgress, UnZipStep.Begin, "");
            }
            if (EnvUtils.IsDEVELOPMENT_BUILD() || EnvUtils.IsUnity_Editor())
            {
                Debug.Log($"### Begin UnzipPatch:{patch.name}");
            }
            Stream stream2 = File.OpenRead(zipFile);
            stream2.Position = 0;
            using (ZipInputStream zipStream = new ZipInputStream(stream2))
            {
                if (!string.IsNullOrEmpty(password))
                {
                    zipStream.Password = password;
                }
                while (true)
                {
                    ent = zipStream.GetNextEntry();
                    if (null == ent)
                    {
                        break;
                    }
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(outPath, ent.Name);
                        fileName = fileName.Replace('\\', '/');
                        var dir = Path.GetDirectoryName(fileName);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                            continue;
                        }
                        if (File.Exists(fileName))
                        {
                            using (var f = File.Open(fileName, FileMode.Open))
                            {
                                if (f.Length == zipStream.Length)
                                {
                                    allWriteSize += zipStream.Length;
                                    unzipSizePerSec += zipStream.Length;
                                    if (iCount % 100 == 0)
                                    {
                                        iCount = 0;
                                        if (null != OnProgressChange)
                                        {
                                            curProgress = allWriteSize * 1f / allFileSize;
                                            OnProgressChange(curProgress, UnZipStep.Progress, "");
                                        }
                                        var ticks = DateTime.Now.Ticks - lastSecondTicks;
                                        if (ticks >= 10000000)
                                        {
                                            if (null != OnUnzipSpdChange)
                                            {
                                                OnUnzipSpdChange((long)(unzipSizePerSec * 1f / (ticks * 1f / 10000000)));
                                                unzipSizePerSec = 0;
                                            }
                                            lastSecondTicks = DateTime.Now.Ticks;
                                        }
                                        yield return new WaitForEndOfFrame();
                                    }
                                    iCount += 1;
                                    continue;
                                }
                            }
                        }
                        using (FileStream fs = File.Create(fileName))
                        {
                            Array.Clear(data_buffer, 0, data_buffer.Length);
                            while (true && ent.Size > 0)
                            {
                                size = zipStream.Read(data_buffer, 0, data_buffer.Length);
                                if (size > 0)
                                {
                                    //fs.Write(data, 0, data.Length);
                                    fs.Write(data_buffer, 0, size);//解决读取不完整情况
                                    allWriteSize += size;
                                    curProgress = allWriteSize * 1f / allFileSize;
                                    unzipSizePerSec += size;
                                }
                                else
                                    break;

                                var ticks = DateTime.Now.Ticks - lastSecondTicks;
                                if (ticks >= 10000000)
                                {
                                    if (null != OnUnzipSpdChange)
                                    {
                                        OnUnzipSpdChange((long)(unzipSizePerSec * 1f / (ticks * 1f / 10000000)));
                                        unzipSizePerSec = 0;
                                    }
                                    yield return new WaitForEndOfFrame();
                                    lastSecondTicks = DateTime.Now.Ticks;
                                }
                                else if (ticks >= tickSpan)
                                {
                                    if (null != OnProgressChange)
                                    {
                                        OnProgressChange(curProgress, UnZipStep.Progress, "");
                                    }
                                    yield return new WaitForEndOfFrame();
                                    lastTicks = DateTime.Now.Ticks;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (null != OnProgressChange)
        {
            OnProgressChange(1f, UnZipStep.Finish, "");
        }
    }
}
