using System.Collections.Generic;
using UnityEngine;

public class TestSkinnedCombined : MonoBehaviour
{
    public GameObject tongyongbody;
    public GameObject newBodyObj;
    public GameObject boneObj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("Test")]
    public void Test()
    {

        SkinnedMeshRenderer smr = newBodyObj.GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            //LogModule.ErrorLog("Not get SkinnedMeshRenderer");
            return;
        }
        List<Transform> bonesTranLst = new List<Transform>();
        List<Material> materialLst = new List<Material>();
        List<Transform> bonesLst = new List<Transform>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        bonesTranLst.AddRange(boneObj.GetComponentsInChildren<Transform>(true));
        materialLst.AddRange(smr.sharedMaterials);

        for (int i = 0; i < smr.sharedMesh.subMeshCount; i++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.subMeshIndex = i;
            combineInstances.Add(ci);
        }
        for (int i = 0; i < smr.bones.Length; i++)
        {
            for (int j = 0; j < bonesTranLst.Count; j++)
            {
                if (smr.bones[i].name.Equals(bonesTranLst[j].name))
                {
                    bonesLst.Add(bonesTranLst[j]);
                    break;
                }
            }
        }

        SkinnedMeshRenderer oldSmr = tongyongbody.GetComponent<SkinnedMeshRenderer>();
        oldSmr.sharedMesh = new Mesh();
        oldSmr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        oldSmr.bones = bonesLst.ToArray();
        oldSmr.sharedMaterials = materialLst.ToArray();

    }
    [ContextMenu("TestBone")]
    public void TestBone()
    {

        SkinnedMeshRenderer smr = newBodyObj.GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            //LogModule.ErrorLog("Not get SkinnedMeshRenderer");
            return;
        }
        List<Transform> bonesTranLst = new List<Transform>();
        List<Material> materialLst = new List<Material>();
        List<Transform> bonesLst = new List<Transform>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        bonesTranLst.AddRange(boneObj.GetComponentsInChildren<Transform>(true));
        materialLst.AddRange(smr.sharedMaterials);

        for (int i = 0; i < smr.sharedMesh.subMeshCount; i++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.subMeshIndex = i;
            combineInstances.Add(ci);
        }
        for (int i = 0; i < smr.bones.Length; i++)
        {
            for (int j = 0; j < bonesTranLst.Count; j++)
            {
                if (smr.bones[i].name.Equals(bonesTranLst[j].name))
                {
                    bonesLst.Add(bonesTranLst[j]);
                    break;
                }
            }
        }

        SkinnedMeshRenderer oldSmr = tongyongbody.GetComponent<SkinnedMeshRenderer>();
        //oldSmr.sharedMesh = new Mesh();
        //oldSmr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        oldSmr.bones = bonesLst.ToArray();
        //oldSmr.sharedMaterials = materialLst.ToArray();

    }
    [ContextMenu("TestBoneMesh")]
    public void TestBoneMesh()
    {

        SkinnedMeshRenderer smr = newBodyObj.GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            //LogModule.ErrorLog("Not get SkinnedMeshRenderer");
            return;
        }
        List<Transform> bonesTranLst = new List<Transform>();
        List<Material> materialLst = new List<Material>();
        List<Transform> bonesLst = new List<Transform>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        bonesTranLst.AddRange(boneObj.GetComponentsInChildren<Transform>(true));
        materialLst.AddRange(smr.sharedMaterials);

        for (int i = 0; i < smr.sharedMesh.subMeshCount; i++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.subMeshIndex = i;
            combineInstances.Add(ci);
        }
        for (int i = 0; i < smr.bones.Length; i++)
        {
            for (int j = 0; j < bonesTranLst.Count; j++)
            {
                if (smr.bones[i].name.Equals(bonesTranLst[j].name))
                {
                    bonesLst.Add(bonesTranLst[j]);
                    break;
                }
            }
        }

        SkinnedMeshRenderer oldSmr = tongyongbody.GetComponent<SkinnedMeshRenderer>();
        oldSmr.sharedMesh = new Mesh();
        oldSmr.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        //oldSmr.bones = bonesLst.ToArray();
        oldSmr.sharedMaterials = materialLst.ToArray();

    }
}
