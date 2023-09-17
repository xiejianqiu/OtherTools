using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEditor.Profiling.Memory.Experimental;
using System.Collections.Generic;
using System.Linq;

namespace Language
{
    internal class ExtractScriptChinese:EditorWindow
    {
        [MenuItem("Tools/多语言/4.提取脚本中的中文",priority = 4)]
        static void Open()
        {
            GetWindow<ExtractScriptChinese>("代码中文", true);
        }
        private Regex chineseRegex = new Regex("[\u4e00-\u9fa5]+");   //获取中文
        /// <summary>
        /// 判断文本中是否包含中文
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private bool IsContainChinese(string txt)
        {
            return chineseRegex.IsMatch(txt);
        }
        private string CodeChineseSavePath = "CodeChinese.txt";
        private float btnHeight = 40;
        private string CodePath;
        UTF8Encoding utfEndcoding = new UTF8Encoding(false);
        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("选择", GUILayout.Width(60), GUILayout.ExpandWidth(false)))
            {
                CodePath = EditorUtility.OpenFolderPanel("提示",CodePath, Environment.CurrentDirectory);
            }
            GUILayout.Label("搜索目录:",GUILayout.Width(50), GUILayout.ExpandWidth(false));
            GUILayout.TextField(CodePath);
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(CodePath) && GUILayout.Button("提取中文", GUILayout.Height(btnHeight)))
            {
                string[] ignorefiles = { "GMPanelLogic.cs", "GMPanelLogic.cs" };
                Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>();
                string parttern = string.Empty;
                var files = Directory.GetFiles(CodePath, "*", SearchOption.AllDirectories).Where(s=>s.EndsWith(".cs")||s.EndsWith(".lua"));
                int iCount = 0;
                foreach (var file in files)
                {
                    bool next = false;
                    foreach (var ignorefile in ignorefiles)
                    {
                        if (file.EndsWith(ignorefile))
                        {
                            next = true;
                            break;
                        }
                    }
                    if (next)
                        continue;
                    iCount += 1;
                    EditorUtility.DisplayProgressBar("提取中文", file, iCount * 1f / file.Length);
                    var relativePath = file.Replace(CodePath,"");
                    var lines = File.ReadAllLines(file, utfEndcoding);
                    foreach (var line in lines)
                    {
                        if(line.Contains("LogModule."))
                        {
                            continue;
                        }
                        if (!line.StartsWith("//") && IsContainChinese(line))
                        {
                            var index = line.IndexOf("//");
                            var matchCollection = Regex.Matches(line, "(?<=\")(\\w)*([\\u4e00-\\u9fa5]+)(.)*(?=\")");
                            if (matchCollection.Count > 0)
                            {
                                foreach (Match match in matchCollection)
                                {
                                    int vIndex = line.IndexOf(match.Value);
                                    if (index >0 && vIndex > index)
                                    {
                                        break;
                                    }
                                    if (!dict.ContainsKey(match.Value))
                                    {
                                        dict[match.Value] = new HashSet<string>();
                                    }
                                        
                                    dict[match.Value].Add(relativePath);
                                }
                            }
                        }
                    }
                    StringBuilder builder = new StringBuilder();
                    var iter = dict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        builder.Append(iter.Current.Key);
                        builder.Append("\t");
                        var iCnt = 0;
                        foreach (var path in iter.Current.Value)
                        {
                            if (iCnt > 0)
                            {
                                builder.Append(";");
                            }
                            builder.Append(path);
                            iCnt += 1;
                        }
                        builder.AppendLine();
                    }
                    var result = builder.ToString();
                    File.WriteAllText(CodeChineseSavePath, result, utfEndcoding);
                }
                ShowNotification(new GUIContent("完成"));
                EditorUtility.ClearProgressBar();
            }
            if (GUILayout.Button("测试", GUILayout.Height(btnHeight)))
            {
                var line = "你好啊";
                var matchCollection = Regex.Matches(line, "(?<=\")(\\w)*([\\u4e00-\\u9fa5]+)(.)*(?=\")");
                if (matchCollection.Count > 0)
                {
                    foreach (Match match in matchCollection)
                    {
                        int vIndex = line.IndexOf(match.Value);
                        Debug.LogError(match.Value);
                    }
                }
            }
        }
    }
}
