using Boo.Lang;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class EncryUtil
    {
        static public void Encript(string filePath, string savePath)
        {
            var data = File.ReadAllBytes(filePath);
            using (var myStream = new MyStream(savePath, FileMode.Create))
            {
                myStream.Write(data, 0, data.Length);
            }
        }
        static public Stream Decript(string filePath)
        {
            var fileStream = new MyStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 4, false);
            return fileStream;
        }
    }
}
