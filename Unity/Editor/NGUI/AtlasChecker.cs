using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Profiling;

public class AtlasChecker : EditorWindow
{
    [MenuItem("UIEditor/UIAtlasChecker", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<AtlasChecker>(false, "UIAtlas Checker", true).Show();
    }
    static private string findPath = @"Assets/BundleData/UI";
    static public Dictionary<string, HashSet<UIAtlas>> atlasDict;
    static public HashSet<string> publicAtlas;
    static public string NumOfFilterAtlas = "2";
    private void OnGUI()
    {
        if (GUILayout.Button("开始检查"))
        {
            atlasDict = new Dictionary<string, HashSet<UIAtlas>>();
            publicAtlas = new HashSet<string>();
            var publicGO = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleData/UI/PreloadLst.prefab");
            if (null != publicGO) {
                var attrList = publicGO.GetComponent<AttributeList>();
                if (null != attrList) {
                    foreach (var t in attrList.m_lstGameObj) {
                        var atlas = t.GetComponent<UIAtlas>();
                        if (null != atlas && !publicAtlas.Contains(atlas.name))
                        {
                            publicAtlas.Add(atlas.name);
                        }
                    }
                }
            }
            atlasDict.Clear();
            var guids = AssetDatabase.FindAssets("t:prefab", new string[] { findPath });
            int nCount = 0;
            foreach (var guid in guids)
            {
                nCount++;
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                HashSet<UIAtlas> atlasSet = null;
                if (!atlasDict.TryGetValue(assetPath, out atlasSet)) {
                    atlasSet = new HashSet<UIAtlas>();
                    atlasDict[assetPath] = atlasSet;
                }
                EditorUtility.DisplayCancelableProgressBar("提示", assetPath, nCount * 1f / guids.Length);
                var allDepAssets = EditorUtility.CollectDependencies(new Object[] { asset });
                foreach (var dep in allDepAssets) {
                    if (dep is UIAtlas atlas)
                    {
                        if (!publicAtlas.Contains(atlas.name))
                        {
                            atlasSet.Add(atlas);
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
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
        if (GUILayout.Button("导出文件")) {
            var iter = atlasDict.GetEnumerator();
            int nCount = int.Parse(NumOfFilterAtlas);
            int index = 0;
            string filePath = $"{Application.dataPath}/../ui_atlas_info.txt";
            using (var fileStream = File.OpenWrite(filePath)) {
                StreamWriter fileWriter = new StreamWriter(fileStream);
                while (iter.MoveNext())
                {
                    index++;
                    EditorUtility.DisplayCancelableProgressBar("提示","正在写入文件...", index * 1f / atlasDict.Count);
                    if (iter.Current.Value.Count >= nCount)
                    {
                        fileWriter.WriteLine(iter.Current.Key);
                        foreach (var atlas in iter.Current.Value)
                        {
                            long curAtlasSize = Profiler.GetRuntimeMemorySizeLong(atlas.texture);
                            fileWriter.WriteLine($"{atlas.name} {EditorUtility.FormatBytes(curAtlasSize)}");
                        }
                        fileWriter.WriteLine();
                        fileWriter.Flush();
                    }
                }
                EditorUtility.ClearProgressBar();
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
        }
        if (GUILayout.Button("打印图集")) {
            HashSet<UIAtlas> atlasSet = new HashSet<UIAtlas>();
            var iter = atlasDict.GetEnumerator();
            while (iter.MoveNext()) {
                foreach (var atlas in iter.Current.Value) {
                    if (!atlasSet.Contains(atlas)) {
                        atlasSet.Add(atlas);
                    }
                }
            }
            string filePath = $"{Application.dataPath}/../use_atlas.txt";
            using (var fs = File.OpenWrite(filePath)) {
                StreamWriter fileWriter = new StreamWriter(fs);
                int index = 0;
                foreach (var atlas in atlasSet) {
                    EditorUtility.DisplayCancelableProgressBar("提示", "正在写入文件...", ++index * 1f / atlasDict.Count);
                    long curAtlasSize = Profiler.GetRuntimeMemorySizeLong(atlas.texture);
                    fileWriter.WriteLine($"{atlas.name} {EditorUtility.FormatBytes(curAtlasSize)}");
                }
                fileWriter.Flush();
                EditorUtility.ClearProgressBar();
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
        }
    }
}
