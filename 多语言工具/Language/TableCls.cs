using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Language
{
    public class LineCls
    {
        public List<string> fieldLst;

        public LineCls(string curLine)
        {
            this.fieldLst = new List<string>();
            var fields = curLine.Split("\t");
            fieldLst.AddRange(fields);
        }
    }
    /// <summary>
    /// 存放带有中文的文件
    /// </summary>
    public class TableCls
    {
        public List<LineCls> lineLst;
        public string filedPath;
        public bool IsChange = false;
        public TableCls(string filePath)
        {
            this.filedPath = filePath;
            this.lineLst = new List<LineCls>();
            if (File.Exists(filePath))
            {
                var conent = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = conent.Split("\n");
                foreach (var curLine in lines)
                {
                    if (!string.IsNullOrEmpty(curLine))
                    {
                        this.AddLine(new LineCls(curLine));
                    }
                }
            }
            else
            {
                Console.WriteLine($"{filePath}  该文件不存在");
            }
        }
        public void AddLine(LineCls lineCls)
        {
            this.lineLst.Add(lineCls);
        }

        internal void Flush()
        {
            if (!this.IsChange)
                return;
            StringBuilder builder = new StringBuilder();
            foreach (var lineCls in lineLst)
            {
                int fieldCnt = lineCls.fieldLst.Count;
                for (var index = 0; index < fieldCnt; index++)
                {
                    var field = lineCls.fieldLst[index];
                    builder.Append(field);
                    if (index < fieldCnt - 1)
                    {
                        builder.Append("\t");
                    }
                }
                builder.Append("\n");
            }
            var result = builder.Replace("\r", "").ToString();
            var end = new UTF8Encoding(false);
            File.WriteAllText(filedPath, result, end);
        }
    }
}
