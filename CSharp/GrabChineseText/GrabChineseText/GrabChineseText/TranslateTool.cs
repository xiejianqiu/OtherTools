using Shark;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LYTools
{
    class TranslateTool
    {
        static Dictionary<string, string[][]> m_allTableDict;
        private static string m_dataTableBasePath;
        private static Regex m_NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
        /// <summary>
        /// 开始
        /// </summary>
        public static void Start(string dataTableBasePath, string savePath)
        {
            if (!Directory.Exists(dataTableBasePath))
            {
                MessageBox.Show("源数据表路径不存在");
                return;
            }

            if( !Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string clientPath = savePath + @"\" + "Client";
            string publicPath = savePath + @"\" + "Public";
            string serverPath = savePath + @"\" + "Server";

            if( !Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
            }

            if (!Directory.Exists(publicPath))
            {
                Directory.CreateDirectory(publicPath);
            }

            if (!Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }
            
            m_dataTableBasePath = dataTableBasePath;
            m_allTableDict = new Dictionary<string, string[][]>();
            DataProvider.Instance.Init(new DataProvider.ReadAllTextDelegate(ReadTxtTable));

            string filePath;
            WaitingForm waiting_form = new WaitingForm();
            waiting_form.Show();
            for (var e = m_allTableDict.GetEnumerator(); e.MoveNext(); )
            {
                if (e.Current.Key == @"Client\UIText.txt" ||
                    e.Current.Key == @"Client\RoleName.txt" ||
                    e.Current.Key == @"Client\ShieldString.txt")
                {
                    //跳过UI翻译表、角色起名表、屏蔽字表
                    continue;
                }

                filePath = e.Current.Key.Replace(".txt", ".xls");
                TranslateChn(e.Current.Value);//翻译
                Data2ExcelTool.ExportExcel(savePath + @"\" + filePath, e.Current.Value);
                //System.Console.WriteLine(fileName);
            }
            if (waiting_form != null)
            {
                System.Windows.Forms.Application.DoEvents();
                waiting_form.Close();
            }
            MessageBox.Show("输出完毕");
        }

        /// <summary>
        /// 将中文翻译成泰文
        /// </summary>
        /// <param name="data"></param>
        static void TranslateChn(string [][]data)
        {
            string content;
            string thai;
            for (int i = 0; i < data.Length; ++i)
            {
                for(int j = 0; j < data[i].Length; ++j)
                {
                    content = data[i][j];
                    if (!string.IsNullOrEmpty(content) && !m_NumReg.IsMatch(content)) continue;//不包含中文则跳过
                    if (GetTranslation(content, out thai))
                    {
                        data[i][j] = thai;
                    }
                }
            }
        }

        static bool GetTranslation(string chinese, out string thai)
        {
            thai = null;
            Dictionary<int, Tab_UIText> vDict = Tab_UITextProvider.Instance.GetData();
            for (var e = vDict.GetEnumerator(); e.MoveNext();)
            {
                if (e.Current.Value.Chinese == chinese)
                {
                    thai = e.Current.Value.Thai;
                    return true;
                }
            }
            return false;
        }

        private static string ReadTxtTable(string strPath)
        {
            strPath = strPath.Replace("/", @"\");

            var localPath = m_dataTableBasePath + @"\" + strPath;
            string strData = null;
            StreamReader sr = null;
            try
            {
                if (!File.Exists(localPath))
                {
                    return null;
                }
                sr = File.OpenText(localPath);
                strData = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("File Can't find!!!!");
            }
            finally
            {
                if (sr != null)
                {
                    sr.Dispose();
                }
            }

            string[] rows = strData.Split('\n');
            string[][] data = new string[rows.Length][];
            string[] srcCols;
            string[] newCols;
            for (int i = 0; i < rows.Length; ++i)
            {
                srcCols = rows[i].Split('\t');
                newCols = new string[srcCols.Length + 1];
                CopyToNewArr(srcCols, newCols);
                data[i] = newCols;
            }

            m_allTableDict.Add(strPath, data);
            return strData;
        }

        static void CopyToNewArr(string []src, string []tgt)
        {
            for(int i = 0; i < src.Length; ++i)
            {
                if( i == 0)
                {
                    tgt[i] = src[i];
                    tgt[i + 1] = string.Empty;
                }
                else
                {
                    tgt[i + 1] = src[i];
                }
            }
        }
    }
}
