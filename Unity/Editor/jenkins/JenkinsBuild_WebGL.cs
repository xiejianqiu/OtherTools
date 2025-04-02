using UnityEditor;
using System;
using System.IO;
using GameFramework;
using System.Text;
using System.Collections.Generic;

namespace Jenkins
{

    public partial class JenkinsBuild
    {
        [MenuItem("jenkins/BuildForWebGL")]
        public static void BuildWebGl()
        {
            bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
            string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = $"{Environment.CurrentDirectory}/webgame{(IsDebug ? "_debug" : "_release")}";
            }
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
            BuildOptions opts = BuildOptions.None;
            if (IsDebug)
            {
                opts |= BuildOptions.Development/* | BuildOptions.ConnectWithProfiler*/;
            }
            PlayerSettings.WebGL.memorySize = 768;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            //PlayerSettings.WebGL.exceptionSupport = IsDebug ? WebGLExceptionSupport.FullWithStacktrace : WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            BuildPipeline.BuildPlayer(AndroidBuildTool.GetBuildScenes().ToArray(), outputPath, BuildTarget.WebGL, opts);
            if (Directory.Exists(outputPath + "/StreamingAssets"))
            {
                Directory.Delete(outputPath + "/StreamingAssets", true);
            }
            //if (Directory.Exists(outputPath + "/StreamingAssets/AssetBundles"))
            //{
            //    Directory.Delete(outputPath + "/StreamingAssets/AssetBundles", true);
            //}
            var buildDir = outputPath + "/Build";
            if (Directory.Exists(buildDir))
            {
                string configDir = string.Empty;
                if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli)
                {
                    configDir = Environment.CurrentDirectory + @"\WebGL\Build\brotli";
                }
                else if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Gzip)
                {
                    configDir = Environment.CurrentDirectory + @"\WebGL\Build\gzip";
                }
                var files = Directory.GetFiles(configDir);

                foreach (var curFile in files)
                {
                    File.Copy(curFile, curFile.Replace(Path.GetDirectoryName(curFile), buildDir));
                }
            }
        }
    }
}
