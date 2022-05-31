using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

public class NGUIPrefabDependce : EditorWindow {

    public static NGUIPrefabDependce instance;
    public Dictionary<string, List<string>> m_Dict;
    private Vector2 mScroll = Vector2.zero;
    private Dictionary<string, Object> m_Cache=new Dictionary<string, Object>();

 
    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }

    void OnGUI()
    {
        if (m_Dict == null)
        {
            return;
        }

        mScroll=GUILayout.BeginScrollView(mScroll);

        foreach (string prefabName in m_Dict.Keys)
        {
            if (NGUIEditorTools.DrawHeader(prefabName))
            {
                foreach (string textureName in m_Dict[prefabName])
                {
                    if (NGUIEditorTools.DrawPrefixButton("Reference"))
                    {
                        List<string> list = LoadAtlasDependence(textureName);
                        GetWindow<NGUIAtlasReference>(false, "UI Dependencies", true).Show();
                        NGUIAtlasReference.instance.m_List = list;
                    }


                        if (!m_Cache.ContainsKey(textureName))
                    {
                        m_Cache.Add(textureName, AssetDatabase.LoadAssetAtPath(textureName, typeof(Texture2D)));
                    }
                    Texture2D go = m_Cache[textureName] as Texture2D;//= AssetDatabase.LoadAssetAtPath(textureName, typeof(Texture2D)) as Texture2D;
                    EditorGUILayout.ObjectField("Texture:" + go.name, go, typeof(Texture2D), false);
                }
               
            }
        }
        GUILayout.EndScrollView();
    }


    private static List<string> LoadAtlasDependence(string strAtlasName)
    {
        List<string> list =new List<string>();
        FindReference.ShowProgress(0, 0, 0);
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets\\Resources\\UI" });
        int i = 0;
        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] names = AssetDatabase.GetDependencies(new string[] { assetPath }); //依赖
            foreach (string name in names)
            {
                if (name.Equals(strAtlasName))
                {
                    list.Add(assetPath);
                }
            }
            FindReference.ShowProgress((float)i / (float)allGuids.Length, allGuids.Length, i);
            i++;
        }
        EditorUtility.ClearProgressBar();

        return list;
    } 




    [MenuItem("Assets/NGUI/Find UI Dependencies", false, 0)]
    /// <summary>
    /// 搜索ui的atlas依赖
    /// </summary>
    public static void FindUIDependencies()
    {
        Dictionary<string,List<string>> dict= LoadUIDependencies();
      
        GetWindow<NGUIPrefabDependce>(false, "UI Dependencies", true).Show();
        instance.m_Dict = dict;

    }

    private static Dictionary<string, List<string>> LoadUIDependencies()
    {
        Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        string strSelectPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        string[] allGuids;

        if (strSelectPath.EndsWith("prefab"))
        {
            allGuids = new string[1];
            allGuids[0] = Selection.assetGUIDs[0];
        }
        else
        {
            //选中文件夹下所有prefab的guid
            allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { strSelectPath });
        }

        FindReference.ShowProgress(0, 0, 0);
        int i = 0;
        foreach (string guid in allGuids)
        {
            string curPathName = AssetDatabase.GUIDToAssetPath(guid);
            string[] names = AssetDatabase.GetDependencies(new string[] { curPathName });

            List<string> texList;

            if (!dict.ContainsKey(curPathName))
            {
                dict.Add(curPathName, new List<string>());
            }
            texList = dict[curPathName];

            foreach (string name in names)
            {
                if (name.EndsWith(".png"))
                {
                    texList.Add(name);

                }
            }
            FindReference.ShowProgress((float)i / (float)allGuids.Length, allGuids.Length, i);
            i++;
        }

        EditorUtility.ClearProgressBar();
        return dict;
    }



    [MenuItem("Assets/NGUI/Print UI And Atlas", false, 0)]
    public static void Print()
    {
        Dictionary<string, List<string>> dict = LoadUIDependencies();
        HashSet<string> atlasSet = new HashSet<string>();

        foreach (string prefabName in dict.Keys)
        {
            foreach (string textureName in dict[prefabName])
            {
                if (!atlasSet.Contains(textureName))
                {
                    atlasSet.Add(textureName);
                }
            }
        }

        Dictionary<string,List<string>> texPrefabDict=new Dictionary<string, List<string>>();

        FindReference.ShowProgress(0, 0, 0);
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets\\Resources\\UI" });
        int i = 0;
        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] names = AssetDatabase.GetDependencies(new string[] { assetPath }); //依赖
            foreach (string name in names)
            {
                if (atlasSet.Contains(name))
                {
                    List<string> list;
                    if (!texPrefabDict.ContainsKey(name))
                    {
                        texPrefabDict.Add(name,new List<string>());
                    }
                    list = texPrefabDict[name];
                    list.Add(assetPath);
                }
            }
            FindReference.ShowProgress((float)i / (float)allGuids.Length, allGuids.Length, i);
            i++;
        }
        EditorUtility.ClearProgressBar();

        StringBuilder stringBuilder=new StringBuilder();

        Debug.Log("UI And Atlas Print Begin\n");

        foreach (string prefabName in dict.Keys)
        {
            Debug.Log("UI Prefab:" + prefabName + " dependence Atlas\n");
           
            foreach (string textureName in dict[prefabName])
            {
               

                if (texPrefabDict.ContainsKey(textureName))
                {
                    stringBuilder.Append("\tUI Atlas:" + textureName + " Reference Prefab\n");
                    foreach (string prefabNameInAtlas in texPrefabDict[textureName])
                    {
                        stringBuilder.Append("\t\t" + prefabNameInAtlas + "\n");
                    }
                    Debug.Log(stringBuilder);
                    stringBuilder.Remove(0,stringBuilder.Length) ;
                }
            }
        }

        Debug.Log("UI And Atlas Print End\n");
       
      
    }





}
