using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

namespace Language
{
    internal class LangTextureWindow : EditorWindow
    {
        [MenuItem("Tools/多语言/3.提取需要翻译的图片", priority = 3)]
        static void Open()
        {
            GetWindow<LangTextureWindow>("多语言图片", true);
        }
        private string oldProjPath;
        private string noChinesePath;
        Dictionary<string, LangTextureInfo> oldProjTexDict;
        Dictionary<string, LangTextureInfo> curProjDict;
        Dictionary<string, LangTextureInfo> noChineseDict;
        Dictionary<string, LangTextureInfo> translatedDict;
        Dictionary<string, LangTextureInfo> extracteddDict;
        /// <summary>
        /// 系列帧图片去重复
        /// </summary>
        string ProJectTranslatePath = "ProJectTranslate";
        /// <summary>
        /// 需要翻译图片
        /// </summary>
        string ProjectNewTexs = "ProJectNewTexs";
        /// <summary>
        /// 已翻译可复用的图片
        /// </summary>
        string ProJectCanUseTexs = "ProJectCanUseTexs";
        /// <summary>
        /// 已翻译的图片存储目录
        /// </summary>
        string TranslatedPath = "";
        /// <summary>
        /// 存放已提前的中文图片
        /// </summary>
        string ExtractedChinesePath = "";
        private float Height = 40;
        void OnGUI()
        {
            GUILayout.Label($"当前gongc:{Environment.CurrentDirectory}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选取", GUILayout.Width(60)))
            {
                oldProjPath = EditorUtility.OpenFolderPanel("素材工程", oldProjPath, oldProjPath);
            }
            float labelWidth = 140;
            GUILayout.Label($"素材工程(可为空):", GUILayout.ExpandWidth(false), GUILayout.Width(labelWidth));
            GUILayout.TextField(oldProjPath, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选取", GUILayout.Width(60)))
            {
                noChinesePath = EditorUtility.OpenFolderPanel("不带中文的图片", noChinesePath, noChinesePath);
            }
            GUILayout.Label("不带中文的图片(可为空):", GUILayout.ExpandWidth(false), GUILayout.Width(labelWidth));
            GUILayout.TextField(noChinesePath, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选取", GUILayout.Width(60)))
            {
                TranslatedPath = EditorUtility.OpenFolderPanel("已翻译的图片", TranslatedPath, TranslatedPath);
            }
            GUILayout.Label("已翻译的图片(可为空):", GUILayout.ExpandWidth(false), GUILayout.Width(labelWidth));
            GUILayout.TextField(TranslatedPath, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选取", GUILayout.Width(60)))
            {
                ExtractedChinesePath = EditorUtility.OpenFolderPanel("已提取的中文图片", ExtractedChinesePath, ExtractedChinesePath);
            }
            GUILayout.Label("已提取的中文图片(可为空):", GUILayout.ExpandWidth(false), GUILayout.Width(labelWidth));
            GUILayout.TextField(ExtractedChinesePath, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            //bool canShowInitBtn = !string.IsNullOrEmpty(noChinesePath);
                //&& !string.IsNullOrEmpty(TranslatedPath)
                //&& !string.IsNullOrEmpty(oldProjPath)
                //&& !string.IsNullOrEmpty(noChinesePathExtractedChinesePath);

            if (/*canShowInitBtn && */GUILayout.Button("初始化", GUILayout.Height(Height)))
            {
                noChineseDict = GetFiles(noChinesePath);
                extracteddDict = GetFiles(ExtractedChinesePath);
                translatedDict = GetFiles(TranslatedPath);

                oldProjTexDict = GetProjTexture(oldProjPath);
                curProjDict = GetProjTexture(Environment.CurrentDirectory);
                var iter = curProjDict.GetEnumerator();
                int iCount = 0;
                while (iter.MoveNext())
                {
                    iCount += 1;
                    EditorUtility.DisplayProgressBar("提示", "图片对比中...", iCount * 1f / curProjDict.Count);
                    var curProjTexInfo = iter.Current.Value;
                    if (noChineseDict.TryGetValue(iter.Current.Key, out var info2))
                    {
                        if (curProjTexInfo.md5 == info2.md5)
                        {
                            curProjTexInfo.LangTexMode = LangTexMode.NO_CHINESE;
                            continue;
                        }
                    }
                    if (extracteddDict.TryGetValue(iter.Current.Key, out var extractedInfo))
                    {
                        if (extractedInfo.md5 == curProjTexInfo.md5)
                        {
                            curProjTexInfo.LangTexMode = LangTexMode.EXTRACTED;
                            continue;
                        }
                    }
                    if (translatedDict.TryGetValue(iter.Current.Key, out var traslatedInfo))
                    {
                        //if (traslatedInfo.md5 == curProjTexInfo.md5)
                        {
                            curProjTexInfo.LangTexMode = LangTexMode.TRANSLATED;
                            continue;
                        }
                    }
                    if (oldProjTexDict.TryGetValue(iter.Current.Key, out var oldProjInfo))//即该图片已翻译
                    {
                        if (null != traslatedInfo && traslatedInfo.md5 == oldProjInfo.md5)//该图片已被提取到翻译目录
                        {
                            oldProjInfo.LangTexMode = LangTexMode.TRANSLATED;
                        }
                        else
                        {
                            var IsEqual = oldProjInfo.md5 == curProjTexInfo.md5;
                            oldProjInfo.LangTexMode = IsEqual ? LangTexMode.MD5_EQUAL : LangTexMode.MD5_NOEQUAL;
                        }

                    }
                    else
                    {
                        curProjTexInfo.LangTexMode = LangTexMode.NEW;
                    }
                }
                EditorUtility.ClearProgressBar();
                ShowNotification(new GUIContent("已完成"));
            }
            if (null != curProjDict && curProjDict.Count > 0 && GUILayout.Button("提取工程新增图片", GUILayout.Height(Height)))
            {
                CleanDir(ProjectNewTexs);
                var iter = curProjDict.GetEnumerator();
                while (iter.MoveNext())
                {
                    var texInfo = iter.Current.Value;
                    if (texInfo.LangTexMode == LangTexMode.NEW)
                    {
                        var relativePath = $"{ProjectNewTexs}/{ParsePath2Name(texInfo.filePath)}";
                        File.Copy(texInfo.filePath, relativePath);
                    }
                }
                var texDict = GetFiles(ProjectNewTexs);
                #region 去除系列帧中重复的图片
                #region 标注系列中中不需要翻译的图片
                var newPaths = ParseRelative2Name(new string[]{ @"ResMS\UI\Atlas\Material\vipeffect_1",
                                        @"ResMS\UI\Atlas\Material\vipeffect_2",
                                        @"ResMS\UI\Atlas\Material\vipeffect_3",
                                        @"ResMS\UI\Atlas\Material\vipeffect_4",
                                        @"ResMS\UI\Atlas\Material\vipeffect_5",
                                        @"ResMS\UI\Atlas\Material\vipeffect_6",
                                        @"ResMS\UI\Atlas\Material\vipeffect_7",
                                        @"ResMS\UI\Atlas\Material\vipeffect_8",
                                        @"ResMS\UI\Atlas\Material\vipeffect_9",
                                        @"ResMS\UI\Atlas\Material\vipeffect_10"}
                );
                iter = texDict.GetEnumerator();
                List<LangTextureInfo> langTexlst = new List<LangTextureInfo>();
                while (iter.MoveNext())
                {
                    foreach (var path in newPaths)
                    {
                        if (iter.Current.Key.StartsWith(path))
                        {
                            langTexlst.Add(iter.Current.Value);
                            break;
                        }
                    }
                }
                Dictionary<string, List<LangTextureInfo>> dict = new Dictionary<string, List<LangTextureInfo>>();
                foreach (var info in langTexlst)
                {
                    var key = info.filePath.Substring(0, info.filePath.LastIndexOf("_"));
                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = new List<LangTextureInfo>();
                    }
                    dict[key].Add(info);
                }
                var dictIter = dict.GetEnumerator();
                while (dictIter.MoveNext())
                {
                    var lst = dictIter.Current.Value;
                    for (int index = 1; index < lst.Count; index++)
                    {
                        lst[index].LangTexMode = LangTexMode.NO_TRANSLATE;
                    }
                }
                #endregion
                CleanDir(ProJectTranslatePath);
                iter = texDict.GetEnumerator();
                while (iter.MoveNext())
                {
                    var texInfo = iter.Current.Value;
                    if (texInfo.LangTexMode != LangTexMode.NO_TRANSLATE)
                    {
                        var newfilePath = $"{ProJectTranslatePath}/{Path.GetFileName(texInfo.filePath)}";
                        File.Copy(texInfo.filePath, newfilePath);
                    }
                }
                #endregion
                ShowNotification(new GUIContent("已完成"));
            }
            if (null != oldProjTexDict && oldProjTexDict.Count > 0 && GUILayout.Button("提取素材工程可复用的图片", GUILayout.Height(Height)))
            {
                CleanDir(ProJectCanUseTexs);
                var iter = oldProjTexDict.GetEnumerator();
                string dstAssetPath = oldProjPath + "\\Assets";
                dstAssetPath = dstAssetPath.Replace("\\", "/").Replace("//", "/");
                while (iter.MoveNext())
                {
                    var texInfo = iter.Current.Value;
                    if (texInfo.LangTexMode == LangTexMode.MD5_NOEQUAL)
                    {
                        var newfilePath = $"{ProJectCanUseTexs}/{ParsePath2Name(texInfo.filePath)}";
                        File.Copy(texInfo.filePath, newfilePath);
                    }
                }
                ShowNotification(new GUIContent("已完成"));
            }
            if (null != translatedDict && translatedDict.Count > 0 && GUILayout.Button("已翻译的图片导入到工程", GUILayout.Height(Height)))
            {
                var iter = translatedDict.GetEnumerator();
                while (iter.MoveNext())
                {
                    var translteTexInfo = iter.Current.Value;
                    if(curProjDict.TryGetValue(iter.Current.Key, out var curProjTexInfo))
                    {
                        File.Copy(translteTexInfo.filePath, curProjTexInfo.filePath, true);

                    }
                }
                AssetDatabase.Refresh();
            }
        }
        private void CleanDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }
        /// <summary>
        /// 将中间目录转化为名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ParseRelative2Name(string path)
        {
            return path.Replace("\\", "/").TrimStart('/').Replace("/", "_");
        }
        private string[] ParseRelative2Name(string[] paths)
        {
            string[] newPaths = new string[paths.Length];
            for (int index = 0; index < newPaths.Length; index++)
            {
                newPaths[index] = ParseRelative2Name(paths[index]);
            }
            return newPaths;
        }
        /// <summary>
        /// 按照指定规则生成路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string ParsePath2Name(string filePath)
        {
            string newFilePath;
            string dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string relativeDir = ParseRelative2Name(dir.Substring(filePath.IndexOf("Assets") + 6));
            newFilePath = $"{relativeDir}#{fileName}";
            return newFilePath;
        }
        private Dictionary<string, LangTextureInfo> GetProjTexture(string projPath)
        {
            Dictionary<string, LangTextureInfo> dstDict = new Dictionary<string, LangTextureInfo>();
            try
            {
                if (Directory.Exists(projPath))
                {
                    List<string> dstLst = new List<string>();
                    string[] texturePath = new string[] { @"Assets\BundleData\Texture", @"Assets\ResMS\UI" };
                    foreach (var dir in texturePath)
                    {
                        var dstFiles = Directory.GetFiles($"{projPath}\\{dir}", "*", SearchOption.AllDirectories)
                            .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"));
                        dstLst.AddRange(dstFiles);
                    }
                    int iCount = 0;
                    foreach (var filePath in dstLst)
                    {
                        iCount += 1;
                        EditorUtility.DisplayProgressBar("提示", "图片信息获取中...", iCount * 1f / dstLst.Count);
                        var key = ParsePath2Name(filePath);
                        dstDict.Add(key, new LangTextureInfo(filePath));
                    }
                }
                else
                {
                    Debug.Log($"{projPath} 路径不存在");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return dstDict;
        }
        private Dictionary<string, LangTextureInfo> GetFiles(string projPath)
        {
            Dictionary<string, LangTextureInfo> dict = new Dictionary<string, LangTextureInfo>();
            if (Directory.Exists(projPath))
            {
                StringBuilder builder = new StringBuilder();
                var dstFiles = Directory.GetFiles(projPath, "*", SearchOption.AllDirectories);
                foreach (var file in dstFiles)
                {
                    var filePath = Path.GetFileName(file);
                    if (dict.ContainsKey(filePath))
                    {
                        builder.AppendLine(file);
                        continue;
                    }
                    dict.Add(Path.GetFileName(file), new LangTextureInfo(file));
                }
                if (builder.Length > 0)
                {
                    Debug.LogError($"重复提取的图片: {builder.ToString()}");
                }
            }
            return dict;
        }
    }
}
