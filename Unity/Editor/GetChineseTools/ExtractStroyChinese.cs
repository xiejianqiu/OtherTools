using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using game.story;
using System;

public class ExtractStroyChinese: EditorWindow
{
    [MenuItem("Tools/GetChineseTool/StoryDialog", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<ExtractStroyChinese>(false, "对话内容", true).Show();
    }
    private string[] prePath = new string[] { @"Assets/BundleData/Prefab/Story" };
    private const string saveStoryDlg = @"Assets/Editor/GetChineseTools/prefabStoryDlg.txt";
    private const string saveStoryOnlyDlg = @"Assets/Editor/GetChineseTools/prefabStoryDlgOnly.txt";
    Regex NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
    private void OnGUI()
    {
        if (GUILayout.Button("提取中文"))
        {
            var guids = AssetDatabase.FindAssets("t:prefab", prePath);
            int nCount = 0;
            var storyDialogLst = new List<string>();
            var nameSet = new HashSet<string>();
            try {
                foreach (var guid in guids)
                {
                    nCount++;
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    EditorUtility.DisplayCancelableProgressBar("提示 ", string.Format(asset.name + " ({0}/{1})", nCount * 1f, guids.Length), nCount * 1f / guids.Length);
                    var LabelList = asset.gameObject.GetComponentsInChildren<UILabel>(true);
                    foreach (var lbl in LabelList)
                    {
                        if (string.IsNullOrEmpty(lbl.text)) continue;
                        if (lbl.mTextID > 0) continue;
                        var lblTr = lbl.transform;
                        if (!string.IsNullOrEmpty(lbl.text) && !storyDialogLst.Contains(lbl.text) && NumReg.IsMatch(lbl.text))
                        {
                            string parentPath = lblTr.name;
                            lblTr = lblTr.parent;
                            while (null != lblTr && null != lblTr.parent && lblTr.parent != asset)
                            {
                                parentPath = lblTr.name + @"/" + parentPath;
                                lblTr = lblTr.parent;
                            }
                            if (!parentPath.Contains("MVPanel")) {
                                var lblContent = lbl.text.TrimEnd();
                                nameSet.Add(lblContent);
                                storyDialogLst.Add(assetPath + "\t" + parentPath + "\t" + lblContent);
                            }
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                if (File.Exists(saveStoryDlg))
                {
                    File.Delete(saveStoryDlg);
                }
                using (var fs = File.Open(saveStoryDlg, FileMode.CreateNew))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    foreach (var name in storyDialogLst)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            writer.WriteLine(name);
                        }
                    }
                    writer.Flush();
                }
                if (File.Exists(saveStoryOnlyDlg))
                {
                    File.Delete(saveStoryOnlyDlg);
                }
                using (var fs = File.Open(saveStoryOnlyDlg, FileMode.CreateNew))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    foreach (var name in nameSet)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            writer.WriteLine(name);
                        }
                    }
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        GUILayout.Space(25);
        if (GUILayout.Button("翻译"))
        {
            List<MovLabInfo> movLabLst = new List<MovLabInfo>();
            var content = File.ReadAllLines(saveStoryDlg);
            foreach (var line in content)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var array = line.Split('\t');
                    if (array.Length < 3)
                    {
                        Debug.Log($"格式错误!!!!  {line}");
                        continue;
                    }
                    movLabLst.Add(new MovLabInfo(array[0], array[1], array[2]));
                }
            }
            Dictionary<string, CNAndFanyi> cnFanyiLst = new Dictionary<string, CNAndFanyi>();
            content = File.ReadAllLines(saveStoryOnlyDlg);
            foreach (var line in content)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var array = line.Split('\t');
                    if (array.Length < 3)
                    {
                        Debug.Log($"格式错误!!!! {line}");
                        continue;
                    }
                    cnFanyiLst.Add(array[0], new CNAndFanyi(array[0], array[1], array[2]));
                }
            }
            int nCount = 0;
            foreach (var info in movLabLst)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(info.assetPath);
                nCount += 1;
                EditorUtility.DisplayCancelableProgressBar("提示 ", string.Format(asset.name + " ({0}/{1})", nCount * 1f, movLabLst.Count), nCount * 1f / movLabLst.Count);
                CNAndFanyi fanyiInfo = cnFanyiLst[info.ctryName];
                if (null == fanyiInfo)
                {
                    Debug.Log($"[{info.ctryName}] 找不到翻译");
                    continue;
                }
                var objTr = asset.transform.Find(info.relativePath);
                if (null != objTr)
                {
                    bool IsFanYi = false;
                    var lbl = objTr.GetComponent<UILabel>();
                    if (null != lbl && lbl.text != fanyiInfo.fanyi)
                    {
                        lbl.text = fanyiInfo.fanyi;
                        IsFanYi = true;
                    }
                    else 
                    {
                        Debug.Log($"{asset.name}/{info.relativePath}  label not find.");
                    }
                    if (IsFanYi)
                    {
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            System.GC.Collect();
        }
    }
}
