using System.Collections;
using System;
using UnityEngine;
using System.IO;
using System.Reflection;

public class EditorGameLoad : GameLoadBase
{
    public override IEnumerator LoadAssembly()
    {
        yield return new WaitForEndOfFrame();
        
        FileStream fsdll = new FileStream(_hotfixdllPath,FileMode.Open);
        //FileStream fspdb = new FileStream(_hotfixpdbPath,FileMode.Open);
        
        byte [] buffReaddll = new byte[fsdll.Length];
        //byte [] buffReadpdb = new byte[fspdb.Length];
        fsdll.Read(buffReaddll, 0, (int)fsdll.Length);
        //fspdb.Read(buffReadpdb, 0, (int) fspdb.Length);
        Assembly assembly = Assembly.Load(buffReaddll,null);
        
        Type type = assembly.GetType("HotFixStart");
        object m_HotObj = assembly.CreateInstance("HotFixStart");
        MethodInfo mStart = type.GetMethod("Start");
        mStart.Invoke(m_HotObj,null);        
    }
}
