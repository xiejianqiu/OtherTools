using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class AssetBundleBuildConfig : ScriptableObject
    {
        public enum Format
        {
            Text,
            Bin
        }

        public enum BundleStrategy
        {
            Normal,
            Pack,
        }

        public Format depInfoFileFormat = Format.Bin;

        public List<AssetBundleFilter> filters = new List<AssetBundleFilter>();
    }

    [System.Serializable]
    public class AssetBundleFilter
    {
        public bool Valid = true;
        public AssetBundleBuildConfig.BundleStrategy BundleStrategy = AssetBundleBuildConfig.BundleStrategy.Normal;
        public string Path = string.Empty;
        public string Filter = "*.prefab";        
    }
}