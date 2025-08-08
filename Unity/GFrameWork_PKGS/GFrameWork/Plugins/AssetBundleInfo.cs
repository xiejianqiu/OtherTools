using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework
{
#if USE_HOTFIX
    //[Beebyte.Obfuscator.Skip]
#endif
    public class AssetBundleInfo
    {
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename] 
#endif
        public delegate void OnUnloadedHandler(AssetBundleInfo abi);
        public OnUnloadedHandler onUnloaded;

        public AssetBundle bundle;

        public string bundleName;
        public AssetBundleData data;

        /// <summary>
        /// 如果没有其它东西引用的情况下，此AB最小生存时间（单位秒）
        /// 否则有可能刚加载完成就被释放了
        /// </summary>
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public float minLifeTime = ABCacheTIme.CACHETIME_10;

        /// <summary>
        /// 准备完毕时的时间
        /// </summary>
        private float _readyTime;
        public float GetReadyTime()
        {
            return _readyTime;
        }

        /// <summary>
        /// 标记当前是否准备完毕
        /// </summary>
        private bool _isReady;

        private Object _mainObject;

        private bool m_bUnloadComplete;
        /// <summary>
        /// 强制的引用计数
        /// </summary>
        [SerializeField]
        public int refCount { get; private set; }

        private HashSet<AssetBundleInfo> deps = new HashSet<AssetBundleInfo>();
        private List<string> depChildren = new List<string>();

        public AssetBundleInfo(bool bUnloadComplete)
        {
            m_bUnloadComplete = bUnloadComplete;
            refCount = 0;
        }

        public void AddDependency(AssetBundleInfo target)
        {
            if (target != null && deps.Add(target))
            {
                target.Retain();
                target.depChildren.Add(this.bundleName);
            }
        }

        public void ResetLifeTime(bool resttime = true)
        {
            if (_isReady && resttime || true)
            {
                _readyTime = Time.time;
            }
        }

        /// <summary>
        /// 引用计数增一
        /// </summary>
        public void Retain()
        {
            refCount++;
            //Debug.Log("Retain    "  + refCount + "   " + data.debugName);
        }

        /// <summary>
        /// 引用计数减一
        /// </summary>
        public void Release()
        {
            refCount--;
            if (refCount <= 0)
            {
                ResetLifeTime(false);
            }
            //Debug.Log("Retain    " + refCount + "   " + data.debugName);
        }

        /// <summary>
        /// 实例化对象
        /// </summary>
        /// <param name="user">增加引用的对象</param>
        /// <returns></returns>
        public virtual GameObject Instantiate()
        {
            return Instantiate(true);
        }

        public virtual GameObject Instantiate(bool enable)
        {
            if (mainObject != null)
            {
                //只有GameObject才可以Instantiate
                if (mainObject is GameObject)
                {
                    GameObject prefab = mainObject as GameObject;
                    //prefab.SetActive(enable);
                    Object inst = Object.Instantiate(prefab);
                    inst.name = prefab.name;
                    Retain();
                    return (GameObject)inst;
                }
                //Debug.LogErrorFormat("Instantiate  {0} is {1}",mainObject.name,mainObject.GetType());
            }
            //Debug.LogErrorFormat("mainObject is null  {0}", bundleName);
            return null;
        }
        
        /// <summary>
        /// 这个资源是否不用了
        /// </summary>
        /// <returns></returns>
        public bool isUnused
        {
            get {
                var ABMgr = AssetBundleManager.Instance;
                return _isReady && refCount <= 0 && 
                    ((minLifeTime >= 0 && Time.time - _readyTime >= minLifeTime)
                    //|| ABMgr.ForceUnloadUnUseAB & (ABMgr.LoadSceneTime - _readyTime > 1)
                ); 
            }
        }
        public float leftTime
        {
            get
            {
                return Time.time - _readyTime;
            }
        }
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public virtual void ResetChildLifeTimeAfterPreload(int leftTime)
        {
            var e = deps.GetEnumerator();
            while (e.MoveNext())
            {
                AssetBundleInfo dep = e.Current;
                if (dep != null)
                    dep.minLifeTime = leftTime;
            }
        }

        public virtual void Dispose()
        {
            UnloadBundle();

            var e = deps.GetEnumerator();
            while (e.MoveNext())
            {
                AssetBundleInfo dep = e.Current;
                dep.depChildren.Remove(this.bundleName);
                dep.Release();
            }
            //this.Release();
            e.Dispose();
            deps.Clear();
            if (onUnloaded != null)
                onUnloaded(this);
        }

        public bool isReady
        {
            get { return _isReady; }

            set { _isReady = value; }
        }
#if USE_HOTFIX
        //[Beebyte.Obfuscator.SkipRename]
#endif
        public virtual Object mainObject
        {
            get
            {
                if (_mainObject == null && _isReady && bundle != null)
                {
                    string[] names = bundle.GetAllAssetNames();
                    _mainObject = bundle.LoadAsset(names[0]);

                    //优化：如果是根，则可以 unload(false) 以节省内存
                    //if (data.compositeType == AssetBundleExportType.Root)
                    //    UnloadBundle();
                }
                return _mainObject;
            }
        }


        internal void SetMainObject(Object obj)
        {
            if(_mainObject == null)
            {
                _mainObject = obj;
            }
        }

        void UnloadBundle()
        {            
            if (bundle != null)
            {
                //Debug.Log("UnloadBundle    " + data.debugName);
                bundle.Unload(true);                                
            }
            bundle = null;
        }
    }
}