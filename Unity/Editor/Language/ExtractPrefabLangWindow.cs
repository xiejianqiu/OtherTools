using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using LitJson;
using Shark;
using System.Drawing;

namespace Language
{
    public class ExtractPrefabLangWindow: EditorWindow
    {
        private Regex m_NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
        [MenuItem("Tools/多语言/2.提取Prefab中文", priority = 2)]
        static void Open()
        {
            GetWindow<ExtractPrefabLangWindow>("提前中文", true);
        }
        private string _prefabFile;
        string prefabFile {
            get {
                if (string.IsNullOrEmpty(_prefabFile))
                {
                    _prefabFile = Application.dataPath + "/BundleData/Tables/PrefabLangInfo.txt";
                }
                return _prefabFile;
            }
        }
        private float btnHeight = 40;
        private void OnGUI()
        {
            if (GUILayout.Button("开始提取", GUILayout.Height(btnHeight)))
            {
                try
                {
                    Dictionary<string, PrefabLangInfo> uiDictInfo = new Dictionary<string, PrefabLangInfo>();
                    string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] {
                        "Assets/BundleData/UI",
                        "Assets/BundleData/Prefab/Effect",
                        "Assets/BundleData/Prefab/Story",
                        "Assets/Resources/Prefab"
                    });
                    int iCount = 0;
                    foreach (var guid in guids)
                    {
                        iCount += 1;
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var assetGo = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var labels = assetGo.GetComponentsInChildren<UILabel>(true);
                        EditorUtility.DisplayProgressBar("提示", assetPath, iCount * 1f / guids.Length);
                        if (labels.Length <= 0)
                        {
                            assetGo = null;
                            continue;
                        }
                        foreach (var lbl in labels)
                        {
                            bool hasChinese = false;
                            if (lbl.mTextID <= 0 && IsContainChinese(lbl.text))
                            {
                                hasChinese = true;
                            }
                            if (!hasChinese)
                                continue;
                            if (!uiDictInfo.ContainsKey(assetPath))
                            {
                                uiDictInfo.Add(assetPath, new PrefabLangInfo(assetGo.name, assetPath));
                            }
                            var info = uiDictInfo[assetPath];
                            info.AddLbl(lbl);
                        }
                    }
                    List<PrefabLangInfo> infoLst = new List<PrefabLangInfo>();
                    infoLst.AddRange(uiDictInfo.Values);
                    var json = JsonMapper.ToJson(infoLst);
                    var utfEndoding = new UTF8Encoding(false);
                    File.WriteAllText(prefabFile, json, utfEndoding);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            if (GUILayout.Button("测试反序列化", GUILayout.Height(btnHeight)))
            {
                if (!File.Exists(prefabFile))
                {
                    Debug.LogError($"{prefabFile} 不存在");
                    return;
                }
                var utfEndoding = new UTF8Encoding(false);
                var content = File.ReadAllText(prefabFile, utfEndoding);
                var info = JsonMapper.ToObject<PrefabLangInfo[]>(content);
            }
            if (GUILayout.Button("预制物体挂LangMono脚本", GUILayout.Height(btnHeight)))
            {
                var utfEndoding = new UTF8Encoding(false);
                var content = File.ReadAllText(prefabFile, utfEndoding);
                var prefabLangInfoArray = JsonMapper.ToObject<PrefabLangInfo[]>(content);
                int iCount = 0;
                foreach (var info in prefabLangInfoArray)
                {
                    iCount += 1;
                    EditorUtility.DisplayProgressBar("提示", info.assetPath, iCount * 1f / prefabLangInfoArray.Length);
                    if (info.lblLst.Count > 0)
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(info.assetPath);
                        if (null != go)
                        {
                            bool NeedSave = false;
                            LangMono mono = go.GetComponent<LangMono>();
                            if (null == mono)
                            {
                                mono = go.AddComponent<LangMono>();
                                NeedSave = true;
                            }
                            if (mono.SaveAssetPath())
                            {
                                NeedSave = true;
                            }
                            if (NeedSave)
                            {
                                EditorUtility.SetDirty(go);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
        /// <summary>
        /// 判断文本中是否包含中文
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private bool IsContainChinese(string txt)
        {
            return m_NumReg.IsMatch(txt);
        }
        
    }
}
