using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 多语言版本管理工具
/// </summary>
public class LanguageMgr
{
    public static LANGUAGE mCurLang = LANGUAGE.ChineseSimplified;
    private static LanguageMgr _Inst;
    public static LanguageMgr Inst {
        get {
            if (null == _Inst)
            {
                _Inst = new LanguageMgr();
            }
            return _Inst;
        }
    }
    /// <summary>
    /// 根据不同语种选择对应的表数据
    /// </summary>
    /// <returns></returns>
    public string GetDataTbABPath()
    {
        return GetTbPathForAB(mCurLang);
    }
    /// <summary>
    /// 根据不同语种选择不同的数据表
    /// </summary>
    /// <returns></returns>
    public string GetDataTbForSrc()
    {
        return GetTbPathForSrc(mCurLang);
    }
    /// <summary>
    /// 根据不同语种选择不同的更新提示
    /// </summary>
    /// <returns></returns>
    public string GetUpdateTipPath()
    {
        return GetUpdateTipsPath(mCurLang);
    }
    /// <summary>
    /// 根据不同语种选择不同的Prefab文本
    /// </summary>
    /// <returns></returns>
    public string GetPrefabLangPath()
    {
        return GetPrefabLangInfoPath(mCurLang);
    }
    /// <summary>
    /// 当前使用的语言
    /// </summary>
    /// <returns></returns>
    public string GetCurLanguage()
    {
        if (string.IsNullOrEmpty(GfxPlayerPrefsData.LastSelectLanguage))
        {
            return Application.systemLanguage.ToString();
        }
        else
        {
            return GfxPlayerPrefsData.LastSelectLanguage;
        }
    }
    /// <summary>
    /// 设置语音
    /// </summary>
    /// <param name="lang"></param>
    public void SetLanguage(LANGUAGE lang)
    {
        mCurLang = lang;
        GfxPlayerPrefsData.LastSelectLanguage = lang.ToString();
    }
    /// <summary>
    /// 自动设置语音版本
    /// </summary>
    public void AutoSetLanguage()
    {
        if (!string.IsNullOrEmpty(GfxPlayerPrefsData.LastSelectLanguage))
        {
            if (Enum.TryParse<LANGUAGE>(GfxPlayerPrefsData.LastSelectLanguage, out var lang))
            {
                SetLanguage(lang);
                return;
            }
        }
        switch (Application.systemLanguage)
        {
            case SystemLanguage.ChineseSimplified:
                SetLanguage(LANGUAGE.ChineseSimplified);
                break;
            case SystemLanguage.ChineseTraditional:
                SetLanguage(LANGUAGE.ChineseTraditional);
                break;
            case SystemLanguage.English:
                SetLanguage(LANGUAGE.English);
                break;
            case SystemLanguage.Korean:
                SetLanguage(LANGUAGE.Korean);
                break;
            case SystemLanguage.Thai:
                SetLanguage(LANGUAGE.Thai);
                break;
            case SystemLanguage.Vietnamese:
                SetLanguage(LANGUAGE.Vietnamese);
                break;
            default:
                SetLanguage(LANGUAGE.English);
                break;
        }
    }
    /// <summary>
    /// 获取AB对应的数据表路径
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    private string GetTbPathForAB(Enum enumValue)
    {
        string value = enumValue.ToString();
        FieldInfo field = enumValue.GetType().GetField(value);
        object[] objs = field.GetCustomAttributes(typeof(TBPathAttribute), false);  //获取描述属性
        if (objs == null || objs.Length == 0)  //当描述属性没有时，直接返回名称
            return "Tables/Pack";
        TBPathAttribute descriptionAttribute = (TBPathAttribute)objs[0];
        return descriptionAttribute.tbPathForAb;
    }
    /// <summary>
    /// 获取Unity中数据表路径
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    private string GetTbPathForSrc(Enum enumValue)
    {
        string value = enumValue.ToString();
        FieldInfo field = enumValue.GetType().GetField(value);
        object[] objs = field.GetCustomAttributes(typeof(TBPathAttribute), false);  //获取描述属性
        if (objs == null || objs.Length == 0)  //当描述属性没有时，直接返回名称
            return "/BundleData/Tables/";
        TBPathAttribute descriptionAttribute = (TBPathAttribute)objs[0];
        return descriptionAttribute.tbPathForSrc;
    }
    /// <summary>
    /// 获取多语言版本更新提示文件
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    private string GetUpdateTipsPath(Enum enumValue)
    {
        string value = enumValue.ToString();
        FieldInfo field = enumValue.GetType().GetField(value);
        object[] objs = field.GetCustomAttributes(typeof(TBPathAttribute), false);  //获取描述属性
        if (objs == null || objs.Length == 0)  //当描述属性没有时，直接返回名称
            return "Tips/UpdateTips";
        TBPathAttribute descriptionAttribute = (TBPathAttribute)objs[0];
        return descriptionAttribute.UpdateTIps;
    }
    /// <summary>
    /// 获取多语言Prefab文本信息
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    private string GetPrefabLangInfoPath(Enum enumValue)
    {
        string value = enumValue.ToString();
        FieldInfo field = enumValue.GetType().GetField(value);
        object[] objs = field.GetCustomAttributes(typeof(TBPathAttribute), false);  //获取描述属性
        if (objs == null || objs.Length == 0)  //当描述属性没有时，直接返回名称
            return "Tables/PrefabLangInfo.txt";
        TBPathAttribute descriptionAttribute = (TBPathAttribute)objs[0];
        return descriptionAttribute.PrefabLangInfo;
    }
}
