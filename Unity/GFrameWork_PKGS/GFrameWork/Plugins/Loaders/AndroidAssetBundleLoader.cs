using System.Collections;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 注意：未经测试，不要用
    /// </summary>
    class AndroidAssetBundleLoader : MobileAssetBundleLoader
    {
        protected override IEnumerator LoadFromPackage()
        {

            //直接用 LoadFromFile
            _assetBundleSourceFile = bundleManager.pathResolver.GetBundleSourceFile(bundleName, false);
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_assetBundleSourceFile);
            yield return req;
            _bundle = req.assetBundle;

            if(NeedPreload)
            {
                if(request == null)
                {
                    request = _bundle.LoadAssetAsync(bundleData.debugName);
                }
                while(!request.isDone)
                {
                    yield return 0;
                }
                ObjAsset = request.asset;
            }
                       

            Complete();
        }
    }
}