using LitJson;
using System;
using UnityEngine;

public partial class WebGLStartUp
{
    /// <summary>
    /// 微端下载地址
    /// </summary>
    private const string clientDLURL = "https://yxdt.game.keniub.com/weiduan/tgsw2cwbol-360.exe";
    public readonly static WebGLStartUp Instance = new WebGLStartUp();
    public bool IsInit { get; private set; }
    /// 用户id
    /// </summary>
    public string uid { get; private set; }
    /// <summary>
    /// 区服id
    /// </summary>
    public string skey { get; private set; }
    /// <summary>
    /// 0 用户未填写实名制信息 1 用户填写过实名制信息，且大于18岁 2 用户填写过实名制信息，但是小于18岁
    /// </summary>
    public string is_adult { get; private set; }
    
    public string CommandLine { get; private set; }
    public string[] CommandLineArgs { get; private set; }
    public string role_not_auto { get; private set; }
    public bool isDev { get; private set; }
    public bool IsClient { get; private set; }
    /// <summary>
    /// 合作伙伴，360:360，迅雷：xl，晴空：qk，联想：lenovo
    /// </summary>
    public string cp { get; private set; }
    /// <summary>
    /// 渠道名称
    /// </summary>
    public string channelName { get; private set; }
    public string token { get; private set; }

    /// <summary>
    /// 联想透传参数
    /// </summary>
    public string lpms { get; private set; }

    /// <summary>
    /// 加密后的联想用户id
    /// </summary>
    public string encLenovoId { get; private set; }
    /// <summary>
    /// 时间
    /// </summary>
    public string time { get; private set; }
    /// <summary>
    /// 签名
    /// </summary>
    public string sign { get; private set; }
    public void Start()
    {
        this.InitEvent();
        CommandLine = Environment.CommandLine;
        CommandLineArgs = Environment.GetCommandLineArgs();
        //uid = GetURLParam("qid");
        //skey = GetURLParam("server_id");
        //is_adult = GetURLParam("isAdult");
        Debug.Log($"URL:{Application.absoluteURL}, {CommandLine}");
        var url = Application.absoluteURL;
        var array = url.Split('?');
        if (null == array || array.Length < 2)
        {
            Debug.Log("URL中没有参数");
            return;
        }
        IsClient = url.Contains("client=pc&version=3");
        var pargmaArray = array[1].Split('&');
        foreach (var pargma in pargmaArray)
        {
            string[] arr = pargma.Split('=');
            if (arr.Length < 2)
                continue;
            if (arr[0] == "qid" || arr[0] == "userId")
            {
                this.uid = arr[1];
                continue;
            }
            if (arr[0] == "server_id")
            {
                this.skey = arr[1];
                continue;
            }
            if (arr[0] == "isAdult")
            {
                this.is_adult = arr[1];
                continue;
            }
            if (arr[0] == "issDev")
            {
                this.isDev = "1" == arr[1];
                continue;
            }
            if (arr[0] == "cp" || arr[0] == "ext")
            {
                this.cp = arr[1];
                if (this.cp == CP.CP_XL)
                {
                    this.channelName = "xl";
                }
                Debug.Log("=====>登陆cp：" + this.cp);
                continue;
            }
            if (arr[0] == "platform")
            {
                this.channelName = arr[1];
                continue;
            }
            if (arr[0] == "token" || arr[0] == "authKey")
            {
                this.token = arr[1];
                continue;
            }
            if (arr[0] == "client" && "pc" == arr[1])
            {
                IsClient = true;
            }
            if (arr[0] == "zk")
            {
                GfxLogicBrige.Instance.IsZK = "1" == arr[1];
            }
            if (arr[0] == "lpms")
            {
                this.lpms = arr[1];
                Debug.Log("=====>联想登陆lpms：" + this.lpms);
                continue;
            }
            if(arr[0] == "time")
            {
                this.time = arr[1];
            }
            if(arr[0] == "sign")
            {
                this.sign = arr[1];
            }
        }
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(skey))
        {
            Debug.Log("360 未检查到uid和skey! 请检查游戏是否配置了websocket启动");
            return;
        }
        if (EnvUtils.IsWebGLLenovo())
        {
            this.encLenovoId = this.token;
        }
        this.IsInit = true;
    }
    /// <summary>
    /// 添加接口用于下载微端
    /// </summary>
    public void DLClent()
    {
        Application.OpenURL(clientDLURL);
    }
    public string GetPayUrl()
    {
        if (EnvUtils.IsWebGLQK())
        {
            return "https://pay11.52gg.com/tgsw2?";
        }
        if (EnvUtils.IsWebGL360())
        {
            return "https://iap.g.360-api.cn/mini.html?";
        }
        return string.Empty;
    }
//#if UNITY_WEBGL && !UNITY_EDITOR
//    [System.Runtime.InteropServices.DllImport("__Internal")]
//    private static extern void superPay(string cpOrderId, string gameId, string productName, int price, string server_id, string role_id, string role_name, string other);
//#else
//    private static void superPay(string cpOrderId, string gameId, string productName, int price, string server_id, string role_id, string role_name, string other)
//    {
//        Debug.Log($"Unexpect SuperPay");
//    }
//#endif
    /// <summary>
    /// 通过js支付
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="serverId"></param>
    /// <param name="roleId"></param>
    public void PayByJs(JsonData jd)
    {
        if (null != jd)
        {
            string cpOrderId = (string)jd["cpOrderId"];
            string gameId = (string)jd["gameId"];
            string productName = (string)jd["productName"];
            int price = (int)jd["price"];
            string server_id = (string)jd["server_id"];
            string role_id = (string)jd["role_id"];
            string role_name = (string)jd["role_name"];
            string other = (string)jd["other"];
            Application.ExternalCall("superPay", cpOrderId, gameId, productName, price, server_id, role_id, role_name, other);
        }
    }

    /// <summary>
    /// 调用联想支付
    /// </summary>
    /// <param name="jd"></param>
    public void LenovoPayByJs(JsonData jd)
    {
        if (null != jd)
        {
            string partner = (string)jd["partner"];
            string notifyUrl = (string)jd["notifyUrl"];
            string outTradeNo = (string)jd["outTradeNo"];
            string encLenovoId = (string)jd["encLenovoId"];
            string gameId = (string)jd["gameId"];
            string extraCommonParam = (string)jd["extraCommonParam"];
            string server = (string)jd["server"];
            string role = (string)jd["role"];
            string lpms = (string)jd["lpms"];
            string totalFee = (string)jd["totalFee"];
            Application.ExternalCall("lenovoPay", partner, notifyUrl, outTradeNo, encLenovoId, gameId, extraCommonParam, server, role, lpms, totalFee);
        }
    }

    /// <summary>
    /// 百度支付
    /// </summary>
    /// <param name="jd"></param>
    public void BaiduPayByJs(JsonData jd)
    {
        if (null != jd)
        {
            string gameId = (string)jd["gameId"];
            string cpServerId = (string)jd["cpServerId"];
            string activeAccount = (string)jd["activeAccount"];
            string roleId = (string)jd["roleId"];
            string cpExtra = (string)jd["cpExtra"];
            Application.ExternalCall("baiduPay", gameId, cpServerId, activeAccount, roleId, cpExtra);
        }
    }

    /// <summary>
    /// 粘贴函数
    /// </summary>
    public void PasteFromClipboard()
    {
        Application.ExternalCall("PasteFromClipboard");
    }
    /// <summary>
    /// 复制文本到剪贴板
    /// </summary>
    /// <param name="text"></param>
    public void CopyToClipboard(string text)
    {
        if (EnvUtils.IsCustomAccout())
        {
            Debug.Log($"CopyToClipboard: {text}");
        }
        Application.ExternalCall("CopyToClipboard", text);
    }
    /// <summary>
    /// js粘贴回调
    /// </summary>
    /// <param name="text"></param>
    public void OnPasteFromClipboard(string text)
    {
        if (EnvUtils.IsCustomAccout())
        {
            Debug.Log($"OnPasteFromClipboard: {null != UICamera.selectedObject} {text}");
        }
        if (null != UICamera.selectedObject)
        {
            UIInput focusedInput = UICamera.selectedObject.GetComponent<UIInput>();
            if (focusedInput != null)
            {
                focusedInput.value = text;
            }
            else
            {
                Debug.Log($"OnPasteFromClipboard 未选中UIInput");
            }
        }
    }
    /// <summary>
    /// 用于监听UIInput焦点状态变化
    /// </summary>
    /// <param name="isSelected"></param>
    private void OnUIInfputSelectStatusChange(bool isSelected)
    {
#if UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = !isSelected;
#endif
        if (isSelected)
        {
            UIInput focusedInput = UICamera.selectedObject.GetComponent<UIInput>();
            var content = null != focusedInput?focusedInput.value:"";
            Application.ExternalCall("UnityShowInputBox", content);
        }
        else
        {
            Application.ExternalCall("UnityHideInputBox");
        }
    }
    /// <summary>
    /// html中的输入
    /// </summary>
    /// <param name="text"></param>
    public void OnReceiveInput(string text)
    {
        if (EnvUtils.IsCustomAccout())
        {
            Debug.Log($"OnReceiveInput: {null != UICamera.selectedObject} {text}");
        }
        if (null != UICamera.selectedObject)
        {
            UIInput focusedInput = UICamera.selectedObject.GetComponent<UIInput>();
            if (focusedInput != null)
            {
                //focusedInput.CustomInsert(text);
                focusedInput.value = text;
            }
            else
            {
                Debug.Log($"OnReceiveInput 未选中UIInput");
            }
        }
    }
    private void InitEvent()
    {
        GfxLogicBrige.GfxEventManager.RegisterEvent<string>(GfxEventType.Gfx_OnPasteFromClipboard, OnPasteFromClipboard);
        GfxLogicBrige.GfxEventManager.RegisterEvent<bool>(GfxEventType.Gfx_UIInputStatusChange, OnUIInfputSelectStatusChange);
        GfxLogicBrige.GfxEventManager.RegisterEvent<string>(GfxEventType.Gfx_OnReceiveInputTxt, OnReceiveInput);
    }
}