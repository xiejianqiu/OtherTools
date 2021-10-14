using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class MyZipTool
{
    public enum UnZipStep{
        Begin,
        Progress,
        Fail,
        Finish,
    }
    public static bool UnZipFile(byte[] ZipByte, string outPath, string password = "")
    {
        bool result = true;
        FileStream fs = null;
        ZipInputStream zipStream = null;
        ZipEntry ent = null;
        string fileName;


        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        try
        {
            //直接使用 将byte转换为Stream，省去先保存到本地在解压的过程
            Stream stream = new MemoryStream(ZipByte);
            zipStream = new ZipInputStream(stream);

            if (!string.IsNullOrEmpty(password))
            {
                zipStream.Password = password;
            }
            while ((ent = zipStream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(ent.Name))
                {
                    fileName = Path.Combine(outPath, ent.Name);

                    #region      Android
                    fileName = fileName.Replace('\\', '/');

                    if (fileName.EndsWith("/"))
                    {
                        Directory.CreateDirectory(fileName);
                        continue;
                    }
                    #endregion
                    fs = File.Create(fileName);

                    int size = 2048;
                    byte[] data = new byte[size];
                    while (true)
                    {
                        size = zipStream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            //fs.Write(data, 0, data.Length);
                            fs.Write(data, 0, size);//解决读取不完整情况
                        }
                        else
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            result = false;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }
            if (zipStream != null)
            {
                zipStream.Close();
                zipStream.Dispose();
            }
            if (ent != null)
            {
                ent = null;
            }
            GC.Collect();
            GC.Collect(1);
        }
        return result;
    }
    public static bool UnZipFile(string zipFile, string outPath, string password = "")
    {
        bool result = true;
        ZipEntry ent = null;
        string fileName;


        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        try
        {
            Stream stream = File.OpenRead(zipFile);
            using (ZipInputStream zipStream = new ZipInputStream(stream))
            {
                if (!string.IsNullOrEmpty(password))
                {
                    zipStream.Password = password;
                }
                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(outPath, ent.Name);


                        fileName = fileName.Replace('\\', '/');

                        if (fileName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        using (FileStream fs = File.Create(fileName))
                        {
                            int size = 2048;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = zipStream.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    //fs.Write(data, 0, data.Length);
                                    fs.Write(data, 0, size);//解决读取不完整情况
                                }
                                else
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            result = false;
        }
        finally
        {
            if (ent != null)
            {
                ent = null;
            }
            GC.Collect();
            GC.Collect(1);
        }
        return result;
    }
    public static IEnumerator UnZipFileCo(string zipFile, string outPath, string password = "", Action<float, UnZipStep> OnProgressChange = null)
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
            long allWriteSize = 0;
            int size = 1024;
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
                                }
                                else
                                    break;
                                if (DateTime.Now.Ticks - lastTicks >= tickSpan)
                                {
                                    if (null != OnProgressChange)
                                    {
                                        OnProgressChange(curProgress, UnZipStep.Progress);
                                    }
                                    lastTicks = DateTime.Now.Ticks;
                                    yield return null;
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
