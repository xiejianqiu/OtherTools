using Shark;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime;
namespace LYTools
{
    class GrabTool
    {
        private static List<string> m_chList;
        private static List<List<string>> m_srcList;
        private static string m_dataTableBasePath;
        private static Regex m_NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文

        public static void Start(string dataTableBasePath, string savePath)
        {
            if( !Directory.Exists(dataTableBasePath))
            {
                MessageBox.Show("源数据表路径不存在");
                return;
            }

            m_chList = new List<string>();
            m_srcList = new List<List<string>>();
            m_dataTableBasePath = dataTableBasePath;

            DataProvider.Instance.Init(new DataProvider.ReadAllTextDelegate(ReadTxtTable));

            string[][] data = MakeEmptyData(m_chList.Count);
            FillHead(data);
            FillMain(data, m_chList, m_srcList);
            WaitingForm waiting_form = new WaitingForm();
            waiting_form.Show();
            Data2ExcelTool.ExportExcel(savePath, data);
            if (waiting_form != null)
            {
                System.Windows.Forms.Application.DoEvents();
                waiting_form.Close();
            }
            MessageBox.Show("输出完毕");
        }


        private static string ReadTxtTable(string strPath)
        {
            strPath = strPath.Replace("/", @"\");
            if (strPath == @"Client\UIText.txt" ||
                strPath == @"Client\RoleName.txt" ||
                strPath == @"Client\ShieldString.txt"||
                strPath == @"Server\ShieldStrings.txt")
            {
                //跳过UI翻译表、角色起名表、屏蔽字表
                return null;
            }

            var localPath = m_dataTableBasePath + @"\" +  strPath;
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

            ParseRows(strData, strPath);
            return strData;
        }

        /// <summary>
        /// 分解数据行
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="strPath"></param>
        private static void ParseRows(string dataTable, string strPath)
        {
            string[] rows = dataTable.Split('\n');
            int len = rows.Length;
            for (int i = 0; i < len; i++)
            {
                if (rows[i].StartsWith("#") || rows[i].Equals(""))
                {
                    continue;
                }

                try
                {
                    ParseCols(rows[i], strPath);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 分解数据列
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="strPath"></param>
        private static void ParseCols(string dataRow, string strPath)
        {
            string[] cols = dataRow.Split('\t');
            List<string> pathList;

            for (int i = 0; i < cols.Length; ++i)
            {
                if (!m_NumReg.IsMatch(cols[i])) continue;//如果不是中文则跳过

                if (!m_chList.Contains(cols[i]))
                {
                    //第一条
                    m_chList.Add(cols[i]);
                    pathList = new List<string>();
                    pathList.Add(strPath);
                    m_srcList.Add(pathList);
                }
                else
                {
                    //已经存在过的
                    int index = m_chList.IndexOf(cols[i]);
                    if (index != -1)
                    {
                        if (index < m_srcList.Count)
                        {
                            if (!m_srcList[index].Contains(strPath))
                            {
                                m_srcList[index].Add(strPath);
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("不可能#1");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("不可能#2");
                    }
                }
            }
        }

        /// <summary>
        /// 制作空白数组
        /// </summary>
        /// <param name="mainRowCount"></param>
        /// <returns></returns>
        static string[][] MakeEmptyData(int mainRowCount)
        {
            string[][] data = new string[mainRowCount + 4][];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new string[4];
            }
            return data;
        }

        /// <summary>
        /// 填写表头
        /// </summary>
        /// <param name="data"></param>
        static void FillHead(string[][] data)
        {
            data[0][0] = "Id";
            data[0][1] = "Desc";
            data[0][2] = "Thai";
            data[0][3] = "Remark";

            data[1][0] = "INT";
            data[1][1] = "STRING";
            data[1][2] = "STRING";
            data[1][3] = "STRING";

            data[2][0] = "Def:";
            data[2][1] = "Nil";
            data[2][2] = "Nil";
            data[2][3] = "Nil";

            data[3][0] = "#ID";
            data[3][1] = "程序不读";
            data[3][2] = "泰文";
            data[3][3] = "备注(在哪些表中)";
        }

        static string MergePath(List<string> pathList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pathList.Count; ++i)
            {
                sb.Append(pathList[i]).Append(";");
            }

            return sb.ToString();
        }

        static void FillMain(string[][] data, List<string> vChineseList, List<List<string>> vRemarkList)
        {
            int mainIndex = 0;
            for (int i = 4; i < data.Length; ++i)
            {
                data[i][0] = mainIndex + "";
                data[i][1] = vChineseList[mainIndex];
                data[i][3] = MergePath(vRemarkList[mainIndex]);
                mainIndex++;
            }
        }
    }
}
