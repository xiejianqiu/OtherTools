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
                if (EnvUtils.IsDefault())
                {
                    PlayerSettings.productName = "太古神王";
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
                        Directory.Delete(dir, true);
                    }
                }
                var cfgFilePath = $"{dstDir}TChineseSDK.projmods";
                if (File.Exists(cfgFilePath))
                {
                    File.Delete(cfgFilePath);
                }
               
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
                if (EnvUtils.IsDefault())
                {
                    File.Copy($"{dstDir}NoCommon/LY_DEFAULT/TChineseWrapper.m", $"{dstDir}TChineseSDK/TChineseWrapper.m");
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
                    ExtractPatchRes(asset_path, pkgType, mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
                }
                if (EnvUtils.IsDefault())
                {
                    File.Copy($"{dstDir}NoCommon/LY_DEFAULT/logo-bingo.png", $"{outputPath}/logo-bingo.png");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"#### Build Fail: {ex.Message}");
            }

        }
    }
}
