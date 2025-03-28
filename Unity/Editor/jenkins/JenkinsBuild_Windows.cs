using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        [MenuItem("jenkins/windows/BuildWindows")]
        static public void BuildWindowsMenu()
        {
            RefreshScene.RefreshABSceneSetting();
            BuildWindows();
        }
        static private void BuildWindows()
        {
            PlayerSettings.fullScreenMode = UnityEngine.FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.runInBackground = true;

            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, false);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect4by3, false);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect5by4, false);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by9, true);
            PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10, false);
            //PlayerSettings.allowFullscreenSwitch = true;
            if (mPargmaTypeCheck.IsTrue(PargmaType.UseIL2CPP))
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.IL2CPP);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, ScriptingImplementation.Mono2x);
            }
            bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
            BuildOptions opts = BuildOptions.AcceptExternalModificationsToPlayer;
            if (IsDebug)
            {
                opts |= BuildOptions.Development | BuildOptions.ConnectWithProfiler;
            }
            string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
            string name = "Frxx";
         
            string exesaveDir = string.Empty;
            if (string.IsNullOrEmpty(outputPath))
            {
                exesaveDir = $"{Environment.CurrentDirectory}/qq{(IsDebug ? "_debug" : "_release")}";
                outputPath = $"{exesaveDir}/{name}.exe";
            }
            if (Directory.Exists(exesaveDir))
            {
                Directory.Delete(exesaveDir, true);   
            }
            Directory.CreateDirectory(exesaveDir);
            var resport = BuildPipeline.BuildPlayer(AndroidBuildTool.GetBuildScenes().ToArray(), outputPath, BuildTarget.StandaloneWindows, opts);
            {
                string abDir = Path.GetDirectoryName(outputPath);
                abDir = $"{abDir}/{name}_Data/StreamingAssets/AssetBundles";
                if (Directory.Exists(abDir))
                {
                    Directory.Delete(abDir, true);
                }
                if (!Directory.Exists(abDir))
                {
                    Directory.CreateDirectory(abDir);
                }
                #region  AB资源拷贝
                if (Enum.TryParse<PkgType>(mPargmaTypeCheck.GetValue(PargmaType.PkgType), true, out var pkgType));
                {
                    ExtractPatchRes(abDir, pkgType);
                }
                #endregion
                var saveABdepFile = abDir + "/" + NewABDepFile;
                if (File.Exists(saveABdepFile))
                {
                    File.Delete(saveABdepFile);
                }
                File.Copy(PathABSaveFolder + ABDepFile, saveABdepFile);
            }
            WriteConfig(exesaveDir + "/config.ini");
            
        }
        /// <summary>
        /// 30212410=3*10000000+2*100000+124*100+10 ——> 3.2.124.10
        /// </summary>
        static private void WriteConfig(string cfgPath)
        {
            var version = Application.version.Trim();
            var verarray = version.Split('.');
            int v1 = int.Parse(verarray[0]);
            int v2 = int.Parse(verarray[1]);
            int v3 = int.Parse(verarray[2]);
            int v4 = int.Parse(verarray[3]);
            var qqVer = v1  * 10000000 + v2 * 100000 + v3 * 100 + v4;

            string cfgtemplate = Environment.CurrentDirectory + "/Config/config.template";
            if (File.Exists(cfgtemplate))
            {
                var cfgContent = File.ReadAllText(cfgtemplate);
                cfgContent = cfgContent.Replace("${VERSION}", qqVer.ToString());
                File.WriteAllText(cfgPath, cfgContent);
            }
        }
    }
}
