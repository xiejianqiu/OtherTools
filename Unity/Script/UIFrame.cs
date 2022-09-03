using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class UIFrame : MonoBehaviour {

    private UILabel m_lblFrame = null;
    private UILabel m_lblMem = null;

    private const string PLUGIN_NAME = "AndroidMemory";

    private float m_fDeltaTime = 0f;
    private int nFramCount = 0;
    private AttributeList m_AttriList;

#if UNITY_IOS && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int GetFreeMemory();
	[DllImport("__Internal")]
	private static extern int GetCurPorMem();
#endif
    private void Awake()
    {
        InitAttr();
    }
    private void InitAttr()
    {
        m_AttriList = this.GetComponent<AttributeList>();
        m_lblFrame = m_AttriList.m_lstLabel[0];
        m_lblMem = m_AttriList.m_lstLabel[1];
    }

    // Use this for initialization
    void Start () {
        m_lblFrame.text = "Frame";
        m_lblMem.text = "Mem";
	}
	
	// Update is called once per frame
	void Update () {

        nFramCount++;
        m_fDeltaTime += Time.deltaTime;
        if(m_fDeltaTime>=1f)
        {
            m_fDeltaTime = 0f;
            m_lblFrame.text = nFramCount.ToString();
            m_lblFrame.color = nFramCount > 28 ? Color.green : (nFramCount > 10 ? Color.yellow : Color.red);
            nFramCount = 0;

            m_lblMem.text = GetMemoryInfo();
        }
	}


    string GetMemoryInfo()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return GetCurPorMem().ToString()+"/" + GetFreeMemory().ToString();
#else
        return ((GC.GetTotalMemory(false)+ UnityEngine.Profiling.Profiler.usedHeapSizeLong ) / (1024 * 1024)).ToString();
#endif

    }

    private string GetAndroidMemory()
    {
        return "1";
//         int andoirdMemory = AndroidFuction.CallPluginMethod<int>(PLUGIN_NAME, "getRunningAppProcessInfo");
//         return andoirdMemory.ToString();
    }
}
