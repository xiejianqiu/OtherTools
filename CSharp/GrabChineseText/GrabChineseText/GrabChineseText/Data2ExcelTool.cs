using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using LfireTool;

namespace LYTools
{
    class Data2ExcelTool
    {
        public static bool has_error = false;
        public static bool ExportExcel(string path, string[][] data)
        {
            if (path == null) return false;
            if (path.Length == 0) return false;
            if (data == null) return false;

            try
            {
                System.Reflection.Missing miss = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.ApplicationClass();//建立Excel对象//建立Excel对象
                excel.Visible = false;
                excel.DisplayAlerts = false;
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
                int cellcount = 10;
                Microsoft.Office.Interop.Excel._Worksheet sheet = (Microsoft.Office.Interop.Excel._Worksheet)book.ActiveSheet;
                for (int i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        sheet.Cells[i + 1, j + 1] = data[i][j];

                    }
                }
                //sheet.SaveAs()

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
    }
}
