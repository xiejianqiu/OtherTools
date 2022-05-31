using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.IO;

class NGUIAtlasSpriteFinder : EditorWindow
{
    public GameObject OrginAtlasGameObject = null;
    public UIAtlas OrginAtlas = null;
    public string OrginSpriteName = null;

    public GameObject TargetAtlasGameObject = null;
    public UIAtlas TargetAtlas = null;
    public string TargetSpriteName = null;

    private Dictionary<GameObject, List<UISprite>> SpriteDic = new Dictionary<GameObject, List<UISprite>>();
    private Dictionary<GameObject, List<UIButton>> ButtonDic = new Dictionary<GameObject, List<UIButton>>();
    private Vector2 ScrollViewPos = Vector2.zero;

    private bool NeedChangeWidthAndHeight = false;
    private int Width = 0;
    private int Height = 0;
    private bool CanEditorSprite = false;
    private string EditorSpriteName = null;

    [MenuItem("NGUI/Open/Sprite Finder", false, 10)]
    static public void OpenSpriteFinderWindow()
    {
        EditorWindow.GetWindow<NGUIAtlasSpriteFinder>(false, "Sprite Finder", true).Show();
    }

    public void OnSelectBtnClick()
    {

    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (NGUIEditorTools.DrawPrefixButton("Orgin Atlas"))
        {
            ComponentSelector.Show<UIAtlas>(OnSelectOrginAtlas);
        }
        EditorGUILayout.LabelField("Orgin Atlas Name : ");
        EditorGUILayout.LabelField(OrginAtlasGameObject == null ? "" : OrginAtlasGameObject.name);
        EditorGUILayout.EndHorizontal();

        if (OrginAtlasGameObject == null)
        {
            return;
        }

        OrginAtlas = OrginAtlasGameObject.GetComponent<UIAtlas>();

        NGUIEditorTools.DrawAdvancedSpriteField(OrginAtlas, OrginSpriteName, OnSelectOrginSprite, false);

        RenameSpriteName();

        if (GUILayout.Button("Select In Scene"))
        {
            SelectAll();
        }

        ScrollViewPos = EditorGUILayout.BeginScrollView(ScrollViewPos);
        var e = SpriteDic.GetEnumerator();
        while (e.MoveNext())
        {
            DrawGroup(e.Current.Key, e.Current.Value);
        }
        e.Dispose();
        EditorGUILayout.EndScrollView();


        if (SpriteDic.Count <= 0)
        {
            EditorGUILayout.LabelField("No UISprite cantain this sprite!");
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if (NGUIEditorTools.DrawPrefixButton("Target Atlas"))
        {
            ComponentSelector.Show<UIAtlas>(OnSelectTargetAtlas);
        }
        EditorGUILayout.LabelField("Target Atlas Name : ");
        EditorGUILayout.LabelField(TargetAtlasGameObject == null ? "" : TargetAtlasGameObject.name);
        EditorGUILayout.EndHorizontal();

        if (TargetAtlasGameObject == null)
        {
            return;
        }

        TargetAtlas = TargetAtlasGameObject.GetComponent<UIAtlas>();

        if (TargetAtlas == null)
        {
            return;
        }

        NGUIEditorTools.DrawAdvancedSpriteField(TargetAtlas, TargetSpriteName, OnSelectTargetSprite, false);

        EditorGUILayout.BeginHorizontal();
        NeedChangeWidthAndHeight = EditorGUILayout.Toggle("Need Change Size : ", NeedChangeWidthAndHeight);
        Width = EditorGUILayout.IntField("Width : ", Width);
        Height = EditorGUILayout.IntField("Height : ", Height);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Change All UISprite"))
        {
            ChangeAllUISprite();
        }
    }

    void OnSelectOrginAtlas(Object obj)
    {
        OrginAtlas = obj as UIAtlas;
        if (OrginAtlas != null)
        {
            OrginAtlasGameObject = OrginAtlas.gameObject;
        }

        Repaint();
    }

    void OnSelectTargetAtlas(Object obj)
    {
        TargetAtlas = obj as UIAtlas;
        if (TargetAtlas != null)
        {
            TargetAtlasGameObject = TargetAtlas.gameObject;
        }

        Repaint();
    }

    void OnSelectOrginSprite(string spriteName)
    {
        OrginSpriteName = spriteName;

        EditorSpriteName = spriteName;

        Repaint();
    }

    void OnSelectTargetSprite(string spriteName)
    {
        TargetSpriteName = spriteName;

        Repaint();
    }

    void SelectAll()
    {
        SpriteDic.Clear();

        string path = "Assets/BundleData/UI";
        //DirectoryInfo direction = new DirectoryInfo("Assets/BundleData/UI");
        //FileInfo[] files = direction.GetFiles("*", SearchOption.TopDirectoryOnly);
        //for (int i = 0; i < files.Length; i++)
        //{
        //    if (files[i].Name.EndsWith(".meta"))
        //    {
        //        continue;
        //    }
        //    Debug.Log("Name:" + files[i].Name);
        //}

        //Object[] roots = AssetDatabase.LoadAllAssetsAtPath("Assets/BundleData/UI");

        //for (int i = 0; i < roots.Length; ++i)
        //{
        //    Debug.Log(roots[i]);
        //    //GameObject go = roots[i] as GameObject;
        //    //FindSprite(go, go.transform);
        //}

        FindSubFile(path);
    }

    void FindSubFile(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".prefab"))
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(direction + "/" + files[i].Name);
                    if (go != null)
                    {
                        FindSprite(go, go.transform);
                    }
                }
            }

            string[] paths = AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < paths.Length; ++i)
            {
                FindSubFile(paths[i]);
            }
        }
    }

    void FindSprite(GameObject root, Transform t)
    {
        UISprite sp = t.GetComponent<UISprite>();
        if (sp != null)
        {
            if (sp.atlas == OrginAtlas && sp.spriteName == OrginSpriteName)
            {
                if (!SpriteDic.ContainsKey(root))
                {
                    SpriteDic.Add(root, new List<UISprite>());
                }

                SpriteDic[root].Add(sp);
            }

            if (sp.atlas == OrginAtlas)
            {
                UIButton button = t.GetComponent<UIButton>();
                if (button != null)
                {
                    if (button.normalSprite == OrginSpriteName || button.pressedSprite == OrginSpriteName || button.disabledSprite == OrginSpriteName)
                    {
                        if (!ButtonDic.ContainsKey(root))
                        {
                            ButtonDic.Add(root, new List<UIButton>());
                        }

                        ButtonDic[root].Add(button);
                    }
                }
            }
        }


        for (int i = 0; i < t.childCount; ++i)
        {
            FindSprite(root, t.GetChild(i));
        }
    }

    void ChangeAllUISprite()
    {
        var e = SpriteDic.GetEnumerator();
        while (e.MoveNext())
        {
            for (int i = 0; i < e.Current.Value.Count; ++i)
            {
                e.Current.Value[i].atlas = TargetAtlas;
                e.Current.Value[i].spriteName = TargetSpriteName;

                if (NeedChangeWidthAndHeight)
                {
                    e.Current.Value[i].width = Width;
                    e.Current.Value[i].height = Height;
                }
            }

            EditorUtility.SetDirty(e.Current.Key);
        }

        var eb = ButtonDic.GetEnumerator();
        while (eb.MoveNext())
        {
            for (int i = 0; i < eb.Current.Value.Count; ++i)
            {
                UIButton button = eb.Current.Value[i];
                if (button.normalSprite == OrginSpriteName)
                {
                    button.normalSprite = TargetSpriteName;
                }

                if (button.pressedSprite == OrginSpriteName)
                {
                    button.pressedSprite = TargetSpriteName;
                }


                if (button.disabledSprite == OrginSpriteName)
                {
                    button.disabledSprite = TargetSpriteName;
                }
            }

            EditorUtility.SetDirty(e.Current.Key);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void DrawGroup(GameObject go, List<UISprite> sp)
    {
        if (NGUIEditorTools.DrawHeader(go.name))
        {
            DrawGameObject(go);
            for (int i = 0; i < sp.Count; ++i)
            {
                DrawSprite(sp[i]);
            }
        }
    }

    void DrawGameObject(GameObject go)
    {
        if (GUILayout.Button(go.name, EditorStyles.miniButton, GUILayout.MinWidth(100f)))
        {
            Selection.activeObject = go;
        }
    }

    void DrawSprite(UISprite vSprite)
    {
        if (vSprite == null)
        {
            return;
        }

        EditorGUILayout.LabelField(vSprite.name + " | " + GetPath(vSprite.transform));
    }

    string GetPath(Transform t)
    {
        if (t.parent != null)
        {
            return GetPath(t.transform.parent) + "/" + t.name;
        }
        else
        {
            return t.name;
        }
    }

    void RenameSpriteName()
    {
        EditorGUILayout.BeginHorizontal();

        if (string.IsNullOrEmpty(OrginSpriteName) || string.IsNullOrEmpty(EditorSpriteName))
        {
            CanEditorSprite = false;
        }

        EditorGUILayout.LabelField("Editor Sprite Name : ", GUILayout.MaxWidth(120f));
        CanEditorSprite = EditorGUILayout.Toggle(CanEditorSprite, GUILayout.MaxWidth(30f));
        if (CanEditorSprite)
        {
            EditorSpriteName = EditorGUILayout.TextField(EditorSpriteName);
            if (GUILayout.Button("ReName", GUILayout.MaxWidth(100f)))
            {
                UISpriteData sprite = OrginAtlas.GetSprite(OrginSpriteName);

                if (sprite != null)
                {
                    NGUIEditorTools.RegisterUndo("Edit Sprite Name", OrginAtlas);
                    sprite.name = EditorSpriteName;

                    SelectAll();

                    TargetAtlasGameObject = OrginAtlasGameObject;
                    TargetAtlas = OrginAtlas;
                    TargetSpriteName = EditorSpriteName;

                    ChangeAllUISprite();
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

}