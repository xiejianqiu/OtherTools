using Boo.Lang;
using JetBrains.Annotations;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    static public class EncryUtil
    {
        static public void EncryFile(string filePath, string savePath)
        {
            var data = File.ReadAllBytes(filePath);
            using (var myStream = new MyStream(savePath, FileMode.Create))
            {
                myStream.Write(data, 0, data.Length);
            }
        }
        static public Stream DecryFile(string filePath)
        {
            var fileStream = new MyStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 4, false);
            return fileStream;
        }
        static public void DecryBytes(byte[] srcData)
        {
            for (int i = 0; i < srcData.Length; i++)
            {
                srcData[i] ^= MyStream.KEY;
            }
        }
        /// <summary>
        /// 加密目录中的ab资源
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="saveDir"></param>
        static public void EncryDir(string srcDir, string saveDir)
        {
            if (Directory.Exists(saveDir))
                Directory.Delete(saveDir, true);
            Directory.CreateDirectory(saveDir);
            var allFiles = Directory.GetFiles(srcDir, "*.ab", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var fileName = Path.GetFileName(file);
                EncryUtil.EncryFile($"{srcDir}/{fileName}", $"{saveDir}/{fileName}");
            }
            Debug.LogError("混淆结束");
        }
    }
}
