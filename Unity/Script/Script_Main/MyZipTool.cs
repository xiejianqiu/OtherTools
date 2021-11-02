using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Threading;
public static class MyZipTool
{
    public enum UnZipStep{
        None,
        Begin,
        Progress,
        Fail,
        Finish,
        End,
    }
    public static void UnZipFile(MonoBehaviour host, string zipFile, string outPath, bool  useThread = false, string password = "", Action<float, UnZipStep> OnProgressChange = null, Action<long> OnUnzipSpdChange = null) {
        if (useThread)
        {
            new Thread(()=> {
                try
                {
                    UnZipFileThread(zipFile, outPath, password, OnProgressChange, OnUnzipSpdChange);
                }
                catch (Exception e) {
                    Debug.LogError($"### MyZipTool {zipFile}, {e.ToString()}");
                    OnProgressChange(1f, UnZipStep.Fail);
                }
            }).Start();
        }
        else
        {
            host.StartCoroutine(UnZipFileCo(zipFile, outPath, password, OnProgressChange, OnUnzipSpdChange));
        }
    }
    private static void UnZipFileThread(string zipFile, string outPath, string password = "", Action<float, UnZipStep> OnProgressChange = null, Action<long> OnUnzipSpdChange = null) {
        ZipEntry ent = null;
        string fileName = string.Empty;

        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        int nCountOfFiles = 10000;
        long allFileSize = 10000;
        try
        {
            Stream stream = File.OpenRead(zipFile);
            using (ZipInputStream zipTmpStream = new ZipInputStream(stream))
            {
                nCountOfFiles = 0;
                while ((ent = zipTmpStream.GetNextEntry()) != null)
                {
                    nCountOfFiles += 1;
                    allFileSize += zipTmpStream.Length;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"## UnZipFileCo  {e.ToString()}");
            if (null != OnProgressChange)
            {
                OnProgressChange(1f, UnZipStep.Fail);
            }
            return;
        }
        float curProgress = 0f;
        if (null != OnProgressChange)
        {
            OnProgressChange(curProgress, UnZipStep.Begin);
        }
        Stream stream2 = File.OpenRead(zipFile);
        using (ZipInputStream zipStream = new ZipInputStream(stream2))
        {
            if (!string.IsNullOrEmpty(password))
            {
                zipStream.Password = password;
            }
            Debug.Log($"### UnZipFileCo extract file count: nCountOfFiles:{nCountOfFiles} allFileSize:{allFileSize * 1.0f / 1024 / 1024}M");
            long tickSpan = 10000000 / 5;
            long lastTicks = DateTime.Now.Ticks;
            long lastSecondTicks = 0;
            long allWriteSize = 0;
            long unzipSizePerSec = 0;
            int size = 1<<22;
            byte[] data_buffer = new byte[size];
            while (true)
            {
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

                        if (fileName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }
                        if (File.Exists(fileName))
                        {
                            using (var f = File.Open(fileName, FileMode.Open))
                            {
                                if (f.Length == zipStream.Length)
                                {
                                    allWriteSize += zipStream.Length;
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
                                    lastSecondTicks = DateTime.Now.Ticks;
                                }
                                else if (ticks >= tickSpan)
                                {
                                    if (null != OnProgressChange)
                                    {
                                        OnProgressChange(curProgress, UnZipStep.Progress);
                                    }
                                    lastTicks = DateTime.Now.Ticks;
                                }
                            }

                        }
                    }
                }
            }
            if (null != OnProgressChange)
            {
                OnProgressChange(1f, UnZipStep.Finish);
            }
        }
    }
    private static IEnumerator UnZipFileCo(string zipFile, string outPath, string password = "", Action<float, UnZipStep> OnProgressChange = null, Action<long> OnUnzipSpdChange=null)
    {
        ZipEntry ent = null;
        string fileName = string.Empty;

        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        int nCountOfFiles = 10000;
        long allFileSize = 10000;
        try
        {
            Stream stream = File.OpenRead(zipFile);
            using (ZipInputStream zipTmpStream = new ZipInputStream(stream))
            {
                nCountOfFiles = 0;
                while ((ent = zipTmpStream.GetNextEntry()) != null)
                {
                    nCountOfFiles += 1;
                    allFileSize += zipTmpStream.Length;
                }
            }
        }
        catch (Exception e) {
            Debug.LogError($"## UnZipFileCo  {e.ToString()}");
            if (null != OnProgressChange)
            {
                OnProgressChange(1f, UnZipStep.Fail);
            }
            yield break;
        }
        float curProgress = 0f;
        if (null != OnProgressChange)
        {
            OnProgressChange(curProgress, UnZipStep.Begin);
        }
        Stream stream2 = File.OpenRead(zipFile);
        using (ZipInputStream zipStream = new ZipInputStream(stream2))
        {
            if (!string.IsNullOrEmpty(password))
            {
                zipStream.Password = password;
            }
            Debug.Log($"### UnZipFileCo extract file count: nCountOfFiles:{nCountOfFiles} allFileSize:{allFileSize * 1.0f / 1024 /1024}M");
            long tickSpan = 10000000 / 5;
            long lastTicks = DateTime.Now.Ticks;
            long lastSecondTicks = 0;
            long allWriteSize = 0;
            long unzipSizePerSec = 0;
            int size = 1 << 22;
            byte[] data_buffer = new byte[size];
            while (true)
            {
                {
                    ent = zipStream.GetNextEntry();
                    if (null == ent) {
                        break;
                    }
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(outPath, ent.Name);
                        fileName = fileName.Replace('\\', '/');

                        if (fileName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }
                        if (File.Exists(fileName))
                        {
                            using (var f = File.Open(fileName, FileMode.Open))
                            {
                                if (f.Length == zipStream.Length)
                                {
                                    allWriteSize += zipStream.Length;
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
                                        OnProgressChange(curProgress, UnZipStep.Progress);
                                    }

                                    yield return new WaitForEndOfFrame();
                                    lastTicks = DateTime.Now.Ticks;
                                }
                            }

                        }
                    }
                }
            }
            if (null != OnProgressChange)
            {
                OnProgressChange(1f, UnZipStep.Finish);
            }
        }
    }
}
