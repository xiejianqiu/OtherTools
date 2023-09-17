using System;
using System.Collections.Generic;
using System.Text;

namespace Language
{
    public enum PargmaType {
        Mode,
        /// <summary>
        /// 原始表目录
        /// </summary>
        SRC_TB_DIR,
        /// <summary>
        /// 待翻译目录
        /// </summary>
        DST_TB_DIR,
        /// <summary>
        /// 存放多语言文件
        /// </summary>
        LangInfo,
        /// <summary>
        /// 新增中文
        /// </summary>
        AddChinese,
        /// <summary>
        /// 用于记录哪些表中有中文
        /// </summary>
        RecordFile,
        /// <summary>
        /// 目标语言
        /// </summary>
        DstLang,
        /// <summary>
        /// 翻译文本
        /// </summary>
        FanYi,
        /// <summary>
        /// 翻译文件中的中文在第几列
        /// </summary>
        COL_CN,
        /// <summary>
        /// 翻译文件中的母包语种在第几列
        /// </summary>
        COL_Dst,
        /// <summary>
        /// Prefab信息文件，包含中文
        /// </summary>
        PrefabInfo,
        /// <summary>
        /// 设置需要查找的字符串
        /// </summary>
        FindStr,
    }
    public class PargmaTypeCheck {
        private Dictionary<PargmaType, string> argsDict;

        public PargmaTypeCheck()
        {
            this.argsDict = new Dictionary<PargmaType, string>();
        }
        public void Init(string[] args) {
            for (int index = 0; index < args.Length; index++)
            {
                try
                {
                    Console.WriteLine($"################ {index}:{args[index]}");
                    if (Enum.TryParse<PargmaType>(args[index], out var val))
                    {
                        argsDict[val] = "";
                        if (index + 1 < args.Length)
                        {
                            if (int.TryParse(args[index + 1], out var iVal) || !Enum.TryParse<PargmaType>(args[index + 1], out var val2))
                            {
                                argsDict[val] = args[index + 1];
                                index += 1;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //=============打印参数信息========================
            StringBuilder logBuilder = new StringBuilder();
            var iter = argsDict.GetEnumerator();
            while (iter.MoveNext()) {
                logBuilder.AppendLine($"{iter.Current.Key} {iter.Current.Value}");
            }
            Console.WriteLine($"################ 透传参数信息:{argsDict.Count}\n{logBuilder.ToString()}");
        }
        /// <summary>
        /// 该函数只对布尔型参数有效
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTrue(PargmaType type) {

            return argsDict.ContainsKey(type) && "true" == argsDict[type];
        }
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetValue(PargmaType type) {
            if (argsDict.ContainsKey(type))
            {
                return argsDict[type];
            }
            return string.Empty;
        }
        public bool HasKey(PargmaType type) {
            return argsDict.ContainsKey(type);
        }
    }
}
