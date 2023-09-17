using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
namespace Language
{
    public sealed class GrabChineseTool
    {
        static string[] ignoreFiles = new string[] { "ShieldStrings.txt", "UIText.txt", "fwqlist.txt", "ServerConfigList.txt" };
        private static Regex m_NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
        /// <summary>
        /// 判断文本中是否包含中文
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsContainChinese(string txt)
        { 
            return m_NumReg.IsMatch(txt);
        }
        /// <summary>
        /// 抓取目录中所有TXT文件中的中文
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="dstDir"></param>
        public static Dictionary<string, HashSet<string>> GrabChinese(string srcDir, string FindStr = null, string[] filtter = null)
        {
            if (!Directory.Exists(srcDir))
            {
                return null;
            }
            
            Dictionary<string, HashSet<string>> cnSetDict = new Dictionary<string, HashSet<string>>();
            var allTxtFiles = Directory.GetFiles(srcDir,"*.txt", SearchOption.AllDirectories);
            foreach (var file in allTxtFiles)
            {
                bool isIgnore = false;
                foreach (var ignorefile in ignoreFiles)
                {
                    if (file.EndsWith(ignorefile))
                    {
                        isIgnore = true;
                        break;
                    }
                }
                if (isIgnore)
                {
                    continue;
                }
                bool ignore = false;
                foreach (var f in filtter)
                {
                    if (file.Equals(f))
                    {
                        ignore = true;
                        break;
                    }
                }
                if (ignore)
                {
                    continue;
                }
                using (StreamReader reader = new StreamReader(file))
                {
                    var content = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(content) && IsContainChinese(content))
                    {
                        if (!cnSetDict.TryGetValue(file, out var tChineseSet))
                        { 
                            tChineseSet = new HashSet<string>();
                            cnSetDict[file] = tChineseSet;
                        }
                        var lines = content.Trim().Split("\n");
                        foreach (var curLine in lines)
                        {
                            if (string.IsNullOrEmpty(curLine))
                            {
                                continue;
                            }
                            var fields = curLine.Trim('\r').Split("\t");
                            foreach (var field in fields)
                            {
                                if (!string.IsNullOrEmpty(field) && IsContainChinese(field))
                                {
                                    tChineseSet.Add(field);
                                    if (field == FindStr)
                                    {
                                        Console.WriteLine($"#### {file}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return cnSetDict;
        }
        /// <summary>
        /// 获取已翻译的内容文本
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        public static Dictionary<string, LgInfo> GetLgInfo(string outputPath)
        {
            Dictionary<string, LgInfo> tSetDict = new Dictionary<string, LgInfo>();
            if (File.Exists(outputPath))
            {
                using (StreamReader reader = new StreamReader(outputPath))
                {
                    var content = reader.ReadToEnd();
                    var lines = content.Split("\n");
                    foreach (var curLine in lines)
                    {
                        if (string.IsNullOrEmpty(curLine) || curLine.StartsWith("###") && curLine.EndsWith("###"))
                            continue;
                        var info = new LgInfo(curLine.Trim('\r'));
                        tSetDict.Add(info.GetChinese(), info);
                    }
                }
            }
            else
            {
                File.Create(outputPath);
            }
            return tSetDict;
        }
        /// <summary>
        /// 分析中文集，提取新增内容
        /// </summary>
        /// <param name="cnSetDict"></param>
        /// <param name="lgInfoPath"></param>
        public static HashSet<string> AnalyzeChinese(Dictionary<string, HashSet<string>> cnSetDict, Dictionary<string, LgInfo> lgInfoDict)
        {
            var iter = cnSetDict.GetEnumerator();
            HashSet<string> newChinese = new HashSet<string>();
            while (iter.MoveNext())
            {
                foreach (var chinese in iter.Current.Value)
                {
                    if (!lgInfoDict.ContainsKey(chinese))
                    {
                        newChinese.Add(chinese);
                    }
                }
            }
            return newChinese;
        }
        public static void AnalyzeChinese(HashSet<string> cnSet, Dictionary<string, LgInfo> lgInfoDict, ref HashSet<string> newChinese)
        {
            foreach (var chinese in cnSet)
            {
                if (!lgInfoDict.ContainsKey(chinese))
                {
                    if (!newChinese.Contains(chinese))
                    {
                        newChinese.Add(chinese);
                    }
                }
            }
        }
        /// <summary>
        /// 将新增文本写入文件
        /// </summary>
        /// <param name="chineseSet"></param>
        /// <param name="outputFile"></param>
        public static void WriteToFile(HashSet<string> chineseSet, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
            if (chineseSet.Count < 0)
            {
                return;
            }
            StringBuilder builder = new StringBuilder();
            foreach (var chinese in chineseSet)
            {
                if (!string.IsNullOrEmpty(chinese))
                {
                    builder.AppendLine(chinese);
                }
            }
            var result = builder.Replace("\r", "").ToString().TrimEnd();
            var end = new UTF8Encoding(false);
            File.WriteAllText(outputFile, result, end);
        }
        /// <summary>
        /// 将带有中文的文件记录下来
        /// </summary>
        /// <param name="cnSetDict"></param>
        public static void RecordChineseFile(Dictionary<string, HashSet<string>> cnSetDict, string topDir, string recordFile)
        {
            var iter = cnSetDict.GetEnumerator();
            HashSet<string> fileSet = new HashSet<string>();
            while (iter.MoveNext())
            {
                fileSet.Add(iter.Current.Key.Replace(topDir, "").TrimEnd().Trim('\n'));
            }
            WriteToFile(fileSet, recordFile);
        }
        /// <summary>
        /// 获取记录文件内容
        /// </summary>
        /// <param name="recordFile"></param>
        /// <returns></returns>
        static public HashSet<TableCls> GetRecordFiles(string recordFile, string srcTxtDir)
        {
           
            HashSet<TableCls> recordSet = new HashSet<TableCls>();
            if (File.Exists(recordFile))
            {
                var content = File.ReadAllText(recordFile, Encoding.UTF8);
                var lines = content.Split('\n');
                foreach (var curLine in lines)
                { 
                    recordSet.Add(new TableCls($"{srcTxtDir}\\{curLine}"));
                }
            }
            return recordSet;
        }
    }
    
}
