using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class ABBuilder
    {
        protected AssetBundleDataWriter dataWriter = new AssetBundleDataWriter();
        protected AssetBundlePathHelper pathResolver;
        public BuildTarget CurTarget = BuildTarget.Android;

        public ABBuilder() : this(new AssetBundlePathHelper())
        {

        }

        public ABBuilder(AssetBundlePathHelper resolver)
        {
            this.pathResolver = resolver;
            this.InitDirs();
            AssetBundleUtils.pathResolver = pathResolver;
        }

        void InitDirs()
        {
            new DirectoryInfo(pathResolver.BundleSavePath).Create();
            new FileInfo(pathResolver.HashCacheSaveFile).Directory.Create();
        }

        public void Begin()
        {
            EditorUtility.DisplayProgressBar("Loading", "Loading...", 0.1f);
            AssetBundleUtils.Init();
        }

        public void End()
        {
            AssetBundleUtils.SaveCache();
            AssetBundleUtils.ClearCache();
            EditorUtility.ClearProgressBar();
        }

        protected virtual void Analyze()
        {
            if (AssetTarget.IsUsePack)
            {
                AssetTarget.CreateAssetPacks();
            }
            var all = AssetBundleUtils.GetAll();
            foreach (AssetTarget target in all)
            {
                target.Analyze();
            }
            all = AssetBundleUtils.GetAll();
            foreach (AssetTarget target in all)
            {
                target.Merge();
            }
            all = AssetBundleUtils.GetAll();
            foreach (AssetTarget target in all)
            {
                target.BeforeExport();
            }
            if (AssetTarget.IsUsePack)
            {
                all = AssetBundleUtils.GetAll();
                foreach (AssetTarget target in all)
                {
                    target.AnalyzeAndCollectPackAsset();
                }
                AssetTarget.ProcessPacks();
            }
        }

        public virtual List<AssetTarget> Export(bool buildAssetbundle = true)
        {
            this.Analyze();
            return null;
        }

        public virtual void WriteFile(List<AssetTarget> lst)
        {
        }

        public void AddRootTargets(DirectoryInfo bundleDir,AssetBundleBuildConfig.BundleStrategy eStrategy, string parttern = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (parttern == null)
                parttern =  "*.*";

            FileInfo[] prefabs = bundleDir.GetFiles(parttern, searchOption);
            switch(eStrategy)
            {
                case AssetBundleBuildConfig.BundleStrategy.Normal:
                    {
                        foreach (FileInfo file in prefabs)
                        {
                            AssetTarget target = AssetBundleUtils.Load(file);
                            target.exportType = AssetBundleExportType.Root;
                        }
                    }
                    break;
                case AssetBundleBuildConfig.BundleStrategy.Pack:
                    {
                        AssetTarget target = AssetBundleUtils.LoadPack(bundleDir);
                        target.exportType = AssetBundleExportType.Root;
                        foreach (FileInfo file in prefabs)
                        {
                            target.AddPackAsset(file);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        internal void SaveDepAll(List<AssetTarget> all)
        {
            string path = Path.Combine(pathResolver.BundleSavePath, pathResolver.DependFileName);
            string detailPath = Path.Combine(Application.dataPath,"abdetail.info");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if(File.Exists(detailPath))
            {
                File.Delete(detailPath);
            }

            List<AssetTarget> exportList = new List<AssetTarget>();
            for (int i = 0; i < all.Count; i++)
            {
                AssetTarget target = all[i];
                if (target.needSelfExport)
                    exportList.Add(target);
            }

            exportList.Sort(delegate (AssetTarget x, AssetTarget y)
            {
                return x.assetPath.CompareTo(y.assetPath);
            });
            AssetBundleDataWriter writer = dataWriter;
            writer.Save(path, exportList.ToArray());
            writer.SaveDetail(detailPath, exportList.ToArray());


        }

        internal void CheckDuplicateName(AssetTarget[] targets)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            for(int i = 0; i < targets.Length; i++)
            {
                AssetTarget vTarget = targets[i];
                if(dic.ContainsKey(vTarget.bundleName))
                {
                    Debug.LogError("DuplicateABName: "+ vTarget.bundleName + "  for file: "+vTarget.assetPath);
                } 
                else
                {
                    dic.Add(vTarget.bundleName,vTarget.assetPath);
                }
            }
        }

        private void SetDataWriter(AssetBundleDataWriter w)
        {
            this.dataWriter = w;
        }

        /// <summary>
        /// 删除未使用的AB，可能是上次打包出来的，而这一次没生成的
        /// </summary>
        /// <param name="all"></param>
        internal void RemoveUnused(List<AssetTarget> all)
        {
            HashSet<string> usedSet = new HashSet<string>();
            for (int i = 0; i < all.Count; i++)
            {
                AssetTarget target = all[i];
                if (target.needSelfExport)
                    usedSet.Add(target.bundleName);
            }

            DirectoryInfo di = new DirectoryInfo(pathResolver.BundleSavePath);
            FileInfo[] abFiles = di.GetFiles("*.ab");
            for (int i = 0; i < abFiles.Length; i++)
            {
                FileInfo fi = abFiles[i];
                if (usedSet.Add(fi.Name))
                {

                    fi.Delete();
                    //for U5X
                    File.Delete(fi.FullName + ".manifest");
                }
            }
        }
    }
}