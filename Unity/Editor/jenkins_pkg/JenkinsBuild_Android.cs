using Jenkins;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        /// <summary>
        /// 构建android aab或者apk逻辑
        /// </summary>
        [MenuItem("jenkins/BuildForAndroid")]
        private static void BuildForAndroid()
        {
            #region 测试命令构建流程
            #endregion
            if (mPargmaTypeCheck.IsTrue(PargmaType.UseIL2CPP))
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.IL2CPP);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.Mono2x);
            }
            PlayerSettings.productName = "tgsw2";
            bool IsContainX86 = false;
            if (EnvUtils.IsLyBTX7())
            {
                IsContainX86 = true;
            }
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP)
            {
                if (IsContainX86)
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                }
                else
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                }
            }
            else
            {
                if (IsContainX86)
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
                }
                else
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                }
            }
            try
            {
                if (Enum.TryParse<PkgType>(mPargmaTypeCheck.GetValue(PargmaType.PkgType), true, out var pkgType))
                {
                    bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
                    if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT))
                    {
                        Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT, true);
                        Directory.CreateDirectory(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT);
                    }
                    BuildOptions opts = BuildOptions.AcceptExternalModificationsToPlayer;
                    if (IsDebug)
                    {
                        opts |= BuildOptions.Development;
                    }
                    if (EnvUtils.IsUseProfiler())
                    {
                        opts |= BuildOptions.ConnectWithProfiler;
                    }
                    var resport = BuildPipeline.BuildPlayer(AndroidBuildTool.GetBuildScenes(mPargmaTypeCheck.IsTrue(PargmaType.IsABTest)).ToArray(), AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT, BuildTarget.Android, opts);
                   
                }
                else
                {
                    Debug.LogError($"################ Parse PkgType Failed. {mPargmaTypeCheck.GetValue(PargmaType.PkgType)}");
                }
                if (Enum.TryParse<BuildPkgType>(mPargmaTypeCheck.GetValue(PargmaType.BuildPkgType), out var result))
                {
                    if (result == BuildPkgType.APKAndAAB)
                    {
                        CopyFiles(pkgType, true);
                        CopyFiles(pkgType, false);
                    }
                    if (result == BuildPkgType.APK)
                    {
                        CopyFiles(pkgType, false);
                    }
                    if (result == BuildPkgType.AAB)
                    {
                        CopyFiles(pkgType, true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"### Build Fail:{e}");
            }
        }
        private static void CopyFiles(PkgType pkgType, bool IsBuildAAB)
        {
            AndroidBuildTool.Config.IS_GOOGLE_PLAY = IsBuildAAB;
            AndroidBuildTool.Config.IsTgswOs = mPargmaTypeCheck.IsTrue(PargmaType.UseTgswOS);
            #region 删除androidstudio项目的数据
            if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/bin/Data"))
            {
                Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/bin/Data", true);
            }

            if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/AssetBundles"))
            {
                Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/AssetBundles", true);
            }
            if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets/AssetBundles"))
            {
                Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets/AssetBundles", true);
            }
            if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/fast_follow_asset_pack/src/main/assets/AssetBundles"))
            {
                Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/fast_follow_asset_pack/src/main/assets/AssetBundles", true);
            }
            if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/output"))
            {
                Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/output", true);
            }
            #endregion
            ExtractPatchRes(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/assets/AssetBundles", pkgType, mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
             AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/assets", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets");
            if (AndroidBuildTool.Config.IS_GOOGLE_PLAY)
            {
                //AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/assets", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets");
                Directory.Move(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/AssetBundles", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets/AssetBundles");
                if (pkgType == PkgType.NoRes)
                {
                    if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets/AssetBundles"))
                    {
                        Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/../install_time_pack/src/main/assets/AssetBundles", true);
                    }
                    if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/fast_follow_asset_pack/src/main/assets/AssetBundles"))
                    {
                        Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/fast_follow_asset_pack/src/main/assets/AssetBundles", true);
                    }
                }
            }
            else
            {
                if (pkgType == PkgType.NoRes)
                {
                    if (Directory.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/AssetBundles"))
                    {
                        Directory.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/assets/AssetBundles", true);
                    }
                }

            }
            //icon
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/drawable", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/drawable");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/drawable-xhdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/drawable-xhdpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-ldpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-ldpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-hdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-hdpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-mdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-mdpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-xhdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-xhdpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-xxhdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-xxhdpi");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/res/mipmap-xxxhdpi", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/res/mipmap-xxxhdpi");


            //ndk libs
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/jniLibs/armeabi-v7a", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/jniLibs/armeabi-v7a");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/jniLibs/arm64-v8a", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/jniLibs/arm64-v8a");
            AndroidBuildTool.CopyFolder(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/src/main/jniLibs/x86", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/jniLibs/x86");

            //jar
            AndroidBuildTool.CopyFile(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/libs", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/libs", "*.jar");

            //生成配置文件
            //game.properties
            Dictionary<string, string> propertiesDict = new Dictionary<string, string>();
            propertiesDict.Add("versionName", Application.version);
            propertiesDict.Add("versionCode", PlayerSettings.Android.bundleVersionCode.ToString());
            AndroidBuildTool.CreateProperties(propertiesDict, AndroidBuildTool.Config.PROJECT_GAME_PROPERTIES_PATH);
            //File.Delete(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/src/main/jniLibs/armeabi-v7a/libmono.so");
            //File.Copy(AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL+ "/libmono.so",AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL+ "/src/main/jniLibs/armeabi-v7a/libmono.so");

            //复制local.properties，里面是unity编辑器中设置的android adk的路径，
            if (File.Exists(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/local.properties"))
            {
                File.Copy(AndroidBuildTool.Config.PROJECT_GOOGLE_EXPORT + "/" + PlayerSettings.productName + "/local.properties", AndroidBuildTool.Config.PROJECT_GOOGLE_MODEL + "/local.properties", true);
            }

        }
    }
}
