using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Shark;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class ExtractChineseTool : EditorWindow
{
    [MenuItem("Tools/GetChineseTool/Tool", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<ExtractChineseTool>(false, "Check Prefab Chinese", true).Show();
    }
    private bool bExtractChinese = false;
    private string searchPath = @"Assets/BundleData/UI";
    /// <summary>
    /// 定义要提取中文的路径
    /// </summary>
    private string[] PrefabPathArray = new string[] { @"Assets/BundleData/UI", @"Assets/BundleData/Prefab/Story", @"Assets/ResMS/Prefab" };
    private string[] MovPathArray = new string[] {@"Assets/BundleData/Prefab/Story"};
    private string[] prePath = new string[] { @"Assets/BundleData/UI" };
    private List<string> extractChineseList;
    private string chinesePath = "";
    Regex NumReg = new Regex("[\u4e00-\u9fa5]+");   //获取中文
    private const string saveContentPath = @"Assets/Editor/GetChineseTools/prefabText.txt";
    private const string newPrefabText = "new_prefabText.txt";
    private const string new_uitext_ui = "new_uitext.txt";
    private bool bRewriteChinese = false;
    private string[] filterStr = new string[] { "UICommonTitle", "SquareItemBig", "SquareItemSmall" , "UICommonRightDiamondTabs" };
    private UnityEngine.Object m_FontObj = null;
    private bool bMergePrefabText = false;
    private bool bMergeUIText = false;
    private bool bFilterPrefabTxt = false;
    private bool bDelSpace = false;
    private bool filterObj(GameObject obj)
    {
        if (obj != null)
        {
            for (int i = 0; i < filterStr.Length; i++)
            {
                if (obj.name.Contains(filterStr[i]))
                {
                    return true;
                }
            }
        }
        return false;

    }
    public string ReadTxtTable(string strPath)
    {
        if (!strPath.Contains("UIText.txt")) {
            return "";
        }
        var localPath = Application.dataPath + "/BundleData/Tables/" + strPath;
        string strData = null;
        StreamReader sr = null;
        try
        {
            if (!File.Exists(localPath))
            {
                return null;
            }
            sr = File.OpenText(localPath);
            strData = sr.ReadToEnd();
            sr.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("File Can't find!!!!");
        }
        finally
        {
            if (sr != null)
            {
                sr.Dispose();
            }
        }

        return strData;
    }
    string trunkTxt = string.Empty;
    string trunkUITextTxt = string.Empty;
    private void OnGUI()
    {
        //if (GUILayout.Button("刷新"))
        //{
        //    AssetDatabase.Refresh();
        //}
        bExtractChinese = GUILayout.Toggle(bExtractChinese, "提取中文");
        #region 提取UI总的中文文本
        if (bExtractChinese)
        {
            //GUILayout.BeginHorizontal();
            //GUILayout.Label("选择路径 :", GUILayout.ExpandWidth(false));
            //Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
            ////如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
            //if ((Event.current.type == EventType.DragUpdated
            //  || Event.current.type == EventType.DragExited)
            //  && rect.Contains(Event.current.mousePosition))
            //{
            //    //改变鼠标的外表  
            //    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            //    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            //    {
            //        searchPath = DragAndDrop.paths[0];
            //    }
            //}
            ////searchPath = EditorGUILayout.TextField(searchPath, GUILayout.ExpandWidth(true));
            //searchPath = EditorGUI.TextField(rect, searchPath);
            //GUILayout.EndHorizontal();
            if (GUILayout.Button("开始提取"))
            {
                var guids = AssetDatabase.FindAssets("t:prefab", prePath);
                int nCount = 0;
                extractChineseList = new List<string>();
                foreach (var guid in guids)
                {
                    nCount++;
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (filterObj(asset)) continue;
                    EditorUtility.DisplayCancelableProgressBar("提示 ", string.Format(asset.name + " ({0}/{1})", nCount * 1f, guids.Length), nCount * 1f / guids.Length);
                    //var duiHuaLabel = asset.gameObject.transform.Find("Objs/UI Root/duihua");
                    //if (duiHuaLabel == null)
                    //    continue;
                    var LabelList = asset.gameObject.GetComponentsInChildren<UILabel>(true);
                    foreach (var item in LabelList)
                    {
                        if (string.IsNullOrEmpty(item.text)) continue;
                        if (item.mTextID > 0) continue;
                        if (!string.IsNullOrEmpty(item.text) && !extractChineseList.Contains(item.text))
                        {
                            extractChineseList.Add(item.text);
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
            }
            GUILayout.Space(20);
            if (GUILayout.Button("导出文本"))
            {
                TryLoadUIText();
                if (!File.Exists(saveContentPath)) {
                    File.Create(saveContentPath);
                }
                var oldZh = File.ReadAllLines(saveContentPath);
                HashSet<string> zhSet = new HashSet<string>(oldZh);
                using (FileStream fs = new FileStream(saveContentPath, FileMode.Append))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    int index = 0;
                    foreach (var item in extractChineseList)
                    {
                        index++;
                        EditorUtility.DisplayCancelableProgressBar("提示", string.Format("正在写入文件 ... " + " ({0}/{1})", index * 1f, extractChineseList.Count), index * 1f / extractChineseList.Count);
                        if (NumReg.IsMatch(item) && !zhSet.Contains(item))
                        {
                            List<Tab_UIText> vList = Tab_UITextProvider.Instance.ListData;
                            bool IsContains = false;
                            for (int i = 0; i < vList.Count; i++)
                            {
                                Tab_UIText data = vList[i];
                                if (data.Chinese == item || data.OldText == item || data.Thai == item)
                                {
                                    IsContains = true;
                                    break;
                                }
                            }
                            if (!IsContains)
                            {
                                sw.WriteLine(item);
                            }
                        }
                    }
                    sw.Flush();
                    sw.Close();
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
            GUILayout.Label("导入文本路径位置：" + saveContentPath, GUILayout.ExpandWidth(false));
        }
        #endregion
        GUILayout.Space(30);
        bRewriteChinese = GUILayout.Toggle(bRewriteChinese, "写入Prefab");
        #region 将中文写入文件
        if (bRewriteChinese)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
            //GUILayout.BeginHorizontal();
            //GUILayout.Label("目标路径 :", GUILayout.ExpandWidth(false));
            //searchPath= EditorGUI.TextField(rect, searchPath);
            //GUILayout.EndHorizontal();

            //GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.Label("目标字体预设（非必填） :", GUILayout.ExpandWidth(false));
            Rect Objrect = EditorGUILayout.GetControlRect(GUILayout.Width(200));
            m_FontObj= EditorGUI.ObjectField(Objrect, m_FontObj, typeof(Font), true);
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            //if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited)
            //{
            //    if (rect.Contains(Event.current.mousePosition))
            //    {
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            //        {
            //            searchPath = DragAndDrop.paths[0];
            //        }
            //    }
            //    if (Objrect.Contains(Event.current.mousePosition))
            //    {
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            //        {
            //            m_FontObj = DragAndDrop.objectReferences[0];
            //        }
            //    }
            //    if (rect2.Contains(Event.current.mousePosition))
            //    {
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            //        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            //        {
            //            defaultpath = DragAndDrop.paths[0];
            //        }
            //    }
            //}
            if (Event.current.type == EventType.DragUpdated)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                if (Objrect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
            }

            if (Event.current.type == EventType.DragExited)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        searchPath = DragAndDrop.paths[0];
                    }
                }
                if (Objrect.Contains(Event.current.mousePosition))
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        m_FontObj = DragAndDrop.objectReferences[0];
                    }
                }
            }
            if (GUILayout.Button("翻译UI中文本"))
            {
                TryLoadUIText();
                foreach (var searchPath in PrefabPathArray)
                {
                    var guids = AssetDatabase.FindAssets("t:prefab", new string[] { searchPath });
                    if (Tab_UITextProvider.Instance.GetData().Count <= 0)
                    {
                        Debug.Log(@"本地表未编译，Client\UIText.xls的Txt不存在");
                        return;
                    }
                    int nCount = 0;
                    foreach (var guid in guids)
                    {
                        bool bChange = false;
                        nCount++;
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        if (filterObj(asset)) continue;
                        //GameObject go = GameObject.Instantiate(asset);
                        var labellist = asset.GetComponentsInChildren<UILabel>(true);
                        foreach (var item in labellist)
                        {
                            if (item.trueTypeFont != null && m_FontObj != null && m_FontObj as Font != null)
                            {
                                if (item.trueTypeFont != m_FontObj)
                                {
                                    item.trueTypeFont = m_FontObj as Font;
                                    bChange = true;
                                }
                            }
                            //if (item.bitmapFont != null)  //把所有字体都替换  包括美术字
                            //{
                            //    item.bitmapFont = null;
                            //    item.trueTypeFont = m_FontObj as Font;
                            //    bChange = true;
                            //}
                            if (item.spacingX < -2)
                            {
                                item.spacingX = 0;
                                bChange = true;
                            }
                            if (item.spacingY < -6)
                            {
                                item.spacingY = 0;
                                bChange = true;
                            }
                            //if(item.spacingX<-2)
                            //{
                            //    item.spacingX = 0;
                            //    bChange = true;
                            //}
                            //if (item.spacingY < -6)
                            //{
                            //    item.spacingY = 0;
                            //    bChange = true;
                            //}               
                            if (string.IsNullOrEmpty(item.text)) continue;
                            if (item.mTextID > 0) continue;
                            List<Tab_UIText> vList = Tab_UITextProvider.Instance.ListData;
                            for (int i = 0; i < vList.Count; i++)
                            {
                                Tab_UIText data = vList[i];
                                if ((data.Chinese == item.text || data.OldText == item.text) && (item.text != data.Thai))
                                {
                                    item.text = data.Thai;
                                    bChange = true;
                                }
                            }
                        }
                        if (bChange)
                        {
                            EditorUtility.SetDirty(asset);
                            AssetDatabase.SaveAssets();
                        }
                        string info = string.Format("正在修改预设：{0}/{1}", nCount, guids.Length);
                        EditorUtility.DisplayCancelableProgressBar("提示 ：", info, nCount * 1f / guids.Length);
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    GC.Collect();
                }
            }
        }
        #endregion

        GUILayout.Space(30);
        #region 过滤出位置UIText中的文本
        bFilterPrefabTxt = GUILayout.Toggle(bFilterPrefabTxt, "从prefabText找出不存在与UIText中的文本");
        GUILayout.Label("过滤出的文本存放路径" + newPrefabText, GUILayout.ExpandWidth(false));
        if (bFilterPrefabTxt)
        {
            if (GUILayout.Button("开始过滤"))
            {
                TryLoadUIText();
                HashSet<string> myTxtSet = new HashSet<string>(File.ReadAllLines(saveContentPath));

                var diffFilePath = newPrefabText;
                if (!File.Exists(diffFilePath))
                {
                    File.CreateText(diffFilePath);
                }
                using (FileStream f = File.Open(diffFilePath, FileMode.Append))
                {
                    StreamWriter writer = new StreamWriter(f);
                    int index = 0;
                    foreach (var line in myTxtSet)
                    {
                        index += 1;
                        EditorUtility.DisplayCancelableProgressBar("提示", string.Format("正在合并 ..."), index * 1f / myTxtSet.Count);
                        if (NumReg.IsMatch(line))
                        {
                            List<Tab_UIText> vList = Tab_UITextProvider.Instance.ListData;
                            bool IsContains = false;
                            for (int i = 0; i < vList.Count; i++)
                            {
                                Tab_UIText data = vList[i];
                                if (data.Chinese == line || data.OldText == line || data.Thai == line)
                                {
                                    IsContains = true;
                                    break;
                                }
                            }
                            if (!IsContains)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                    writer.Flush();
                    writer.Close();
                }
                EditorUtility.ClearProgressBar();
                Debug.LogError("文本合并完毕！");
            }
        }
        #endregion
        GUILayout.Space(30);
        #region 合并目标PrefabText内容到项目
        bMergePrefabText = GUILayout.Toggle(bMergePrefabText, "合并主干prefabText新增内容");
        if (bMergePrefabText) {

            if (GUILayout.Button("获取主干Prefab文本"))
            {
                trunkTxt = EditorUtility.OpenFilePanel("trunk", "", "*");
            }
            if (!string.IsNullOrEmpty(trunkTxt)) {
                GUILayout.Label($"trunkTxt:{trunkTxt}");
            }
            if (GUILayout.Button("开始合并"))
            {
                TryLoadUIText();
                HashSet<string> trunkTxtSet = new HashSet<string>(File.ReadAllLines(trunkTxt));
                HashSet<string> myTxtSet = new HashSet<string>(File.ReadAllLines(saveContentPath));

                HashSet<string> diffSet = new HashSet<string>();
                int index = 0;
                foreach (var txt in trunkTxtSet) {
                    index += 1;
                    EditorUtility.DisplayCancelableProgressBar("提示", string.Format("正在检测 ..."), index * 1f / trunkTxtSet.Count);
                    if (!myTxtSet.Contains(txt)) {
                        diffSet.Add(txt);
                    }
                }
                if (!File.Exists(saveContentPath)) {
                    File.CreateText(saveContentPath);
                }
                FileStream f = File.Open(saveContentPath, FileMode.Append);
                using (StreamWriter writer = new StreamWriter(f))
                {
                    index = 0;
                    foreach (var line in diffSet)
                    {
                        index += 1;
                        EditorUtility.DisplayCancelableProgressBar("提示", string.Format("正在合并 ..."), index * 1f / diffSet.Count);
                        if (NumReg.IsMatch(line))
                        {
                            List<Tab_UIText> vList = Tab_UITextProvider.Instance.ListData;
                            bool IsContains = false;
                            for (int i = 0; i < vList.Count; i++)
                            {
                                Tab_UIText data = vList[i];
                                if (data.Chinese == line || data.OldText == line || data.Thai == line)
                                {
                                    IsContains = true;
                                    break;
                                }
                            }
                            if (!IsContains)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                    writer.Flush();
                    writer.Close();
                }
                EditorUtility.ClearProgressBar();
                Debug.LogError("文本合并完毕！");
            }
        }
        #endregion
        GUILayout.Space(30);
        #region 合并目标UIText新增内容到项目
        bMergeUIText = GUILayout.Toggle(bMergeUIText, "合并主干UIText新增内容");
        GUILayout.Label("新增的文本存放路径" + new_uitext_ui, GUILayout.ExpandWidth(false));
        if (bMergeUIText)
        {
            if (GUILayout.Button("获取主干UIText文本"))
            {
                trunkUITextTxt = EditorUtility.OpenFilePanel("trunk", "", "*");
            }
            GUILayout.Label($"trunkTxt:{trunkUITextTxt}");
            if (GUILayout.Button("开始提提取新增中文"))
            {
                TryLoadUIText();
                if (!string.IsNullOrEmpty(trunkUITextTxt))
                {
                    HashSet<string> zhTxtArray = new HashSet<string>(File.ReadAllLines(trunkUITextTxt));
                    HashSet<string> diffSet = new HashSet<string>();
                    int index = 0;
                    foreach (var line in zhTxtArray)
                    {
                        index += 1;
                        EditorUtility.DisplayCancelableProgressBar("提示", string.Format("正在提取新的中文 ..."), index * 1f / zhTxtArray.Count);
                        if (string.IsNullOrEmpty(line))
                            return;
                        bool IsContain = false;
                        List<Tab_UIText> vList = Tab_UITextProvider.Instance.ListData;
                        for (int i = 0; i < vList.Count; i++)
                        {
                            Tab_UIText data = vList[i];
                            if (data.Chinese == line)
                            {
                                IsContain = true;
                                break;
                            }
                        }
                        if (!IsContain)
                        {
                            diffSet.Add(line);
                        }
                    }
                    EditorUtility.ClearProgressBar();
                    var saveUITxtPath = new_uitext_ui;
                    if (File.Exists(saveUITxtPath)) {
                        File.Delete(saveUITxtPath);
                    }
                    File.WriteAllLines(saveUITxtPath, diffSet);
                }
            }
        }
        #endregion

        GUILayout.Space(30);
        bDelSpace = GUILayout.Toggle(bDelSpace, "去掉label字间距");
        if(bDelSpace)
        {
            if (GUILayout.Button("开始执行"))
            {
                var guids = AssetDatabase.FindAssets("t:prefab", prePath);
                int nCount = 0;
                foreach (var guid in guids)
                {
                    bool bChange = false;
                    nCount++;
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (filterObj(asset)) continue;
                    EditorUtility.DisplayCancelableProgressBar("提示 ", string.Format(asset.name + " ({0}/{1})", nCount * 1f, guids.Length), nCount * 1f / guids.Length);
                    var LabelList = asset.GetComponentsInChildren<UILabel>(true);
                    foreach (var item in LabelList)
                    {
                        if (item.bitmapFont != null)
                            continue;
                        if (item.spacingX == 0)
                            continue;
                        if (item.spacingX > 0)
                        {
                            item.spacingX = 0;
                            bChange = true;
                        }

                    }
                    if (bChange)
                    {
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }
                    string info = string.Format("正在修改预设：{0}/{1}", nCount, guids.Length);
                    EditorUtility.DisplayCancelableProgressBar("提示 ：", info, nCount * 1f / guids.Length);
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                GC.Collect();
            }

        }
    }
    private void TryLoadUIText() {
        if (Tab_UITextProvider.Instance.ListData.Count <= 0)
        {
            DataProvider.Instance.Init(new DataProvider.ReadAllTextDelegate(ReadTxtTable));
        }
    }
}
