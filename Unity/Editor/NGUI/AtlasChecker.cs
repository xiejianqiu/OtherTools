using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Profiling;
using static UIAtlasMaker;

public class AtlasChecker : EditorWindow
{
    [MenuItem("UIEditor/UIAtlasChecker", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<AtlasChecker>(false, "UIAtlas Checker", true).Show();
    }
    #region 查找UI中使用的图集信息
    private bool bListUIAtlas = false;
    static private string findPath = @"Assets/BundleData/UI";
    static public Dictionary<string, HashSet<UIAtlas>> atlasDict;
    static public HashSet<string> publicAtlas;
    static public HashSet<string> publicAtlasPath;
    static public Dictionary<string, List<string>> mRefPath;
    static public string NumOfFilterAtlas = "2";
    #endregion

    #region------------------图集替换相关-----------
    private bool bSelect = false;
    private string spriteFilter;
    private Object tarAtlasObj;
    private Object srcAtlasObj;
    private Dictionary<GameObject, List<UISprite>> atlasSpDict;
    #endregion
    #region 查找图集被哪些资源引用
    private Dictionary<GameObject, List<UISprite>> atlasUISpDict;
    private bool bUseAtlasUI = false;
    private bool bUseFilter = false;
    #endregion
    #region 查找当个UI用到的图集信息
    Dictionary<GameObject, Dictionary<UIAtlas, List<UISprite>>> mUIAtlasSpDic;
    HashSet<UIAtlas> allAtlas;
    private bool bListUIAtlasInfo = false;
    private Object selectUIObj;
    #endregion
    private void OnGUI()
    {
        InitPublic();
        if (GUILayout.Button("刷新")) {
            AssetDatabase.Refresh();
        }
        bListUIAtlas = GUILayout.Toggle(bListUIAtlas, "UI引用图集查找");
        #region ==========UI引用多图集查找===================
        if (bListUIAtlas)
        {
            if (GUILayout.Button("开始检查"))
            {
                atlasDict = new Dictionary<string, HashSet<UIAtlas>>();
                mRefPath = new Dictionary<string, List<string>>();
             
                var guids = AssetDatabase.FindAssets("t:prefab", new string[] { findPath });
                int nCount = 0;
                foreach (var guid in guids)
                {
                    nCount++;
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    HashSet<UIAtlas> atlasSet = null;
                    if (!atlasDict.TryGetValue(assetPath, out atlasSet))
                    {
                        atlasSet = new HashSet<UIAtlas>();
                        atlasDict[assetPath] = atlasSet;
                    }
                    EditorUtility.DisplayCancelableProgressBar("提示", assetPath, nCount * 1f / guids.Length);
                    var allDepAssets = EditorUtility.CollectDependencies(new Object[] { asset });
                    foreach (var dep in allDepAssets)
                    {
                        if (dep is UIAtlas atlas)
                        {
                            if (!publicAtlas.Contains(atlas.name))
                            {
                                atlasSet.Add(atlas);
                            }
                        }
                    }
                    List<string> list = null;
                    if (!mRefPath.TryGetValue(assetPath, out list))
                    {
                        list = new List<string>();
                        mRefPath[assetPath] = list;
                    }
                    ///输出sprite使用的图集名称
                    GameObject go = GameObject.Instantiate(asset);
                    var sps = go.GetComponentsInChildren<UISprite>(true);
                    foreach (var sp in sps)
                    {
                        if (null != sp.atlas && atlasSet.Contains(sp.atlas))
                        {
                            list.Add($"({sp.atlas.name}){sp.spriteName} {sp.name}   {(null == sp.atlas.GetSprite(sp.spriteName) ? "丢失" : "")}");
                        }
                    }
                    var allDependency = AssetDatabase.GetDependencies(assetPath);
                    foreach (var path in allDependency)
                    {
                        if (null != path && !publicAtlasPath.Contains(path) && path != assetPath && path.EndsWith(".prefab"))
                        {
                            list.Add($"{path}");
                        }
                    }
                    GameObject.DestroyImmediate(go);
                }
                EditorUtility.ClearProgressBar();
                Resources.UnloadUnusedAssets();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.ExpandWidth(true)))
            {
                int curNum = int.Parse(NumOfFilterAtlas);
                curNum -= 1;
                if (curNum < 0)
                {
                    curNum = 0;
                }
                NumOfFilterAtlas = curNum.ToString();
            }
            GUI.enabled = false;
            NumOfFilterAtlas = GUILayout.TextField(NumOfFilterAtlas);
            GUI.enabled = true;
            if (GUILayout.Button("+", GUILayout.ExpandWidth(true)))
            {
                int curNum = int.Parse(NumOfFilterAtlas);
                curNum += 1;
                NumOfFilterAtlas = curNum.ToString();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("导出文件"))
            {
                List<KeyValuePair<string, HashSet<UIAtlas>>> atalsList = new List<KeyValuePair<string, HashSet<UIAtlas>>>(atlasDict);
                atalsList.Sort((a, b) =>
                {
                    return a.Value.Count - b.Value.Count;
                });

                var iter = atalsList.GetEnumerator();
                int nCount = int.Parse(NumOfFilterAtlas);
                int index = 0;
                string filePath = GetFilePath("ui_atlas_info.txt");
                using (var fileStream = File.OpenWrite(filePath))
                {
                    StreamWriter fileWriter = new StreamWriter(fileStream);
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在写入文件...", index * 1f / atlasDict.Count);
                        if (iter.Current.Value.Count >= nCount)
                        {
                            fileWriter.WriteLine($"{iter.Current.Key} ####");
                            foreach (var atlas in iter.Current.Value)
                            {
                                fileWriter.WriteLine($"{atlas.name} {GetMemSize(atlas.texture)}");
                            }
                            //--------打印引用图集的组件-------------
                            if (mRefPath.TryGetValue(iter.Current.Key, out var list))
                            {
                                foreach (var info in list)
                                {
                                    fileWriter.WriteLine(info);
                                }
                            }
                            fileWriter.WriteLine();
                            fileWriter.Flush();
                        }
                        if (index == atlasDict.Count)
                        {
                            System.Diagnostics.Process.Start("notepad.exe", filePath);
                        }
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
            if (GUILayout.Button("打印图集"))
            {
                HashSet<UIAtlas> atlasSet = new HashSet<UIAtlas>();
                var iter = atlasDict.GetEnumerator();
                while (iter.MoveNext())
                {
                    foreach (var atlas in iter.Current.Value)
                    {
                        if (!atlasSet.Contains(atlas))
                        {
                            atlasSet.Add(atlas);
                        }
                    }
                }

                List<UIAtlas> sortList = new List<UIAtlas>(atlasSet);
                sortList.Sort((a, b)=> {
                    return b.texture.width - a.texture.width;
                });

                string filePath = GetFilePath("use_atlas");
                using (var fs = File.OpenWrite(filePath))
                {
                    StreamWriter fileWriter = new StreamWriter(fs);
                    int index = 0;
                    foreach (var atlas in sortList)
                    {
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在写入文件...", ++index * 1f / atlasDict.Count);
                        long curAtlasSize = Profiler.GetRuntimeMemorySizeLong(atlas.texture);
                        fileWriter.WriteLine($"{atlas.name} {EditorUtility.FormatBytes(curAtlasSize)}");
                    }
                    fileWriter.Flush();
                    EditorUtility.ClearProgressBar();
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }

            GUILayout.Space(10);
        }
        #endregion
        bSelect = GUILayout.Toggle(bSelect, "Sprite查找与图集替换");
        #region 引用指定图集的sprite查找与图集替换
        if (bSelect)
        {
            bUseFilter = GUILayout.Toggle(bUseFilter, "使用名称过滤"); ;
            GUILayout.BeginHorizontal();
            GUILayout.Label("目标图集", GUILayout.ExpandWidth(false));
            tarAtlasObj = EditorGUILayout.ObjectField(tarAtlasObj, typeof(UIAtlas), false);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("原图集", GUILayout.ExpandWidth(false));
            srcAtlasObj = EditorGUILayout.ObjectField(srcAtlasObj, typeof(UIAtlas), false);
            GUILayout.EndHorizontal();
            if (bUseFilter)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("sp开头:", GUILayout.ExpandWidth(false));
                spriteFilter = GUILayout.TextField(spriteFilter, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                if (bUseFilter && null != srcAtlasObj && !string.IsNullOrEmpty(spriteFilter) && GUILayout.Button($"查找使用图集{srcAtlasObj.name},且sp名称以{spriteFilter}开头", GUILayout.MinWidth(40)))
                {
                    atlasSpDict = new Dictionary<GameObject, List<UISprite>>();
                    var guids = AssetDatabase.FindAssets("t:prefab", new string[] { findPath });
                    int index = 0;
                    foreach (var guid in guids)
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在查找...", index * 1f / guids.Length);
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var sprites = asset.GetComponentsInChildren<UISprite>(true);
                        if (!atlasSpDict.TryGetValue(asset, out var list))
                        {
                            list = new List<UISprite>();
                            atlasSpDict[asset] = list;
                        }
                        foreach (var sp in sprites)
                        {
                            if (null != sp.atlas && Object.ReferenceEquals(sp.atlas, srcAtlasObj))
                            {
                                if (sp.spriteName.StartsWith(spriteFilter))
                                {
                                    list.Add(sp);
                                }
                            }

                        }
                    }
                    EditorUtility.ClearProgressBar();
                    Resources.UnloadUnusedAssets();
                }

                if (bUseFilter && null != tarAtlasObj && null != atlasSpDict && atlasSpDict.Count > 0 && !string.IsNullOrEmpty(spriteFilter) && GUILayout.Button($"图集替换{srcAtlasObj.name}->{tarAtlasObj.name}", GUILayout.MinWidth(40)))
                {
                    UIAtlas tarAtlas = tarAtlasObj as UIAtlas;
                    if (null == tarAtlas)
                    {
                        Debug.LogError("，目标图集为空！！");
                        return;
                    }
                    int index = 0;
                    var iter = atlasSpDict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在替换...", index * 1f / atlasSpDict.Count);
                        bool bChange = false;
                        foreach (var cmp in iter.Current.Value)
                        {
                            if (null != cmp.atlas && Object.ReferenceEquals(cmp.atlas, srcAtlasObj))
                            {
                                if (cmp.spriteName.StartsWith(spriteFilter))
                                {
                                    if (null != tarAtlas.GetSprite(cmp.spriteName))
                                    {
                                        cmp.atlas = tarAtlas;
                                        bChange = true;
                                    }
                                    else
                                    {
                                        Debug.LogError($"{tarAtlas.name} 不包含 {cmp.spriteName}");
                                    }
                                }
                            }
                        }
                        if (bChange)
                        {
                            EditorUtility.SetDirty(iter.Current.Key);
                        }
                    }
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.SaveAssets();
                    Resources.UnloadUnusedAssets();
                    EditorUtility.ClearProgressBar();
                }
                if (null != tarAtlasObj && !string.IsNullOrEmpty(spriteFilter) && GUILayout.Button($"{spriteFilter}系sprite的图集{srcAtlasObj.name},重置图集为{tarAtlasObj.name}")) {
                    UIAtlas tarAtlas = tarAtlasObj as UIAtlas;
                    if (null == tarAtlas)
                    {
                        Debug.LogError("，目标图集为空！！");
                        return;
                    }
                    int index = 0;
                    var iter = atlasSpDict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在替换...", index * 1f / atlasSpDict.Count);
                        bool bChange = false;
                        foreach (var cmp in iter.Current.Value)
                        {
                            if (null != cmp.atlas && Object.ReferenceEquals(cmp.atlas, srcAtlasObj))
                            {
                                if (cmp.spriteName.StartsWith(spriteFilter) && null == cmp.atlas.GetSprite(cmp.spriteName))
                                {
                                    cmp.atlas = tarAtlas;
                                    bChange = true;
                                }
                            }
                        }
                        if (bChange)
                        {
                            EditorUtility.SetDirty(iter.Current.Key);
                        }
                    }
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.SaveAssets();
                    Resources.UnloadUnusedAssets();
                    EditorUtility.ClearProgressBar();
                }

                //if (bUseFilter && null != srcAtlasObj && !string.IsNullOrEmpty(spriteFilter) && GUILayout.Button($"***{srcAtlasObj.name}图集去除{spriteFilter}开头的sprite***", GUILayout.MinWidth(40)))
                //{
                //    List<SpriteEntry> sprites = new List<SpriteEntry>();
                //    UIAtlasMaker.ExtractSprites(srcAtlasObj as UIAtlas, sprites);
                //    List<string> mDelNames = new List<string>();
                //    foreach (var se in sprites)
                //    {
                //        if (se.name.StartsWith(spriteFilter))
                //        {
                //            mDelNames.Add(se.name);
                //        }
                //    }
                //    for (int i = sprites.Count; i > 0;)
                //    {
                //        SpriteEntry ent = sprites[--i];
                //        if (mDelNames.Contains(ent.name))
                //            sprites.RemoveAt(i);
                //    }

                //    UIAtlasMaker.UpdateAtlas(srcAtlasObj as UIAtlas, sprites);
                //    mDelNames.Clear();
                //    NGUIEditorTools.RepaintSprites();
                //    EditorUtility.SetDirty(srcAtlasObj);
                //    AssetDatabase.SaveAssets();
                //    AssetDatabase.Refresh();
                //}

            }
            else {
                if (null != srcAtlasObj && GUILayout.Button($"查找选中的sprite使用{srcAtlasObj.name}的图集", GUILayout.MinWidth(40)))
                {
                    UIAtlas srcAtlas = srcAtlasObj as UIAtlas;
                    var objs = Selection.GetFiltered<Texture>(SelectionMode.Assets);
                    HashSet<string> nameSet = new HashSet<string>();
                    foreach (var obj in objs)
                    {
                        nameSet.Add(obj.name);
                    }

                    atlasSpDict = new Dictionary<GameObject, List<UISprite>>();
                    var guids = AssetDatabase.FindAssets("t:prefab", new string[] { findPath });
                    int index = 0;
                    foreach (var guid in guids)
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在查找...", index * 1f / guids.Length);
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var sprites = asset.GetComponentsInChildren<UISprite>(true);
                        if (!atlasSpDict.TryGetValue(asset, out var list))
                        {
                            list = new List<UISprite>();
                            atlasSpDict[asset] = list;
                        }
                        foreach (var sp in sprites)
                        {
                            if (null != sp.atlas && object.ReferenceEquals(sp.atlas, srcAtlas) && nameSet.Contains(sp.spriteName))
                            {
                                list.Add(sp);
                            }

                        }
                    }
                    EditorUtility.ClearProgressBar();
                    Resources.UnloadUnusedAssets();
                }
                if (null!=srcAtlasObj && null != tarAtlasObj && GUILayout.Button($"图集替换{srcAtlasObj.name}->{tarAtlasObj.name} 替换选中的图标"))
                {

                    UIAtlas tarAtlas = tarAtlasObj as UIAtlas;
                    if (null == tarAtlas)
                    {
                        Debug.LogError("，目标图集为空！！");
                        return;
                    }
                    var objs = Selection.GetFiltered<Texture>(SelectionMode.Assets);
                    HashSet<string> nameSet = new HashSet<string>();
                    foreach (var obj in objs)
                    {
                        nameSet.Add(obj.name);
                    }
                    int index = 0;
                    var iter = atlasSpDict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在替换...", index * 1f / atlasSpDict.Count);
                        bool bChange = false;
                        foreach (var cmp in iter.Current.Value)
                        {
                            if (null != cmp.atlas && Object.ReferenceEquals(cmp.atlas, srcAtlasObj))
                            {
                                if (nameSet.Contains(cmp.spriteName))
                                {
                                    if (null != tarAtlas.GetSprite(cmp.spriteName))
                                    {
                                        cmp.atlas = tarAtlas;
                                        bChange = true;
                                    }
                                    else
                                    {
                                        Debug.LogError($"{tarAtlas.name} 不包含 {cmp.spriteName}");
                                    }
                                }
                            }
                        }
                        if (bChange)
                        {
                            EditorUtility.SetDirty(iter.Current.Key);
                        }
                    }
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.SaveAssets();
                    Resources.UnloadUnusedAssets();
                    EditorUtility.ClearProgressBar();
                }

            }
            if (null != atlasSpDict && atlasSpDict.Count > 0 && GUILayout.Button("导出到文件", GUILayout.MinWidth(40)))
            {
                string filePath = GetFilePath($"FF_{srcAtlasObj.name}_{spriteFilter}");
                int index = 0;
                using (var fs = File.OpenWrite(filePath))
                {
                    StreamWriter fileWriter = new StreamWriter(fs);
                    var iter = atlasSpDict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在输出...", index * 1f / atlasSpDict.Count);
                        if (iter.Current.Value.Count <= 0)
                        {
                            continue;
                        }
                        fileWriter.WriteLine($"{AssetDatabase.GetAssetPath(iter.Current.Key)}");
                        foreach (var sp in iter.Current.Value)
                        {
                            fileWriter.WriteLine($"{sp.name}  {sp.spriteName}({sp.atlas.name}) {(null == sp.atlas.GetSprite(sp.spriteName) ? "丢失" : "")}");
                        }
                        fileWriter.WriteLine();
                    }
                    fileWriter.Flush();
                    EditorUtility.ClearProgressBar();
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }
        }
        #endregion
        bUseAtlasUI = GUILayout.Toggle(bUseAtlasUI, "引用该图集的UI");
        #region 找出使用该图集的UI
        if (bUseAtlasUI)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("目标图集", GUILayout.ExpandWidth(false));
            srcAtlasObj = EditorGUILayout.ObjectField(srcAtlasObj, typeof(UIAtlas), false);
            GUILayout.EndHorizontal();
            if (null != srcAtlasObj && GUILayout.Button("开始查找"))
            {
                atlasUISpDict = new Dictionary<GameObject, List<UISprite>>();
                var guids = AssetDatabase.FindAssets("t:prefab", new string[] { findPath });
                int index = 0;
                foreach (var guid in guids)
                {
                    index++;
                    EditorUtility.DisplayCancelableProgressBar("提示", "正在查找...", index * 1f / guids.Length);
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var sprites = asset.GetComponentsInChildren<UISprite>(true);

                    foreach (var sp in sprites)
                    {
                        if (null != sp.atlas && Object.ReferenceEquals(sp.atlas, srcAtlasObj))
                        {
                            if (!atlasUISpDict.TryGetValue(asset, out var list))
                            {
                                list = new List<UISprite>();
                                atlasUISpDict[asset] = list;
                            }
                            list.Add(sp);
                        }

                    }
                }
                EditorUtility.ClearProgressBar();
                Resources.UnloadUnusedAssets();
            }
            if (null != atlasUISpDict && atlasUISpDict.Count > 0 && GUILayout.Button("导出到文件", GUILayout.MinWidth(40)))
            {
                string filePath = GetFilePath($"USE_{srcAtlasObj.name}");
                int index = 0;
                using (var fs = File.OpenWrite(filePath))
                {
                    StreamWriter fileWriter = new StreamWriter(fs);
                    var iter = atlasUISpDict.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在输出...", index * 1f / atlasUISpDict.Count);
                        if (iter.Current.Value.Count <= 0)
                        {
                            continue;
                        }
                        fileWriter.WriteLine($"{AssetDatabase.GetAssetPath(iter.Current.Key)}");
                        foreach (var sp in iter.Current.Value)
                        {
                            fileWriter.WriteLine($"{sp.name}  {sp.spriteName}({sp.atlas.name}) {(null == sp.atlas.GetSprite(sp.spriteName) ? "丢失" : "")}");
                        }
                        fileWriter.WriteLine();
                    }
                    fileWriter.Flush();
                    EditorUtility.ClearProgressBar();
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }
        }
        #endregion

        #region 查找当个UI用到的图集信息
        bListUIAtlasInfo = GUILayout.Toggle(bListUIAtlasInfo, "查找UI使用的图集详细信息");
        if (bListUIAtlasInfo)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("目标图集", GUILayout.ExpandWidth(false));
            selectUIObj = EditorGUILayout.ObjectField(selectUIObj, typeof(GameObject), false);
            GUILayout.EndHorizontal();
            if (null != selectUIObj && GUILayout.Button("开始查找"))
            {
                mUIAtlasSpDic = new Dictionary<GameObject, Dictionary<UIAtlas, List<UISprite>>>();
                allAtlas = new HashSet<UIAtlas>();
                var depsPath = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(selectUIObj));
                int index = 0;
                foreach (var path in depsPath) {
                    index++;
                    EditorUtility.DisplayCancelableProgressBar("提示", "正在查找...", index * 1f / depsPath.Length);
                    if (path.EndsWith(".prefab")) {
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        var sprites = asset.GetComponentsInChildren<UISprite>(true);
                        if (!mUIAtlasSpDic.TryGetValue(asset, out var atlasDict)) {
                            atlasDict = new Dictionary<UIAtlas, List<UISprite>>();
                            mUIAtlasSpDic[asset] = atlasDict;
                        }
                        foreach (var sp in sprites) {
                            if (null != sp.atlas) {
                                if (!atlasDict.TryGetValue(sp.atlas, out var list)) {
                                    list = new List<UISprite>();
                                    atlasDict[sp.atlas] = list;
                                }
                                list.Add(sp);
                            }
                        }
                        var atlas = asset.GetComponent<UIAtlas>();
                        if (null != atlas && !allAtlas.Contains(atlas))
                        {
                            allAtlas.Add(atlas);
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                Resources.UnloadUnusedAssets();
            }
            if (null != mUIAtlasSpDic && mUIAtlasSpDic.Count > 0 && GUILayout.Button("导出到文件", GUILayout.MinWidth(40)))
            {
                string filePath = GetFilePath($"UI_{selectUIObj.name}");
                int index = 0;
                using (var fs = File.OpenWrite(filePath))
                {
                    StreamWriter fileWriter = new StreamWriter(fs);
                    var iter = mUIAtlasSpDic.GetEnumerator();
                    HashSet<UIAtlas> useAtlas = new HashSet<UIAtlas>();//在sprite中使用的图集
                    while (iter.MoveNext())
                    {
                        index++;
                        
                        EditorUtility.DisplayCancelableProgressBar("提示", "正在输出...", index * 1f / mUIAtlasSpDic.Count);
                        if (iter.Current.Value.Count <= 0)
                        {
                            continue;
                        }
                        fileWriter.WriteLine($"{AssetDatabase.GetAssetPath(iter.Current.Key)}");
                        var iter2 = iter.Current.Value.GetEnumerator();
                        while (iter2.MoveNext()) {
                            if (!useAtlas.Contains(iter2.Current.Key))
                            {
                                useAtlas.Add(iter2.Current.Key);
                            }
                            if (publicAtlas.Contains(iter2.Current.Key.name)) {
                                continue;
                            }
                            fileWriter.WriteLine($"{iter2.Current.Key.name} {GetMemSize(iter2.Current.Key.texture)}");
                            foreach (var sp in iter2.Current.Value) {
                                fileWriter.WriteLine($"{iter2.Current.Key.name} {sp.name}({sp.spriteName}) {(null == sp.atlas.GetSprite(sp.spriteName)?"丢失":"")}");
                            }
                        }
                        fileWriter.WriteLine();
                    }
                    fileWriter.WriteLine($"直接引用图集:");
                    foreach (var spAtlas in allAtlas) {
                        if (!useAtlas.Contains(spAtlas) && !publicAtlas.Contains(spAtlas.name)) {
                            fileWriter.WriteLine($"{spAtlas.name}");
                        }
                    }
                    fileWriter.Flush();
                    EditorUtility.ClearProgressBar();
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }
        }
        #endregion
    }
    static void InitPublic() {
        if (null != publicAtlas && null != publicAtlasPath)
            return;
        publicAtlas = new HashSet<string>();
        publicAtlasPath = new HashSet<string>();
        var publicGO = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleData/UI/PreloadLst.prefab");
        if (null != publicGO)
        {
            var attrList = publicGO.GetComponent<AttributeList>();
            if (null != attrList)
            {
                foreach (var t in attrList.m_lstGameObj)
                {
                    var atlas = t.GetComponent<UIAtlas>();
                    if (null != atlas && !publicAtlas.Contains(atlas.name))
                    {
                        publicAtlas.Add(atlas.name);
                        publicAtlasPath.Add(AssetDatabase.GetAssetPath(t));
                    }
                }
            }
        }
    }
    static void DeleteFile(string filePath) {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    static string GetMemSize(Object obj) {
        long curAtlasSize = Profiler.GetRuntimeMemorySizeLong(obj);
        return EditorUtility.FormatBytes(curAtlasSize);
    }
    static string GetFilePath(string fileName) {
        if (!fileName.EndsWith(".txt")) {
            fileName = $"{fileName}.txt";
        }
        string dir = $"{Application.dataPath}/../UIAtlasChecker";
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        var filePath = $"{dir}/{fileName}";
        DeleteFile(filePath);

        return filePath;
    }
}
