using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using LfireTool;

namespace LYTools
{
    static class Program
    {
        public static string version = "v1.0";
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}