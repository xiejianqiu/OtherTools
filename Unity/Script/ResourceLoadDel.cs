using Shark;
using Games.LogicObj;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceLoadDel
{
    private static MaterialPropertyBlock m_prop = new MaterialPropertyBlock();
    public static void OnLoadUI(UILoadTask vTask)
    {
        ProfilerUtil.BeginSample("ResourceLoadDel.DoAddUI");
        UIManager.Instance().DoAddUI((UIPathData)vTask.m_UIPathData, vTask.InstGO, vTask.m_OnOpen,false, vTask.m_Param);
        ProfilerUtil.EndSample();
    }

    public enum EModelType
    {
        ECharModel = 1,
        ECharCreateModel =2,
        ECharCreateOtherModel =3,
        EFakeObjModel =4,
        EFakeObjBackGround =5,
        ETeamFakeObjModel =6,
        EMountModel =7,
        EWeaponModel =8,
        EPetSkillModel =9,
        EStarSoulModel =10,
        ECharWing =11,
        EUIModel =12,
        EShouHu =13,
        EArtifact =14,
        EXingHun =15,
        ENpcMission =16,
        EDestinyObjModel =17,
        EWarPetModel = 18,
    }
    public static void OnLoadModel(CharModelLoadTask vTask)
    {
        if (vTask.InstGO == null)
        {
            return;
        }
        vTask.InstGO.name = "Model";
        switch ((EModelType)vTask.m_eModelType)
        {
            case EModelType.ECharModel:
                {
                    ObjManager.ReloadModelOver(vTask.m_objParent, vTask.InstGO, (int)vTask.m_Param3, null);
                    if(vTask.m_delFun != null)
                    {
                        vTask.m_delFun("",vTask.InstGO,vTask.m_Param1,vTask.m_Param2,vTask.m_Param3);
                    }
                }
                break;
            case EModelType.EFakeObjModel:
                {
                    ObjManager.LoadFakeObjOver(vTask.m_Param3 as Games.FakeObject.FakeObject, vTask.InstGO, vTask.m_Param1, vTask.m_Param2);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                    //加载回调里边有的需要透传参数
                    //if (null != vTask.m_Param1 && vTask.m_Param1 is List<object> list)
                    //{
                    //    MyListPool<object>.Recycle(list);
                    //    if (list.Count > 3 && list[3] is List<AvatarColorInfo> lst2)
                    //    {
                    //        if (null != lst2)
                    //        {
                    //            MyListPool<AvatarColorInfo>.Recycle(lst2);
                    //        }
                    //    }
                    //}
                }
                break;
            case EModelType.EFakeObjBackGround:
                {
                    ObjManager.AsycLoadFakeObjBGOver(vTask.m_Param1 as Games.FakeObject.FakeObject, vTask.InstGO, vTask.m_Param2);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.ETeamFakeObjModel:
                {
                    ObjManager.LoadTeamFakeObjOver(vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.EMountModel:
                {                    
                    vTask.m_delFun("", vTask.InstGO, vTask.m_objParent, vTask.m_Param1, vTask.m_Param2);
                }
                break;
            case EModelType.EWeaponModel:
                {
                    ObjManager.ReloadWeaponOver(null, vTask.InstGO, vTask.m_objParent, vTask.m_Param1, vTask.m_Param2);
                    if (vTask.m_delFun != null)
                    {
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                    }
                    //--不能只及回收，引用用可能使用角色身上携带的list
                    //if (null != vTask.m_Param1 && vTask.m_Param1 is List<object> list)
                    //{
                    //    MyListPool<object>.Recycle(list);
                    //    if (list.Count > 4 && list[4] is List<AvatarColorInfo> lst2) {
                    //        MyListPool<AvatarColorInfo>.Recycle(lst2);
                    //    }
                    //}
                }
                break;
            case EModelType.EPetSkillModel:
                {
                    vTask.InstGO.transform.parent = vTask.m_objParent.transform;
                    if (vTask.m_delFun != null)
                    {
                        vTask.m_delFun("", vTask.InstGO, vTask.m_objParent, vTask.m_Param1, vTask.m_Param2);
                    }
                }
                break;

            case EModelType.EStarSoulModel:
                {
                    //vTask.InstGO.name = "StarSoul";
                    ObjManager.ReloadStarSoulModelOver(vTask.m_objParent, vTask.InstGO, vTask.m_Param1 as Tab_CharSoul, vTask.m_Param2 as Obj_Character);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.EWarPetModel:
                {
                    //vTask.InstGO.name = "StarSoul";
                    ObjManager.ReloadWarPetModelOver(vTask.m_objParent, vTask.InstGO, vTask.m_Param1 as Tab_WarPetModel, vTask.m_Param2 as Obj_Character);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.EXingHun:
                {
                    vTask.InstGO.name = "XingHun";
                    ObjManager.ReloadXingHunModelOver(vTask.m_objParent, vTask.InstGO, vTask.m_Param1 as Tab_CharXingHun, vTask.m_Param2 as Obj_OtherPlayer);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.EShouHu:
                {
                    if (vTask.m_delFun != null)
                    {
                        vTask.m_delFun(vTask.Path, vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                    }
                }
                break;
            case EModelType.ECharCreateModel:
                {
                    ObjManager.ReloadModelOver(vTask.m_objParent, vTask.InstGO, (int)vTask.m_Param3, null);
                    vTask.m_delFun("",vTask.InstGO,vTask.m_Param1,vTask.m_Param2,vTask.m_Param3);
                }
                break;
            case EModelType.ECharCreateOtherModel:
                {
                    vTask.m_delFun("", vTask.InstGO, vTask.m_objParent, vTask.m_Param1, vTask.m_Param2);
                }
                break;
            case EModelType.ECharWing:
                {
                    vTask.m_delFun("",vTask.InstGO,vTask.m_Param1,vTask.m_Param2,vTask.m_Param3);
                }
                break;
            case EModelType.EUIModel:
                {
                    Transform trans = vTask.InstGO.transform;                    
                    trans.parent = vTask.m_objParent.transform;
                    trans.localPosition = Vector3.zero;
                    trans.localScale = Vector3.one * 200;
                    trans.GetAndAddComponent<UIModelClip>();
                    
                    vTask.InstGO.layer = vTask.m_objParent.layer;
                    trans.SetChildLayer(vTask.m_objParent.layer);
                    Renderer[] vecRender = trans.GetComponentsInChildren<Renderer>();
                    
                    if (UIManager.GetUIByUIInfo(UIInfo.CardGamePanel) 
                        || UIManager.GetUIByUIInfo(UIInfo.CardTreasureHuntPanel) 
                        || UIManager.GetUIByUIInfo(UIInfo.CardRankUI) || UIManager.GetUIByUIInfo(UIInfo.JuLingEnter))
                    {
                        EffectSettingCollection[] effects = trans.GetComponentsInChildren<EffectSettingCollection>();
                        for (int i = 0; i < effects.Length; i++)
                        {
                            if (effects[i].gameObject.activeSelf)
                            {
                                effects[i].gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        trans.GetAndAddComponent<UIParticleClip>();
                    }
                    for(int i = 0; i < vecRender.Length; i++)
                    {
                        vecRender[i].sortingLayerID = SortingLayer.layers[10].id;
                        if (UIManager.GetUIByUIInfo(UIInfo.FirstRecharge))
                        {
                            vecRender[i].sortingLayerID = SortingLayer.layers[5].id;
                        }
                        else if (UIManager.GetUIByUIInfo(UIInfo.ZhanJiaScene))
                        {
                            vecRender[i].sortingLayerID = SortingLayer.layers[4].id;
                        }
                        else if (UIManager.GetUIByUIInfo(UIInfo.GetNewModelUI)==null&&UIManager.GetUIByUIInfo(UIInfo.PurchaseAndRestitutUI))
                        {
                            vecRender[i].sortingLayerID = SortingLayer.layers[4].id;
                        }
                        else if (UIManager.GetUIByUIInfo(UIInfo.CardGamePanel)
                                 || UIManager.GetUIByUIInfo(UIInfo.CardTreasureHuntPanel) 
                                 || UIManager.GetUIByUIInfo(UIInfo.CardRankUI))
                        {
                            vecRender[i].sortingLayerID = SortingLayer.layers[4].id;
                            if (trans.GetComponent<Animation>().clip.name != "chujidaiji" && trans.GetComponent<Animation>().clip.name != "zhandoudaiji")
                            {
                                if (trans.GetComponent<Animation>().GetClip("chujidaiji") != null)
                                    trans.GetComponent<Animation>().clip = trans.GetComponent<Animation>().GetClip("chujidaiji");
                                else if (trans.GetComponent<Animation>().GetClip("zhandoudaiji") != null)
                                    trans.GetComponent<Animation>().clip = trans.GetComponent<Animation>().GetClip("zhandoudaiji");
                                trans.GetComponent<Animation>().playAutomatically = true;
                                trans.GetComponent<Animation>().Play();
                            }
                        }
                        vecRender[i].GetPropertyBlock(m_prop);
                        if (m_prop != null)
                        {
                            m_prop.SetFloat(ShaderProp.FixedViewDir, 1);
                            m_prop.SetFloat(ShaderProp.TeamFixedViewDir, 0);
                            m_prop.SetVector("_UIDir", new Vector4(0, 0, -1, 0));
                        }
                        vecRender[i].SetPropertyBlock(m_prop);
                    }
                    Tab_UIModel vInfo = (Tab_UIModel)vTask.m_Param1;
                    if(vInfo!=null)
                    {
                        trans.localPosition = new Vector3(vInfo.XOffSet,vInfo.YOffSet,vInfo.ZOffset);
                        trans.localRotation = Quaternion.Euler(vInfo.XRotation,vInfo.YRotation,vInfo.ZRotation);
                        trans.localScale = vInfo.Scale * trans.localScale;
                    }

                    if(vTask.m_delFun != null)
                    {
                        vTask.m_delFun(vTask.Path, vTask.InstGO, null, vTask.m_Param2);
                    }
                }
                break;
            case EModelType.EArtifact:
                ObjManager.AsycLoadArtifactModelOver(vTask.m_objParent, vTask.InstGO, vTask.m_Param1 as Tab_ArtifactBase, vTask.m_Param2 as ArtifactMoveControl);
                if (vTask.m_delFun != null)
                    vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                break;
            case EModelType.ENpcMission:
                {
                    vTask.InstGO.name = "NpcMission";
                    ObjManager.ReloadNpcMissionModelOver(vTask.m_objParent, vTask.InstGO, vTask.m_Param1 as Tab_CharSoul, vTask.m_Param2 as Obj_Character);
                    if (vTask.m_delFun != null)
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                }
                break;
            case EModelType.EDestinyObjModel:
                {
                    if(vTask.InstGO != null)
                    {
                        vTask.InstGO.transform.SetParent(vTask.m_objParent.transform);
                        vTask.InstGO.transform.localPosition = Vector3.zero;
                        vTask.InstGO.transform.localRotation = Quaternion.identity;
                        vTask.InstGO.transform.localScale = Vector3.one;
                    }
                    if (vTask.m_delFun != null)
                    {
                        vTask.m_delFun("", vTask.InstGO, vTask.m_Param1, vTask.m_Param2, vTask.m_Param3);
                    }
                }
                break;                
        }
    }
    private static void OnReadFinish()
    {
        GameManager.gameManager.IsFinishReadTables = true;
        GameManager.gameManager.ABManager.RemoveLoader(TablesLoadTask.m_abLoader, true);
        TablesLoadTask.m_abLoader = null;
        _tablesLoadTask = null;
    }
    private static void OnReadClientTxtFinish()
    {
        GameManager.gameManager.IsFinishReadClientTables = true;
        if (GameManager.gameManager.IsFinishReadPublicTables && GameManager.gameManager.IsFinishReadClientTables)
        {
            OnReadFinish();
        }
    }
    private static void OnReadPublicTxtFinish()
    {
        GameManager.gameManager.IsFinishReadPublicTables = true;
        if (GameManager.gameManager.IsFinishReadPublicTables && GameManager.gameManager.IsFinishReadClientTables) {
            OnReadFinish();
        }
    }
    private static TablesLoadTask _tablesLoadTask = null;
    public static void OnLoadTable(TablesLoadTask vTask)
    {
        _tablesLoadTask = vTask;
        DataProvider.Instance.ClientInit(ReadTxtTable,UnityInvoke.Instance, OnReadFinish);
    }
    public static void OnLoadClientTxtTable(TablesLoadTask vTask)
    {
        _tablesLoadTask = vTask;
        DataProvider.Instance.ClientTxtInit(ReadTxtTable, UnityInvoke.Instance, OnReadClientTxtFinish);
    }
    public static void OnLoadPublicTxtTable(TablesLoadTask vTask)
    {
        _tablesLoadTask = vTask;
        DataProvider.Instance.PublicTxtInit(ReadTxtTable, UnityInvoke.Instance, OnReadPublicTxtFinish);
    }
    public static string ReadTxtTable(string path)
    {
        return _tablesLoadTask.ReadTxtTable(path);
    }

    public static void OnLoadStoryAnimation(GameObject vGo)
    {
        StoryManager.Instance.OnStoryLoad(vGo);
    }

    public static void OnLoadSound(SoundLoadtask vTask)
    {
        AudioClip ac = vTask.ABLoader.bundleInfo.mainObject as AudioClip;

        GameManager.gameManager.SoundManager.m_SoundClipPools.OnLoadSound(vTask.Path,ac,vTask.ABLoader,(Tab_Sounds)vTask.TabInfo,(SoundLoadtask.GetSoundClipDelegate)vTask.DelFun,(SoundClipParam)vTask.ClipParam);
    }
}