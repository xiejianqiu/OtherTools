using System;
using System.Collections.Generic;
using System.Text;

namespace Language
{
    public enum LANGUAGE
    { 
        NONE = -1,
        /// <summary>
        /// 简体中文
        /// </summary>
        CHINESE = 0,
        /// <summary>
        /// 繁体中文
        /// </summary>
        TCHINESE,
        /// <summary>
        /// 英文
        /// </summary>
        ENGLISH,
        /// <summary>
        /// 韩文
        /// </summary>
        KOREA,
        /// <summary>
        /// 泰文
        /// </summary>
        THAILAND,
        /// <summary>
        /// 越南文
        /// </summary>
        VIETNAM,
        MAX,
    }
    /// <summary>
    /// 用于存放翻译内容
    /// </summary>
    public class LgInfo
    {
        public List<string> otheLgLst;

        public LgInfo(string line)
        {
            var fields = line.Split("\t");
            otheLgLst = new List<string>();
            foreach (var field in fields)
            {
                otheLgLst.Add(field);
            }
            while (otheLgLst.Count < (int)LANGUAGE.MAX)
            {
                otheLgLst.Add("");
            }
        }
        public string GetChinese()
        {
            return otheLgLst[(int)LANGUAGE.CHINESE];
        }
        public string GetLang(LANGUAGE lang)
        {
            var iLang = (int)lang;
            if (iLang < otheLgLst.Count)
            {
                return otheLgLst[(int)lang];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
