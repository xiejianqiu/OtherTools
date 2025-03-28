using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        [MenuItem("jenkins/build/BuildForIOSDebug")]
        private static void BuildForIOSDebug()
        {
            mPargmaTypeCheck.Init(new string[] {"ABNameWithHash",
                                    "false",
                                    "StripLvl",
                                    "Disabled",
                                    "StripEngineCode",
                                    "false",
                                    "UseIL2CPP",
                                    "false",
                                    "AssetLoadType",
                                    "PKG",
                                    "BuildPatch",
                                    "false",
                                    "AbdetailVer",
                                    "1_baseVer",
                                    "Platform",
                                    "iOS",
                                    "BuildRes",
                                    "false",
                                    "ResVer",
                                    "1.0.0.1",
                                    "IsDebug",
                                    "true",
                                    "BuildPkgTypeAPK",
                                    "IsBuildPkg",
                                    "true",
                                    "VersionName",
                                    "1.0.0",
                                    "VersionCode",
                                    "1",
                                    "PkgType",
                                    "AllRes",
                                    "RemoveManifest",
                                    "true" });
            BuildMain();
        }
        [MenuItem("jenkins/build/BuildForIOSRelease")]
        private static void BuildForIOSRelease()
        {
            mPargmaTypeCheck.Init(new string[] {"ABNameWithHash",
                                    "false",
                                    "StripLvl",
                                    "Disabled",
                                    "StripEngineCode",
                                    "false",
                                    "UseIL2CPP",
                                    "false",
                                    "AssetLoadType",
                                    "PKG",
                                    "BuildPatch",
                                    "false",
                                    "AbdetailVer",
                                    "1_baseVer",
                                    "Platform",
                                    "iOS",
                                    "BuildRes",
                                    "false",
                                    "ResVer",
                                    "1.0.0.1",
                                    "IsDebug",
                                    "false",
                                    "BuildPkgTypeAPK",
                                    "IsBuildPkg",
                                    "true",
                                    "VersionName",
                                    "1.0.0",
                                    "VersionCode",
                                    "1",
                                    "PkgType",
                                    "AllRes",
                                    "RemoveManifest",
                                    "true" });
            BuildMain();
        }
        /// <summary>
        /// xcode 导出逻辑
        /// </summary>
        [MenuItem("jenkins/build/BuildForIOS")]
        private static void BuildForIOS()
        {
            try
            {
                if (EnvUtils.IsLyBT1Y())
                {
                    PlayerSettings.productName = "太古神王2（0.1折无限充）";
                }
                else if (EnvUtils.IsLySZCX())
                {
                    PlayerSettings.productName = "天命契灵";
                }
                else if (EnvUtils.IsU1Game())
                {
                    PlayerSettings.productName = "剑与神灵";
                }
                else if (EnvUtils.IsMiaoLe())
                {
                    PlayerSettings.productName = "凡人修仙";
                    if (EnvUtils.IsYJQY())
                    {
                        PlayerSettings.productName = "倚剑奇缘";
                    }
                    else if (EnvUtils.IsJYZT())
                    {
                        PlayerSettings.productName = "剑佑诸天";
                    }
                    else if (EnvUtils.IsJZTX())
                    {
                        PlayerSettings.productName = "剑指天下";
                    }
                }
                else if (EnvUtils.IsMLQuick())
                {
                    PlayerSettings.productName = "凡人修仙-增强现实版";
                }
                bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
                PlayerGenerator.isOniOSDebugBuild = IsDebug;
                string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = $"{Application.dataPath}/../../xcodeproject";
                }
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
                PlayerGenerator.isOniOSDebugBuild = IsDebug;
                var dstDir = "./Assets/Editor/XUPorter/Mods/";
                var dirtyDirs = new string[] { $"{dstDir}TChineseSDK", $"{dstDir}zhengshu", $"{dstDir}NoCommon" };
                foreach (var dir in dirtyDirs)
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir,true);
                    }
                }
                var cfgFilePath = $"{dstDir}TChineseSDK.projmods";
                if (File.Exists(cfgFilePath))
                {
                    File.Delete(cfgFilePath);
                }
                if (EnvUtils.IsLYLoveFun())
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/LoveFun/", dstDir);
                }
                else if(EnvUtils.IsU1Game())
                {
                    PlayerSettings.applicationIdentifier = ChanConnector.PKG_IOS_U1Game;
                    PlayerGenerator.CopyDirectory("./iOSSDK/U1GAME/", dstDir);
                }
                else if (EnvUtils.IsMiaoLe())
                {
                    PlayerSettings.applicationIdentifier = ChanConnector.PKG_IOS_MiaoLe;
                    PlayerGenerator.CopyDirectory("./iOSSDK/MiaoLe/", dstDir);
                }
                else if (EnvUtils.IsMLQuick())
                {
                    PlayerSettings.applicationIdentifier = ChanConnector.PKG_IOS_MiaoLe;
                    PlayerGenerator.CopyDirectory("./iOSSDK/Quick/", dstDir);
                }
                else
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/TChinese/", dstDir);
                    string sdkCodeFile = $"{dstDir}TChineseSDK/TChineseWrapper.m";
                    string logoFile = $"{dstDir}TChineseSDK/logo-bingo.png";
                    if (File.Exists(sdkCodeFile))
                    {
                        File.Delete(sdkCodeFile);
                    }
                    if (File.Exists(logoFile))
                    {
                        File.Delete(logoFile);
                    }
                    if (EnvUtils.IsLYZX())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_ZX/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyBT())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_BT/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyBT1Y())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_BT_1Y/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyBTX7())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_BT_X7/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyBTGO())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_BT_GO/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyBTLZ())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_BT_LZ/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLySZCX())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_SZCX/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else if (EnvUtils.IsLyManDi())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_MANDI/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                    }
                    else
                    {
                        if (EnvUtils.IsLYSelfNoQG())
                        {
                            File.Copy($"{dstDir}NoCommon/LY_SELF_NOQG/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                        }
                        else
                        {
                            File.Copy($"{dstDir}NoCommon/LY_SELF/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
                        }

                    }
                }

                List<string> lstScenes = new List<string>();
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                if (scenes != null)
                {
                    int ct = 2;
                    for (int i = 0; i < ct; i++)
                    {
                        if (scenes[i] != null && scenes[i].enabled)
                        {
                            lstScenes.Add(scenes[i].path);
                        }
                    }
                }
                string[] allScene = lstScenes.ToArray();

                BuildOptions opts = BuildOptions.None;
                if (IsDebug)
                {
                    opts |= BuildOptions.Development;
                }
                if (EnvUtils.IsUseProfiler())
                {
                    opts |= BuildOptions.ConnectWithProfiler;
                }
                BuildPipeline.BuildPlayer(allScene, outputPath, BuildTarget.iOS, opts);
                Debug.Log($"################ BuildPlayer END  {outputPath} IsDev:{EnvUtils.IsDEVELOPMENT_BUILD()},{IsDebug}");
                if (Enum.TryParse<PkgType>(mPargmaTypeCheck.GetValue(PargmaType.PkgType), true, out var pkgType))
                {
                    string asset_path = outputPath + "/Data/Raw/AssetBundles";
                    ExtractPatchRes(asset_path, pkgType,mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
                }
                if (EnvUtils.IsLYZX())
                {
                    File.Copy($"{dstDir}NoCommon/LY_ZX/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyBT())
                {
                    File.Copy($"{dstDir}NoCommon/LY_BT/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyBT1Y())
                {
                    File.Copy($"{dstDir}NoCommon/LY_BT_1Y/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyBTX7())
                {
                    File.Copy($"{dstDir}NoCommon/LY_BT_X7/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyBTGO())
                {
                    File.Copy($"{dstDir}NoCommon/LY_BT_GO/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyBTLZ())
                {
                    File.Copy($"{dstDir}NoCommon/LY_BT_LZ/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLySZCX())
                {
                    File.Copy($"{dstDir}NoCommon/LY_SZCX/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsLyManDi())
                {
                    File.Copy($"{dstDir}NoCommon/LY_MANDI/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else if (EnvUtils.IsU1Game())
                {
                    File.Copy("./iOSSDK/U1GAME/TChineseSDK/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                }
                else
                {
                    if (EnvUtils.IsLYSelfNoQG())
                    {
                        File.Copy($"{dstDir}NoCommon/LY_SELF_NOQG/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                    }
                    else
                    {
                        File.Copy($"{dstDir}NoCommon/LY_SELF/logo-bingo.png", $"{outputPath}/logo-bingo.png", true);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"#### Build Fail: {ex.Message}");
            }

        }
    }
}
