using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

class TranslateChinese
{
    [MenuItem("UIEditor/代码文本相关/GrabChinese")]
    static public void GrabLuaChinese()
    {
        Regex NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
        string dir = EditorUtility.OpenFolderPanel("提示", "", "");
        Regex rx = new Regex("\"[^\"]*\"");
        HashSet<string> fileSet = new HashSet<string>();
        var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
        fileSet.UnionWith(files);
        files = Directory.GetFiles(dir, "*.lua", SearchOption.AllDirectories);
        fileSet.UnionWith(files);
        string savepath = "chinese.txt";
        if (File.Exists(savepath))
        {
            File.Delete(savepath);
        }
        using (var f1 = File.Open(savepath, FileMode.CreateNew))
        {
            StreamWriter writer = new StreamWriter(f1);
            int iCount = 0;
            foreach (var file in fileSet)
            {
                EditorUtility.DisplayProgressBar("提示", "文本提取中...", iCount * 1f / fileSet.Count);
                using (var f = File.OpenRead(file))
                {
                    StreamReader rd = new StreamReader(f);
                    string txt = string.Empty;
                    while ((txt = rd.ReadLine()) != null)
                    {
                        if (NumReg.IsMatch(txt))
                        {
                            var results = rx.Matches(txt);
                            foreach (var result in results)
                            {
                                writer.WriteLine($"{file}\t{result}");
                            }
                        }
                    }
                }
            }
            writer.Flush();
            EditorUtility.ClearProgressBar();
        }
    }
    [MenuItem("UIEditor/代码文本相关/ToTChinese")]
    static public void FanYiLuaChinese()
    {
        #region 读取翻译文件
        string resultPath = "dic.txt";
        if (!File.Exists(resultPath))
        {
            return;
        }
        Dictionary<string, Dictionary<string, string>> luaDict = new Dictionary<string, Dictionary<string, string>>();
        string txt = File.ReadAllText(resultPath);
        var array = txt.Split('\n');
        int iCount = 0;
        foreach (var line in array)
        {
            iCount += 1;
            var tmpArray = line.Split('\t');
            EditorUtility.DisplayProgressBar("提示", "初始化中...", iCount * 1f / array.Length);
            if (tmpArray.Length > 2)
            {
                Dictionary<string, string> dict = null;
                if (!luaDict.ContainsKey(tmpArray[0]))
                {
                    dict = new Dictionary<string, string>();
                    luaDict.Add(tmpArray[0], dict);
                }
                else
                {
                    dict = luaDict[tmpArray[0]];
                }
                dict[tmpArray[1].Trim()] = tmpArray[2].Trim();
            }
        }
        var iter = luaDict.GetEnumerator();
        StringBuilder builder = new StringBuilder();
        iCount = 0;
        while (iter.MoveNext())
        {
            iCount += 1;
            EditorUtility.DisplayProgressBar("提示", "翻译中...", iCount * 1f / luaDict.Count);
            string filePath = iter.Current.Key;
            var iter2 = iter.Current.Value.GetEnumerator();
            txt = File.ReadAllText(filePath);
            while (iter2.MoveNext())
            {
                try
                {
                    txt = txt.Replace(iter2.Current.Key, iter2.Current.Value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{filePath}  {e}");
                }
            }
            File.Delete(filePath);
            using (var fs = File.OpenWrite(filePath))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(txt);
                writer.Flush();
            }
        }
        EditorUtility.ClearProgressBar();
        #endregion


    }
}
