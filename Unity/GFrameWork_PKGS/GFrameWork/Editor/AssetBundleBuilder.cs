using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class AssetBundleBuilder : ABBuilder
    {
        public AssetBundleBuilder(AssetBundlePathHelper resolver)
            : base(resolver)
        {

        }

        public override List<AssetTarget> Export(bool buildAssetbundle=true)
        {
            base.Export();

            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            var packSaveDir = Application.dataPath + "/../Packs/";
            if (Directory.Exists(packSaveDir))
            {
                Directory.Delete(packSaveDir, true);
            }
            Directory.CreateDirectory(packSaveDir);
            //标记所有 asset bundle name
            var all = AssetBundleUtils.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                AssetTarget target = all[i];
                if (target.needSelfExport)
                {
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = target.bundleName;
                    if (target.hashSet.Count > 0)
                    {
                        build.assetNames = target.hashSet.ToArray();
                        list.Add(build);
                        File.WriteAllLines(packSaveDir + target.bundleShortName,target.hashSet.ToArray(), System.Text.Encoding.UTF8);
                        continue;
                    }
                    if (target.assetsPathLst.Count > 0)
                    {
                        build.assetNames = target.assetsPathLst.ToArray();
                    }
                    else
                    {
                        build.assetNames = new string[] { target.assetPath };
                    }
                    list.Add(build);
                    //if (target.assetPath.EndsWith("LoginUI.prefab"))
                    //{
                    //    StringBuilder builder = new StringBuilder();
                    //    foreach (var asset in target.dependencies)
                    //    {
                    //        builder.AppendLine(asset.assetPath);
                    //    }
                    //    Debug.Log(builder.ToString());
                    //}
                }
            }
            list.Sort(delegate (AssetBundleBuild x, AssetBundleBuild y)
            {
                return x.assetBundleName.CompareTo(y.assetBundleName);
            });
            if (buildAssetbundle)
            {
                BuildAssetBundleOptions opts = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree | BuildAssetBundleOptions.DisableWriteTypeTree;
                //开始打包
                BuildPipeline.BuildAssetBundles(pathResolver.BundleSavePath, list.ToArray(), opts, CurTarget);


                //AssetBundle ab = AssetBundle.LoadFromFile(pathResolver.BundleSavePath + "/AssetBundles");

                //AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                //hash
                for (int i = 0; i < all.Count; i++)
                {
                    AssetTarget target = all[i];
                    //Hash128 hash = manifest.GetAssetBundleHash(target.bundleName);
                    //target.bundleCrc = hash.ToString();
                    target.bundleCrc = GfxUtils.GetMD5Hash(pathResolver.BundleSavePath + "/" + target.bundleName);
                }
                //ab.Unload(true);
            }
            return all;
        }

        public override void WriteFile(List<AssetTarget> lst)
        {
            this.SaveDepAll(lst);

            this.RemoveUnused(lst);

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }
    }
}
