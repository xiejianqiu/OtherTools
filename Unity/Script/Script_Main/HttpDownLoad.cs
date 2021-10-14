using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class HttpDownLoad
{
    public float progress { get; private set; }

    public bool isDone { get; private set; }

    private bool isStop;

    public IEnumerator Start(string url, string filePath, Action<bool, string> callBack, Action<float> OnProgressChange)
    {
        this.isStop = false;
        Debug.Log($"### download {url}");
        var headRequest = UnityWebRequest.Head(url);

        yield return headRequest.SendWebRequest();
        if (!string.IsNullOrEmpty(headRequest.error)) {
            Debug.LogError($"{headRequest.error} {url}");
            if (null != callBack) {
                callBack(false, headRequest.error);
            }
            yield break;
        }
        var totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        long fileLength = 0;
        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fileLength = fs.Length;
            if (fileLength > 1024)
            {
                fileLength -= 1024;
            }
            else {
                fileLength = 0;
            }
            Debug.Log($"## BEGIN DOWNLOAD: {fileLength} {totalLength}");
            if (fileLength < totalLength)
            {
                fs.Seek(fileLength, SeekOrigin.Begin);

                var request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                request.SendWebRequest();

                var index = 0;
                long tickSpan = 10000000 / 5;
                long lastTicks = DateTime.Now.Ticks;
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
                    yield return null;
                    var buff = request.downloadHandler.data;
                    if (buff != null)
                    {
                        while (true)
                        {
                            var length = buff.Length - index;
                            if (length <= 0)
                            {
                                break;
                            }
                            fs.Write(buff, index, length);
                            index += length;
                            fileLength += length;

                            if (fileLength == totalLength)
                            {
                                progress = 1f;
                            }
                            else
                            {
                                progress = fileLength / (float)totalLength;
                            }
                            if (null != OnProgressChange)
                            {
                                OnProgressChange(progress);
                            }
                            if (DateTime.Now.Ticks - lastTicks >= tickSpan)
                            {
                                lastTicks = DateTime.Now.Ticks;
                                yield return null;
                            }
                        }
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

    public void Stop()
    {
        isStop = true;
    }
}