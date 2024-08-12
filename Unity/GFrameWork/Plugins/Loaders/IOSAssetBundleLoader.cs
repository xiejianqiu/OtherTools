using System.Collections;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 在IOS下的加载
    /// 注意：
    /// IOS下加载可以进行优化：直接在raw目录里进行File读取
    /// </summary>
    public class IOSAssetBundleLoader : MobileAssetBundleLoader
    {
        protected override IEnumerator LoadFromPackage()
        {

            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_assetBundleSourceFile);
            yield return req;
            if(req!=null)
            {
                _bundle = req.assetBundle;
            }

            if(_bundle == null)
            {
                Error();
            }
            else
            {
                if (NeedPreload)
                {
                    if (request == null)
                    {
                        request = _bundle.LoadAllAssetsAsync();
                        if(request == null)
                        {
                            Error();
                            yield break;
                        }
                    }
                    while (!request.isDone)
                    {
                        yield return 0;
                    }
                    ObjAsset = request.asset;
                }

                this.Complete();
            }
            
        }
    }
}