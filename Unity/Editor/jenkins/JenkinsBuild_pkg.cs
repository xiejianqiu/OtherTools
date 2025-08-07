using JGGame;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        [MenuItem("jenkins/pkg/BuildPkg")]
        public static void BuildResPkgMMM()
        {
            if (Directory.Exists(PkgBuildFolder))
            {
                Directory.Delete(PkgBuildFolder, true);
            }
            Directory.CreateDirectory(PkgBuildFolder);
            var pkgFile = PkgBuildFolder + "/pkg.kg";
            var pkgFileInfo = PkgBuildFolder + "/pkg.info";
            var abdetail = mPargmaTypeCheck.GetValue(PargmaType.AbdetailVer);
            bool IsUpdateRes = !abdetail.Contains("1_baseVer");
            string pkgUrl = string.Empty;
            string relativePath = string.Empty;
            if (EnvUtils.IsUNITY_ANDROID())
            {
                relativePath = "Formal/android";
            }
            else if (EnvUtils.IsUNITY_IOS() || EnvUtils.IsUNITY_IPHONE())
            {
                relativePath = "Formal/apple";
            }
            if (IsUpdateRes)
            {
                
                pkgUrl = $"{URLConfig.Instance.GetPkgUrl()}/{relativePath}/{mPargmaTypeCheck.GetValue(PargmaType.PkgFileName)}";
                JGGame.PkgTool.BuildUpdatePkg(SrcFileDir: PatchBuilderFolder, pkgFile: pkgFile, pkgInfoFile: pkgFileInfo, mPargmaTypeCheck.GetValue(PargmaType.ResVer), pkgUrl);
            }
            else
            {
                pkgUrl = $"{URLConfig.Instance.GetPkgUrl()}/{relativePath}/pkgs/{mPargmaTypeCheck.GetValue(PargmaType.PkgFileName)}";
                string dlOrderUrl = $"{URLConfig.Instance.GetCDNURL()}/{relativePath}/pkgs/lb_origin_cfg_win.txt";
                JGGame.PkgTool.BuildAllResPkg(SrcFileDir: PatchBuilderFolder, pkgFile: pkgFile, pkgInfoFile: pkgFileInfo, mPargmaTypeCheck.GetValue(PargmaType.ResVer), pkgUrl, dlOrderUrl, slientTask: 1, crazyTask:5);
            }
        }
        [MenuItem("jenkins/pkg/UnBuildPkg")]
        public static void UnBuildPkg()
        {
            if (Directory.Exists(PkgBuildFolder))
            {
                Directory.Delete(PkgBuildFolder, true);
            }
            Directory.CreateDirectory(PkgBuildFolder);
            var pkgFile = PkgBuildFolder + "/pkg.kg";
            var pkgFileInfo = PkgBuildFolder + "/pkg.info";
            var unzipDir = PkgBuildFolder + "/Unzip";
            if (!Directory.Exists(unzipDir))
            {
                Directory.CreateDirectory(unzipDir);
            }
            JGGame.PkgTool.UnzipPkg(pkgFile: pkgFile, unZipDir: unzipDir, pkgInfoFile: pkgFileInfo);
        }
        [MenuItem("jenkins/pkg/UnBuildCDNPkg")]
        public static void UnBuildCDNPkg()
        {
            if (Directory.Exists(PkgBuildFolder))
            {
                Directory.Delete(PkgBuildFolder, true);
            }
            Directory.CreateDirectory(PkgBuildFolder);
            Action<string, JGGame.LogType> logCallBack = (msg, lvl) =>
            {
                Debug.Log($"{msg}");
            };
            LogUtil.RegisterLogHandler(logCallBack);
            var pkgFileInfoUrl = "https://cdn.cyygame.cn/game/pkg.info";
            var unzipDir = PkgBuildFolder + "/UnCDNzip";
            if (Directory.Exists(unzipDir))
            {
                Directory.Delete(unzipDir, true);
            }
            Directory.CreateDirectory(unzipDir);
            JGGame.PkgTool.UnzipCDNPkg(unZipDir: unzipDir, pkgInfoUrl: pkgFileInfoUrl);

            LogUtil.UnRegisterLogHandler(logCallBack);
        }
    }
}
