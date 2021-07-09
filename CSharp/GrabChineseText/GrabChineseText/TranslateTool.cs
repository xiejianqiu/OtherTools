using Shark;
using System;
using System.Collections;
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


            //DataProvider.Instance.Init();

            string filePath;
            WaitingForm waiting_form = new WaitingForm();
            waiting_form.Show();
            for (var e = m_allTableDict.GetEnumerator(); e.MoveNext(); )
            {
                if (e.Current.Key == @"Client\UIText.txt" ||
                    e.Current.Key == @"Client\RoleName.txt" ||
                    e.Current.Key == @"Client\ShieldString.txt"||
                    e.Current.Key == @"Server\ShieldStrings.txt")
                {
                    //跳过UI翻译表、角色起名表、屏蔽字表
                    continue;
                }

                filePath = e.Current.Key.Replace(".txt", ".xls");
                TranslateChn(e.Current.Value, false);//翻译
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

        static bool GetOldText(string oldText, out string thai)
        {
            thai = null;
            if (oldText.IndexOf("\n") > -1)
            {
                oldText = oldText.Replace("\n", "");
            }
            //Dictionary<int, Tab_UIText> vDict = Tab_UITextProvider.Instance.GetData();
            //List<Tab_UIText> uitext= Tab_UITextProvider.Instance.ListData;
            //for (var e = vDict.GetEnumerator(); e.MoveNext();)
            //{
            //    if (e.Current.Value.OldText == oldText)
            //    {
            //        thai = e.Current.Value.Thai;
            //        return true;
            //    }
            //}
            return false;
        }

        /// <summary>
        /// 将中文翻译成泰文
        /// </summary>
        /// <param name="data"></param>
        static void TranslateChn(string [][]data, bool isTransForRawData)
        {
            string content;
            string thai;
            for (int i = 0; i < data.Length; ++i)
            {
                if(isTransForRawData)
                {
                    if (i < 4) continue;//表头不需要翻译
                }
                for(int j = 0; j < data[i].Length; ++j)
                {
                    if (isTransForRawData)
                    {
                        if (j == 1) continue;//第二列不需要翻译
                    }
                    content = data[i][j];
                    if (!string.IsNullOrEmpty(content) && !m_NumReg.IsMatch(content)) continue;//不包含中文则跳过
                    if (GetTranslation(content, out thai) || GetOldText(data[i][j], out thai))
                    {
                        data[i][j] = thai;
                    }
                }
            }
        }

        static bool GetTranslation(string chinese, out string thai)
        {
            thai = null;
            if (chinese.IndexOf("\n") > -1)
            {
                chinese = chinese.Replace("\n", "");
            }
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

        static List<string> GetAllFilesNames(string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            FileInfo[] finfo = dir.GetFiles();
            List<string> allFileNameArr = new List<string>();
            // 取得指定目录所有Excel文件路径
            foreach (FileInfo xx in finfo)
            {
                string extension = xx.Extension;
                if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                {
                    if (extension.EndsWith(@"UIText.xlsx") || extension.EndsWith(@"UIText.xls"))
                    {
                        continue;
                    }
                    allFileNameArr.Add(xx.Name);
                }
                    
            }
            return allFileNameArr;
        }

        static void LoadOneDirExcels(string dirPath, string subPath, Dictionary<string, string[][]> vAllDict)
        {
            List<string> allFileNames = GetAllFilesNames(dirPath + @"\" + subPath);
            if (allFileNames == null || allFileNames.Count == 0) return;

            string targetPath;
            string[][] data;
            string key;
            for (int i = 0; i < allFileNames.Count; ++i)
            {
                key = subPath + @"\" + allFileNames[i];

                targetPath = dirPath + @"\" + key;
                data = Data2ExcelTool.LoadExcel(targetPath);
                if ( !vAllDict.ContainsKey(key))
                {
                    vAllDict.Add(key, data);
                }
                else
                {
                    MessageBox.Show("不应有相同的文件");
                }
            }
        }

        static Dictionary<string, string[][]> LoadAllExcels(string basePath)
        {
            Dictionary<string, string[][]> vAllDict = new Dictionary<string, string[][]>();
            string[] dirs = new string[] { @"Client" , @"Public" , @"Server" };
            for(int i = 0; i < dirs.Length; ++i)
            {
                LoadOneDirExcels(basePath , dirs[i], vAllDict);
            }
            
            return vAllDict;
        }

        private static string ReadTxtTable2(string strPath)
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

            return strData;
        }


        /// <summary>
        /// 开始
        /// </summary>
        public static void Start2(string dataTableBasePath, string srcPath, string tgtPath)
        {
            if (!Directory.Exists(dataTableBasePath) || !Directory.Exists(srcPath))
            {
                MessageBox.Show("源数据表路径不存在");
                return;
            }

            if (!Directory.Exists(tgtPath))
            {
                Directory.CreateDirectory(tgtPath);
            }

            string clientPath = tgtPath + @"\" + "Client";
            string publicPath = tgtPath + @"\" + "Public";
            string serverPath = tgtPath + @"\" + "Server";

            if (!Directory.Exists(clientPath))
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
            //读取所有的数据表txt,为使用Tab_UIText做准备
            DataProvider.Instance.Init(new DataProvider.ReadAllTextDelegate(ReadTxtTable2));

            string filePath;
            WaitingForm waiting_form = new WaitingForm();
            waiting_form.Show();
            //手动加载所有的xlsx表数据
            Dictionary<string, string[][]> allTableDict = LoadAllExcels(srcPath);
            for (var e = allTableDict.GetEnumerator(); e.MoveNext();)
            {
                if (e.Current.Key == @"Client\UIText.txt" ||
                       e.Current.Key == @"Client\RoleName.txt" ||
                       e.Current.Key == @"Client\ShieldString.txt" ||
                       e.Current.Key == @"Server\ShieldStrings.txt")
                {
                    //跳过UI翻译表、角色起名表、屏蔽字表
                    continue;
                }
                
                filePath = e.Current.Key;
                TranslateChn(e.Current.Value, true);//翻译
                Data2ExcelTool.ExportExcel(tgtPath + @"\" + filePath, e.Current.Value);
            }
            if (waiting_form != null)
            {
                System.Windows.Forms.Application.DoEvents();
                waiting_form.Close();
            }
            MessageBox.Show("输出完毕");
        }
    }
}
