#if !AB_MODE && UNITY_EDITOR
#else
#define _AB_MODE_
#endif

using JGGame;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameFramework
{
#if USE_HOTFIX
    //[Beebyte.Obfuscator.Skip]
#endif
    public sealed class ABCacheTIme
    {
        public const int CACHETIME_10 = 10;
        public const int CACHETIME_30 = 10;
        public const int CACHETIME_60 = 10;
        public const int CACHETIME_120 = 10;
        public const int CACHETIME_240 = 10;
        public const int CACHETIME_360 = 10;
        public const int CACHETIME_PERMANENT = 360000;
        /// <summary>
        /// 驻留内存
        /// </summary>
        public const int CACHETIME_1 = 360000;
    }
    public enum LoadState
    {
        State_None = 0,
        State_Loading = 1,
        State_Error = 2,
        State_Complete = 3
    }
    public class AssetBundleManager : MonoBehaviour
    {
        /// <summary>
        /// 每次卸载ab最大个数
        /// </summary>
        int unloadLimit = 10;
        private bool _InChangeScene = false;
        /// <summary>
        /// 是否在切换场景
        /// </summary>
        public bool ForceUnloadUnUseAB { 
            get {
                return _InChangeScene;
            }
            set {
                this._InChangeScene = value;
                if (this._InChangeScene)
                {
                    LoadSceneTime = Time.time;
                }
            } 
        }

        public float LoadSceneTime { get; private set; }
        public static Version version = new Version(0, 1, 0);
        public static AssetBundleManager Instance;
        public static string NAME = "AssetBundleManager";
        public static bool enableLog = true;

#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public delegate void LoadAssetCompleteHandler(AssetBundleInfo info);
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public delegate void LoaderCompleteHandler(AssetBundleLoader info);
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public delegate void LoadProgressHandler(AssetBundleLoadProgress progress);
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public delegate UnityEngine.Object EditorNoneABLoadler(string path, System.Type type);

        public EditorNoneABLoadler EditorNoneABLoadFun = null;

        /// <summary>
        /// 同时最大的加载数
        /// </summary>
        public static int MAX_REQUEST = 100;
        /// <summary>
        /// 可再次申请的加载数
        /// </summary>
        public int _requestRemain = MAX_REQUEST;
        public int CountOfRequest {
            get
            {
                return MAX_REQUEST - _requestRemain;
            }
        }
        /// <summary>
        /// 当前申请要加载的队列
        /// </summary>
        private List<AssetBundleLoader> _requestQueue = new List<AssetBundleLoader>();

        /// <summary>
        /// 加载队列
        /// </summary>
        private List<AssetBundleLoader> _currentLoadQueue = new List<AssetBundleLoader>();
        /// <summary>
        /// 未完成的
        /// </summary>
        private HashSet<AssetBundleLoader> _nonCompleteLoaderSet = new HashSet<AssetBundleLoader>();
        /// <summary>
        /// 此时加载的所有Loader记录，(用于在全加载完成之后设置 minLifeTime)
        /// </summary>
        private HashSet<AssetBundleLoader> _thisTimeLoaderSet = new HashSet<AssetBundleLoader>();
        /// <summary>
        /// 已加载完成的缓存列表
        /// </summary>
        private Dictionary<string, AssetBundleInfo> _loadedAssetBundle = new Dictionary<string, AssetBundleInfo>();
        /// <summary>
        /// 已创建的所有Loader列表(包括加载完成和未完成的)
        /// </summary>
        private Dictionary<string, AssetBundleLoader> _loaderCache = new Dictionary<string, AssetBundleLoader>();
        public string GetDebugMsg()
        {
            return $"{_loadedAssetBundle.Count}-{_loaderCache.Count}-{mCacheAssetTsk.Count}";
        }
        public void PrintBundleAndCacheLoader()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("######内存中的ab");
            var iter = _loadedAssetBundle.GetEnumerator();
            List<AssetBundleInfo> lst = new List<AssetBundleInfo>(_loadedAssetBundle.Values);
            lst.Sort((a, b) => {
                int numa = a.refCount/* + 10000 * (a.minLifeTime < 0?1:-1)*/;
                int numb = b.refCount/* + 10000 * (b.minLifeTime < 0?1:-1)*/;
                return numb - numa;
            });
            foreach(var bundleInfo in lst)
            {
                builder.AppendLine($"{bundleInfo.bundleName}\t\t {bundleInfo.refCount}\t{bundleInfo.isUnused}\t{bundleInfo.minLifeTime}\t{bundleInfo.leftTime}");
            }
            builder.AppendLine();
            builder.AppendLine();
            builder.Append("######未完成laoder");
            foreach (var key in _loaderCache.Keys)
            {
                if (_loadedAssetBundle.ContainsKey(key))
                    continue;
                builder.AppendLine(key);
            }
            Debug.Log(builder.ToString()); ;
        }
        /// <summary>
        /// 当前是否还在加载，如果加载，则暂时不回收
        /// </summary>
        private bool _isCurrentLoading;

        private AssetBundleLoadProgress _progress = new AssetBundleLoadProgress();
        /// <summary>
        /// 进度
        /// </summary>
        public LoadProgressHandler onProgress;

        public AssetBundlePathHelper pathResolver;

        private AssetBundleDataReader _depInfoReader;

        private Action _initCallback;

        private Dictionary<GameObject, AssetBundleLoader> m_dicInstIdToABInfo = new Dictionary<GameObject, AssetBundleLoader>();
        private Dictionary<GameObject, AssetBundleLoader> m_dicInstIdToABInfoSwap = new Dictionary<GameObject, AssetBundleLoader>();


        private List<LoadTask> m_lstLoadTask = new List<LoadTask>();
        private Dictionary<AssetBundleLoader, LoadTask> mCacheAssetTsk = new Dictionary<AssetBundleLoader, LoadTask>();
        public string BundleDataPath = "BundleData";
        public bool IsABMod = true;
        public bool IsABFromCDN = false;
        public bool UseJGGame = false;
        private Dictionary<GameObject, List<AssetBundleLoader>> m_dicOwner = new Dictionary<GameObject, List<AssetBundleLoader>>();
        private Dictionary<GameObject, List<AssetBundleLoader>> m_dicOwnerSwap = new Dictionary<GameObject, List<AssetBundleLoader>>();
        //private Dictionary<UnityEngine.Object, AssetBundleLoader> m_dicRefs = new Dictionary<UnityEngine.Object, AssetBundleLoader>();
        public int LoadDelay = 0;
        private float m_LastCheckTime = 0;
        public Action<AssetBundleData> OnCollectBundleInfoHandler;
        public AssetBundleManager()
        {
            Instance = this;
            pathResolver = new AssetBundlePathHelper();
        }

        public AssetBundleDataReader depInfoReader { get { return _depInfoReader; } }

        public void Awake()
        {
			
        }

        public void Update()
        {
            if (_isCurrentLoading)
            {
                CheckNewLoaders();
                CheckQueue();
            }
            else
            { 
                UpdateIdleTsk();
            }
        }
        public void LateUpdate()
        {
            if (Time.time - m_LastCheckTime > 1)
            {
                CheckUnusedBundle(ForceUnloadUnUseAB);
                m_LastCheckTime = Time.time;
            }
        }

        public void Init(Action callback)
        {
            _initCallback = callback;
            if (UseJGGame)
            {
                this.StartCoroutine(LoadDepInfoJGGame());
                return;
            }
            this.StartCoroutine(LoadDepInfo());

        }

        public void Init(Stream depStream, Action callback)
        {
            if (depStream.Length > 4)
            {
                BinaryReader br = new BinaryReader(depStream);
                if (br.ReadChar() == 'A' && br.ReadChar() == 'B' && br.ReadChar() == 'D')
                {
                    if (br.ReadChar() == 'T')
                        _depInfoReader = new AssetBundleDataReader();
                    else
                        _depInfoReader = new AssetBundleDataBinaryReader();

                    depStream.Position = 0;
                    _depInfoReader.Read(depStream);
                }
            }

            depStream.Close();

            if (callback != null)
                callback();
        }

        void InitComplete()
        {
            if (_initCallback != null)
                _initCallback();
            _initCallback = null;
        }

        public IEnumerator LoadDepInfo()
        {
            string depFile = string.Format("{0}/{1}", pathResolver.BundleCacheDir, pathResolver.DependFileName);

            if (File.Exists(depFile))
            {
                FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                Init(fs, null);
                fs.Close();
            }
            else
            {
                if (EnvUtils.IsUnity_Editor())
                {
                    depFile = string.Format("{0}/{1}", EnvUtils.IsEncryAB()?pathResolver.BundleSavePathEncry:pathResolver.BundleSavePath, pathResolver.DependFileName);
                    if (File.Exists(depFile))
                    {
                        FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                        Init(fs, null);
                        fs.Close();
                    }
                    else
                    {
                        Debug.LogError(string.Format("#### {0} not exist!", depFile));
                    }
                }
                else
                {
                    string srcURL = pathResolver.GetBundleSourceFile(pathResolver.DependFileName);
                    WWW w = new WWW(srcURL);
                    yield return w;

                    if (w.error == null)
                    {
                        Init(new MemoryStream(w.bytes), null);
                    }
                    else
                    {
                        Debug.LogError(string.Format("#### {0} not exist!", srcURL));
                    }
                }
            }
            this.InitComplete();
        }
        public IEnumerator LoadDepInfoJGGame()
        {
            string depFile = string.Format("{0}/{1}", pathResolver.BundleCacheDir, pathResolver.DependFileName);

            if (File.Exists(depFile))
            {
                FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                Init(fs, null);
                fs.Close();
                this.InitComplete();
                yield break;
            }
            depFile = string.Format("{0}/{1}", pathResolver.PkgBundleCacheDir, pathResolver.DependFileName);
            if (File.Exists(depFile))
            {
                FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                Init(fs, null);
                fs.Close();
                this.InitComplete();
                yield break;
            }
            if (UseJGGame)
            {
                JGGame.YGame.Inst.RuntimeDL("dep.all", (result) =>
                {
                    if (result)
                    {
                        FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                        Init(fs, null);
                        fs.Close();
                        this.InitComplete();
                    }
                    else
                    {
                        Debug.LogError($"dep.all,从Pkg中获取失败");
                    }
                });
                yield break;
            }
            
            if (EnvUtils.IsUnity_Editor())
            {
                depFile = string.Format("{0}/{1}", EnvUtils.IsEncryAB() ? pathResolver.BundleSavePathEncry : pathResolver.BundleSavePath, pathResolver.DependFileName);
                if (File.Exists(depFile))
                {
                    FileStream fs = new FileStream(depFile, FileMode.Open, FileAccess.Read);
                    Init(fs, null);
                    fs.Close();
                }
                else
                {
                    Debug.LogError(string.Format("#### {0} not exist!", depFile));
                }
                this.InitComplete();
                yield break;
            }

            string srcURL = pathResolver.GetBundleSourceFile(pathResolver.DependFileName);
            WWW w = new WWW(srcURL);
            yield return w;

            if (w.error == null)
            {
                Init(new MemoryStream(w.bytes), null);
            }
            else
            {
                Debug.LogError(string.Format("#### {0} not exist!", srcURL));
            }
                this.InitComplete();
        }
        public void LoadDepInfo(byte[] data, Action OnCallBack)
        {
            Init(new MemoryStream(data), OnCallBack);
        }
        public void LoadDepInfo(string filePath, Action OnCallBack)
        {
            Init(File.OpenRead(filePath), OnCallBack);
        }
        public void LoadDepInfo(Stream stream, Action OnCallBack)
        {
            Init(stream, OnCallBack);
        }
        public void OnDestroy()
        {
            this.RemoveAll();
        }
        private Dictionary<string,string> packAssetDict = new Dictionary<string, string>();
        /// <summary>
        /// 加载包内资源清单
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadClientRes()
        {
            string srcURL = pathResolver.GetBundleSourceFile(pathResolver.ClientResName);
            UnityEngine.Networking.UnityWebRequest w = UnityEngine.Networking.UnityWebRequest.Get(srcURL);
            yield return w.SendWebRequest();
            try
            {
                if (string.IsNullOrEmpty(w.error))
                {
                    using (MemoryStream ms = new MemoryStream(w.downloadHandler.data))
                    {
                        using (StreamReader reader = new StreamReader(ms))
                        {
                            var content = reader.ReadToEnd();
                            BaseFileInfo[] ress = JsonMapper.ToObject<BaseFileInfo[]>(content);
                            if (null != ress && ress.Length > 0)
                            {
                                foreach (var res in ress)
                                {
                                    packAssetDict.Add(res.fn, res.md);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError(string.Format("#### {0} not exist!", srcURL));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadClientRes Exception {e.ToString()}");
            }
            finally
            {
                Debug.LogError($"### clientres:{packAssetDict.Count}");
            }
        }
        public bool IsPackAsset(string bundleName)
        {
            return packAssetDict.ContainsKey(bundleName);
        }
        public bool IsPackAsset(string bundleName, string md)
        {
            if (!packAssetDict.ContainsKey(bundleName))
                return false;
            return packAssetDict[bundleName] == md;
        }
        /// <summary>
        /// 通过ShortName获取FullName
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <returns></returns>
        public string GetAssetBundleFullName(string shortFileName)
        {
            return _depInfoReader.GetFullName(shortFileName);
        }

        /// <summary>
        /// 通过一个路径加载ab
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="prority">优先级</param>
        /// <param name="handler">回调</param>
        /// <returns></returns>
        private AssetBundleLoader Load(string path, int prority, bool bNeedPreload, LoadAssetCompleteHandler handler = null,int nMinLifeTime = ABCacheTIme.CACHETIME_10)
        {
            AssetBundleLoader loader = null;
            if (IsABMod)
            {
                loader = this.CreateLoader(HashUtil.Get(HashUtil.ConvertToABName(path)) + ".ab", path, bNeedPreload);
            }
            else
            {
                loader = this.CreateLoader(path,null, bNeedPreload);
            }            
                        
            loader.prority = prority;
            loader.onComplete += handler;
            loader.MinLifeTime = nMinLifeTime;

            _isCurrentLoading = true;
            _nonCompleteLoaderSet.Add(loader);
            _thisTimeLoaderSet.Add(loader);

            return loader;
        }
        internal AssetBundleLoader CreateLoader(string abFileName, string oriName = null,bool bNeedPreload = true)
        {
            AssetBundleLoader loader = null;
            string bundleName = abFileName;
            #region 获取ab名称
            AssetBundleData data = null;
            if (null != _depInfoReader)
            {
                data = _depInfoReader.GetAssetBundleInfo(abFileName);
                if (data == null && oriName != null)
                {
                    data = _depInfoReader.GetAssetBundleInfoByShortName(oriName.ToLower());
                }
            }
            if (IsABFromCDN)
            {
                bundleName = (data == null ? abFileName : data.fullNameWithHash);
            }
            else
            {
                bundleName = (data == null ? abFileName : data.fullName);
            }
            #endregion

            if (_loaderCache.ContainsKey(bundleName))
            {
                loader = _loaderCache[bundleName];
            }
            else
            {
                if (null != _depInfoReader)
                {
                    if (data == null && IsABMod)
                    {
                        if (EnvUtils.IsUnity_Editor() || EnvUtils.IsDEVELOPMENT_BUILD())
                        {
                            Debug.LogErrorFormat("CreateLoader :File {0} not exist!!!", oriName == null ? abFileName : oriName + "  " + abFileName);
                        }
                        MissAssetBundleLoader missLoader = new MissAssetBundleLoader();
                        missLoader.bundleManager = this;
                        return missLoader;
                    }
                }
                loader = this.CreateLoader();
                loader.bundleManager = this;
                loader.bundleData = data;
                loader.bundleName = bundleName;
                loader.md = null != data? data.crc:"111111";
                _loaderCache[loader.bundleName] = loader;
            }

            loader.NeedPreload = bNeedPreload;
            return loader;
        }

        protected virtual AssetBundleLoader CreateLoader()
        {
            if(!IsABMod)
            {
                if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    return new EditorNoneABLoader();
                }
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                return new MobileAssetBundleLoader();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return new IOSAssetBundleLoader();
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return new WebGLAssetBundleLoader();
            }
            else
            {
                return new MobileAssetBundleLoader();
            }
        }

        public void RemoveLoader(AssetBundleLoader abLoader,bool bForce = false)
        {
            if (abLoader.bundleInfo != null && !abLoader.bundleInfo.isUnused && !bForce)
            {
                //Debug.LogError("RemoveLoader  bundlename ="+abLoader.bundleData == null?abLoader.bundleName:abLoader.bundleData.debugName);
                return;
            }
            if(abLoader is MissAssetBundleLoader || abLoader.bundleName == null)
            {
                return;
            }
            if(abLoader.isComplete)
            {
                if (_loaderCache.ContainsKey(abLoader.bundleName))
                {
                    _loaderCache.Remove(abLoader.bundleName);
                }
                RemoveBundleInfo(abLoader.bundleInfo);
            }
            else
            {
                //Debug.LogError("RemoveLoader  loader has not complete!!!" + abLoader.bundleData == null ? abLoader.bundleName : abLoader.bundleData.debugName);
            }
        }
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        void CheckNewLoaders()
        {
            if (_nonCompleteLoaderSet.Count > 0)
            {
                List<AssetBundleLoader> loaders = ListPool<AssetBundleLoader>.Get();
                loaders.AddRange(_nonCompleteLoaderSet);
                _nonCompleteLoaderSet.Clear();

                var e = loaders.GetEnumerator();
                while (e.MoveNext())
                {                    
                    if(!_currentLoadQueue.Contains(e.Current))
                    {
                        _currentLoadQueue.Add(e.Current);
                        //Debug.LogErrorFormat("_currentLoadQueue add bundle: {0}", e.Current.bundleName);
                    }
                }
                e.Dispose();
                _progress = new AssetBundleLoadProgress();
                _progress.total = _currentLoadQueue.Count;

                e = loaders.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.Start();
                }
                e.Dispose();
                ListPool<AssetBundleLoader>.Release(loaders);
            }
        }
        
        public void RemoveAll()
        {
            this.StopAllCoroutines();

            _currentLoadQueue.Clear();
            _requestQueue.Clear();
            foreach (AssetBundleInfo abi in _loadedAssetBundle.Values)
            {
                abi.Dispose();
            }
            _loadedAssetBundle.Clear();
            _loaderCache.Clear();
        }

        public AssetBundleInfo GetBundleInfo(string key)
        {
            if(IsABMod)
            {
                key = HashUtil.Get(key) + ".ab";
            }
            else
            {
                key = key.ToLower();
            }         

            

            var e = _loadedAssetBundle.GetEnumerator();
            while (e.MoveNext())
            {
                AssetBundleInfo abi = e.Current.Value;
                if (abi.bundleName == key)
                    return abi;
            }
            e.Dispose();
            return null;
        }

        /// <summary>
        /// 请求加载Bundle，这里统一分配加载时机，防止加载太卡
        /// </summary>
        /// <param name="loader"></param>
        internal void Enqueue(AssetBundleLoader loader)
        {
            if (_requestRemain < 0)
                _requestRemain = 0;
            _requestQueue.Add(loader);
        }
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        void CheckQueue()
        {
            //if (_requestRemain > 0 && _requestQueue.Count > 0)
            //    _requestQueue.Sort();

            while (_requestRemain > 0 && _requestQueue.Count > 0)
            {
                AssetBundleLoader loader = _requestQueue[0];
                _requestQueue.RemoveAt(0);
                LoadBundle(loader);
            }
        }

        void LoadBundle(AssetBundleLoader loader)
        {
            if (!loader.isComplete)
            {
                loader.LoadBundle();
                _requestRemain--;
                if (_requestRemain < 0)
                {
                    _requestRemain = 0;
                }
            }
        }

        internal void LoadError(AssetBundleLoader loader)
        {
            //Debug.LogWarning("Cant load AB : " + loader.bundleName, this);
            LoadComplete(loader);
        }

        internal void LoadComplete(AssetBundleLoader loader)
        {
            _requestRemain++;
            if (_requestRemain > MAX_REQUEST)
            {
                _requestRemain = MAX_REQUEST;
            }
            _currentLoadQueue.Remove(loader);
            //Debug.LogErrorFormat("_currentLoadQueue Remove bundle: {0}", loader.bundleName);
            if (onProgress != null)
            {
                _progress.loader = loader;
                _progress.complete = _progress.total - _currentLoadQueue.Count;
                onProgress(_progress);
            }
            //Debug.LogErrorFormat("_currentLoadQueue length: {0}, _nonCompleteLoaderSet length: {1}", _currentLoadQueue.Count,_nonCompleteLoaderSet.Count);
            if (_currentLoadQueue.Count > 0)
            {
                //Debug.LogErrorFormat("next loader in queue: {0}", _currentLoadQueue[0].bundleName);
            }

            if (null != loader.bundleInfo)
            {
                loader.bundleInfo.ResetLifeTime();
                _thisTimeLoaderSet.Remove(loader);
            }
            //all complete
            if (_currentLoadQueue.Count == 0 && _nonCompleteLoaderSet.Count == 0)
            {
                _isCurrentLoading = false;

                var e = _thisTimeLoaderSet.GetEnumerator();
                //while (e.MoveNext())
                //{
                //    AssetBundleLoader cur = e.Current;
                //    if (cur.bundleInfo != null)
                //        cur.bundleInfo.ResetLifeTime();
                //}
                _thisTimeLoaderSet.Clear();
                e.Dispose();
            }
        }

        internal AssetBundleInfo CreateBundleInfo(AssetBundleLoader loader, bool bUnloadComplete = false, AssetBundleInfo abi = null,AssetBundle assetBundle = null)
        {
            if (abi == null)
                abi = new AssetBundleInfo(bUnloadComplete);
            abi.bundleName = loader.bundleName;
            abi.bundle = assetBundle;
            abi.data = loader.bundleData;

            _loadedAssetBundle[abi.bundleName] = abi;
            return abi;
        }

        internal void RemoveBundleInfo(AssetBundleInfo abi)
        {
            if(abi!=null)
            {
                abi.Dispose();
                _loadedAssetBundle.Remove(abi.bundleName);

            }
        }

        /// <summary>
        /// 从pack中获取资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T GetAsset<T>(string path, string assetName) where T : UnityEngine.Object
        {
            if (!IsABMod)
                return default(T);
            string abName = HashUtil.Get(HashUtil.ConvertToABName($"Assets/BundleData/{path}")) + ".ab";
            var info = _depInfoReader.GetAssetBundleInfo(abName);
            //Debug.Log($"===========GetAsset {info.fullNameWithHash} {_loadedAssetBundle.ContainsKey(info.fullNameWithHash)}");
            if(null != info &&  _loadedAssetBundle.ContainsKey(info.fullNameWithHash))
            {
                var ab = _loadedAssetBundle[info.fullNameWithHash];
                if (typeof(T).IsAssignableFrom(typeof(GameObject)))
                {
                    var asset = ab.bundle.LoadAsset<T>(assetName);
                    if (null != asset)
                    {
                        return GameObject.Instantiate<T>(ab.bundle.LoadAsset<T>(assetName));
                    }
                    else
                    {
                        //if (EnvUtils.IsUnity_Editor())
                        //{
                        //    Debug.LogError($"<color=blue>{assetName} Is Not Fond In {abName}</color>");
                        //}
                        return null;
                    }
                }
                return ab.bundle.LoadAsset<T>(assetName);
            }
            return default(T);
        }
        /// <summary>
        /// 当前是否在加载状态
        /// </summary>
        public bool isCurrentLoading { get { return _isCurrentLoading; } }

        public void CheckUnusedBundle(bool bForce = false)
		{            
            RecycleInvalidInstance(bForce);
            RecycleInvalidOwner(bForce);
            this.UnloadUnusedBundle(bForce);
		}
        void RecycleInvalidInstance(bool bForce = false)
        {
            if(!_isCurrentLoading||bForce)
            {
                var enu = m_dicInstIdToABInfo.GetEnumerator();
                List<GameObject> lstGO = new List<GameObject>();
                int nCount = 0;
                int nBefore = m_dicInstIdToABInfo.Count;
                Dictionary<GameObject, AssetBundleLoader> tmpDic = m_dicInstIdToABInfoSwap;
                while(enu.MoveNext())
                {
                    if(enu.Current.Key == null || enu.Current.Key.Equals(null) )
                    {                        
                        lstGO.Add(enu.Current.Key);
                        nCount++;
                        enu.Current.Value.Release();                                                                    
                    }
                    else
                    {
                        tmpDic.Add(enu.Current.Key,enu.Current.Value);
                    }
                }
                enu.Dispose();
                m_dicInstIdToABInfo.Clear();
                m_dicInstIdToABInfoSwap = m_dicInstIdToABInfo;
                m_dicInstIdToABInfo = tmpDic;

                if(nCount>0)
                {
                    //Debug.LogFormat("RecycleInvalidInstance   {0}   before ={1}  now={2}",nCount,nBefore,m_dicInstIdToABInfo.Count);
                }
                lstGO.Clear();                
            }
        }
        void RecycleInvalidOwner(bool bForce = false)
        {
            if(!_isCurrentLoading || bForce)
            {
                var enu = m_dicOwner.GetEnumerator();
                int nCount = 0;
                Dictionary<GameObject, List<AssetBundleLoader>> tmpDic = m_dicOwnerSwap;
                while(enu.MoveNext())
                {
                    if (enu.Current.Key == null || enu.Current.Key.Equals(null))
                    {
                        nCount++;
                        for(int i =0; i < enu.Current.Value.Count; i++)
                        {
                            enu.Current.Value[i].Release();
                        }
                        enu.Current.Value.Clear();
                    }
                    else
                    {
                        tmpDic.Add(enu.Current.Key,enu.Current.Value);
                    }
                }
                enu.Dispose();
                m_dicOwner.Clear();
                m_dicOwnerSwap = m_dicOwner;
                m_dicOwner = tmpDic;

                if(nCount>0)
                {
                    //Debug.LogFormat("RecycleInvalidOwner   {0}",nCount);
                }
            }
        }
        /// <summary>
        /// 卸载不用的
        /// </summary>
        public void UnloadUnusedBundle(bool force = false)
        {
            if (_isCurrentLoading == false || force)
            {
                //一次最多卸载的个数，防止卸载过多太卡
                int unloadCount = 0;

                List<AssetBundleLoader> lstRemove = new List<AssetBundleLoader>();
                var enu = _loaderCache.GetEnumerator();
                while (enu.MoveNext() && (!force &&unloadCount < unloadLimit || force))
                {
                    if (enu.Current.Value.state == LoadState.State_Complete
                            && enu.Current.Value.CanRelease()
                        )
                    {
                        AssetBundleInfo abi = enu.Current.Value.bundleInfo;
                        if (abi != null && abi.isUnused)
                        {
                            unloadCount++;
                            lstRemove.Add(enu.Current.Value);
                        }
                    }
                }
                enu.Dispose();
                for (int i = 0; i < lstRemove.Count; i++)
                {
                    if (lstRemove[i].bundleName == MobileAssetBundleLoader.CDebugABName)
                    {
                        MobileAssetBundleLoader.DebugMsg($"UnloadUnusedBundle: {lstRemove[i].bundleInfo.refCount} {lstRemove[i].bundleData.debugName}");
                    }
                    RemoveLoader(lstRemove[i]);
                }
                lstRemove.Clear();
                              

                if(Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (unloadCount > 0 && enableLog)
                    {
                        //Debug.Log("===>> Unload Count: " + unloadCount + "  LeftCount " + _loaderCache.Count);
                    }
                }
            }
        }
        Dictionary<AssetBundleLoader, int> refDict = new Dictionary<AssetBundleLoader, int>();
        List<AssetBundleLoader> lstRmove = new List<AssetBundleLoader>();
        /// <summary>
        /// 卸载未被引用的AssetLoader
        /// </summary>
        public void ManualUnloadTask(bool bForce =  false)
        {
            //refDict.Clear();
            //lstRmove.Clear();
            ////CheckUnusedBundle(bForce);
            //using (var iter = mCacheAssetTsk.GetEnumerator())
            //{
            //    foreach (var kv in mCacheAssetTsk)
            //    {
            //        if (!_loaderCache.ContainsKey(kv.Key.bundleName) || null == kv.Key.bundleInfo)
            //        {
            //            lstRmove.Add(kv.Key);
            //        }
            //    }
            //    foreach (var tLoader in lstRmove)
            //    {
            //        mCacheAssetTsk.Remove(tLoader);
            //    }
            //    lstRmove.Clear();
            //}
            ////筛选出被引用的Asset Loader，并从assetLoaderLst中删除
            //{
            //    if (!bForce)
            //    {
            //        foreach (var kv in mCacheAssetTsk)
            //        {
            //            foreach (var v in _loaderCache.Values)
            //            {
            //                if (v.ContainChild(kv.Key) && v != kv.Key)
            //                {
            //                    if (!refDict.ContainsKey(kv.Key))
            //                    {
            //                        refDict[kv.Key] = 0;
            //                    }
            //                    refDict[kv.Key] += 1;
            //                }
            //            }
            //        }
            //    }
            //    foreach (var kv in mCacheAssetTsk)
            //    {
            //        var bundleInfo = kv.Key.bundleInfo;
            //        if (refDict.ContainsKey(kv.Key) || null == bundleInfo)
            //            continue;
            //        var activeTime = Time.time - bundleInfo.GetReadyTime();
            //        if (bundleInfo.minLifeTime >=0 && bundleInfo.isUnused
            //            //&& !bForce && activeTime > bundleInfo.minLifeTime * 2 || bForce && activeTime > bundleInfo.minLifeTime
            //        )
            //        {
            //            lstRmove.Add(kv.Key);
            //        }
            //    }
            //    foreach (var tLoader in lstRmove)
            //    {
            //        tLoader.Release();
            //        if (null != tLoader.bundleInfo && tLoader.bundleInfo.refCount <= 0)
            //        {
            //            var task = mCacheAssetTsk[tLoader];
            //            task.AfterExecute();
            //            mCacheAssetTsk.Remove(tLoader);
            //        }
            //    }
            //    if (EnvUtils.IsUnity_Editor() && EnvUtils.IsEDITOR_CDN_ABMOD() && lstRmove.Count > 0)
            //    {
            //        Debug.Log($"<color=blue>### ManualUnloadTask:{lstRmove.Count}-{mCacheAssetTsk.Count}</color>");
            //    }
            //}
        }
        public GameObject InstantiateAsset(AssetBundleLoader abLoader)
        {
            if(abLoader == null)
            {
                //Debug.Log("abLoaderabLoaderabLoaderabLoader");
            }
            if (abLoader.bundleInfo == null)
            {
                //Debug.Log("abLoader.bundleInfoabLoader.bundleInfoabLoader.bundleInfoabLoader.bundleInfo");
            }
            
            GameObject vObj = abLoader.bundleInfo.Instantiate();
            if(vObj != null)
            {
                m_dicInstIdToABInfo.Add(vObj, abLoader);
            }
            
            return vObj;
        }

        public void Recycle(GameObject vInstance)
        {
            if(vInstance == null)
            {
                return;
            }
            AssetBundleLoader vABLoader = null;
            if(m_dicInstIdToABInfo.TryGetValue(vInstance,out vABLoader))
            {                
                vABLoader.Release();                
                m_dicInstIdToABInfo.Remove(vInstance);
            }

            Destroy(vInstance);
            
        }

        public void AddLoadTask(LoadTask vTask)
        {
            if(string.IsNullOrEmpty(vTask.Path))
            {
                //Debug.LogError("vTask.Path  is null");
                return;
            }

            if (IsABMod)
            {
                if (vTask.GetTaskType() == LoadTask.TaskType.TaskType_Scene)
                {
                    vTask.Path = "Assets" + vTask.Path;
                }
                else if (vTask.GetTaskType() == LoadTask.TaskType.TaskType_Texture)
                {
                    vTask.Path = string.Format("Assets/BundleData/{0}.png", vTask.Path);
                }
                else
                {
                    vTask.Path = "Assets/BundleData/" + vTask.Path;
                }
            }

            vTask.ABLoader = Load(vTask.Path, vTask.Prority, vTask.NeedPreLoad, ProcessTask,vTask.LifeTime);
            if (vTask.GetTaskType() == LoadTask.TaskType.TaskType_Scene)
            {
                //Debug.LogErrorFormat("add task {0},{1}, {2}", vTask.ToString(), vTask.Path,vTask.ABLoader.bundleName);
            }
            vTask.AddOwer();
            m_lstLoadTask.Add(vTask);
            if (null != OnCollectBundleInfoHandler)
            {
                OnCollectBundleInfoHandler(vTask.ABLoader.bundleData);
            }
        }

        private void ProcessTask(AssetBundleInfo vABInfo = null)
        {
            int i = 0;
            while(i<m_lstLoadTask.Count)
            {
                var task = m_lstLoadTask[i];
                if (task.GetTaskType() == LoadTask.TaskType.TaskType_Scene)
                {
                   // Debug.LogErrorFormat("ProcessTask begin {0},{1}", task.ToString(), task.Path);
                }
                if (task.IsPrepared)
                {
                    if (task.NeedExecute())
                    {
                        task.BeforeExecute(this);
                        try
                        {
                            task.Execute();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.ToString());
                        }
                        //if (task.GetTaskType() == LoadTask.TaskType.TaskType_Animation
                        //   || task.GetTaskType() == LoadTask.TaskType.TaskType_Texture
                        //   //|| task.GetTaskType() == LoadTask.TaskType.TaskType_Lightmap
                        //   || task.GetTaskType() == LoadTask.TaskType.TaskType_Sound
                        //   || task.GetTaskType() == LoadTask.TaskType.TaskType_Asset
                        //)
                        //{
                        //    if (null != task.ABLoader && null != task.ABLoader.bundleInfo && !mCacheAssetTsk.ContainsKey(task.ABLoader))
                        //    {
                        //        mCacheAssetTsk.Add(task.ABLoader, task);
                        //    }
                        //}
                        //else
                        {
                            task.AfterExecute();
                        }
                    }
                    m_lstLoadTask.RemoveAt(i);

                   
                }
                else if (task.IsFailed)
                {
                    //Debug.LogErrorFormat("ProcessTask Failed {0},{1}", task.ToString(), task.Path);

                    RemoveLoader(task.ABLoader);
                    m_lstLoadTask.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        internal void AddOwer(GameObject owner,AssetBundleLoader abLoader)
        {
            List<AssetBundleLoader> lst = null;
            if(!m_dicOwner.TryGetValue(owner,out lst))
            {
                lst = new List<AssetBundleLoader>();
                m_dicOwner.Add(owner, lst);
            }

            for(int i =0; i < lst.Count; i++)
            {
                if(lst[i] == abLoader)
                {
                    return;
                }
            }

            lst.Add(abLoader);
        }

        public List<AssetBundleLoader> GetReferenceList(GameObject owner)
        {
            List<AssetBundleLoader> lst = null;
            m_dicOwner.TryGetValue(owner,out lst);
            return lst;
        }

        public void OnOwerDestory(GameObject obj)
        {
            List<AssetBundleLoader> lst = null;
            if (m_dicOwner.TryGetValue(obj, out lst))
            {
                m_dicOwner.Remove(obj);
                for(int i = 0; i < lst.Count; i++)
                {
                    lst[i].Release();
                }
                lst.Clear();
            }
        }
        private Queue<LoadTask> mTaskQueue = new Queue<LoadTask>();
        /// <summary>
        /// 添加需要预加载的资源
        /// </summary>
        /// <param name="vTask"></param>
        public void PreLoadTask(LoadTask vTask)
        {
            mTaskQueue.Enqueue(vTask);
    }
        /// <summary>
        /// 空闲时预加载资源
        /// </summary>
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        private void UpdateIdleTsk()
        {
            if (mTaskQueue.Count <= 0)
                return;
            AddLoadTask(mTaskQueue.Dequeue());
        }
    }
}
