using System;

namespace Language
{
    class Program
    {
        static void Main(string[] args)
        {
            PargmaTypeCheck mPargmaTypeCheck = new PargmaTypeCheck();
            mPargmaTypeCheck.Init(args);
            int mode = int.Parse(mPargmaTypeCheck.GetValue(PargmaType.Mode));
            string SRC_TB_DIR = mPargmaTypeCheck.GetValue(PargmaType.SRC_TB_DIR);
            string DST_TB_DIR = mPargmaTypeCheck.GetValue(PargmaType.DST_TB_DIR);
            string LangInfoPath = mPargmaTypeCheck.GetValue(PargmaType.LangInfo);
            string addChinese = mPargmaTypeCheck.GetValue(PargmaType.AddChinese);
            string recordFile = mPargmaTypeCheck.GetValue(PargmaType.RecordFile);
            string findStr = mPargmaTypeCheck.GetValue(PargmaType.FindStr);
            string prefabInfoFilePath = mPargmaTypeCheck.GetValue(PargmaType.PrefabInfo);
            LANGUAGE dstLang = LANGUAGE.NONE;
            Enum.TryParse<LANGUAGE>(mPargmaTypeCheck.GetValue(PargmaType.DstLang), out dstLang);

            #region 提取新增中文并记录带有中文的文件
            var lgInfoDict = GrabChineseTool.GetLgInfo(LangInfoPath);
            if (0 != (mode & 1))
            {
                Console.WriteLine("########## 1.1 开始提取中文...");
                var cnSetDict = GrabChineseTool.GrabChinese(SRC_TB_DIR, findStr, new string[] { LangInfoPath, addChinese, recordFile, prefabInfoFilePath });
                var newChinese = GrabChineseTool.AnalyzeChinese(cnSetDict, lgInfoDict);
                #region  prefab ui 中文提取
                Console.WriteLine($"########## 1.2 处理ui中文");
                var uiCnSet = GrabJsonChinese.GrabChinese(prefabInfoFilePath);
                GrabChineseTool.AnalyzeChinese(uiCnSet, lgInfoDict, ref newChinese);
                #endregion
                Console.WriteLine($"########## 1.3 新增中文写入文件:{addChinese}");
                GrabChineseTool.WriteToFile(newChinese, addChinese);
                GrabChineseTool.RecordChineseFile(cnSetDict, SRC_TB_DIR, recordFile);
            }
            #endregion

            #region 将翻译内容添加到LangInfo中
            if (0 != (mode & 2))
            {
                if (dstLang != LANGUAGE.NONE)
                {
                    Console.WriteLine($"########## 2.1 将翻译内容添加到记录本中...");
                    string FanYiFilePath = mPargmaTypeCheck.GetValue(PargmaType.FanYi);
                    int FanYi_CN = int.Parse(mPargmaTypeCheck.GetValue(PargmaType.COL_CN));
                    int FanYi_Dst = int.Parse(mPargmaTypeCheck.GetValue(PargmaType.COL_Dst));
                    MergeLangTool.MergeLangFromTxt(lgInfoDict, dstLang, FanYiFilePath, FanYi_CN, FanYi_Dst);
                    Console.WriteLine($"########## 2.2 将翻译内容保存到文件,{LangInfoPath}");
                    TranslateTableTool.WriteLgInfoToFile(lgInfoDict, LangInfoPath);
                }
                else
                {
                    Console.WriteLine("DstLang Is None");
                }
            }
            #endregion

            #region 将已翻译内容替换到表中
            if (0 != (mode & 4))
            {
                Console.WriteLine("########## 4.1 正在拷贝文件");
                TranslateTableTool.CopyDir(SRC_TB_DIR, DST_TB_DIR);
            }
            if (0 != (mode & 8))
            {
                if (dstLang != LANGUAGE.NONE)
                {
                    
                    Console.WriteLine("########## 8.1 翻译文本导入到表中");
                    var recordFileSet = GrabChineseTool.GetRecordFiles(recordFile, DST_TB_DIR);
                    TranslateTableTool.FanYiTableTxt(recordFileSet, lgInfoDict, dstLang);
                    TranslateTableTool.FlushFanYiToTable(recordFileSet);

                    #region 翻译json文件
                    Console.WriteLine("########## 8.2 翻译Prefab文本");
                    GrabJsonChinese.FanYiChinese(prefabInfoFilePath, lgInfoDict, dstLang);
                    #endregion
                }
                else
                {
                    Console.WriteLine("DstLang Is None");
                }
            }
            #endregion
            Console.WriteLine("已完成");
            lgInfoDict.Clear();
            System.GC.Collect();
        }
    }
}
