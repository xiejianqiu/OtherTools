using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
/// <summary>
/// 图集引用关系查找工具
/// </summary>
public class NGUIAtlasFinder : EditorWindow
{
    private Vector2 mScroll = Vector2.zero;
    public string AtlasName = "New_loginUI";
    private Dictionary<string, Vector2> m_dictScroll = new Dictionary<string, Vector2>();
    [MenuItem("NGUI/Open/Atlas Finder", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<NGUIAtlasFinder>(false, "Atlas Finder", true).Show();
    }


    void OnGUI()
    {
        NGUIEditorTools.SetLabelWidth(80f);


        List<UISprite> vList = NGUIEditorTools.FindAll<UISprite>();
        //            List<UISprite> vList=new List<UISprite>();
        //            UIManager.GetComponentInChilds(Selection.activeGameObject.transform, vList);
        //            UISprite vSpriteSelect= Selection.activeGameObject.GetComponent<UISprite>();
        //            if (vSpriteSelect)
        //            {
        //                vList.Add(vSpriteSelect);
        //            }

        Dictionary<UIAtlas, List<UISprite>> dict = new Dictionary<UIAtlas, List<UISprite>>();

        for (int i = 0; i < vList.Count; i++)
        {
            UISprite vSprite = vList[i];
            if (vSprite)
            {
                List<UISprite> vListInSameAtlas;
                if (vSprite.atlas != null)
                {
                    if (!dict.ContainsKey(vSprite.atlas))
                    {
                        dict.Add(vSprite.atlas, new List<UISprite>());
                    }
                    vListInSameAtlas = dict[vSprite.atlas];
                    vListInSameAtlas.Add(vSprite);
                }
            }
        }

        foreach (var VARIABLE in dict)
        {
            UIAtlas vAtlas = VARIABLE.Key;
//            mScroll = GUILayout.BeginScrollView(mScroll);
            if (NGUIEditorTools.DrawHeader(vAtlas.name))
            {
                Vector2 ScrollRow = Vector2.zero;
                if (!m_dictScroll.ContainsKey(vAtlas.name))
                {
                    m_dictScroll.Add(vAtlas.name, Vector2.zero);
                }
                ScrollRow = m_dictScroll[vAtlas.name];



                EditorGUILayout.ObjectField("", vAtlas.texture, typeof(Texture2D), false);

                List<UISprite> vListDraw = VARIABLE.Value;

                if (vListDraw.Count > 0)
                {
                    //   NGUIEditorTools.DrawSeparator();
                    GUILayout.BeginVertical();
                    m_dictScroll[vAtlas.name] = GUILayout.BeginScrollView(ScrollRow);
                    foreach (UISprite sprite in vListDraw)
                    {
                        DrawRow(sprite);
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }
             

            }
//            GUILayout.EndScrollView();

        }
    }

    /// <summary>
    /// Helper function used to print things in columns.
    /// </summary>

    void DrawRow(UISprite vSprite)
    {
        bool highlight = (vSprite != null && Selection.activeGameObject != null) && Selection.activeGameObject == vSprite;

        if (vSprite != null)
        {
            GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUILayout.BeginHorizontal();
        }

        bool enabled = (vSprite == null || (NGUITools.GetActive(vSprite.gameObject) && vSprite.enabled));

        GUI.color = Color.white;

        if (vSprite != null)
        {
            if (enabled != EditorGUILayout.Toggle(enabled, GUILayout.Width(20f)))
            {
                vSprite.enabled = !enabled;
                EditorUtility.SetDirty(vSprite.gameObject);
            }
        }
        else
        {
            ;
            GUILayout.Space(30f);
        }

        if (enabled)
        {
            GUI.color = highlight ? new Color(0f, 0.8f, 1f) : Color.white;
        }
        else
        {
            GUI.color = highlight ? new Color(0f, 0.5f, 0.8f) : Color.grey;
        }

        string atlasName;

        if (vSprite == null)
        {
            atlasName = AtlasName;

        }
        else
        {
            atlasName = vSprite.name;
        }

        if (GUILayout.Button(atlasName, EditorStyles.label, GUILayout.MinWidth(100f)) && vSprite != null)
        {
            Selection.activeGameObject = vSprite.gameObject;
            EditorUtility.SetDirty(vSprite.gameObject);
        }

        GUI.color = enabled ? Color.white : new Color(0.7f, 0.7f, 0.7f);


        GUILayout.EndHorizontal();
    }


    void OnSelectionChange() { Repaint(); }
}
