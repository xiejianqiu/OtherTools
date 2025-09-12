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
                else if (EnvUtils.IsU1GameQk())
                {
                    PlayerSettings.productName = "Wind Blade Edge";
                    PlayerSettings.applicationIdentifier = "com.divinity.ios";
                }
                else if (EnvUtils.IsU1Game() || EnvUtils.IsCustomAccout())
                {
                    if (EnvUtils.IsU1Game())
                    {
                        PlayerSettings.productName = "Blade of Divinity";
                        PlayerSettings.applicationIdentifier = ChanConnector.PKG_IOS_U1Game;
                    }
                    else
                    {
                        PlayerSettings.applicationIdentifier = ChanConnector.PKG_IOS_U1GameNew;
                        PlayerSettings.productName = "赛博仙道之命运的气息";
                    }
                }
                else if (EnvUtils.IsFrxxKorea())
                {
                    PlayerSettings.productName = "벨라토르M: 불멸의 검";
                    PlayerSettings.applicationIdentifier = "com.gamerepublckor.apple.lodastra";
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
                else if (EnvUtils.IsU1Game() || EnvUtils.IsCustomAccout())
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/U1GAME/", dstDir);
                }
                else if (EnvUtils.IsU1GameQk())
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/QuickGame/", dstDir);
                }
                else
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/Korea/SDk/", dstDir);
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
            }
            catch (Exception ex)
            {
                Debug.LogError($"#### Build Fail: {ex.Message}");
            }

        }
    }
}
