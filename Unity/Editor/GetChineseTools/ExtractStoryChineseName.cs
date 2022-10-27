using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Shark;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Text;
using game.story;
namespace game.story
{
    public class MovLabInfo
    {
        public string assetPath;
        public string relativePath;
        public string ctryName;

        public MovLabInfo(string path, string relativePath, string ctryName)
        {
            this.assetPath = path;
            this.relativePath = relativePath;
            this.ctryName = ctryName;
        }
    }
    public class CNAndFanyi
    {
        public string ctryname;
        public string oldfanyi;
        public string fanyi;

        public CNAndFanyi(string ctryname, string oldfanyi, string fanyi)
        {
            this.ctryname = ctryname;
            this.oldfanyi = oldfanyi;
            this.fanyi = fanyi;
        }
    }
}
public class ExtractStoryChineseName:EditorWindow
{
    /// <summary>
    /// mov中boss名称存储位置
    /// </summary>
    
    [MenuItem("Tools/GetChineseTool/StoryBossName", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<ExtractStoryChineseName>(false, "Stroy", true).Show();
    }
    private string[] StoryPath = new string[] { @"Assets/BundleData/Prefab/Story" };
    private const string saveStoryName = @"Assets/Editor/GetChineseTools/prefabStoryName.txt";
    private const string saveStoryOnlyame = @"Assets/Editor/GetChineseTools/prefabStoryOnlyName.txt";
    Regex NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
    private void OnGUI()
    {
        if (GUILayout.Button("名称")) 
        {
            var guids = AssetDatabase.FindAssets("t:prefab", StoryPath);
            int nCount = 0;
            var storyNameLst = new List<string>();
            var nameSet = new HashSet<string>();
            StringBuilder builder = new StringBuilder();
            string[] nameTran = new string[] { 
                "Objs/UI Root/LB_down/MVPanel", "Objs/UI Root/LB_down/MVPanel1", 
                "UI Root/LB_down/MVPanel", "UI Root/LB_down/MVPanel1",
                "Objs/UI Root/MVPanel","Objs/UI Root/MVPanel1"
            };
            foreach (var guid in guids)
            {
                nCount++;
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                EditorUtility.DisplayCancelableProgressBar("提示 ", string.Format(asset.name + " ({0}/{1})", nCount * 1f, guids.Length), nCount * 1f / guids.Length);
                foreach (var relative in nameTran)
                {
                    var tmpTr = asset.transform.Find(relative);
                    if (null != tmpTr)
                    {
                        builder.Clear();
                        var labLst = tmpTr.GetComponentsInChildren<UILabel>(true);
                        foreach (var lab in labLst)
                        {
                            if (lab.enabled && lab.gameObject.activeSelf && NumReg.IsMatch(lab.text))
                            {
                                builder.Append(lab.text);
                            }
                        }
                        if (builder.Length > 0)
                        {
                            var tr = tmpTr;
                            string parentPath = tr.name;
                            tr = tr.parent;
                            while (null != tr.parent && tr.parent != asset)
                            {
                                parentPath = tr.name + @"/" + parentPath;
                                tr = tr.parent;
                            }
                            nameSet.Add(builder.ToString());
                            storyNameLst.Add(assetPath + "\t" + parentPath + "\t" + builder.ToString());
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            if (File.Exists(saveStoryName))
            {
                File.Delete(saveStoryName);
            }
            using (var fs = File.Open(saveStoryName, FileMode.CreateNew)) {
                StreamWriter writer = new StreamWriter(fs);
                foreach (var name in storyNameLst) {
                    if (!string.IsNullOrEmpty(name))
                    {
                        writer.WriteLine(name);
                    }
                }
                writer.Flush();
            }
            if (File.Exists(saveStoryOnlyame))
            {
                File.Delete(saveStoryOnlyame);
            }
            using (var fs = File.Open(saveStoryOnlyame, FileMode.CreateNew))
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
        GUILayout.Space(25);
        if (GUILayout.Button("翻译")) {
            List<MovLabInfo> movLabLst = new List<MovLabInfo>();
            var content = File.ReadAllLines(saveStoryName);
            foreach (var line in content) {
                if (!string.IsNullOrEmpty(line)) {
                    var array = line.Split('\t');
                    if (array.Length < 3)
                    {
                        Debug.Log($"格式错误!!!!  {line}");
                        continue;
                    }
                    movLabLst.Add(new MovLabInfo(array[0], array[1], array[2]));
                }
            }
            Dictionary<string,CNAndFanyi> cnFanyiLst = new Dictionary<string, CNAndFanyi>();
            content = File.ReadAllLines(saveStoryOnlyame);
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
            foreach (var info in movLabLst) {
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
                    var labArray = objTr.GetComponentsInChildren<UILabel>();
                    if (labArray.Length > 0) {
                        bool IsFanYi = false;
                        foreach (var lab in labArray) 
                        {
                            if (!IsFanYi && lab.enabled && lab.gameObject.activeSelf)
                            {
                                if (lab.text != fanyiInfo.fanyi)
                                {
                                    lab.text = fanyiInfo.fanyi;
                                    lab.overflowMethod = UILabel.Overflow.ResizeHeight;
                                    lab.alignment = NGUIText.Alignment.Center;
                                    lab.width = 300;
                                    lab.fontSize = 36;
                                    IsFanYi = true;
                                    var verPos = lab.transform.localPosition;
                                    verPos.x = 60;
                                    lab.transform.localPosition = verPos;
                                }
                            }
                            else
                            {
                                lab.enabled = false;
                            }
                        }
                        if (IsFanYi)
                        {
                            EditorUtility.SetDirty(asset);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            GC.Collect();
        }
    }
}
