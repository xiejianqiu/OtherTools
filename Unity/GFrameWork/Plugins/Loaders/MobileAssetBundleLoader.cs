using System.Collections;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework
{
    /// <summary>
    /// 在手机运行时加载
    /// </summary>
    public class MobileAssetBundleLoader : AssetBundleLoader
    {
        protected AssetBundle _bundle;
        protected bool _hasError;
        protected string _assetBundleCachedFile;
        protected string _assetBundleSourceFile;
        protected string _assetBundleCDNURL;

        /// <summary>
        /// 开始加载
        /// </summary>
        override public void Start()
        {
            if (_hasError)
                state = LoadState.State_Error;

            if (state == LoadState.State_None)
            {
                state = LoadState.State_Loading;
                if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
                {
                    //Debug.LogErrorFormat("LoadDepends Start bundle: {0}", bundleName);
                }
                this.LoadDepends();
                if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
                {
                   //Debug.LogErrorFormat("LoadDepends end bundle: {0}", bundleName);
                }
            }
            else if (state == LoadState.State_Error)
            {
                this.Error();
            }
            else if (state == LoadState.State_Complete)
            {
                this.Complete();
            }
        }

        /// <summary>
        /// 先加载依赖项
        /// </summary>
        void LoadDepends()
        {
            if (depLoaders == null)
            {
                depLoaders = new AssetBundleLoader[bundleData.dependencies.Length];
                for (int i = 0; i < bundleData.dependencies.Length; i++)
                {
                    depLoaders[i] = bundleManager.CreateLoader(bundleData.dependencies[i]);
                    if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
                    {
                        //Debug.LogErrorFormat("add depLoaders : {0}", depLoaders[i].bundleName);
                    }

                    //if (MinLifeTime == -1||(MinLifeTime > depLoaders[i].MinLifeTime&& depLoaders[i].MinLifeTime !=-1))
                    //{
                    //    depLoaders[i].MinLifeTime = MinLifeTime;
                    //}
                    //改为-1，防止主资源还没有加载完成就被卸载
                    //depLoaders[i].MinLifeTime = ABCacheTIme.CACHETIME_TMP;
                    depLoaders[i].IncreaseLock();
                    if (depLoaders[i].MinLifeTime < MinLifeTime)
                    {
                        depLoaders[i].MinLifeTime = MinLifeTime;
                    }
                }
                RefreshPrority();
            }

            _currentLoadingDepCount = 0;
            for (int i = 0; i < depLoaders.Length; i++)
            {
                AssetBundleLoader depLoader = depLoaders[i];
                if (!depLoader.isComplete)
                {
                    _currentLoadingDepCount++;
                    depLoader.onComplete += OnDepComplete;
                    depLoader.Start();
                }
                else
                {
                    if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
                    {
                        //Debug.LogErrorFormat("depLoader already complete : {0}", depLoaders[i].bundleName);
                    }
                }
            }
            this.CheckDepComplete();
        }

        /// <summary>
        /// 其它都准备好了，加载AssetBundle
        /// 注意：这个方法只能被 AssetBundleManager 调用
        /// 由 Manager 统一分配加载时机，防止加载过卡
        /// </summary>
        override public void LoadBundle()
        {
            _assetBundleCachedFile = string.Format("{0}/{1}", bundleManager.pathResolver.BundleCacheDir, bundleName);
            _assetBundleSourceFile = bundleManager.pathResolver.GetBundleSourceFile(bundleName, false);
            
            if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
            {
                //Debug.LogErrorFormat("LoadBundle bundle: {0}", bundleName);
            }
            if (File.Exists(_assetBundleCachedFile))
            {
                bundleManager.StartCoroutine(LoadFromCachedFile());
            }
            else
            if (bundleManager.IsPackAsset(bundleName))
            {
                bundleManager.StartCoroutine(LoadFromPackage());
            }
            else
            {
                if (bundleManager.IsABFromCDN)
                {
                    bundleManager.StartCoroutine(LoadFromCDN());
                    return;
                }
                bundleManager.StartCoroutine(LoadFromPackage());
            }
        }

        /// <summary>
        /// 从已缓存的文件里加载
        /// </summary>
        /// <returns></returns>
        protected AssetBundleRequest request = null;
        protected virtual IEnumerator LoadFromCachedFile()
        {
            if (state != LoadState.State_Error)
            {
                if(bundleManager.LoadDelay >0)
                {
                    yield return new WaitForSeconds(bundleManager.LoadDelay/1000f);
                }
                AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_assetBundleCachedFile);
                yield return req;
                if(req!=null)
                {
                    _bundle = req.assetBundle;
                }
                if(_bundle==null)
                {
                    Error();
                    yield break;
                }
             
                if (NeedPreload)
                {
                    if(request == null)
                    {
                        request = _bundle.LoadAssetAsync(bundleData.debugName);
                        if(request==null)
                        {
                            Error();
                            yield break;
                        }
                    }
                    while(!request.isDone)
                    {
                       yield  return 0;
                    }
                    ObjAsset = request.asset;
                    //ObjAsset = _bundle.LoadAsset(bundleData.debugName);
                }

                this.Complete();
            }
        }

        /// <summary>
        /// 从源文件(安装包里)加载
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LoadFromPackage()
        {
            if (state != LoadState.State_Error)
            {
                if (bundleManager.LoadDelay > 0)
                {
                    yield return new WaitForSeconds(bundleManager.LoadDelay / 1000f);
                }
                //加载主体
                AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_assetBundleSourceFile);
                //Debug.Log("LoadFromPackage   "  + bundleData.debugName);
                yield return req;
                if(req!=null)
                {
                    _bundle = req.assetBundle;
                }
                if(_bundle==null)
                {
                    Error();
                    yield break;
                }

                if (NeedPreload)
                {
                    if (request == null)
                    {
                        request = _bundle.LoadAllAssetsAsync();
                        if(request==null)
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
                    //ObjAsset = _bundle.LoadAsset(bundleData.debugName);
                }
                this.Complete();
            }
        }
        protected void OnDownloadFinisFromCDN(string savePath, bool isHttpError)
        {
            if (isHttpError)
            {
                Error();
            }
            else
            {
                this.LoadBundle();
            }
        }
        /// <summary>
        /// 从CDN下载资源
        /// </summary>
        /// <returns></returns>
        protected IEnumerator LoadFromCDN() {
            if (state != LoadState.State_Error)
            {
                if (bundleManager.LoadDelay > 0)
                {
                    yield return new WaitForSeconds(bundleManager.LoadDelay / 1000f);
                }
                string abUrl = bundleManager.pathResolver.GetABUrl(bundleName);
                MyUnityWebRequest.Get(abUrl, _assetBundleCachedFile, this.bundleData.crc, OnDownloadFinisFromCDN, MyUnityWebRequest.REQ_PRIORITY.HIGHT);
                //using (var req =UnityWebRequestAssetBundle.GetAssetBundle(abUrl))
                //{
                //    yield return req.SendWebRequest();
                //    if (req.isHttpError)
                //    {
                //        Debug.LogError($"### {abUrl}, ERROR:{req.error}");
                //        Error();
                //        yield break;
                //    }
                //    _bundle = (req.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                //    if (_bundle == null)
                //    {
                //        Error();
                //        yield break;
                //    }

                //    if (NeedPreload)
                //    {
                //        if (request == null)
                //        {
                //            request = _bundle.LoadAllAssetsAsync();
                //            if (request == null)
                //            {
                //                Error();
                //                yield break;
                //            }
                //        }
                //        while (!request.isDone)
                //        {
                //            yield return 0;
                //        }
                //        ObjAsset = request.asset;
                //        //ObjAsset = _bundle.LoadAsset(bundleData.debugName);
                //    }
                //    this.Complete();
                //}
            }
        }
        void OnDepComplete(AssetBundleInfo abi)
        {
            if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
            {
               //Debug.LogErrorFormat("complete depLoaders : {0}", abi.bundleName);
            }
            _currentLoadingDepCount--;
            this.CheckDepComplete();
        }

        /// <summary>
        /// 依赖项结束，加载自身
        /// </summary>
        void CheckDepComplete()
        {
            if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
            {
                //Debug.LogErrorFormat("CheckDepCompletebundle: {0},_currentLoadingDepCount: {1} ", bundleName, _currentLoadingDepCount);
            }
            if (_currentLoadingDepCount == 0)
            {
                if (bundleName == "ac14d37a7768b88f52c39271e4e4f4342e838bed.ab")
                {
                    //Debug.LogErrorFormat("bundleManager.Enqueue: {0}", bundleName);
                }
                bundleManager.Enqueue(this);
            }
        }

        override protected void Complete()
        {
            if (bundleInfo == null)
            {
                this.state = (state == LoadState.State_Error?LoadState.State_Error:LoadState.State_Complete);

                this.bundleInfo = bundleManager.CreateBundleInfo(this,UnloadComplete, null, _bundle);
                this.bundleInfo.isReady = true;
                bundleInfo.minLifeTime = MinLifeTime;
                this.bundleInfo.onUnloaded = OnBundleUnload;
                bundleInfo.SetMainObject(ObjAsset);
                
                foreach (AssetBundleLoader depLoader in depLoaders)
                {
                    bundleInfo.AddDependency(depLoader.bundleInfo);
                    depLoader.DeCreaseLock();
                    if (null != depLoader.bundleInfo)
                    {
                        depLoader.bundleInfo.minLifeTime = depLoader.MinLifeTime;
                    }
                }

                _bundle = null;
            }
            else
            {
                if (bundleInfo.minLifeTime < MinLifeTime)
                {
                    bundleInfo.minLifeTime = MinLifeTime;
                }
            }
            base.Complete();
        }

        private void OnBundleUnload(AssetBundleInfo abi)
        {
            this.bundleInfo = null;
            this.state = LoadState.State_None;
            this.prority = 0;
        }

        override protected void Error()
        {
            _hasError = true;
            this.bundleInfo = null;
            base.Error();
        }
    }
}
