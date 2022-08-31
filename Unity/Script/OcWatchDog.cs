using System;
using UnityEngine;
public class OcWatchDog : IDisposable
{
    private string mDesc = "";
    private DateTime mStartTime;
    public static void RecordPorcessTime(string desc) {
#if DEVELOPMENT_BUILD||UNITY_EDITOR || UNITY_ANDROID
        Debug.Log($"<color=blue>### {desc} {DateTime.Now}</color>");
#endif
    }
    public OcWatchDog(string desc)
    {
        this.mDesc = desc;
        mStartTime = DateTime.Now;
    }
    public void Dispose()
    {
#if DEVELOPMENT_BUILD||UNITY_EDITOR  || UNITY_ANDROID
        Debug.Log($"<color=green>### {mDesc}  StartTime: {mStartTime} EndTime:{DateTime.Now} UseTime:{(DateTime.Now - mStartTime).Seconds}</color>");
#endif
    }
}
