using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Jenkins
{
    public enum PargmaType {
        Platform,//Android,iOS,WebGL
        BuildRes,//构建打包所有资源
        ResVer,//资源版本号
        IsDebug,//是否为debug包
        BuildPkgType,//AAB,APK,APKAndAAB,IOS
        IsBuildPkg, //构建安装包或者导出xcode或者webgl工程
        VersionName,//版本号
        VersionCode,//版本Code
        PkgType,//AllRes：整包（带有全部资源），HalfRes：中包（带部分资源），NoRes：小包（不带资源）
        OutputPath,//导出路径
        RemoveManifest,//是否删除资源清单
        BuildPatch,//构建补丁
        AbdetailVer,//基础资源清单存放目录
        AssetLoadType,//WEBGL首包资源存放在CDN：0, 本地：1
        UseIL2CPP,//将脚本端切换到IL2CPP
        StripEngineCode,//裁剪引擎代码
        StripLvl,//裁剪级别从Disabled,Low,Medium,Hight
        ABNameWithHash,//ab名称是否带有hash
        UseHotFix,//是否使用代码热更,
        PkgName,//设置包名
        Macro,//设置宏
        BuildMode,//构建模式，3:构建所有资源，2：构建表数据，1：构建构建热更代码
        IsABTest,//是否未AB测试包
        UseTgswOS,//android使用tgsw_os
        EncryAB,//是否加密AB资源
        ObfucateCode,//是否混淆代码
        /// <summary>
        /// 是否生成自定义资源包
        /// </summary>
        BuildResPkg,
        /// <summary>
        /// 资源包名称
        /// </summary>
        PkgFileName,
    }
    public enum PkgType { 
        AllRes,
        HalfRes,
        NoRes
    }
    public enum BuildMode
    {
        BuildAll=0,
        BuidHotfix = 1,
        BuildTb=2,
        BuildTbAndHotfix=3,
    }
    public enum AssetLoadType
    {
        CDN,
        PKG,
    }
    public enum BuildPkgType
    { 
        APK,
        AAB,
        APKAndAAB,
        IOS
    }
    public class PargmaTypeCheck {
        private Dictionary<PargmaType, string> argsDict;

        public PargmaTypeCheck()
        {
            this.argsDict = new Dictionary<PargmaType, string>();
        }
        public void Init(string[] args) {
            for (int index = 1; index < args.Length; index++)
            {
                try
                {
                    Debug.Log($"################ {index}:{args[index]}");
                    if (Enum.TryParse<PargmaType>(args[index], out var val))
                    {
                        argsDict[val] = "";
                        if (index + 1 < args.Length)
                        {
                            if (int.TryParse(args[index + 1], out var iVal) || !Enum.TryParse<PargmaType>(args[index + 1], out var val2))
                            {
                                argsDict[val] = args[index + 1];
                                index += 1;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            //=============打印参数信息========================
            StringBuilder logBuilder = new StringBuilder();
            var iter = argsDict.GetEnumerator();
            while (iter.MoveNext()) {
                logBuilder.AppendLine($"{iter.Current.Key} {iter.Current.Value}");
            }
            Debug.Log($"################ 透传参数信息:{argsDict.Count}\n{logBuilder.ToString()}");
        }
        /// <summary>
        /// 该函数只对布尔型参数有效
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTrue(PargmaType type) {

            return argsDict.ContainsKey(type) && "true" == argsDict[type];
        }
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetValue(PargmaType type) {
            if (argsDict.ContainsKey(type))
            {
                return argsDict[type];
            }
            return string.Empty;
        }
        public bool HasKey(PargmaType type) {
            return argsDict.ContainsKey(type);
        }
        public void SetValue(PargmaType type,string val)
        {
            argsDict[type] = val;
        }
    }
}
