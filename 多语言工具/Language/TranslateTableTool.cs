using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace Language
{
    /// <summary>
    /// 该工具用于将已翻译内容写入到Txt表
    /// </summary>
    static public class TranslateTableTool
    {
        /// <summary>
        /// 生成测试用的翻译文本
        /// </summary>
        /// <param name="lgInfoDict"></param>
        /// <param name="output"></param>
        static public void FanYiFalseTxt(Dictionary<string, LgInfo> lgInfoDict)
        {
            var iter = lgInfoDict.GetEnumerator();
            while (iter.MoveNext())
            {
                var lst = iter.Current.Value.otheLgLst;
                var cn = lst[0];
                if (string.IsNullOrEmpty(cn))
                {
                    lst.Add(cn);
                }
                else
                {
                    var newFanYi = $"{cn}Test";
                    lst.Add(newFanYi);
                }

            }
        }
        /// <summary>
        /// 将lgInfo写入到文件
        /// </summary>
        /// <param name="lgInfoDict"></param>
        /// <param name="outputFile"></param>
        static public void WriteLgInfoToFile(Dictionary<string, LgInfo> lgInfoDict, string outputFile)
        {
            StringBuilder builder = new StringBuilder();
            var iter = lgInfoDict.GetEnumerator();
            Dictionary<LANGUAGE, Dictionary<string, string>> langDict = new Dictionary<LANGUAGE, Dictionary<string, string>>();
            Dictionary<LANGUAGE, Dictionary<string, string>> noFanyiDict = new Dictionary<LANGUAGE, Dictionary<string, string>>();
            while (iter.MoveNext())
            {
                var lst = iter.Current.Value.otheLgLst;
                for (int index=0; index < lst.Count; index++)
                {
                    builder.Append(lst[index]);
                    if (index < lst.Count - 1)
                    {
                        builder.Append("\t");
                    }
                    else
                    {
                        builder.AppendLine();
                    }
                    LANGUAGE lang = (LANGUAGE)index;
                    if (!langDict.ContainsKey(lang))
                    {
                        langDict[lang] = new Dictionary<string, string>();
                    }
                    langDict[lang].Add(lst[0],lst[index]);
                }
            }
            var result = builder.Replace("\r","").ToString().TrimEnd();
            var end = new UTF8Encoding(false);
            File.WriteAllText(outputFile, result, end);

            var dir = Path.GetDirectoryName(outputFile);
            var langIter = langDict.GetEnumerator();
            while (langIter.MoveNext())
            {
                var tmpIter = langIter.Current.Value.GetEnumerator();
                builder.Clear();
                while (tmpIter.MoveNext())
                {
                    if (!string.IsNullOrEmpty(tmpIter.Current.Value))
                    {
                        builder.Append($"{tmpIter.Current.Key}\t{tmpIter.Current.Value}".TrimEnd());
                        builder.AppendLine();
                    }
                }
                result = builder.ToString().TrimEnd();
                File.WriteAllText($"{dir}/Lang_{langIter.Current.Key.ToString().ToLower()}.txt", result, end);
            }

            langIter = langDict.GetEnumerator();
            while (langIter.MoveNext())
            {
                
                var tmpIter = langIter.Current.Value.GetEnumerator();
                builder.Clear();
                while (tmpIter.MoveNext())
                {
                    if (string.IsNullOrEmpty(tmpIter.Current.Value))
                    {
                        builder.Append($"{tmpIter.Current.Key}".TrimEnd());
                        builder.AppendLine();
                    }
                }
                result = builder.ToString().TrimEnd();

                string last_not_fanyifile = $"{dir}/not_fanyi_Lang_{langIter.Current.Key.ToString().ToLower()}.csv";
                if (File.Exists(last_not_fanyifile))
                {
                    File.Delete(last_not_fanyifile);
                }
                File.WriteAllText(last_not_fanyifile, result, end);
            }
        }

        /// <summary>
        /// 把已翻译的文本写入到Table表中，即使用lgInfoDict翻译recordFileSet中记录的文件
        /// </summary>
        /// <param name="lgInfoDict"></param>
        /// <param name="srcDir"></param>
        /// <param name="filtter"></param>
        static public void FanYiTableTxt(HashSet<TableCls> recordFileSet, Dictionary<string, LgInfo> lgInfoDict, LANGUAGE lang)
        {

            foreach (var recordCls in recordFileSet)
            {
                foreach (var lineCls in recordCls.lineLst)
                {

                    for (int index = 0; index < lineCls.fieldLst.Count; index++)
                    {
                        var curField = lineCls.fieldLst[index];
                        if (GrabChineseTool.IsContainChinese(curField))
                        {
                            if (lgInfoDict.TryGetValue(curField, out var dstLang))
                            {
                                var dstTxt = dstLang.GetLang(lang);
                                if (!string.IsNullOrEmpty(dstTxt))
                                {
                                    
                                    lineCls.fieldLst[index] = dstTxt;
                                    recordCls.IsChange = true;
                                }
                            }
                        }                    
                    }
                }
            }
        }
        /// <summary>
        /// 将翻译内容写入本地
        /// </summary>
        /// <param name="recordFileSet"></param>
        static public void FlushFanYiToTable(HashSet<TableCls> recordFileSet)
        {
            foreach (var recordCls in recordFileSet)
            {
                recordCls.Flush();
            }
        }
        /// <summary>
        /// 拷贝文件
        /// </summary>
        static public void CopyDir(string srcDir, string dstDir)
        {
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            var files = Directory.GetFiles(srcDir, "*.txt", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var newFilePath = file.Replace(srcDir, dstDir);
                var newDir = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
                File.Copy(file, file.Replace(srcDir,dstDir), true);
            }
        }
    }
}
