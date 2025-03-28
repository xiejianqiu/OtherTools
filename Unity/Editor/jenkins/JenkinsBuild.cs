using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
#if UNITY_IOS
using UnityEditor.iOS;
#endif
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        static string[] Platforms = { "Android", "iOS", "WebGL", "StandaloneWindows" };
        private static PargmaTypeCheck mPargmaTypeCheck = new PargmaTypeCheck();
        public static void BuildMain()
        {
            try
            {
                PlayerSettings.SplashScreen.showUnityLogo = false;
                UpdateProjectSetting(false);
                //PlayerSettings.productName = @"太古神王2";
                mPargmaTypeCheck.Init(Environment.GetCommandLineArgs());
                bool buildRes = mPargmaTypeCheck.IsTrue(PargmaType.BuildRes);
                string Platform = mPargmaTypeCheck.GetValue(PargmaType.Platform);

                string versionName = mPargmaTypeCheck.GetValue(PargmaType.VersionName);
                string versionCode = mPargmaTypeCheck.GetValue(PargmaType.VersionCode);
                bool RemoveManifest = mPargmaTypeCheck.IsTrue(PargmaType.RemoveManifest);
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                SetAppIcons();
                #region 安装包版号设置
                PlayerSettings.bundleVersion = versionName;
                if (EnvUtils.IsUNITY_ANDROID())
                {
                    PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
                }
                else if (EnvUtils.IsUNITY_IOS())
                {
                    PlayerSettings.iOS.buildNumber = versionCode;
                }
                #endregion
                //#region 平台检查
                BuildTargetGroup buildTarGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                #region 构建资源
                if (buildRes)
                {
                    BuildMode builMode = BuildMode.BuildAll;
                    Enum.TryParse<BuildMode>(mPargmaTypeCheck.GetValue(PargmaType.BuildMode), out builMode);

                    RefreshScene.RefreshEditorSceneSetting();
                    ABBuildPanel.BuildAssetBundles(buildTarget, true, false, RemoveManifest, builMode);
                    Debug.LogError("################ BuildAssetBundles  End!");
                    DefaultGenerateUpdateInfo(mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash));
                }
                #endregion
                #region 资源版本号设置
                string resVer = mPargmaTypeCheck.GetValue(PargmaType.ResVer);
                if (!string.IsNullOrEmpty(resVer))
                {
                    string verInfo = Application.streamingAssetsPath + PathConfig.Instance.VersionFileName;
                    if (File.Exists(verInfo))
                    {
                        File.Delete(verInfo);
                    }
                    using (var writer = File.CreateText(verInfo))
                    {
                        writer.WriteLine(resVer);
                        writer.WriteLine("1");
                        if (Enum.TryParse<PkgType>(mPargmaTypeCheck.GetValue(PargmaType.PkgType), true, out var pkgType))
                        {
                            if (pkgType != PkgType.AllRes)
                            {
                                writer.WriteLine("1");
                            }
                        }
                        else {
                            Debug.LogError($"################ Parse PkgType Failed. {mPargmaTypeCheck.GetValue(PargmaType.PkgType)}");
                        }
                    }
                }
                #endregion
                #region 补丁文件生成
                if (mPargmaTypeCheck.IsTrue(PargmaType.BuildPatch))
                {
                    if (mPargmaTypeCheck.IsTrue(PargmaType.ABNameWithHash))
                    {
                        BuildPatchWithHashName();
                    }
                    else
                    {
                        BuildPatchFile();
                    }
                }
                if (mPargmaTypeCheck.IsTrue(PargmaType.BuildResPkg))
                {
                    BuildResPkgMMM();
                }
                #endregion
                #region 代码大小优化
                if (mPargmaTypeCheck.IsTrue(PargmaType.StripEngineCode))
                {
                    PlayerSettings.stripEngineCode = true;
                }
                else
                {
                    PlayerSettings.stripEngineCode = false;
                }
                if (Enum.TryParse<ManagedStrippingLevel>(mPargmaTypeCheck.GetValue(PargmaType.StripLvl), true, out var stripLvl))
                {
                    PlayerSettings.SetManagedStrippingLevel(buildTarGroup, stripLvl);
                }
                else
                {
                    Debug.LogError($"################ Parse StripLvl Failed. {mPargmaTypeCheck.GetValue(PargmaType.StripLvl)}");
                }
                #endregion
                #region 安装包导出
                bool IsBuildPkg = mPargmaTypeCheck.IsTrue(PargmaType.IsBuildPkg);
                if (IsBuildPkg)
                {


                    if (mPargmaTypeCheck.IsTrue(PargmaType.IsABTest))
                    {
                        RefreshScene.RefreshABTestSceneSetting();
                    }
                    else
                    {
                        RefreshScene.RefreshABSceneSetting();
                    }
                    if (Platform == Platforms[0])
                    {
                        BuildForAndroid();
                    }
                    else if (Platform == Platforms[1])
                    {
                        BuildForIOS();
                    }
                    else if (Platform == Platforms[2])
                    {
                        BuildWebGl();
                    }
                    else if (Platform == Platforms[3])
                    {
                        BuildWindows();
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Debug.LogError($"################ {e}");
            }
        }
        private static string GetIconName()
        {
            string iconName = "oversea.png";
            return iconName;

        }
        [MenuItem("jenkins/SetAppIcons")]
        public static void SetAppIcons()
        {
            Texture2D[] textures = new Texture2D[1];
            textures[0] = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/ResMS/Icon/{GetIconName()}");

            PlayerSettings.resolutionDialogBanner = textures[0];
            BuildTargetGroup platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlatformIconKind kind = null;
#if UNITY_IOS
            kind = iOSPlatformIconKind.Application;
#elif UNITY_ANDROID
            kind = AndroidPlatformIconKind.Legacy;		
#else
            return;
#endif
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, kind);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].SetTextures(textures[0]);
            }
            PlayerSettings.SetPlatformIcons(platform, kind, icons);
        }
        /// <summary>
        /// 根据参数开启hotfix
        /// </summary>
        public static void CheckUseHotfix()
        {
            //资源构建太慢
            //if (EnvUtils.IsUNITY_IOS())
            //{
            //    AtlasRGBASpliter.ChangeAtlasShader();
            //}
            mPargmaTypeCheck.Init(Environment.GetCommandLineArgs());
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            string symbols = "CROSS_PLATFORM_INPUT;HOTFIX_ENABLE;UNITY_POST_PROCESSING_STACK_V2;DISABLE_ILRUNTIME_DEBUG";
            string hotfix_symbol = "USE_HOTFIX";
            if (mPargmaTypeCheck.IsTrue(PargmaType.UseHotFix))
            {
                //ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis();
                if (!symbols.Contains(hotfix_symbol))
                {
                    symbols = $"{symbols};{hotfix_symbol}";
                }
            }
            else
            {
                if (symbols.Contains(hotfix_symbol))
                {
                    symbols = symbols.Replace(hotfix_symbol, "");
                }
            }
            //每次执行该函数时需要到ProjectSetting目录下执行svn revert
            var macro = mPargmaTypeCheck.GetValue(PargmaType.Macro);
            if (!string.IsNullOrEmpty(macro))
            {
                if (!symbols.Contains(macro))
                {
                    symbols = $"{symbols};{macro}";
                }
            }
            string encryMacro = "ENCRYAB";
            if (mPargmaTypeCheck.IsTrue(PargmaType.EncryAB))
            {
                if (!symbols.Contains(encryMacro))
                {
                    symbols = $"{symbols};{encryMacro}";
                }
            }
            else
            {
                if (symbols.Contains(encryMacro))
                {
                    symbols = symbols.Replace(encryMacro, "");
                }
            }
            string obfucatorMacro = "OBFUCATE_CODE";
            if (mPargmaTypeCheck.IsTrue(PargmaType.ObfucateCode))
            {
                if (!symbols.Contains(obfucatorMacro))
                {
                    symbols = $"{symbols};{obfucatorMacro}";
                }
            }
            else
            {
                if (symbols.Contains(obfucatorMacro))
                {
                    symbols = symbols.Replace(obfucatorMacro, "");
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            RefreshScene.RefreshEditorSceneSetting();
            if (mPargmaTypeCheck.IsTrue(PargmaType.IsBuildPkg))
            {
                ReplaceFile("PlatformRes/DefaultIcon.png", "Assets/ResMS/Icon/DefaultIcon.png");
            }
        }
        static void ReplaceFile(string srcFile, string tarFile)
        {
            if (File.Exists(srcFile) && File.Exists(tarFile))
            {
                File.Copy(srcFile, tarFile, true);
            }
        }
        [MenuItem("jenkins/scene/SwitchToTorist")]
        public static void SwitchToTorist()
        {
            UpdateProjectSetting(true);
        }
        [MenuItem("jenkins/scene/SwitchToNoorm")]
        public static void SwitchToNoorm()
        {
            UpdateProjectSetting(false);
        }
        private static void UpdateProjectSetting(bool IsTorist)
        {
            string setting = "ProjectSettings/TimeManager.asset";
            SerializedObject mgr = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(setting)[0]);
            SerializedProperty property = mgr.FindProperty("Fixed Timestep");
            property.floatValue = IsTorist? 0.02f:0.1f;
            mgr.ApplyModifiedProperties();


            setting = "ProjectSettings/DynamicsManager.asset";
            mgr = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(setting)[0]);
            property = mgr.FindProperty("m_AutoSimulation");
            property.boolValue = IsTorist;
            property = mgr.FindProperty("m_AutoSyncTransforms");
            property.boolValue = !IsTorist;
            property = mgr.FindProperty("m_ReuseCollisionCallbacks");
            property.boolValue = IsTorist;
            mgr.ApplyModifiedProperties();
        }
    }
}