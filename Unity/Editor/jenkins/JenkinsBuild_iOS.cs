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
        /// xcode 导出逻辑
        /// </summary>
        private static void BuildForIOS()
        {
            try
            {
                PkgType pkgType = (PkgType)int.Parse(mPargmaTypeCheck.GetValue(PargmaType.PkgType));
                bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
                bool IsAAB = mPargmaTypeCheck.IsTrue(PargmaType.IsAAB);
                string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
                AndroidBuildTool.Config.IS_GOOGLE_PLAY = IsAAB;
                PlayerGenerator.isOniOSDebugBuild = IsDebug;
                if (ChanConnector.IsJJYChannel())
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/jiujiuyou/", "./Assets/Editor/XUPorter/Mods/");
                }
                else
                {
                    PlayerGenerator.CopyDirectory("./iOSSDK/chujian/", "./Assets/Editor/XUPorter/Mods/");
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
                    opts |= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
                }
                BuildPipeline.BuildPlayer(allScene, PlayerGenerator.GetLocation(), BuildTarget.iOS, opts);
                if (pkgType == PkgType.NoRes)
                {
                    string asset_path = outputPath + "/Data/Raw/AssetBundles";
                    if (Directory.Exists(asset_path))
                    {
                        Directory.Delete(asset_path, true);
                    }
                }
                else if (pkgType == PkgType.HalfRes)
                {
                    string asset_path = outputPath + "/Data/Raw/AssetBundles";
                    PlayerGenerator.ExtractPatchRes(asset_path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"#### Build Fail: {ex.Message}");
            }

        }
    }
}
