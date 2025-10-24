using UnityEngine;
#if USE_HOTFIX
[Beebyte.Obfuscator.SkipRename]
#endif
public class AndroidFuction
{
    private const string SDK_JAVA_CLASS = "com.bingougame.sdk.SdkManager";
    private const string UNITY_JAVA_CLASS = "com.bingougame.tgsw.UnityPlayerActivity";
    /// <summary>
    /// 访问渠道sdk
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="objs"></param>
    /// <returns>json或字符串</returns>
#if USE_HOTFIX
    [Beebyte.Obfuscator.SkipRename]
#endif
    public static T CallChannelMethod<T>(string functionName, params object[] objs)
    {
        if (EnvUtils.IsCustomAccout())
            return default(T);

#if ANDROID_LOCAL_BUILD
         return default(T);
#endif
        using (AndroidJavaClass cls = new AndroidJavaClass(SDK_JAVA_CLASS))
        {
            AndroidJavaObject jSdkManager = cls.CallStatic<AndroidJavaObject>("getInstance");
            AndroidJavaObject jChannel = jSdkManager.Get<AndroidJavaObject>("currentChannel");
            return jChannel.Call<T>(functionName, objs);
        }
    }
#if USE_HOTFIX
    [Beebyte.Obfuscator.SkipRename]
#endif
    public static void CallChannelMethod(string functionName, params object[] objs)
    {
        if (EnvUtils.IsCustomAccout())
            return;
#if ANDROID_LOCAL_BUILD
         return ;
#endif
        using (AndroidJavaClass cls = new AndroidJavaClass(SDK_JAVA_CLASS))
        {
            AndroidJavaObject jSdkManager = cls.CallStatic<AndroidJavaObject>("getInstance");
            AndroidJavaObject jChannel = jSdkManager.Get<AndroidJavaObject>("currentChannel");
            jChannel.Call(functionName, objs);
            return;
        }
    }

#if USE_HOTFIX
    [Beebyte.Obfuscator.SkipRename]
#endif
    public static T CallUnityPlayerActivityMethod<T>(string functionName, params object[] objs)
    {
        if (EnvUtils.IsCustomAccout())
            return default(T);
#if ANDROID_LOCAL_BUILD
         return ;
#endif
        using (AndroidJavaClass cls = new AndroidJavaClass(UNITY_JAVA_CLASS))
        {
            AndroidJavaObject jObj = cls.CallStatic<AndroidJavaObject>("getInstance");
            return jObj.Call<T>(functionName, objs);
        }
    }

    /// <summary>
    /// 访问功能sdk
    /// </summary>
    /// <param name="pluginName"></param>
    /// <param name="functionName"></param>
    /// <param name="objs"></param>
    /// <returns>json或字符串</returns>
#if USE_HOTFIX
    [Beebyte.Obfuscator.SkipRename]
#endif
    public static T CallPluginMethod<T>(string pluginName, string functionName, params object[] objs)
    {
        if (EnvUtils.IsCustomAccout())
            return default(T);
#if ANDROID_LOCAL_BUILD
         return default(T);
#endif
        using (AndroidJavaClass cls = new AndroidJavaClass(SDK_JAVA_CLASS))
        {
            AndroidJavaObject jSdkManager = cls.CallStatic<AndroidJavaObject>("getInstance");
            AndroidJavaObject jPlugin = jSdkManager.Call<AndroidJavaObject>("getPlugin", pluginName);
            return jPlugin.Call<T>(functionName, objs);
        }
    }



}
