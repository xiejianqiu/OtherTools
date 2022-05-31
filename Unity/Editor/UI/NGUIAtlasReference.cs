using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class NGUIAtlasReference : EditorWindow
{

    public static NGUIAtlasReference instance;
    private Vector2 mScroll = Vector2.zero;
    private Dictionary<string, Object> m_Cache = new Dictionary<string, Object>();
    public List<string> m_List=new List<string>(); 
  

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }

    private void OnGUI()
    {
        if (m_List==null)
        {
            return;
        }
        mScroll = GUILayout.BeginScrollView(mScroll);
        foreach (string prefabName in m_List)
        {
            if (!m_Cache.ContainsKey(prefabName))
            {
                m_Cache.Add(prefabName, AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject)));
            }
            GameObject go = m_Cache[prefabName] as GameObject;
            if (go)
            {
                EditorGUILayout.ObjectField(go.name, go, typeof (GameObject), false);
            }
        }
        GUILayout.EndScrollView();
    }


}
