using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shark;
using Games.GlobeDefine;
using Games.LogicObj;

public class CombineSmrMgr : ComponentBase
{
    List<AvatarColorInfo> m_Infos = null;
    List<Transform> transforms = null;

    List<Material> materials = null;
    List<CombineInstance> combineInstances = null;
    List<Transform> bones = null;
    int m_ModelId = -1;
    int m_HairId = -1;
    //int m_FaceId = -1;
    private GameObject m_ObjRoot;
    public GameObject ObjRoot { get => m_ObjRoot; set => m_ObjRoot = value; }
    private GameObject[] m_FaceModels = new GameObject[4];

    public GameObject m_FaceObj;


    private GameObject m_BodyBoneObj;
    private GameObject m_HairBoneObj;
    //private GameObject m_WeaponBoneObj;
    private GameObject m_HairObj;
    private GameObject m_BodyObj;

    
    public GameDefine_Globe.OBJ_TYPE ObjType;
    public SMDKneadFaceFinalData m_OtherPlayerKneadFaceData;
    public int m_OtherHairId = 0;
    public int m_OtherBody = 0;
    public int m_OtherFaceId = 0;
    public List<AvatarColorInfo> m_OtherAvatarColorInfos = null;
    private Dictionary<int, int> m_PartDic = new Dictionary<int, int>();

    private GameObject m_HairPart = null;
    private GameObject m_BodyPart = null;

    private Animation m_BodyAni;
    private Animation m_HairAni;

    private bool m_IsKneadfacePrefab = false;
    #region Mono Function
    public override void Awake()
    {
        GameManager.gameManager.AddCombineSmrMgrList(this);
        transforms = new List<Transform>();

        materials = new List<Material>();
        combineInstances = new List<CombineInstance>();
        bones = new List<Transform>();
        AttributeList vScripy = GetAttrList();
        if (vScripy == null)
        {
            return;
        }
        m_BodyBoneObj = vScripy.m_lstGameObj[0];
        m_HairBoneObj = vScripy.m_lstGameObj[1];
        //m_WeaponBoneObj = vScripy.m_lstGameObj[2];
        m_HairObj = vScripy.m_lstGameObj[3];
        m_BodyObj = vScripy.m_lstGameObj[4];
        m_FaceObj = vScripy.m_lstGameObj[5];
        if (m_FaceObj != null)
        {
            m_IsKneadfacePrefab = true;
        }
        m_FaceModels[0] = vScripy.m_lstGameObj[6];
        m_FaceModels[1] = vScripy.m_lstGameObj[7];
        m_FaceModels[2] = vScripy.m_lstGameObj[8];
        m_FaceModels[3] = vScripy.m_lstGameObj[9];
        SetSkinRenderType hairRender = m_HairObj.GetComponent<SetSkinRenderType>();
        if (hairRender == null)
        {
            hairRender = m_HairObj.AddComponent<SetSkinRenderType>();
        }
        m_HairAni = m_HairObj.transform.parent.GetComponent<Animation>();
        hairRender.SkinType = PlayerSkinType.SkinType_Hair;
        SetSkinRenderType bodyRender = m_BodyObj.GetComponent<SetSkinRenderType>();
        if (bodyRender == null)
        {
            bodyRender = m_BodyObj.AddComponent<SetSkinRenderType>();
        }
        m_BodyAni = m_BodyObj.transform.parent.GetComponent<Animation>();
        bodyRender.SkinType = PlayerSkinType.SkinType_Body;

        //InitFace();
        OnEnable();
    }



    public override void OnEnable()
    {
        CEventManager.Instance.RegisterEvent(EEventType.Event_ChangeFaceMesh, KneadFaceOfMainPlayer);
        CEventManager.Instance.RegisterEvent(EEventType.Event_ChangeInitClothing, InitMainPlayer);
        CEventManager.Instance.RegisterEvent<bool,int, KneadFaceDataType>(EEventType.Event_ChangeClothing, new Action<bool,int, KneadFaceDataType>(ChangeMainPlayerSmr));
        Init();
    }

    public override void OnDisable()
    {
        CEventManager.Instance.UnRegisterEvent<bool,int, KneadFaceDataType>(EEventType.Event_ChangeClothing, new Action<bool,int, KneadFaceDataType>(ChangeMainPlayerSmr));
        CEventManager.Instance.UnRegisterEvent(EEventType.Event_ChangeInitClothing, InitMainPlayer);
        CEventManager.Instance.UnRegisterEvent(EEventType.Event_ChangeFaceMesh, KneadFaceOfMainPlayer);
    }

    public override void OnDestroy()
    {
        OnDisable();
        transforms.Clear();
        materials.Clear();
        combineInstances.Clear();
        bones.Clear();
        if (m_HairPart != null)
        {
           GameObject.Destroy(m_HairPart);
        }
        if (m_BodyPart != null)
        {
            GameObject.Destroy(m_BodyPart);
        }
        if (m_FaceObj != null)
        {
            GameObject.Destroy(m_FaceObj);
        }
    }
    #endregion

    #region Init Function
    public void InitFace()
    {
        int prefabId = -1;
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
        {
            prefabId = GameManager.gameManager.PlayerDataPool.KneadFaceData.GetPrefabTabId();
        }
        else if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER || ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            prefabId = m_OtherFaceId;
        }
        Tab_KneadFaceOfPrefab vData = Tab_KneadFaceOfPrefabProvider.Instance.GetDataById(prefabId);
        if (vData != null && m_FaceModels[0] != null && !m_IsKneadfacePrefab)
        {
            for (int i = 0; i < m_FaceModels.Length; i++)
            {
                if (m_FaceModels[i] != null)
                {
                    if (i == vData.FaceType)
                    {
                        m_FaceObj = m_FaceModels[vData.FaceType];
                        m_FaceObj.SetActive(true);
                    }
                    else
                    {
                        m_FaceModels[i].SetActive(false);
                    }
                }

            }
            if (vData.FaceType < m_FaceModels.Length)
            {
                m_FaceObj = m_FaceModels[vData.FaceType];
            }
        }
        if (m_FaceObj != null && !m_IsKneadfacePrefab)
        {
            SetSkinRenderType faceRender = m_FaceObj.GetComponent<SetSkinRenderType>();
            if (faceRender == null)
            {
                faceRender = m_FaceObj.AddComponent<SetSkinRenderType>();
            }
            faceRender.SkinType = PlayerSkinType.SkinType_Face;
            if (!GameManager.gameManager.PlayerDataPool.KneadFaceData.m_IsKneadFace)
            {
                CheckAndAddComp<MeshChange>(m_FaceObj);
            }
        }
        if (m_FaceObj == null || m_IsKneadfacePrefab)
        {
            if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
            {
                ChangeSmr(GameManager.gameManager.PlayerDataPool.KneadFaceData.GetPrefabTabId(), KneadFaceDataType.Face);
            }
            else if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER || ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
            {
                //if (m_OtherPlayerKneadFaceData == null)
                //{
                //    return;
                //}
                if (this.m_ObjRoot != null && ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
                {
                    Obj_SelectRole createPlayer = ObjFactory.Inst.GetComponent<Obj_SelectRole>(this.m_ObjRoot);
                    if (createPlayer != null)
                    {
                        m_Infos = createPlayer.GetAvatarColorInfo();
                    }
                }
                if (this.ObjRoot != null)
                {
                    Obj_OtherPlayer otherPlayer = ObjFactory.Inst.GetComponent<Obj_OtherPlayer>(this.ObjRoot);
                    if (otherPlayer == null)
                    {

                        ChangeSmr(m_OtherFaceId, KneadFaceDataType.Face);

                    }
                    else
                    {
                        ChangeSmr(otherPlayer.ShowFaceId, KneadFaceDataType.Face);
                    }
                }
                else
                {
                    if (m_OtherFaceId > 0)
                    {
                        ChangeSmr(m_OtherFaceId, KneadFaceDataType.Face);
                    }
                }
            }
        }
    }

    public void Init(bool inScene = false)
    {
        m_ModelId = -1;
        m_HairId = -1;
        if (null != m_Infos) {
            ObjManager.Instance.RecleAwatarColorInfoLst(m_Infos);
        }
        m_Infos = null;
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER||ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            //CEventManager.Instance.RegisterEvent<int, KneadFaceDataType>(EEventType.Event_ChangeClothing, new Action<int, KneadFaceDataType>(ChangeSmr));
        }

        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
        {
            InitMainPlayer();
        }
        else if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER || ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            InitOtherPlayer();
        }

        if (m_FaceObj == null)
        {
            return;
        }
        MeshChange vMC = ComponentFactory.Inst.GetComponent<MeshChange>(m_FaceObj);
        if (vMC == null)
        {
            return;
        }
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
        {
            vMC.m_ObjType = GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER;
        }
        else if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER || ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            vMC.m_ObjType = GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER;
            vMC.m_OtherPlayerKneadFaceData = m_OtherPlayerKneadFaceData;
        }
        vMC.Init(inScene);
    }
    
    private void InitMainPlayer()
    {
        if (ObjType != GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
        {
            return;
        }
        if (this.m_ObjRoot != null && ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            Obj_SelectRole createPlayer = ObjFactory.Inst.GetComponent<Obj_SelectRole>(this.m_ObjRoot);
            if (createPlayer != null)
            {
                m_Infos = createPlayer.GetAvatarColorInfo();
            }
        }
        //int hairId = GameManager.gameManager.PlayerDataPool.KneadFaceData.HairId;
        int hairId = GameManager.gameManager.PlayerDataPool.AvatarData.EquipHairId;
        Tab_PlayerSkinBase vSkinHairBase = Tab_PlayerSkinBaseProvider.Instance.GetDataById(hairId);
        if (vSkinHairBase != null)
        {
            ChangeSmr(vSkinHairBase.ModelID, KneadFaceDataType.Hair);
        }
        int clothId = GameManager.gameManager.PlayerDataPool.AvatarData.EquipClothId;
        Tab_PlayerSkinBase vSkinBase = Tab_PlayerSkinBaseProvider.Instance.GetDataById(clothId);
        if (vSkinBase != null)
        {
            ChangeSmr(vSkinBase.ModelID, KneadFaceDataType.Cloth);
        }
        int prefabId = GameManager.gameManager.PlayerDataPool.KneadFaceData.GetPrefabTabId();

        ChangeSmr(prefabId, KneadFaceDataType.Face);
    }

    private void InitOtherPlayer()
    {
        if (this.m_ObjRoot != null && ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            Obj_SelectRole createPlayer = ObjFactory.Inst.GetComponent<Obj_SelectRole>(this.m_ObjRoot);
            if (createPlayer != null)
            {
                m_Infos = createPlayer.GetAvatarColorInfo();
            }
        }
        if (this.ObjRoot != null)
        {
            Obj_OtherPlayer otherPlayer = ObjFactory.Inst.GetComponent<Obj_OtherPlayer>(this.ObjRoot);
            if (otherPlayer == null)
            {
                if (m_OtherHairId > 0)
                {
                    ChangeSmr(m_OtherHairId, KneadFaceDataType.Hair);
                }
                if (m_OtherBody > 0)
                {
                    ChangeSmr(m_OtherBody, KneadFaceDataType.Cloth);
                }
                if (m_OtherAvatarColorInfos != null)
                {
                    m_Infos = m_OtherAvatarColorInfos;
                }
            }
            else
            {
                m_Infos = otherPlayer.AvatarColorInfos;
                ChangeSmr(otherPlayer.ShowHairId, KneadFaceDataType.Hair);
                ChangeSmr(otherPlayer.ShowModelID, KneadFaceDataType.Cloth);
            }
        }
        else
        {
            if (m_OtherHairId > 0)
            {
                ChangeSmr(m_OtherHairId, KneadFaceDataType.Hair);
            }
            if (m_OtherBody > 0)
            {
                ChangeSmr(m_OtherBody, KneadFaceDataType.Cloth);
            }
            if (m_OtherAvatarColorInfos != null)
            {
                m_Infos = m_OtherAvatarColorInfos;
            }
        }
    }
    #endregion

    #region Change Function
    private void ChangeMainPlayerSmr(bool bMainPlayer, int modelId, KneadFaceDataType param)
    {
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
        {
            return;
        }
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_SElECTROLE)
        {
            return;
        }

        ChangeSmr(modelId, param);
    }
    private void ChangeSmr(int modelId, KneadFaceDataType param)
    {
        //if (path == null || path == "" || path == "nil")
        //{
        //    Debug.LogError("Tab_KneadfaceOClothing 出错");
        //    return;
        //}

        if (modelId == -1)
        {
            return;
        }
        if (this.gameObject == null)
        {
            return;
        }
        if (this.transform.parent != null && !this.transform.parent.gameObject.activeSelf)
        {
            return;
        }
        switch (param)
        {
            case KneadFaceDataType.Cloth:
                Tab_CharModel charModel = Tab_CharModelProvider.Instance.GetDataById(modelId);
                if (charModel != null)
                {
                    if (m_PartDic.ContainsKey((int)KneadFaceDataType.Cloth))
                    {
                        if (m_PartDic[(int)KneadFaceDataType.Cloth] != modelId)
                        {
                            m_ModelId = modelId;
                            m_PartDic[(int)KneadFaceDataType.Cloth] = modelId;
                            GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(charModel.ResPath, LoadPart, gameObject, m_BodyObj, m_BodyBoneObj, (int)KneadFaceDataType.Cloth));
                        }
                    }
                    else
                    {
                        m_ModelId = modelId;
                        m_PartDic.Add((int)KneadFaceDataType.Cloth, modelId);
                        GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(charModel.ResPath, LoadPart, gameObject, m_BodyObj, m_BodyBoneObj, (int)KneadFaceDataType.Cloth));
                    }


                }
                break;
            case KneadFaceDataType.Hair:
                Tab_HairModel hairModel = Tab_HairModelProvider.Instance.GetDataById(modelId);
                if (hairModel != null)
                {
                    if (m_PartDic.ContainsKey((int)KneadFaceDataType.Hair))
                    {
                        if (m_PartDic[(int)KneadFaceDataType.Hair] != modelId)
                        {
                            m_PartDic[(int)KneadFaceDataType.Hair] = modelId;
                            m_HairId = modelId;
                            GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(hairModel.ResPath, LoadPart, gameObject, m_HairObj, m_HairBoneObj, (int)KneadFaceDataType.Hair));
                        }
                    }
                    else
                    {
                        m_PartDic.Add((int)KneadFaceDataType.Hair, modelId);
                        m_HairId = modelId;
                        GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(hairModel.ResPath, LoadPart, gameObject, m_HairObj, m_HairBoneObj, (int)KneadFaceDataType.Hair));
                    }

                }
                break;
            case KneadFaceDataType.Face:
                Tab_KneadFaceOfPrefab faceModel = Tab_KneadFaceOfPrefabProvider.Instance.GetDataById(modelId);
                if (faceModel != null)
                {
                    //if (m_FaceId == modelId)
                    //{
                    //    return;
                    //}
                    //m_FaceId = modelId;
                    if (m_FaceModels[0] == null)
                    {
                        //this.gameObject.SetActive(false);
                        GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(faceModel.FacePath, LoadFace, gameObject));
                    }
                    //else
                    //{
                    //    GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask("Prefab/Model/" + faceModel.FaceMeshPath, LoadFace, gameObject));
                    //}
                }
                break;
                //case KneadFaceDataType.Weapon:
                //    GameManager.gameManager.ABManager.AddLoadTask(new PrefabLoadTask(path, LoadPart, gameObject, m_WeaponObj,m_WeaponBoneObj));
                //    break;

        }
    }
    private void LoadFace(GameObject obj, object param1, object param2, object param3)
    {
        if (obj == null)
        {
            return;
        }
        if (m_FaceObj != null)
        {
            //MeshControlHelper meshCH = ComponentFactory.Inst.GetComponent<MeshControlHelper>(m_FaceObj);
            //if (meshCH != null)
            //{
            //    meshCH.Remove();
            //}
            GameObject.Destroy(m_FaceObj);
        }
        if (this.gameObject == null)
        {
            return;
        }
        m_FaceObj = obj;
        m_FaceObj.transform.parent = this.gameObject.transform;
        //this.gameObject.SetActive(true);
        m_FaceObj.transform.localPosition = Vector3.zero;
        m_FaceObj.transform.localRotation = Quaternion.identity;
        m_FaceObj.transform.localScale = Vector3.one;
        SetSkinRenderType faceRender = m_FaceObj.GetComponent<SetSkinRenderType>();
        if (faceRender == null)
        {
            faceRender = m_FaceObj.AddComponent<SetSkinRenderType>();
        }
        faceRender.SkinType = PlayerSkinType.SkinType_Face;
        if (!GameManager.gameManager.PlayerDataPool.KneadFaceData.m_IsKneadFace)
        {
            SkinnedMeshRenderer smr = null;
            smr = m_FaceObj.GetComponent<SkinnedMeshRenderer>();
            if (smr == null)
            {
                smr = m_FaceObj.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            if (smr != null)
            {
                MeshChange vMC = CheckAndAddComp<MeshChange>(smr.gameObject);
            }
        }
        Animation faceAni = m_FaceObj.GetComponent<Animation>();


        if (faceAni != null && m_HairAni != null && m_BodyAni != null)
        {

            //AnimationState state1 = faceAni["nieliandaiji"];

            //AnimationState state2 = m_HairAni["nieliandaiji"];

            //AnimationState state3 = m_BodyAni["nieliandaiji"];

            faceAni.CrossFade("nieliandaiji");

            m_HairAni.Stop("nieliandaiji");
            m_HairAni.Play("nieliandaiji");
            m_BodyAni.Stop("nieliandaiji");
            m_BodyAni.Play("nieliandaiji");
            //m_HairAni.CrossFade("nieliandaiji",0,PlayMode.StopAll);
            //m_BodyAni.CrossFade("nieliandaiji", 0, PlayMode.StopAll);

            //if (state1 != null && state2 != null && state3 != null)
            //{
            //    state1.time = 0;
            //    state2.time = 0;
            //    state3.time = 0;
            //    faceAni.Sample();
            //    m_HairAni.Sample();
            //    m_BodyAni.Sample();
            //}
        }
        CEventManager.Instance.FireEvent(EEventType.Event_LoadedFaceModel);

    }

    private void LoadPart(GameObject obj, object param1, object param2, object param3)
    {
        if (!GameManager.gameManager.m_bChangeMeshData)
        {
            return;
        }
        if (this.gameObject == null)
        {
            return;
        }
        GameObject boneObj = param2 as GameObject;
        GameObject partObj = param1 as GameObject;
        int partType = (int)param3;
        if (partObj == null)
        {
            Debug.LogError("This part is null");
            return;
        }

        if (boneObj == null)
        {
            Debug.LogError("This boneObj is null");
            return;
        }

        SkinnedMeshRenderer smr = obj.GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            Debug.LogError("Not get SkinnedMeshRenderer");
            return;
        }

        transforms.Clear();
        materials.Clear();
        combineInstances.Clear();
        bones.Clear();

        transforms.AddRange(boneObj.GetComponentsInChildren<Transform>(true));
        materials.AddRange(smr.sharedMaterials);

        for (int i = 0; i < smr.sharedMesh.subMeshCount; i++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.subMeshIndex = i;
            combineInstances.Add(ci);
        }
        for (int i = 0; i < smr.bones.Length; i++)
        {
            for (int j = 0; j < transforms.Count; j++)
            {
                if (smr.bones[i].name.Equals(transforms[j].name))
                {
                    bones.Add(transforms[j]);
                    break;
                }
            }
        }
        
        SkinnedMeshRenderer oldSmr = partObj.GetComponent<SkinnedMeshRenderer>();
        oldSmr.sharedMesh = new Mesh();
        oldSmr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        oldSmr.bones = bones.ToArray();
        oldSmr.sharedMaterials = materials.ToArray();

        obj.SetActive(false);
        ChangeModelColor(partObj);
        switch (partType)
        {
            case (int)KneadFaceDataType.Cloth:
                if (m_BodyPart != null)
                {
                    GameObject.Destroy(m_BodyPart);
                }
                m_BodyPart = obj;
                GameObject.DontDestroyOnLoad(m_BodyPart);
                break;
            case (int)KneadFaceDataType.Hair:
                if (m_HairPart != null)
                {
                    GameObject.Destroy(m_HairPart);
                }
                m_HairPart = obj;
                GameObject.DontDestroyOnLoad(m_HairPart);
                break;
        }
        //Destroy(obj);
    }

    private void ChangeModelColor(GameObject obj)
    {

        SetSkinRenderType bodyRender = obj.GetComponent<SetSkinRenderType>();
        if (bodyRender != null)
        {
            PlayerSkinType curType = bodyRender.SkinType;
            if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
            {
                Obj_MainPlayer player = Singleton<ObjManager>.Instance.MainPlayer;
                if (player != null)
                {
                    m_Infos = player.AvatarColorInfos;
                    if (!GameManager.gameManager.PlayerDataPool.KneadFaceData.m_IsKneadFace)
                    {
                        Utils.OnEquipStarCountChange(this.gameObject.transform.parent.gameObject, GameManager.Instance.PlayerDataPool.OldStarCount, GameManager.Instance.PlayerDataPool.EquipPack.GetEquipStarCount(), player.MenPai);
                    }
                }
                if (FittingManager.Instance.IsFitting)
                {
                    FittingManager.Instance.bChangeEffect = true;
                }
            }
            if (m_Infos != null)
            {
                switch (curType)
                {
                    case PlayerSkinType.SkinType_Hair:
                        if (m_HairId != -1)
                            FittingTool.ChangePlayerHairModelColor(this.gameObject, m_HairId, m_Infos);
                        break;
                    case PlayerSkinType.SkinType_Body:
                        if (m_ModelId != -1)
                            FittingTool.ChangePlayerModelColor(this.gameObject, m_ModelId, m_Infos);
                        break;
                }
            }
        }
    }

    private void KneadFaceOfMainPlayer()
    {
        if (ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
        {
            InitFace();
            if (m_FaceObj == null)
            {
                return;
            }
            MeshChange vMC = ComponentFactory.Inst.GetComponent<MeshChange>(m_FaceObj);
            if (vMC == null)
            {
                return;
            }


            vMC.m_ObjType = GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER;
            InitMainPlayer();

            vMC.Init();
        }
    }
    #endregion
}
