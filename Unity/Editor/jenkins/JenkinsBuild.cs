using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        static string[] Platforms = { "Android", "iOS", "WebGL" };
        private static PargmaTypeCheck mPargmaTypeCheck = new PargmaTypeCheck();
        public static void BuildMain()
        {
            try
            {
                mPargmaTypeCheck.Init(Environment.GetCommandLineArgs());
                bool buildRes = mPargmaTypeCheck.IsTrue(PargmaType.BuildRes);
                string Platform = mPargmaTypeCheck.GetValue(PargmaType.Platform);

                string versionName = mPargmaTypeCheck.GetValue(PargmaType.VersionName);
                string versionCode = mPargmaTypeCheck.GetValue(PargmaType.VersionCode);
                bool RemoveManifest = mPargmaTypeCheck.IsTrue(PargmaType.RemoveManifest);
                PlayerSettings.bundleVersion = versionName;
                if (EnvUtils.IsUNITY_ANDROID())
                {
                    PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
                }
                else if (EnvUtils.IsUNITY_IOS())
                {
                    PlayerSettings.iOS.buildNumber = versionCode;
                }

                if (buildRes)
                {
                    BuildTargetGroup buildTarGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                    BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                    if (Platform == Platforms[0])
                    {
                        buildTarGroup = BuildTargetGroup.Android;
                        buildTarget = BuildTarget.Android;
                    }
                    else if (Platform == Platforms[1])
                    {
                        buildTarGroup = BuildTargetGroup.iOS;
                        buildTarget = BuildTarget.iOS;
                    }
                    else if (Platform == Platforms[2])
                    {
                        buildTarGroup = BuildTargetGroup.WebGL;
                        buildTarget = BuildTarget.WebGL;
                    }
                    if (EditorUserBuildSettings.selectedBuildTargetGroup != buildTarGroup)
                    {
                        var result = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarGroup, buildTarget);
                        if (!result)
                        {
                            Debug.LogError($"SwitchActiveBuildTarget Failed: buildTarGroup: {buildTarGroup}, buildTarget:{buildTarget}");
                        }
                    }
                    
                    ABBuildPanel.BuildAssetBundles(buildTarget, true, false, RemoveManifest);
                    PlayerGenerator.GenerateUpdateInfo();
                }

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
                        PkgType pkgType = (PkgType)int.Parse(mPargmaTypeCheck.GetValue(PargmaType.PkgType));
                        writer.WriteLine(resVer);
                        writer.WriteLine("1");
                        if (pkgType != PkgType.AllRes)
                        {
                            writer.WriteLine("1");
                        }
                    }
                }
                bool IsBuildPkg = mPargmaTypeCheck.IsTrue(PargmaType.IsBuildPkg);
                if (IsBuildPkg)
                {
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
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"################ {e}");
            }
        }
    }
}