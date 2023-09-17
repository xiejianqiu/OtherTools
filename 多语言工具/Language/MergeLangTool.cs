using System;
using System.Collections.Generic;
using System.Text;

namespace Language
{
    /// <summary>
    /// 多语言合并工具
    /// </summary>
    public class MergeLangTool
    {

        /// <summary>
        ///  将已有翻译合并到LangInfo中
        /// </summary>
        /// <param name="lgInfoDict"></param>
        /// <param name="fanYiFilePath"></param>
        /// <param name="fanYi_CN">中文在第几列</param>
        /// <param name="fanYi_Dst">目标语言</param>
        internal static void MergeLangFromTxt(Dictionary<string, LgInfo> lgInfoDict, LANGUAGE lang, string fanYiFilePath, int fanYi_CN, int fanYi_Dst)
        {
            TableCls langFile = new TableCls(fanYiFilePath);
            foreach (var lineCls in langFile.lineLst)
            {
                if (lgInfoDict.TryGetValue(lineCls.fieldLst[fanYi_CN], out var langInfo))
                {
                    int langIndex = (int)lang;
                    while (langInfo.otheLgLst.Count < langIndex + 1)
                    {
                        langInfo.otheLgLst.Add("");
                    }
                    langInfo.otheLgLst[langIndex] = lineCls.fieldLst[fanYi_Dst];
                }
            }
        }
    }
}
