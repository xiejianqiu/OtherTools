using System.Text;
using UnityEngine;

namespace Language
{
    public class LblInfo
    {
        public string name;
        public string relativePath;
        public string text;
        //public int fontSize;
        //public int depth;
        //public int mTextID;
        //public bool supportEncoding;
        //public string pivot;
        //public string alignment;
        //public string overflowMethod;

        public LblInfo()
        {
        }

        public LblInfo(UILabel lbl)
        {
            this.name = lbl.name;
            this.relativePath = this.GetPath(lbl.transform).Replace(@"\", "/");
            this.text = lbl.text.Replace("\n", "#r").Replace("\r", "#r");
            //this.fontSize = lbl.fontSize;
            //this.depth = lbl.depth;
            //this.mTextID = lbl.mTextID;
            //this.supportEncoding = lbl.supportEncoding;
            //this.pivot = lbl.pivot.ToString();
            //this.alignment = lbl.alignment.ToString();
            //this.overflowMethod = lbl.overflowMethod.ToString();
        }
        /// <summary>
        /// 获取相对于根目录的路径
        /// </summary>
        /// <param name="tran"></param>
        /// <returns></returns>
        private string GetPath(Transform tran)
        {
            var builder = new StringBuilder();
            builder.Append(tran.name);
            while (null != tran)
            {
                tran = tran.parent;
                if (null != tran && tran != tran.root)
                {
                    builder.Insert(0, $"{tran.name}\\");
                }
            }
            var path = builder.ToString();
            return path;
        }
    }
}
