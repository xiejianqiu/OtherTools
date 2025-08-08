using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public partial class AssetTarget : System.IComparable<AssetTarget>
    {
        public static bool IsUsePack = false;
        /// <summary>
        /// 设置不能单独单包的目录
        /// </summary>
        private string[] unstandaloneDir = new string[] { 
            //@"Assets\ResMS\", 
            //@"Assets\ResMS\UI\Texture", 
            //@"Assets\ResMS\UI\Texture", 
            //@"Assets\ResMS\UI\UITexture\",

            @"Assets\ResMS\Model\",
            @"Assets\(Temporary)\",
            @"Assets\Scene\Effect\",
            @"Assets\Scene\Content\",
        };
        /// <summary>
        /// 哪些格式不能单独打包
        /// </summary>
        private string[] partterns = new string[] {
            //光照贴图
            ".exr",
            ".compute",
            //".FBX",
            //".fbx",
            ".asset",
            //".mat",
            //".png",
            //".PNG",
            //".jpg",
            //".tga",
            //".controller"
        };
        public HashSet<string> hashSet { get; private set; } = new HashSet<string>();
        public static void CreateAssetPacks()
        {
            AssetTarget.CreateCustomPacks();
        }
        private static string PackPath
        {
            get
            {
                return Application.dataPath + @"\BundleData\Packs\";
            }
        }
        static public string GetPackDir(string fileName)
        {
            if (!Directory.Exists(PackPath))
            {
                Directory.CreateDirectory(PackPath);
            }
            return $"{PackPath}{fileName}";
        }
        private static void CheckAndCreateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }
        private void ProcessPackDepedency(string path, AssetTarget pack)
        {
            pack.hashSet.Add(path);
        }
        static Dictionary<string, AssetTarget> customPackDict;
        /// <summary>
        /// 创建AssetTarget
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="at"></param>
        /// <returns></returns>
        public static AssetTarget CreateCustomPack(string fileName, out AssetTarget at)
        {
            string filepath = GetPackDir(fileName);
            CheckAndCreateFile(filepath);
            FileInfo file = new FileInfo(filepath);
            AssetTarget target = AssetBundleUtils.Load(file);
            target.exportType = AssetBundleExportType.Root;
            at = target;
            if (null == customPackDict)
            {
                customPackDict = new Dictionary<string, AssetTarget>();
            }
            customPackDict[fileName] = target;
            return target;
        }
        private static AssetTarget basePack;
        public static void CreateCustomPacks()
        {
            CreateCustomPack("shader.all", out shadersAssetTarget);
            CreateCustomPack("music.all", out musicAssetTarget);
            //CreateCustomPack("pknead.all", out playerBaseAT);
            CreateCustomPack("plugins.all", out pluginsPack);
            //CreateCustomPack("mainuidep.all", out mainuidepAT);
            CreateCustomPack("base.all", out basePack);
            AssetTarget.CreateDirPack();
            CreateCustomPack("effect_mat.all", out effectMatAT);
        }
        /// <summary>
        /// 检查asset是否需要打成一个包
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool CheckAssetAndPack(string path)
        {
            if (CheckShaderAndAddToPack(path))
                return true;
            if (CheckEffectDirAndAddToPack(path))
                return true;
            if (CheckBaseAndAddToPack(path))
                return true;
            return false;
        }
        bool CheckRootAndPack(string path)
        {
            if (CheckAssetAndPack(path))
                return true;
            if (CheckMusicAndAddToPack(path))
                return true;
            if (CheckPlayerBaseAndAddToPack(path))
                return true;
            if (CheckMainUIDepAndPack(path))
                return true;
            return false;
        }
        public bool CheckCanStandalone()
        {
            if (IsUsePack)
            {
                foreach (var p in partterns)
                {
                    if (this.assetPath.StartsWith(@"Assets\ResMS\UI\")
                        || this.assetPath.StartsWith(@"Assets\ResMS\NewUI\")
                        || this.assetPath.StartsWith(@"Assets\English\UI\")
                        || this.assetPath.StartsWith(@"Assets\English\NewUI\")
                    )
                        continue;
                    if (this.assetPath.EndsWith(p))
                        return false;
                }

                foreach (var dir in unstandaloneDir)
                {
                    if (this.assetPath.StartsWith(dir))
                        return false;
                }
            }
            return true;
        }
        public void AnalyzeAndCollectPackAsset()
        {
            //if (this.exportType != AssetBundleExportType.Asset)
            //    return;
            CheckRootAndPack(this.assetPath);
        }
        /// <summary>
        /// 取出共有的资源
        /// </summary>
        static public void ProcessPacks()
        {
            Dictionary<string, int> assetCntDict = new Dictionary<string, int>();
            #region 确保pack间不包含相同资源
            using (var iter = customPackDict.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var curSelectPack = iter.Current.Value;
                    foreach (var ap in curSelectPack.hashSet)
                    {
                        if (!assetCntDict.ContainsKey(ap))
                        {
                            assetCntDict[ap] = 0;
                        }
                        assetCntDict[ap]  += 1;
                    }
                }
            }
            using (var iter = assetCntDict.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    if (iter.Current.Value <= 1)
                        continue;
                    var assetPath = iter.Current.Key;
                    basePack.hashSet.Add(assetPath);
                    var packIter = customPackDict.GetEnumerator();
                    while (packIter.MoveNext())
                    {
                        var curAT = packIter.Current.Value;
                        if (curAT == basePack)
                            continue;
                        if (curAT.hashSet.Contains(assetPath))
                        {
                            curAT.hashSet.Remove(assetPath);
                            curAT.AddDependParent(basePack);
                        }
                    }
                }
            }
            #endregion
            #region 处理pack已各个资源之间的依赖关系
            using (var packIter = customPackDict.GetEnumerator())
            {
                while (packIter.MoveNext())
                {
                    ProcessPack(packIter.Current.Value);
                }
            }
            #endregion
        }
        /// <summary>
        /// 处理pack与各个资源之间的依赖关系
        /// </summary>
        /// <param name="pack"></param>
        static void ProcessPack(AssetTarget pack)
        {
            foreach (var path in pack.hashSet)
            {
                FileInfo fi = new FileInfo(path);
                AssetTarget target = AssetBundleUtils.Load(fi);
                target.exportType = AssetBundleExportType.Asset;

                #region 对target的依赖改为对pack的依赖
                var childrenSet = new List<AssetTarget>(target._dependChildrenSet);
                target.RemoveDependChildren();

                pack.AddDependParent(target);
                foreach (var child in childrenSet)
                {
                    child.AddDependParent(pack);
                }
                #endregion

                #region 处理一个资源包含于多个ab中,t,target的依赖项变为pack的依赖项                  
                var parentSet = new List<AssetTarget>(target._dependParentSet);
                foreach (var child in parentSet)
                {
                    target.RemoveDependParent(child);
                }
                foreach (var child in parentSet)
                {
                    pack.AddDependParent(child);
                }
                #endregion
            }
        }
    }
}
