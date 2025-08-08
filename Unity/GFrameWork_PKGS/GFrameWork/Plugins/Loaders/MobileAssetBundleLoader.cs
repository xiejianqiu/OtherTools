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
        protected string _assetBundleSourceFile;
        protected string _assetBundleCDNURL;
        public const string CDebugABName = "eed3184d14ca2bc2a6474bf8a70d54b4350ccf46.ab";
        const string MainABName = "ae30c7a46d2ef905aedb925fdf35bd39cbc3d4fa.ab";
        static public void DebugMsg(string msg)
        {
            if (EnvUtils.IsCustomAccout())
            {
                Debug.LogError($"###D {msg}");
            }
        }
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
                if (bundleName == CDebugABName || bundleName == MainABName)
                {
                    DebugMsg($"LoadDepends Start bundle: {bundleName}");
                }
                this.LoadDepends();
                if (bundleName == CDebugABName || bundleName == MainABName)
                {
                   DebugMsg($"LoadDepends end bundle: {bundleName}");
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
                    if (bundleName == CDebugABName)
                    {
                        DebugMsg($"add depLoaders : {depLoaders[i].bundleName}");
                    }

                    //if (MinLifeTime == -1||(MinLifeTime > depLoaders[i].MinLifeTime&& depLoaders[i].MinLifeTime !=-1))
                    //{
                    //    depLoaders[i].MinLifeTime = MinLifeTime;
                    //}
                    //改为-1，防止主资源还没有加载完成就被卸载
                    //depLoaders[i].MinLifeTime = ABCacheTIme.CACHETIME_TMP;
                    depLoaders[i].IncreaseLock();
                    if (MinLifeTime == -1 
                        ||(
                        MinLifeTime > depLoaders[i].MinLifeTime 
                        && depLoaders[i].MinLifeTime > 0
                        ))
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
                    if (bundleName == CDebugABName)
                    {
                        DebugMsg($"depLoader already complete : {depLoaders[i].bundleName}");
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

            if (bundleName == CDebugABName || bundleName == MainABName)
            {
                DebugMsg($"LoadBundle bundle: {bundleName}");
            }
            if (EnvUtils.IsUnity_Editor())
            {
                if (EnvUtils.IsEncryAB())
                {
                    _assetBundleSourceFile = string.Format("{0}/{1}", bundleManager.pathResolver.BundleSavePathEncry, bundleName);
                }
                else
                {
                    _assetBundleSourceFile = string.Format("{0}/{1}", bundleManager.pathResolver.BundleSavePath, bundleName);
                }
            }
            else
            { 
                _assetBundleSourceFile = bundleManager.pathResolver.GetBundleSourceFile(bundleName, EnvUtils.IsEncryAB());
            }
            var _assetBundleCachedFile = string.Format("{0}/{1}", bundleManager.pathResolver.BundleCacheDir, bundleName);
            if (File.Exists(_assetBundleCachedFile))
            {
                bundleManager.StartCoroutine(LoadFromCachedFile(_assetBundleCachedFile));
                return;
            }
            if (bundleManager.IsPackAsset(bundleName, md))
            {
                bundleManager.StartCoroutine(LoadFromPackage());
                return;
            }
            if (AssetBundleManager.Instance.UseJGGame)
            {
                var pkgAssetPath = string.Format("{0}/{1}", bundleManager.pathResolver.PkgBundleCacheDir, bundleName);
                if (File.Exists(pkgAssetPath))
                {
                    bundleManager.StartCoroutine(LoadFromCachedFile(pkgAssetPath));
                    return;
                }
                bundleManager.StartCoroutine(LoadFromPkg(_assetBundleCachedFile));
                return;
            }
            bundleManager.StartCoroutine(LoadFromPackage());
        }

        /// <summary>
        /// 从已缓存的文件里加载
        /// </summary>
        /// <returns></returns>
        protected AssetBundleRequest request = null;
        protected virtual IEnumerator LoadFromCachedFile(string assetPath)
        {
            if (state != LoadState.State_Error)
            {
                if(bundleManager.LoadDelay >0)
                {
                    yield return new WaitForSeconds(bundleManager.LoadDelay/1000f);
                }
                AssetBundleCreateRequest req = null;
                if (EnvUtils.IsEncryAB())
                {
                    var data = File.ReadAllBytes(assetPath);
                    EncryUtil.DecryBytes(data);
                    req = AssetBundle.LoadFromMemoryAsync(data);
                }
                else
                {
                    req = AssetBundle.LoadFromFileAsync(assetPath);
                }
                yield return req;
                if (req!= null)
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
                AssetBundleCreateRequest req = null;
                
                if (EnvUtils.IsUnity_Editor())
                {
                    var data = File.ReadAllBytes(_assetBundleSourceFile);
                    if (EnvUtils.IsEncryAB())
                    {
                        EncryUtil.DecryBytes(data);
                    }
                    req = AssetBundle.LoadFromMemoryAsync(data);
                }
                else
                {
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
                }
                yield return req;
                //Debug.Log("LoadFromPackage   "  + bundleData.debugName);
                if (req!=null)
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
        protected IEnumerator LoadFromPkg(string assetPath)
        {
            if (state != LoadState.State_Error)
            {
                if (bundleManager.LoadDelay > 0)
                {
                    yield return new WaitForSeconds(bundleManager.LoadDelay / 1000f);
                }
                JGGame.YGame.Inst.RuntimeDL(bundleName, OnDlFromPkgFinish);
            }
        }
        protected void OnDlFromPkgFinish(bool IsSuccess)
        {
            if (IsSuccess)
            {
                this.LoadBundle();
            }
            else
            {
                Error();
            }
        }
        void OnDepComplete(AssetBundleInfo abi)
        {
            if (bundleName == CDebugABName)
            {
               DebugMsg($"complete depLoaders : {abi.bundleName}");
            }
            _currentLoadingDepCount--;
            this.CheckDepComplete();
        }

        /// <summary>
        /// 依赖项结束，加载自身
        /// </summary>
        void CheckDepComplete()
        {
            if (bundleName == CDebugABName)
            {
                DebugMsg($"CheckDepCompletebundle: {bundleName},_currentLoadingDepCount: {_currentLoadingDepCount} ");
            }
            
            if (_currentLoadingDepCount == 0)
            {
                if (bundleName == CDebugABName)
                {
                    DebugMsg($"bundleManager.Enqueue: {bundleName}");
                }
                if (bundleName == MainABName)
                {
                    DebugMsg($"bundleManager.Enqueue: {bundleName} {this.bundleData.debugName}");
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
                }

                _bundle = null;
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
            if (EnvUtils.IsDEVELOPMENT_BUILD())
            {
                Debug.LogError($"{bundleName}, not found");
            }
        }
    }
}
