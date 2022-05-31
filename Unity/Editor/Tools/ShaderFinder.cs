using System.Collections;
using System.Collections.Generic;
using System.IO;
using Shark;
using UnityEditor;
using UnityEngine;
using WellFired;

public class ShaderFinder : EditorWindow
{
    [MenuItem("Tools/ShaderFinder", false, 11)]
    static void Start()
    {
        string shaderName = "Standard";
        GameObject[] objs = Selection.gameObjects;
        HashSet<string> paths = new HashSet<string>();
        foreach (GameObject obj in objs)
        {
            Renderer[] vRenderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer vRenderer in vRenderers)
            {
                if (vRenderer)
                {
                    foreach (Material mat in vRenderer.sharedMaterials)
                    {
                        if (mat)
                        {
                            if (mat.shader.name.Contains(shaderName))
                            {
                                //                                Debug.LogError(mat.shader.name);
                                //                                Debug.LogError(vRenderer.transform.GetFullHierarchyPath());
                                string pathName = vRenderer.transform.GetFullHierarchyPath();
                                if (!paths.Contains(pathName))
                                {
                                    paths.Add(pathName);
                                }
                            }
                        }
                    }
                }

            }

            GetPathInMesh<MeshFilter>(obj, shaderName, paths);
            GetPathInMesh<SkinnedMeshRenderer>(obj, shaderName, paths);
        }

        foreach (string path in paths)
        {
            Debug.LogError(path);
        }

        Debug.LogError("finish");
    }

    private static void GetPathInMesh<T>(GameObject obj, string shaderName, HashSet<string> paths) where T : Object
    {
        T[] vMeshes = obj.GetComponentsInChildren<T>();
        foreach (T vMesh in vMeshes)
        {
            Object[] depObjs = EditorUtility.CollectDependencies(new Object[] { vMesh });
            foreach (Object depObj in depObjs)
            {
                if (depObj is Mesh)
                {
                    string pathParent = AssetDatabase.GetAssetPath(depObj);

                    Object[] fbxDepObjs = EditorUtility.CollectDependencies(new Object[] { AssetDatabase.LoadAssetAtPath(pathParent, typeof(Object)) });
                    foreach (Object fbxDepObj in fbxDepObjs)
                    {
                        if (fbxDepObj is Shader && fbxDepObj.name.Contains(shaderName))
                        {
                            //                                Debug.LogError(fbxDepObj);
                            //                                Debug.LogError(pathParent);
                            //                                Debug.LogError(vMesh.gameObject.name);

                            if (!paths.Contains(pathParent))
                            {
                                paths.Add(pathParent);
                            }
                        }
                    }
                }
            }
        }
    }

//    [MenuItem("Tools/CheckUIInfo", false, 11)]
//    public static void CheckUIInfo()
//    {
//    
//        FindReference.ShowProgress(0,0,0);
//        DirectoryInfo vInfo = new DirectoryInfo(Application.dataPath + "/Script");
//
//        FileInfo[] fileInfos = vInfo.GetFiles("*.cs", SearchOption.AllDirectories);
//        Dictionary<string, string> dict = new Dictionary<string, string>();
//        foreach (FileInfo fileInfo in fileInfos)
//        {
//
//            string fullPath = fileInfo.DirectoryName + "/" + fileInfo.Name;
//
//            StreamReader vStreamReader = fileInfo.OpenText();
//            string text = vStreamReader.ReadToEnd();
//            vStreamReader.Close();
//            vStreamReader.Dispose();
//
//            if (text.Contains("UIInfo") && !"UIInfo.cs".Equals(fileInfo.Name))
//            {
//                dict.Add(fullPath, text);
//            }
//        }
//
//        DataProvider.Instance.Init(new DataProvider.ReadAllTextDelegate(MyFileUtil.ReadTxtTable));
//        Dictionary<int, Tab_UIInfo> vInfos = Tab_UIInfoProvider.Instance.GetData();
//        Dictionary<string, List<string>> unUsedDict = new Dictionary<string, List<string>>();
//        int index = 0;
//        foreach (Tab_UIInfo vUIInfo in vInfos.Values)
//        {
//            if (vUIInfo != null && !vUIInfo.Path.Equals("nil"))
//            {
//                string path = Application.dataPath + "/BundleData/UI/" + vUIInfo.Path + ".prefab";
//                if (!File.Exists(path))
//                {
//                    foreach (KeyValuePair<string, string> pairs in dict)
//                    {
//                        string strId = "UIInfo." + vUIInfo.Name;
//                        if (pairs.Value.Contains(strId))
//                        {
//                            if (!unUsedDict.ContainsKey(vUIInfo.Name))
//                            {
//                                unUsedDict.Add(vUIInfo.Name, new List<string>());
//                            }
//                            unUsedDict[vUIInfo.Name].Add(pairs.Key);
//                        }
//                    }
//                }
//            }
//            index++;
//            FindReference.ShowProgress(index/ (float)vInfos.Values.Count,  vInfos.Values.Count, index);
//        }
//
//        foreach (var pairs in unUsedDict)
//        {
//            Debug.LogError("UI Name:"+pairs.Key);
//            foreach (string path in pairs.Value)
//            {
//                Debug.LogError(path);
//            }
//            Debug.LogError("-------------" );
//        }
//        EditorUtility.ClearProgressBar();
//    }

}
