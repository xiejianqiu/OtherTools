using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ILRuntime.CLR.Method;
using UnityEngine;

public class GameLoadBase
{
    protected string _hotfixdllPath = Application.dataPath + "/BundleData/dll/hotfix.dll.bytes";
    protected string _hotfixpdbPath = Application.dataPath + "/BundleData/dll/hotfix.pdb.bytes";
    public static GameLoadBase CreateLoadLogic()
    {
        GameLoadBase vRet = null;
#if UNITY_EDITOR && USE_HOTFIX
        vRet = new HotFixEditorGameLoad();
#elif UNITY_EDITOR
        vRet = new EditorGameLoad();
        //HotFixStart.Start();
#elif  USE_HOTFIX
        vRet = new iOSGameLoad();
#else
        vRet = new AndroidGameLoad();
#endif        
        return vRet;
    }

    public virtual IEnumerator LoadDll()
    {
        yield return new WaitForEndOfFrame();
    }

    public virtual IEnumerator LoadAssembly()
    {
        yield return new WaitForEndOfFrame();
    }

    public virtual void OnLoadAssmblyOk()
    {
#if !UNITY_EDITOR && !USE_HOTFIX
        HotFixStart.Start();
#endif       
    }
    public void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        domain.RegisterCrossBindingAdaptor(new ICompareAdapter());
        domain.RegisterCrossBindingAdaptor(new ILRunTimeTypeICompareAdapter());
        domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
        domain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
        domain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
        AutoGeneralDelegate.Reg(domain);
        DelegateConvertorGen.Reg(domain);
        FunctionDelegateGen.Reg(domain);
        ILHelper.InitProtoBuf(domain);
    }
    protected void LoadILRuntime(byte[] buffdll, byte[] buffpdb)
    {
        ILRuntime.Runtime.Enviorment.AppDomain appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        if (buffpdb == null)
        {
            appdomain.LoadAssembly(new MemoryStream(buffdll), null, null);
        }
        else
        {
            appdomain.LoadAssembly(new MemoryStream(buffdll), new MemoryStream(buffpdb), new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        InitILRuntime(appdomain);
        appdomain.DebugService.StartDebugService(56000);
        GfxLogicBrige.Instance.appDomain = appdomain;

#if UNITY_EDITOR
        appdomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif

#if USE_HOTFIX
        ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
#endif
        IMethod startMethod = appdomain.GetType("HotFixStart").GetMethod("Start", 0); ;
        appdomain.Invoke(startMethod, null, null);
    }
}
