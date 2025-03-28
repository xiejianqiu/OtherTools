using GameFramework;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jenkins
{
    public partial class JenkinsBuild
    {
        #region 切换资源模式
        private enum SWITCHOPS
        {
            Orgin,
            PkgAB,
            CDN,
        }
        [MenuItem("jenkins/资源模式/CDN资源")]
        public static void SwitchCDNAB()
        {
            SwitchABMod(SWITCHOPS.CDN);
        }
        [MenuItem("jenkins/资源模式/工程内AB资源")]
        public static void SwitchPkgAB()
        {
            SwitchABMod(SWITCHOPS.PkgAB);
        }
        [MenuItem("jenkins/资源模式/源资源")]
        public static void SwitchOrgin()
        {
            SwitchABMod(SWITCHOPS.Orgin);
        }
        private static void SwitchABMod(SWITCHOPS target)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (target == SWITCHOPS.Orgin)
            {
                symbols = symbols.Replace($";EDITOR_ABMOD", "").Replace($";EDITOR_CDN_ABMOD", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                RefreshScene.RefreshEditorSceneSetting();
            }
            else if (target == SWITCHOPS.PkgAB)
            {
                if (!symbols.Contains("EDITOR_ABMOD"))
                {
                    symbols = $"{symbols};EDITOR_ABMOD".Replace(";EDITOR_CDN_ABMOD", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                    RefreshScene.RefreshABSceneSetting();
                }
            }
            else if (target == SWITCHOPS.CDN)
            {

                if (!symbols.Contains("EDITOR_CDN_ABMOD"))
                {
                    symbols = $"{symbols};EDITOR_CDN_ABMOD".Replace(";EDITOR_ABMOD", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                    RefreshScene.RefreshABSceneSetting();
                }
            }
        }
        #endregion
        #region 便捷开关
        [MenuItem("jenkins/便捷开关/SwitchLogToFile")]
        public static void SwitchLogToFile()
        {
            DelOrAddSymbol("USE_LOGTOFILE");
        }
        [MenuItem("jenkins/便捷开关/SwitchProfiler")]
        public static void SwitchProfiler()
        {
            DelOrAddSymbol("USE_PROFILER");
        }
        [MenuItem("jenkins/便捷开关/SwitchMsgProfiler")]
        public static void SwitchMsgProfiler()
        {
            DelOrAddSymbol("MSG_PROFILER");
        }
        [MenuItem("jenkins/便捷开关/切换hotfix")]
        public static void SwitchHotfix()
        {
            DelOrAddSymbol("USE_HOTFIX");
        }
        [MenuItem("jenkins/便捷开关/切换fwqlist")]
        public static void SwitchFwqlist()
        {
            DelOrAddSymbol("VISIT_CDN");
        }
        [MenuItem("jenkins/便捷开关/源码开关")]
        public static void UseSrcCode()
        {
            DelOrAddSymbol("USE_SRC_CODE");
        }
        [MenuItem("jenkins/便捷开关/收集数据表")]
        public static void CollectDataTb()
        {
            DelOrAddSymbol("COLLECT_DATA_TABLE");
        }
        [MenuItem("jenkins/便捷开关/收集丢失数据")]
        public static void CollectLostDataTb()
        {
            DelOrAddSymbol("COLLECT_LOST_DATA");
        }
        [MenuItem("jenkins/便捷开关/Up开关")]
        public static void OpenProfilerPer()
        {
            DelOrAddSymbol("PROFILER_PER", 2);
        }
        [MenuItem("jenkins/资源收集/开启收集资源")]
        public static void CollectRes()
        {
            DelOrAddSymbol("COLLECT_EDITOR_RES",1);
        }
        [MenuItem("jenkins/资源收集/关闭资源收集")]
        public static void CloseCollectRes()
        {
            DelOrAddSymbol("COLLECT_EDITOR_RES", 2);
        }
        [MenuItem("jenkins/乐变/导出配置文件")]
        public static void GenerateLBCfgForAndroid()
        {
            ResTool.GenerateLBCfg();
        }
        [MenuItem("jenkins/乐变/处理DLMissing资源")]
        public static void PaseABNameToDebugName()
        {
            ResTool.PaseABNameToDebugName();
        }
        [MenuItem("jenkins/乐变/导出UI资源和前三个场景")]
        public static void GenerateDefaultRes()
        {
            ResTool.GenerateDefaultRes();
        }
        [MenuItem("jenkins/乐变/收集小资源")]
        public static void CollectSmallAB()
        {
            ResTool.CollectSmallAB();
        }
        [MenuItem("Assets/场景资源优化", false, 0)]
        public static void SceneOpt()
        {
            // 获取当前选中的对象
            var selectedObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);

            // 如果没有选中任何对象，则返回
            if (selectedObjects.Length == 0)
            {
                Debug.Log("No folder selected.");
                return;
            }
            string floder = AssetDatabase.GetAssetPath(selectedObjects[0]);
            if (!Directory.Exists(floder))
                return;
            try
            {
                var guids = AssetDatabase.FindAssets("t:texture", new string[] { floder });

                int nCnt = 0;
                foreach (var guid in guids)
                {
                    nCnt += 1;
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    EditorUtility.DisplayProgressBar("优化贴图", assetPath, nCnt * 1f / guids.Length);
                    TextureImporter txImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings();
                    if (null == txImporter)
                        continue;
                    androidSettings.format = txImporter.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
                    androidSettings.overridden = true;
                    androidSettings.name = "Android";
                    androidSettings.textureCompression = TextureImporterCompression.Compressed;
                    txImporter.SetPlatformTextureSettings(androidSettings);

                    TextureImporterPlatformSettings iosSettings = new TextureImporterPlatformSettings();
                    iosSettings.format = txImporter.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.ASTC_RGBA_4x4 : TextureImporterFormat.ASTC_RGB_4x4;
                    iosSettings.overridden = true;
                    iosSettings.name = "iPhone";
                    iosSettings.textureCompression = TextureImporterCompression.Compressed;
                    txImporter.SetPlatformTextureSettings(iosSettings);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        /// <summary>
        /// 删除或者添加宏定义
        /// </summary>
        /// <param name="tarSymbol"></param>
        /// <param name="mode">1:不删除宏， 2不添加宏，0：添加或者删除宏</param>
        private static void DelOrAddSymbol(string tarSymbol, int mode=0)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (symbols.Contains(tarSymbol))
            {
                if (mode == 1) return;
                symbols = symbols.Replace(tarSymbol, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
            else
            {
                if (mode == 2) return;
                symbols = $"{symbols};{tarSymbol}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }
        #endregion
        [MenuItem("jenkins/Tools/TestManifest")]
        public static void TestManifest()
        {
            Resources.UnloadUnusedAssets();
            AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/AssetBundles");
            try
            {
                AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                var allBundle = manifest.GetAllAssetBundles();

                int index = 0;
                //foreach (var abHashName in allBundle) {
                //    Debug.LogError($"{abHashName}  {manifest.GetAssetBundleHash(abHashName)}");
                //    index++;
                //    if (index > 10)
                //    {
                //        break;
                //    }
                //}

                foreach (var abName in allBundle)
                {
                    string[] info = abName.Split('_');
                    if (info.Length == 2)
                    {
                        var bundleName = info[0] + ".ab";
                        var hash = info[1].Substring(0, info[1].Length - 3);
                        Debug.Log($"{abName}  bundleName:{bundleName}  hash:{hash}  ");
                    }
                    else
                    {
                        Debug.LogError($"### {abName} 该bundle名称异常");
                    }
                    index++;
                    if (index > 10)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"TestManifest   {e}");
            }
            ab.Unload(true);
        }
        [MenuItem("jenkins/Tools/UnloadUnusedAssets")]
        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }
        [MenuItem("jenkins/Tools/SwitchActiveBuildTarget")]
        public static void SwitchActiveBuildTarget()
        {
            //BuildTargetGroup buildTarGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            BuildTargetGroup buildTarGroup =  BuildTargetGroup.Standalone;
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            Debug.Log($"buildTarGroup:{buildTarGroup} buildTarget:{buildTarget}");
            var result = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarGroup, buildTarget);
            if (!result)
            {
                Debug.LogError($"################ SwitchActiveBuildTarget Failed: buildTarGroup: {buildTarGroup}, buildTarget:{buildTarget}");
            }
        }
        [MenuItem("jenkins/Encry/混淆指定文件")]
        public static void EncryFile()
        {
            EncryUtil.EncryFile("VersionInfo.txt", "VersionInfo_Encrypt.txt");
            EncryUtil.EncryFile("VersionInfo_Encrypt.txt", "VersionInfo_Decript.txt");
        }
        [MenuItem("jenkins/Encry/混淆ab资源到新目录")]
        public static void EnCryDir()
        {
            if (!mPargmaTypeCheck.IsTrue(PargmaType.EncryAB))
                return;
            EncryUtil.EncryDir(AssetBundlePathHelper.instance.BundleSavePath, AssetBundlePathHelper.instance.BundleSavePathEncry);
            var dep = AssetBundlePathHelper.instance.BundleSavePath + ABDepFile;
            var newdep = AssetBundlePathHelper.instance.BundleSavePathEncry + ABDepFile;
            File.Copy(dep, newdep, true);
        }
    }
}
