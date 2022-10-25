using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GameFramework;

public class ABBuildPanel : EditorWindow
{

    [MenuItem("ABSystem/Builder Panel")]
    static void Open()
    {
        GetWindow<ABBuildPanel>("ABSystem", true);
    }

    [MenuItem("ABSystem/Builde AssetBundles")]
    public static void BuildAssetBundles()
    {
        BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget,false);
    }

    public static void BuildAssetBundles(BuildTarget target, bool bIncludeScene = true,bool isBwbx = false)
    {
        AssetBundleBuildConfig config = LoadAssetAtPath<AssetBundleBuildConfig>(savePath);

        if (config == null)
            return;

        ABBuilder builder = new AssetBundleBuilder(new AssetBundlePathHelper());
              
        builder.CurTarget = target;


        builder.Begin();

        for (int i = 0; i < config.filters.Count; i++)
        {
            AssetBundleFilter f = config.filters[i];
            if (f.Valid)
                builder.AddRootTargets(new DirectoryInfo(f.Path),f.BundleStrategy,  f.Filter );
        }
#if USE_HOTFIX
        builder.AddRootTargets(new DirectoryInfo("Assets/BundleData/dll"),AssetBundleBuildConfig.BundleStrategy.Normal ,"*.bytes");
#endif
        if (bIncludeScene)
        {
            for (int i = 2; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (!EditorBuildSettings.scenes[i].enabled) continue;
                string scene = EditorBuildSettings.scenes[i].path;
                string scenePath = scene.Substring(0, scene.LastIndexOf("/"));
                string sceneName = scene.Substring(scene.LastIndexOf("/") + 1, scene.Length - scenePath.Length - 1);
                builder.AddRootTargets(new DirectoryInfo(scenePath), AssetBundleBuildConfig.BundleStrategy.Normal, sceneName);
            }
        }
        List<AssetTarget> lstRes = builder.Export();

        //List<AssetTarget> lstScene = null;
        //if (bIncludeScene)
        //{
        //    builder.Begin();

        //    for (int i = 2; i < EditorBuildSettings.scenes.Length; i++)
        //    {
        //        if (!EditorBuildSettings.scenes[i].enabled) continue;//没有勾选的话则返回,by ydz
        //        string scene = EditorBuildSettings.scenes[i].path;
        //        string scenePath = scene.Substring(0, scene.LastIndexOf("/"));
        //        string sceneName = scene.Substring(scene.LastIndexOf("/") + 1, scene.Length - scenePath.Length - 1);
        //        builder.AddRootTargets(new DirectoryInfo(scenePath), AssetBundleBuildConfig.BundleStrategy.Normal, sceneName);
        //    }
        //    lstScene = builder.Export();
        //}

        //if (lstScene != null)
        //{
        //    Dictionary<string, int> vDicRes = new Dictionary<string, int>();
        //    for (int i = 0; i < lstRes.Count; i++)
        //    {
        //        vDicRes.Add(lstRes[i].assetPath, i);
        //    }
        //    List<AssetTarget> lstRemoveRes = new List<AssetTarget>();
        //    List<AssetTarget> lstRemoveScene = new List<AssetTarget>();
        //    for (int i = 0; i < lstScene.Count; i++)
        //    {
        //        int nIdx = -1;
        //        if (vDicRes.TryGetValue(lstScene[i].assetPath, out nIdx))
        //        {
        //            if (!lstRes[nIdx].needSelfExport || lstScene[i].needSelfExport)
        //            {
        //                lstRemoveRes.Add(lstRes[nIdx]);
        //            }
        //            else
        //            {
        //                lstRemoveScene.Add(lstScene[i]);
        //            }

        //        }
        //    }
        //    for (int i = 0; i < lstRemoveRes.Count; i++)
        //    {
        //        lstRes.Remove(lstRemoveRes[i]);
        //    }
        //    for (int i = 0; i < lstRemoveScene.Count; i++)
        //    {
        //        lstScene.Remove(lstRemoveScene[i]);
        //    }
        //    lstRes.AddRange(lstScene);
        //}

        builder.WriteFile(lstRes);
        
        builder.End();

        RemoveAllMainfest();

        //if (isBwbx)
        //{
        //    BwBxResSplitTools.SplitRes(false);
        //}
    }

    static void RemoveAllMainfest()
    {
        DirectoryInfo abDir = new DirectoryInfo(Application.streamingAssetsPath+ "/AssetBundles");
        FileInfo[] files = abDir.GetFiles("*manifest*");
        for(int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i].FullName);
        }
    }

	static T LoadAssetAtPath<T>(string path) where T:Object
	{
		return AssetDatabase.LoadAssetAtPath<T>(savePath);
	}

    const string savePath = "Assets/Editor/ABBuilder/config.asset";

    private AssetBundleBuildConfig _config;
    private ReorderableList _list;

    ABBuildPanel()
    {

    }

    void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
    {
        const float GAP = 5;

        AssetBundleFilter filter = _config.filters[index];
        rect.y++;

        Rect r = rect;
        r.width = 16;
        r.height = 18;
        filter.Valid = GUI.Toggle(r, filter.Valid, GUIContent.none);

        r.xMin = r.xMax + GAP;
        r.xMax = rect.xMax - 300;
        GUI.enabled = false;
        filter.Path = GUI.TextField(r, filter.Path);
        GUI.enabled = true;

        r.xMin = r.xMax + GAP;
        r.width = 50;
        if (GUI.Button(r, "Select"))
        {
            string dataPath = Application.dataPath;
            string selectedPath = EditorUtility.OpenFolderPanel("Path", dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(dataPath))
                {
                    filter.Path = "Assets/" + selectedPath.Substring(dataPath.Length + 1);
                }
                else
                {
                    ShowNotification(new GUIContent("不能在Assets目录之外!"));
                }
            }
        }

        r.xMin = r.xMax + GAP;
        r.width = 140;
        filter.Filter = GUI.TextField(r, filter.Filter);


        r.xMin = r.xMax;
        r.xMax = rect.xMax;
        
        filter.BundleStrategy = (AssetBundleBuildConfig.BundleStrategy)EditorGUI.EnumPopup(r,filter.BundleStrategy);

    }

    void OnListHeaderGUI(Rect rect)
    {
        EditorGUI.LabelField(rect, "Asset Filter");
    }

    void OnGUI()
    {
        bool execBuild = false;
        if (_config == null)
        {
            _config = LoadAssetAtPath<AssetBundleBuildConfig>(savePath);
            if (_config == null)
            {
                _config = new AssetBundleBuildConfig();
            }
        }

        if (_list == null)
        {
            _list = new ReorderableList(_config.filters, typeof(AssetBundleFilter));
            _list.drawElementCallback = OnListElementGUI;
            _list.drawHeaderCallback = OnListHeaderGUI;
            _list.draggable = true;
            _list.elementHeight = 22;
        }

        //tool bar
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            if (GUILayout.Button("Add", EditorStyles.toolbarButton))
            {
                _config.filters.Add(new AssetBundleFilter());
            }
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                Save();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build", EditorStyles.toolbarButton))
            {
                execBuild = true;
            }
        }
        GUILayout.EndHorizontal();

        //context
        GUILayout.BeginVertical();

        GUILayout.Space(10);
            
        _list.DoLayoutList();
        GUILayout.EndVertical();

        //set dirty
        if (GUI.changed)
            EditorUtility.SetDirty(_config);

        if (execBuild)
            Build();
    }

    private void Build()
    {
        Save();
        BuildAssetBundles();
    }

    void Save()
    {
        AssetBundlePathHelper.instance = new AssetBundlePathHelper();

        if (LoadAssetAtPath<AssetBundleBuildConfig>(savePath) == null)
        {
            AssetDatabase.CreateAsset(_config, savePath);
        }
        else
        {
            EditorUtility.SetDirty(_config);
        }
    }
}
