using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System;

public class AtlasPicChecker : Editor
{
    [MenuItem("UIEditor/图集超限检查", false, 10)]
    static public void BeginCheck()
    {
        string findPath = @"Assets/BundleData/TPProject";
        var guids = AssetDatabase.FindAssets("t:texture", new string[] { findPath });
        StringBuilder builder = new StringBuilder();
        int iCount = 0;
        foreach (var guid in guids) {
            iCount += 1;
            EditorUtility.DisplayProgressBar("图集超限检查", "正在检查中...", iCount * 1f / guids.Length);
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var tex = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid));
            if (tex.width > 2048 || tex.height > 2048) {
                builder.AppendLine(assetPath);
            }
        }
        if (builder.Length > 0)
        {
            Debug.LogError($"超限图集：{builder}");
        }
        EditorUtility.ClearProgressBar();
    }
}
