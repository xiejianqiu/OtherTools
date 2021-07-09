using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Windows.Forms;

namespace LYTools
{
    class FileTools
    {
        public static void CreateDirPath(string abs_path)
        {
            string path = abs_path;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        ///   用来遍历删除目录下的文件以及该文件夹
        /// </summary>
        public static void DeleteFileByDirectory(DirectoryInfo info)
        {
            foreach (DirectoryInfo newInfo in info.GetDirectories())
            {
                DeleteFileByDirectory(newInfo);
            }
            foreach (FileInfo newInfo in info.GetFiles())
            {
                newInfo.Attributes = newInfo.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                newInfo.Delete();
            }
            info.Attributes = info.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            info.Delete();
        }

        /// <summary>
        /// 取是指定目录下所有的.svn目录
        /// </summary>
        /// <param name="strBaseDir"></param>
        /// <param name="al"></param>
        public static void GetAllSvnDirList(string strBaseDir, ArrayList al)
        {
            DirectoryInfo di = new DirectoryInfo(strBaseDir);
            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                if (Directory.Exists(diA[i].FullName))
                {
                    if (diA[i].FullName.ToLower().EndsWith(".svn"))
                        al.Add(diA[i]);
                }

                //diA[i].FullName是某个子目录的绝对地址，把它记录在ArrayList中
                GetAllSvnDirList(diA[i].FullName, al);
                //注意：递归了。逻辑思维正常的人应该能反应过来
            }
        }

        /// <summary>
        /// 取是指定目录下所有指定扩展名的文件
        /// </summary>
        /// <param name="strBaseDir"></param>
        /// <param name="al"></param>
        public static void GetAllFilePathListByExtName(string strBaseDir, ArrayList retList, ArrayList fileNameFiltersArr)
        {
            
            DirectoryInfo di = new DirectoryInfo(strBaseDir);
            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                if (Directory.Exists(diA[i].FullName))
                {
                    if (diA[i].FullName.ToLower().EndsWith(".svn"))
                        retList.Add(diA[i]);
                }

                //diA[i].FullName是某个子目录的绝对地址，把它记录在ArrayList中
                GetAllSvnDirList(diA[i].FullName, retList);
                //注意：递归了。逻辑思维正常的人应该能反应过来
            }
        }

        /// <summary>
        /// 查找指定文件夹下指定后缀名的文件
        /// </summary>
        /// <param name="directory">文件夹</param>
        /// <param name="pattern">后缀名</param>
        /// <returns>文件路径</returns>
        public static List<string> GetFiles(DirectoryInfo directory, List<string> result, string pattern, bool isIncludeDir)
        {
            if( pattern == "" || pattern == null )
                 pattern = "*.*";

             if (directory != null && directory.Exists)
            {
                try
                {
                    foreach (FileInfo info in directory.GetFiles(pattern))
                    {
                        result.Add(info.FullName.ToString());
                    }
                }
                catch 
                { 

                }
                foreach (DirectoryInfo info in directory.GetDirectories())
                {
                    if( isIncludeDir )
                        result.Add(info.FullName.ToString());//目录名称也包含进去
                    GetFiles(info, result, pattern, isIncludeDir);
                }
            }
            return result;
        }

//找了一下，以前写过这个方法，调用它就可以得到结果。
//比如List<string> FindResult = GetFiles(@"C:\","*.*");就可以得到C盘下所有文件。
//你也可以只查找图片，List<string> FindResult = GetFiles(@"盘符:\a","*.jpg");

        /// <summary>
        /// 取得盘符路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetDiskPath(string fullPath)
        {
            string str = "";
            int pos_start = fullPath.IndexOf('\\');
            if (pos_start == -1) return fullPath + '\\';

            str = fullPath.Substring(0, pos_start + 1);
            return str;
        }

        /// <summary>
        /// 取最后一个斜杆与扩展名逗点中间的文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExNameFromPath(string path)
        {
            string str = "";
            int pos_start = path.LastIndexOf('\\') + 1;
            str = path.Substring(pos_start);
            int pos_end = path.LastIndexOf('.');
            str = path.Substring(pos_start, pos_end - pos_start);
            return str;
        }

        /// <summary>
        /// 通过路径取得文件所在的目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileDir(string path)
        {
            string str = "";
            str = path.Substring(0, path.LastIndexOf('\\'));
            return str;
        }


        /// <summary>
        /// 取最后一个斜杆与扩展名逗点中间的文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileNameWithExNameFromPath(string path)
        {
            string str = "";
            int pos_start = path.LastIndexOf('\\') + 1;
            str = path.Substring(pos_start);
            str = path.Substring(pos_start, path.Length - pos_start);
            return str;
        }
    }
}
