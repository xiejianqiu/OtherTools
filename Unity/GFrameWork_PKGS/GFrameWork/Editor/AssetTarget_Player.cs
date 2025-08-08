using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace GameFramework
{
    public partial class AssetTarget : System.IComparable<AssetTarget>
    {
        static public void CreatePlayerPackAset()
        {
                var dirRoot = Application.dataPath + @"\ResMS\Model\Player";
                var dirs = Directory.GetDirectories(dirRoot);
                foreach (var dir in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    string filepath = GetPackDir("Player" + dirInfo.Name.ToLower() + ".all");
                    if (!File.Exists(filepath))
                    {
                        File.Create(filepath);
                    }
                }
        }
        static string[][] sPlayerPack;
        static public void CreatePlayerAssetTarget()
        {

            sPlayerPack = new string[][] {
                new string[] { @"Assets\ResMS\Model\Player"}
            };
            var dirRoot = Application.dataPath + @"\ResMS\Model\Player";
            var dirs = Directory.GetDirectories(dirRoot);
            foreach (var dir in dirs)
            {
                var dirName = Path.GetDirectoryName(dir).ToLower();

                string filepath = GetPackDir(dirName + ".all");
                CheckAndCreateFile(filepath);
                FileInfo file = new FileInfo(filepath);
                AssetTarget target = AssetBundleUtils.Load(file);
                target.exportType = AssetBundleExportType.Root;
                shadersAssetTarget = target;
            }
        }
    }
}
