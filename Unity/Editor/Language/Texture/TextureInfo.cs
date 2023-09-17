using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language
{
    public enum LangTexMode
    { 
        None,
        /// <summary>
        /// md5相同
        /// </summary>
        MD5_EQUAL,
        /// <summary>
        /// md5不同
        /// </summary>
        MD5_NOEQUAL,
        /// <summary>
        /// 连夜联运新增
        /// </summary>
        NEW,
        /// <summary>
        /// 标注为不需要翻译，用于系列帧动画
        /// </summary>
        NO_TRANSLATE,
        /// <summary>
        /// 图片已翻译
        /// </summary>
        TRANSLATED,
        /// <summary>
        /// 已提取
        /// </summary>
        EXTRACTED,
        /// <summary>
        /// 不带中文的图片
        /// </summary>
        NO_CHINESE,
    }
    /// <summary>
    /// 新增的图片需要翻译，md5不同的图片可以直接使用
    /// </summary>
    internal class LangTextureInfo
    {
        public string md5;
        public string filePath;
        public bool IsEqual = false;
        public LangTexMode LangTexMode;
        public LangTextureInfo(string filePath)
        {
            this.filePath = filePath;
            md5 = GfxUtils.GetMD5Hash(this.filePath);
        }
    }
}
