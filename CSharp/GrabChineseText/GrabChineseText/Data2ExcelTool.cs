using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using LfireTool;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;

namespace LYTools
{
    class Data2ExcelTool
    {
        public static bool has_error = false;

        private static Dictionary<object, string> err_map = new Dictionary<object, string>()
        {
            {-2146826281, "#DIV/0!"},
            {-2146826246, "#N/A"},
            {-2146826245, "#GETTING_DATA"},
            {-2146826259, "#NAME?"},
            {-2146826288, "#NULL!"},
            {-2146826252, "#NUM!"},
            {-2146826265, "#REF!"},
            {-2146826273, "#VALUE!"}
        };

        public static bool ExportExcel(string path, string[][] data)
        {
            if (path == null) return false;
            if (path.Length == 0) return false;
            if (data == null) return false;

            try
            {
                has_error = false;
                System.Reflection.Missing miss = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();//建立Excel对象//建立Excel对象
                excel.Visible = false;
                excel.DisplayAlerts = false;
                excel.ScreenUpdating = false;
                excel.AlertBeforeOverwriting = false;

                excel.Workbooks.Add(true); //Excel表为添加状态
                if (excel == null)
                {
                    MessageBox.Show("没有找到Excel！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    has_error = true;
                    return false;
                }
                Microsoft.Office.Interop.Excel.Workbooks books = excel.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook book = books.Add(miss);
                Microsoft.Office.Interop.Excel._Worksheet sheet = (Microsoft.Office.Interop.Excel._Worksheet)book.ActiveSheet;

                int rowCount = data.Length;
                int colCount = data[0].Length;
                Range range = sheet.get_Range("A1", miss);
                range = range.get_Resize(rowCount, colCount);
                object[,] cells = new object[rowCount, colCount];
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        cells[i, j] = "'" + data[i][j];
                    }
                }
                range.set_Value(miss, cells);

                sheet.SaveAs(path, miss, miss, miss, miss, miss, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, miss, miss, miss);
                book.Close(false, miss, miss);
                excel.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(book);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(books);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                GC.Collect(); //强执对所有代码进行垃圾回收
                // MessageBox.Show("数据已经成功导出！！");
                //System.Diagnostics.Process.Start(strname);  //关联进程
                return true;
            }
            catch (Exception ex)
            {
                has_error = true;
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static string [][]LoadExcel(string path)
        {
            string[][] data = null;
            has_error = false;
            try
            {
                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();//建立Excel对象//建立Excel对象
                if (excel == null)
                {
                    System.Console.WriteLine("<script>alert('Can't access excel')</script>");
                    has_error = true;
                    return data;
                }
                else
                {
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    excel.ScreenUpdating = false;
                    excel.UserControl = true;
                    // 以只读的形式打开EXCEL文件
                    Workbook book = excel.Application.Workbooks.Open(path, missing, true, missing, missing, missing,
                     missing, missing, missing, true, missing, missing, missing, missing, missing);
                    //取得第一个工作薄
                    Worksheet sheet = (Worksheet)book.Worksheets.get_Item(1);

                    object[,] cells = (object[,])sheet.UsedRange.Value;
                    object cell = null;
                    int rowCount = cells.GetLength(0);
                    int colCount = cells.GetLength(1);
                    if (rowCount != 0 && colCount != 0)
                    {
                        data = new string[rowCount][];
                        for (int i = 0; i < rowCount; ++i)
                        {
                            data[i] = new string[colCount];
                            for (int j = 0; j < colCount; ++j)
                            {
                                cell = cells[i + 1, j + 1];
                                if (cell != null)
                                {
                                    if (!err_map.ContainsKey(cell))
                                    {
                                        data[i][j] = cell.ToString();
                                    }
                                    else
                                    {
                                        data[i][j] = err_map[cell];
                                    }
                                }
                                else
                                {
                                    data[i][j] = "";
                                }
                            }
                        }
                    }

                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(book);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                    GC.Collect();
                    excel = null;
                    return data;
                }
            }
            catch(Exception ex)
            {
                has_error = true;
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
