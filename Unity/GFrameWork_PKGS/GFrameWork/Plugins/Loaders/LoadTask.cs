using GameFramework;
using UnityEngine;
using System.Collections.Generic;
using System;
#if USE_HOTFIX
//[Beebyte.Obfuscator.SkipRename]
#endif
public abstract class LoadTask
{
    public enum TaskType
    {
        NONE,
        TaskType_Table,
        TaskType_Animation,
        TaskType_StoryAnimation,
        TaskType_Prefab,
        TaskType_UI,
        TaskType_Effect,
        TaskType_CharModle,
        TaskType_Sound,
        TaskType_Texture,
        TaskType_PreloadRs,
        TaskType_Scene,
        TaskType_Lightmap,
        /// <summary>
        /// 用于加载资源，加载完后回调
        /// </summary>
        TaskType_Asset,
    }
    public LoadTask(string ResPath,GameObject owner,bool bPreload = true)
    {
        ABLoader = null;
        InstGO = null;
        Prority = 0;
        Path = ResPath;
        NeedPreLoad = bPreload;
        Owner = owner;
    }

#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public AssetBundleLoader ABLoader;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public AnimationClip AnimClip = null;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public GameObject InstGO;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public int Prority;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public string Path;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public bool NeedPreLoad;
    internal GameObject Owner;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public int LifeTime = 5;
#if USE_HOTFIX
    //[Beebyte.Obfuscator.SkipRename]
#endif
    public Action _ActExecute = null;

    public bool IsPrepared
    {
        get { return ABLoader.state == LoadState.State_Complete; }
    }
    public bool IsFailed
    {
        get { return ABLoader.state == LoadState.State_Error; }
    }
    internal bool NeedExecute()
    {
        bool bRet = true;

        switch(GetTaskType())
        {
            case TaskType.TaskType_UI:
            case TaskType.TaskType_Sound:
            case TaskType.TaskType_Table:
            case TaskType.TaskType_StoryAnimation:
            case TaskType.TaskType_PreloadRs:
            case TaskType.TaskType_Scene:
                break;
            default:
                {
                    if (Owner == null || Owner.Equals(null))
                    {
                        bRet = false;
                    }
                }
                break;
        }
        
        return bRet;
    }
    internal void AddOwer()
    {
        if(GetTaskType() == TaskType.TaskType_Animation
            || GetTaskType() == TaskType.TaskType_Texture 
            || GetTaskType() == TaskType.TaskType_Lightmap
            || GetTaskType() == TaskType.TaskType_Asset
            )
        {
            ABLoader.bundleManager.AddOwer(Owner, ABLoader);
        }
    }
    internal void BeforeExecute(AssetBundleManager vManager)
    {
        switch(GetTaskType())
        {
            case TaskType.TaskType_Animation:
            case TaskType.TaskType_Texture:
            case TaskType.TaskType_Lightmap:
            case TaskType.TaskType_Sound:
            case TaskType.TaskType_Scene:
            case TaskType.TaskType_Asset:
                {
                    ABLoader.Retain();                    
                }
                break;
            case TaskType.TaskType_CharModle:
            case TaskType.TaskType_Effect:
            case TaskType.TaskType_Prefab:
            case TaskType.TaskType_StoryAnimation:
                {
                    InstGO = vManager.InstantiateAsset(ABLoader);
                }
                break;
            case TaskType.TaskType_UI:
                {
                    if(ABLoader.bundleInfo.refCount<1)
                    {
                        InstGO = vManager.InstantiateAsset(ABLoader);
                    }
                }
                break;
            case TaskType.TaskType_Table:
            case TaskType.TaskType_PreloadRs:
                {

                }
                break; 
        }
    }
    public abstract void Execute();
    public abstract TaskType GetTaskType();
    public void AfterExecute()
    {
        ABLoader = null;
        InstGO = null;
        Path = null;
    }
}
