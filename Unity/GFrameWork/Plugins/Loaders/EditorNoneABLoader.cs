using UnityEngine;

namespace GameFramework
{
    class EditorNoneABLoader: AssetBundleLoader
    {
        public override void Start()
        {
            this.Complete();
        }


        protected override void Complete()
        {
            if (bundleInfo == null)
            {
                this.state = LoadState.State_Complete;
                this.bundleInfo = bundleManager.CreateBundleInfo(this, UnloadComplete, null, null);
                this.bundleInfo.isReady = true;
                this.bundleInfo.onUnloaded = null;

                Object obj = null;
                if (!bundleName.StartsWith("Scene"))
                {
                    if (bundleName.StartsWith("Music"))
                    {
                        obj = bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".wav",
                            typeof (AudioClip));
                        if (obj == null)
                        {
                            obj = bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".mp3",
                                typeof (AudioClip));
                        }
                    }
                    else if (bundleName.StartsWith("Animation"))
                    {
                        obj = bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".anim",
                            typeof (AnimationClip));
                    }
                    else if (bundleName.StartsWith("Texture"))
                    {
                        obj = bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".png",
                            typeof (Texture));
                        if (obj == null)
                        {
                            obj = bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".exr", typeof(Texture));
                        }
                    }
                    else
                    {
                        obj =
                            obj =
                                bundleManager.EditorNoneABLoadFun("Assets/BundleData/" + bundleName + ".prefab",
                                    typeof (GameObject));
                    }
                    if (obj == null)
                    {
                        //Debug.LogErrorFormat("EditorNoneABLoader Load {0} Fail", bundleName);
                    }
                    bundleInfo.SetMainObject(obj);
                }
            }
            base.Complete();
        }
    }
}
