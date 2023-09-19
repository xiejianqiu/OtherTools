using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 强制设置Unity游戏窗口的宽高比,可以调整设置窗口的大小,会强制保持一定比例
/// 通过拦截窗口大小调整事件(WindowProc回调)并相应地修改它们来实现
/// 也可以用像素为窗口设置最大 or 最小宽度和高度
/// 宽高比和最大 or 最小分辨率都和窗口区域有关.标题栏和边框不包括在内
/// 该脚本还将在应用程序处于全屏状态时强制设置宽高比,当切换到全屏,应用程序自动获取显示器上的最大分辨率,但仍然保持原有宽高比,
/// 如果显示器没有相同宽高比,则会在左/右或上/下添加黑条,确保在PlayerSetting中设置了"Resizable Window",否则无法调整大小
/// 如果取消不支持的宽高比在PlayerSetting中设置"Supported Aspect Rations"
/// 注意:因为使用WinAPI,所以只能在window上工作
/// </summary>
public class AspectRatioController : MonoBehaviour
{
    public static AspectRatioController Inst;
    [Serializable]
    public class ResolutionChangedEvent : UnityEvent<int, int, bool> { }

    public Action QuitBackCall = null;

    #region 参数
    public ResolutionChangedEvent resolutionChangedEvent;

    [SerializeField]
    private bool allowFullscreen = true;
    [SerializeField]
    private float aspectRatioWidth = 16;
    [SerializeField]
    private float aspectRatioHeight = 9;
    [SerializeField]
    private int minWidthPixel = 512;
    [SerializeField]
    private int minHeightPixel = 288;
    [SerializeField]
    private int maxWidthPixel = 2048;
    [SerializeField]
    private int maxHeightPixel = 1152;
    private float aspect;
    private int setWith = -1;
    private int setHeight = -1;
    private bool wasFullscreenLastFrame;
    public bool IsCanQuit { get; set; }
    private bool quitStarted = false;
    private int pixelWidthOfCurrentScreen;
    private int pixelHeightOfCurrentScreen;

    #endregion 参数

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    #region WinAPI

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private const int WM_SIZING = 0x214;
    private const int WMSZ_LEFT = 1;
    private const int WMSZ_RIGHT = 2;
    private const int WMSZ_TOP = 3;
    private const int WMSZ_BOTTOM = 6;

    private const int GMLP_WNDPROC = -4;
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private WndProcDelegate wndProcDelegate;

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumProc, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);
    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, ref Rect lpRect);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    private const string UNITY_WND_CLASSNAME = "UnityWndClass";
    private IntPtr unityHWnd;
    private IntPtr oldWndProcPtr;
    private IntPtr newWndProcPtr;

    #endregion WinAPI
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

#else

#endif

    #region UnityAPI
    void Awake()
    {
        Inst = this;
    }
    void Start()
    {
        //注册回调,应用程序想要退出时相应5.6版本不存在这个函数,尝试在OnApplicationQuit调用
#if !UNITY_EDITOR
        UnityEngine.Application.wantsToQuit += ApplicationWantsToQuit;
#endif
        //不要在Unity编辑模式中注册WindowProc回调函数,它会指向Unity编辑窗口的,而不是Game视图
#if UNITY_STANDALONE_WIN

        EnumThreadWindows(GetCurrentThreadId(), (hWnd, lParam) =>
        {
            StringBuilder classText = new StringBuilder(UNITY_WND_CLASSNAME.Length + 1);
            GetClassName(hWnd, classText, classText.Capacity);
            if (classText.ToString().Equals(UNITY_WND_CLASSNAME))
            {
                unityHWnd = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);

        SetAspectRatio(aspectRatioWidth, aspectRatioHeight, true);

        wasFullscreenLastFrame = Screen.fullScreen;
        wndProcDelegate = wndProc;
        newWndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
        oldWndProcPtr = SetWindowLong(unityHWnd, GMLP_WNDPROC, newWndProcPtr);

        IsCanQuit = false;
#elif UNITY_STANDALONE_OSX

#else

#endif
    }

    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (!allowFullscreen && Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }

        if (Screen.fullScreen && !wasFullscreenLastFrame)
        {
            int height, width;
            bool blackBarsLeftRight = aspect < (float)pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;
            if (blackBarsLeftRight)
            {
                height = pixelHeightOfCurrentScreen;
                width = Mathf.RoundToInt(height * aspect);
            }
            else
            {
                width = pixelWidthOfCurrentScreen;
                height = Mathf.RoundToInt(width / aspect);
            }

            Screen.SetResolution(width, height, true);
            resolutionChangedEvent.Invoke(width, height, true);
        }
        else if (!Screen.fullScreen && wasFullscreenLastFrame)
        {
            Screen.SetResolution(setWith, setHeight, false);
            resolutionChangedEvent.Invoke(setWith, setHeight, false);
        }
        else if (!Screen.fullScreen && setWith != -1 && setHeight != -1 && (Screen.width != setWith || Screen.height != setHeight))
        {
            //根据宽高比设置宽度,因为Aero Snap 不会触发WM_SIZING
            setHeight = Screen.height;
            setWith = Mathf.RoundToInt(setHeight * aspect);
            Screen.SetResolution(setWith, setHeight, false);
            resolutionChangedEvent.Invoke(setWith, setHeight, false);
        }
        else if (!Screen.fullScreen)
        {
            pixelHeightOfCurrentScreen = Screen.currentResolution.height;
            pixelWidthOfCurrentScreen = Screen.currentResolution.width;
        }
        wasFullscreenLastFrame = Screen.fullScreen;
#if UNITY_EDITOR
        if (Screen.width != setWith || Screen.height != setHeight)
        {
            setHeight = Screen.height;
            setWith = Screen.width;
        }
#endif

#elif UNITY_STANDALONE_OSX

#else

#endif
    }

    //正常的退出会执行该函数,但是类似于杀进程可能不会调用
//    void OnApplicationQuit()
//    {
//        //注册回调,应用程序想要退出时相应5.6版本不存在这个函数,尝试在OnApplicationQuit调用
//        //UnityEngine.Application.wantsToQuit += ApplicationWantsToQuit;
//#if UNITY_STANDALONE_WIN
//        ApplicationWantsToQuit(); //5.6需要这么写
//#elif UNITY_STANDALONE_OSX

//#else

//#endif
//    }
    #endregion UnityAPI

    #region RatioCtrl
    private void SetAspectRatio(float newAspectWidth, float newAspectHeight, bool apply)
    {
        aspectRatioWidth = newAspectWidth;
        aspectRatioHeight = newAspectHeight;
        aspect = aspectRatioWidth / aspectRatioHeight;

        if (apply)
        {
            Screen.SetResolution(Screen.width, Mathf.RoundToInt(Screen.width / aspect), Screen.fullScreen);
        }
    }

    /// <summary>
    /// 退出时Quit被调用
    /// </summary>
    /// <returns>true:确认退出, false:取消退出 </returns>
    private bool ApplicationWantsToQuit()
    {
        if(!IsCanQuit)
        {
            //Application.CancelQuit();
            return false;
        }
        if (QuitBackCall != null)
        {
            QuitBackCall.Invoke();
            //Application.CancelQuit();
            return false;
        }

        if (!quitStarted)
        {
            StartCoroutine("DelayQuit");
            //Application.CancelQuit();
            return false;
        }
        return true;
    }

    IEnumerator DelayQuit()
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        SetWindowLong(unityHWnd, GMLP_WNDPROC, oldWndProcPtr);
        #endif
        yield return new WaitForEndOfFrame();
        quitStarted = true;
        Application.Quit();
    }
#if UNITY_STANDALONE_WIN

    private IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_SIZING)//resize事件
        {
            Rect rect = (Rect)Marshal.PtrToStructure(lParam, typeof(Rect));

            Rect windowRect = new Rect();
            GetWindowRect(unityHWnd, ref windowRect);

            Rect clientRect = new Rect();
            GetClientRect(unityHWnd, ref clientRect);

            int borderWidth = windowRect.Right - windowRect.Left - clientRect.Right + clientRect.Left;
            int borderHeight = windowRect.Bottom - windowRect.Top - clientRect.Bottom + clientRect.Top;

            rect.Right -= borderWidth;
            rect.Bottom -= borderHeight;

            int newWidth = Mathf.Clamp(rect.Right - rect.Left, minWidthPixel, maxWidthPixel);
            int newHeight = Mathf.Clamp(rect.Bottom - rect.Top, minHeightPixel, maxHeightPixel);

            switch (wParam.ToInt32())
            {
                case WMSZ_LEFT:
                case WMSZ_LEFT + WMSZ_BOTTOM:
                    rect.Left = rect.Right - newWidth;
                    rect.Bottom = rect.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_RIGHT:
                case WMSZ_RIGHT + WMSZ_BOTTOM:
                    rect.Right = rect.Left + newWidth;
                    rect.Bottom = rect.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_TOP:
                    rect.Top = rect.Bottom - newHeight;
                    rect.Right = rect.Left + Mathf.RoundToInt(newHeight * aspect);
                    break;
                case WMSZ_BOTTOM:
                    rect.Bottom = rect.Top + newHeight;
                    rect.Right = rect.Left + Mathf.RoundToInt(newHeight * aspect);
                    break;
                case WMSZ_RIGHT + WMSZ_TOP:
                    rect.Right = rect.Left + newWidth;
                    rect.Top = rect.Bottom - Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_LEFT + WMSZ_TOP:
                    rect.Left = rect.Right - newWidth;
                    rect.Top = rect.Bottom - Mathf.RoundToInt(newWidth / aspect);
                    break;
            }

            setWith = rect.Right - rect.Left;
            setHeight = rect.Bottom - rect.Top;

            rect.Right += borderWidth;
            rect.Bottom += borderHeight;

            resolutionChangedEvent.Invoke(setWith, setHeight, Screen.fullScreen);

            Marshal.StructureToPtr(rect, lParam, true);
        }

        return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
    }

    private IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLong64(hWnd, nIndex, dwNewLong);
    }

#elif UNITY_STANDALONE_OSX

#endif
#endregion RatioCtrl

    public override string ToString()
    {
        string text = string.Empty;
        text += "started: " + IsCanQuit + " , quitStarted: " + quitStarted + "\n";
        text += "allowFullscreen: " + allowFullscreen + " , aspect: " + aspect + "\n wasFullscreenLastFrame: " + wasFullscreenLastFrame + "\n";
        text += "(setWith: " + setWith + " , setHeight: " + setHeight + ") \n";
        text += "(minWidthPixel: " + minWidthPixel + " , minHeightPixel: " + minHeightPixel + ") \n ";
        text += "(maxWidthPixel: " + maxWidthPixel + " , maxHeightPixel: " + maxHeightPixel + ")\n";
        text += " (aspectRatioWidth: " + aspectRatioWidth + " , aspectRatioHeight: " + aspectRatioHeight + ")\n";
        text += "(currentScreen: " + pixelWidthOfCurrentScreen + " , currentScreen: " + pixelHeightOfCurrentScreen + ")\n";
        return text;
    }
}
