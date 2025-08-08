using System.Collections;
using System.IO;
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
            AssetBundleCreateRequest req = null;
            if (EnvUtils.IsEncryAB())
            {
                using (WWW w = new WWW(_assetBundleSourceFile))
                {
                    yield return w;
                    if (!string.IsNullOrEmpty(w.error))
                    {
                        Debug.LogError(string.Format("#### {0} not exist!", _assetBundleSourceFile));
                    }
                    var data = w.bytes;
                    if (EnvUtils.IsEncryAB())
                    {
                        EncryUtil.DecryBytes(data);
                    }
                    req = AssetBundle.LoadFromMemoryAsync(data);
                }
            }
            else
            {
                req = AssetBundle.LoadFromFileAsync(_assetBundleSourceFile);
            }
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