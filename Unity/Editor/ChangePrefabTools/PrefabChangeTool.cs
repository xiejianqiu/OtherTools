using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using WellFired;

public class PrefabChangeTool : MonoBehaviour
{
    public static string m_PrefabChangePath = "Assets/Editor/ChangePrefabTools/PrefabChangeData.xml";
    private static Transform m_Child;

    [MenuItem("Tools/ChangePrefab/ChangePrefabStart")]
    public static void StartChange()
    {
        try
        {
            XmlDocument vDoc = LoadData();
            if(vDoc == null)
            {
                Debug.LogError("数据文件不存在：" + m_PrefabChangePath);
                return;
            }

            XmlElement root = vDoc.DocumentElement;
            XmlNodeList elmList = (XmlNodeList)root.SelectNodes("/Items/Item");
            XmlElement theElem;
            foreach (XmlNode xnf in elmList)
            {
                theElem = (XmlElement)xnf;
                if (theElem != null && theElem.InnerText != null)
                {
                    ChangePrefabPosition(theElem);
                }
            }
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
        }
    }


    public static void ChangePrefabPosition(XmlElement vElm)
    {
        try
        {
            string path = vElm.GetAttribute("path");
            string subPath = vElm.GetAttribute("subPath");
            m_Child = null;
            GameObject vOldPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (vOldPrefab == null)
            {
                Debug.LogError("路径有问题");
                return;
            }
            string s = subPath;

            string[] sList = s.Split('/');
            FindChildOfName(sList, vOldPrefab.transform, 1, sList.Length);

            if (m_Child == null)
            {
                Debug.LogError("孩子获取不到");
                return;
            }

            UILabel vLabel = m_Child.GetComponent<UILabel>();
            if(vLabel != null)
            {
                LoadLabel(vLabel, vElm);
            }

            //设置下面的参数要放到最后
            UIWidget vWidget = m_Child.GetComponent<UIWidget>();
            if(vWidget != null)
            {
                LoadWidget(vWidget, vElm);
            }
            UIPanel vPanel = m_Child.GetComponent<UIPanel>();
            if (vPanel != null)
            {
                LoadPanel(vPanel, vElm);
            }

            BoxCollider vBoxCollider = m_Child.GetComponent<BoxCollider>();
            if (vBoxCollider != null)
            {
                LoadBoxCollider(vBoxCollider, vElm);
            }

            LoadPosition(m_Child, vElm);

            //GameObject vPrefabObj = PrefabUtility.InstantiatePrefab(vOldPrefab) as GameObject;
            //PrefabUtility.ReplacePrefab(vPrefabObj, vOldPrefab, ReplacePrefabOptions.ConnectToPrefab);
            NGUITools.SetDirty(vOldPrefab);
            AssetDatabase.SaveAssets();
            //GameObject.DestroyImmediate(vPrefabObj);
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
        }
    }

    static void LoadPosition(Transform vChild, XmlElement vElm)
    {
        try
        {
            Vector3 vPos = new Vector3(float.Parse(vElm.GetAttribute("pX")), float.Parse(vElm.GetAttribute("pY")), float.Parse(vElm.GetAttribute("pZ")));
            Vector3 vRotation = new Vector3(float.Parse(vElm.GetAttribute("rX")), float.Parse(vElm.GetAttribute("rY")), float.Parse(vElm.GetAttribute("rZ")));
            Vector3 vScale = new Vector3(float.Parse(vElm.GetAttribute("sX")), float.Parse(vElm.GetAttribute("sY")), float.Parse(vElm.GetAttribute("sZ")));

            vChild.localPosition = vPos;
            vChild.localRotation = Quaternion.Euler(vRotation);
            vChild.localScale = vScale;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
        }
    }

    static void SaveLabelParams(UILabel vLabel, XmlElement vElm)
    {
        vElm.SetAttribute("fontSize", vLabel.fontSize.ToString());
        vElm.SetAttribute("fontStyle", ((int)vLabel.fontStyle).ToString());
        vElm.SetAttribute("overflow", ((int)vLabel.overflowMethod).ToString());
        vElm.SetAttribute("alignment", ((int)vLabel.alignment).ToString());
        vElm.SetAttribute("keepCrisp", ((int)vLabel.keepCrispWhenShrunk).ToString());
        vElm.SetAttribute("gradient", vLabel.applyGradient.ToString());
        vElm.SetAttribute("gradientTop", ColorUtility.ToHtmlStringRGBA(vLabel.gradientTop));
        vElm.SetAttribute("gradientBottom", ColorUtility.ToHtmlStringRGBA(vLabel.gradientBottom));
        vElm.SetAttribute("effect", ((int)(vLabel.effectStyle)).ToString());
        vElm.SetAttribute("effectColor", ColorUtility.ToHtmlStringRGBA(vLabel.effectColor));
        vElm.SetAttribute("effectDistX", vLabel.effectDistance.x.ToString());
        vElm.SetAttribute("effectDistY", vLabel.effectDistance.y.ToString());
        vElm.SetAttribute("maxLines", vLabel.maxLineCount.ToString());
        vElm.SetAttribute("maxWidth", vLabel.overflowWidth.ToString());
        vElm.SetAttribute("colorTint", ColorUtility.ToHtmlStringRGBA(vLabel.color));


        vElm.SetAttribute("useFloatSpacing", vLabel.useFloatSpacing.ToString());

        if (vLabel.useFloatSpacing)
        {
            vElm.SetAttribute("floatSpacingX", vLabel.floatSpacingX.ToString());
            vElm.SetAttribute("floatSpacingY", vLabel.floatSpacingY.ToString());
            vElm.SetAttribute("spacingX", "0");
            vElm.SetAttribute("spacingY", "0");
        }
        else
        {
            vElm.SetAttribute("floatSpacingX", "0");
            vElm.SetAttribute("floatSpacingY", "0");
            vElm.SetAttribute("spacingX", vLabel.spacingX.ToString());
            vElm.SetAttribute("spacingY", vLabel.spacingY.ToString());
        }
    }

    static void SaveWidget(UIWidget vWidget, XmlElement vElm)
    {
        vElm.SetAttribute("pivot", ((int)vWidget.pivot).ToString());
        vElm.SetAttribute("wDepth", vWidget.depth.ToString());
        vElm.SetAttribute("width", vWidget.width.ToString());
        vElm.SetAttribute("height", vWidget.height.ToString());
        vElm.SetAttribute("aspectRatio", vWidget.aspectRatio.ToString());
        vElm.SetAttribute("keepAspectRatio", ((int)vWidget.keepAspectRatio).ToString());
        vElm.SetAttribute("bcAutoResize", vWidget.autoResizeBoxCollider.ToString());
    }

    static void SaveBoxCollider(BoxCollider vBoxCollider, XmlElement vElm)
    {
        vElm.SetAttribute("bcIsTrigger", vBoxCollider.isTrigger.ToString());
        vElm.SetAttribute("bcCenterX", vBoxCollider.center.x.ToString());
        vElm.SetAttribute("bcCenterY", vBoxCollider.center.y.ToString());
        vElm.SetAttribute("bcCenterZ", vBoxCollider.center.z.ToString());
        vElm.SetAttribute("bcSizeX", vBoxCollider.size.x.ToString());
        vElm.SetAttribute("bcSizeY", vBoxCollider.size.y.ToString());
        vElm.SetAttribute("bcSizeZ", vBoxCollider.size.z.ToString());
    }

    static void LoadBoxCollider(BoxCollider vBoxCollider, XmlElement vElm)
    {
        if (vElm.HasAttribute("bcIsTrigger"))
        {
            vBoxCollider.isTrigger = System.Convert.ToBoolean(vElm.GetAttribute("bcIsTrigger"));
        }

        Vector3 vSize = Vector3.zero;
        if (vElm.HasAttribute("bcSizeX"))
        {
            vSize.x = float.Parse(vElm.GetAttribute("bcSizeX"));
        }
        if (vElm.HasAttribute("bcSizeY"))
        {
            vSize.y = float.Parse(vElm.GetAttribute("bcSizeY"));
        }
        if (vElm.HasAttribute("bcSizeZ"))
        {
            vSize.z = float.Parse(vElm.GetAttribute("bcSizeZ"));
        }
        vBoxCollider.size = vSize;

        Vector3 vCenter = Vector3.zero;
        if (vElm.HasAttribute("bcCenterX"))
        {
            vCenter.x = float.Parse(vElm.GetAttribute("bcCenterX"));
        }
        if (vElm.HasAttribute("bcCenterY"))
        {
            vCenter.y = float.Parse(vElm.GetAttribute("bcCenterY"));
        }
        if (vElm.HasAttribute("bcCenterZ"))
        {
            vCenter.z = float.Parse(vElm.GetAttribute("bcCenterZ"));
        }
        vBoxCollider.center = vCenter;
    }

    static void SavePanel(UIPanel vPanel, XmlElement vElm)
    {
        vElm.SetAttribute("pDepth", vPanel.depth.ToString());
        vElm.SetAttribute("clipping", ((int)vPanel.clipping).ToString());
        if (vPanel.clipping != UIDrawCall.Clipping.None)
        {
            vElm.SetAttribute("clipOffsetX", ((float)vPanel.clipOffset.x).ToString());
            vElm.SetAttribute("clipOffsetY", ((float)vPanel.clipOffset.y).ToString());
            vElm.SetAttribute("clipSoftnessX", ((float)vPanel.clipSoftness.x).ToString());
            vElm.SetAttribute("clipSoftnessY", ((float)vPanel.clipSoftness.y).ToString());
            vElm.SetAttribute("clipRegionX", ((float)vPanel.baseClipRegion.x).ToString());
            vElm.SetAttribute("clipRegionY", ((float)vPanel.baseClipRegion.y).ToString());
            vElm.SetAttribute("clipRegionW", ((float)vPanel.baseClipRegion.w).ToString());
            vElm.SetAttribute("clipRegionZ", ((float)vPanel.baseClipRegion.z).ToString());
        }
        vElm.SetAttribute("sortLayer", vPanel.sortingLayerName.ToString());
        vElm.SetAttribute("sortOrder", vPanel.sortingOrder.ToString());
    }

    static void LoadPanel(UIPanel vPanel, XmlElement vElm)
    {
        if( vElm.HasAttribute("pDepth"))
        {
            vPanel.depth = int.Parse(vElm.GetAttribute("pDepth"));
        }

        if (vElm.HasAttribute("clipping"))
        {
            vPanel.clipping = (UIDrawCall.Clipping)(int.Parse(vElm.GetAttribute("clipping")));
            if(vPanel.clipping != UIDrawCall.Clipping.None)
            {
                Vector2 vOffsetX = Vector2.zero;
                if (vElm.HasAttribute("clipOffsetX"))
                {
                    vOffsetX.x = float.Parse(vElm.GetAttribute("clipOffsetX"));
                }
                if (vElm.HasAttribute("clipOffsetY"))
                {
                    vOffsetX.y = float.Parse(vElm.GetAttribute("clipOffsetY"));
                }
                vPanel.clipOffset = vOffsetX;

                Vector2 vSoftnessX = Vector2.zero;
                if (vElm.HasAttribute("clipSoftnessX"))
                {
                    vSoftnessX.x = float.Parse(vElm.GetAttribute("clipSoftnessX"));
                }
                if (vElm.HasAttribute("clipSoftnessY"))
                {
                    vSoftnessX.y = float.Parse(vElm.GetAttribute("clipSoftnessY"));
                }
                vPanel.clipSoftness = vSoftnessX;

                Vector4 vRange = Vector2.zero;
                if (vElm.HasAttribute("clipRegionX"))
                {
                    vRange.x = float.Parse(vElm.GetAttribute("clipRegionX"));
                }
                if (vElm.HasAttribute("clipRegionY"))
                {
                    vRange.y = float.Parse(vElm.GetAttribute("clipRegionY"));
                }
                if (vElm.HasAttribute("clipRegionW"))
                {
                    vRange.w = float.Parse(vElm.GetAttribute("clipRegionW"));
                }
                if (vElm.HasAttribute("clipRegionZ"))
                {
                    vRange.z = float.Parse(vElm.GetAttribute("clipRegionZ"));
                }
                vPanel.baseClipRegion = vRange;
            }
        }

        if (vElm.HasAttribute("sortLayer"))
        {
            vPanel.sortingLayerName = vElm.GetAttribute("sortLayer");
        }

        if (vElm.HasAttribute("sortOrder"))
        {
            vPanel.sortingOrder = int.Parse(vElm.GetAttribute("sortOrder"));
        }
    }

    static XmlElement MakeXmlNode(XmlDocument doc, Transform vChild, string path, string subPath)
    {
        XmlElement vElm = doc.CreateElement("Item");
        int nId = doc.DocumentElement.ChildNodes.Count + 1;
        vElm.SetAttribute("id", nId.ToString());
        vElm.SetAttribute("path", path);
        vElm.SetAttribute("subPath", subPath);

        Vector3 vPos = vChild.localPosition;
        Vector3 vScale = vChild.localScale;
        Vector3 vRotation = vChild.localRotation.eulerAngles;
        UILabel vLabel = vChild.GetComponent<UILabel>();
        if (vLabel != null)
        {
            SaveLabelParams(vLabel, vElm);
        }

        UIWidget vWidget = vChild.GetComponent<UIWidget>();
        if (vWidget != null)
        {
            SaveWidget(vWidget, vElm);
        }

        UIPanel vPanel = vChild.GetComponent<UIPanel>();
        if (vPanel != null)
        {
            SavePanel(vPanel, vElm);
        }

        BoxCollider vBoxCollider = vChild.GetComponent<BoxCollider>();
        if (vBoxCollider != null)
        {
            SaveBoxCollider(vBoxCollider, vElm);
        }

        vElm.SetAttribute("pX", vPos.x.ToString());
        vElm.SetAttribute("pY", vPos.y.ToString());
        vElm.SetAttribute("pZ", vPos.z.ToString());
        vElm.SetAttribute("rX", vRotation.x.ToString());
        vElm.SetAttribute("rY", vRotation.y.ToString());
        vElm.SetAttribute("rZ", vRotation.z.ToString());
        vElm.SetAttribute("sX", vScale.x.ToString());
        vElm.SetAttribute("sY", vScale.y.ToString());
        vElm.SetAttribute("sZ", vScale.z.ToString());
        return vElm;
    }

    static void LoadLabel(UILabel vLabel, XmlElement vElm)
    {
        try
        {
            if (vLabel != null)
            {
                if(vElm.HasAttribute("fontSize"))
                    vLabel.fontSize = int.Parse(vElm.GetAttribute("fontSize"));
                if (vElm.HasAttribute("fontStyle"))
                    vLabel.fontStyle = (FontStyle)(int.Parse(vElm.GetAttribute("fontStyle")));
                if (vElm.HasAttribute("keepCrisp"))
                    vLabel.keepCrispWhenShrunk = (UILabel.Crispness)(int.Parse(vElm.GetAttribute("keepCrisp")));
                if (vElm.HasAttribute("overflow"))
                    vLabel.overflowMethod = (UILabel.Overflow)(int.Parse(vElm.GetAttribute("overflow")));
                if (vElm.HasAttribute("alignment"))
                    vLabel.alignment = (NGUIText.Alignment)(int.Parse(vElm.GetAttribute("alignment")));
                if (vElm.HasAttribute("gradient"))
                    vLabel.applyGradient = (System.Convert.ToBoolean(vElm.GetAttribute("gradient")));
                if (vElm.HasAttribute("gradientTop"))
                {
                    Color vColor;
                    if( ColorUtility.TryParseHtmlString("#" + vElm.GetAttribute("gradientTop"), out vColor))
                        vLabel.gradientTop = vColor;
                }
                if (vElm.HasAttribute("gradientBottom"))
                {
                    Color vColor;
                    if( ColorUtility.TryParseHtmlString("#" + vElm.GetAttribute("gradientBottom"), out vColor) )
                        vLabel.gradientBottom = vColor;
                }

                if (vElm.HasAttribute("effect"))
                {
                    vLabel.effectStyle = (UILabel.Effect)(int.Parse(vElm.GetAttribute("effect")));
                }

                if (vElm.HasAttribute("effectColor"))
                {
                    Color vColor;
                    if( ColorUtility.TryParseHtmlString("#" + vElm.GetAttribute("effectColor"), out vColor) )
                        vLabel.effectColor = vColor;
                }

                Vector2 vDist = Vector2.zero;
                bool bIsHasX = false;
                bool bIsHasY = false;
                if (vElm.HasAttribute("effectDistX"))
                {
                    bIsHasX = true;
                    vDist.x = float.Parse(vElm.GetAttribute("effectDistX"));
                }
                    
                if (vElm.HasAttribute("effectDistY"))
                {
                    bIsHasY = true;
                    vDist.y = float.Parse(vElm.GetAttribute("effectDistY"));
                }
                    
                if(bIsHasX && bIsHasY)
                    vLabel.effectDistance = vDist;

                if (vElm.HasAttribute("maxLines"))
                    vLabel.maxLineCount = int.Parse(vElm.GetAttribute("maxLines"));
                if (vElm.HasAttribute("maxWidth"))
                    vLabel.overflowWidth = int.Parse(vElm.GetAttribute("maxWidth"));
                if (vElm.HasAttribute("colorTint"))
                {
                    Color vColor;
                    if (ColorUtility.TryParseHtmlString("#" + vElm.GetAttribute("colorTint"), out vColor))
                        vLabel.color = vColor;
                }

                if (vElm.HasAttribute("useFloatSpacing"))
                    vLabel.useFloatSpacing = (System.Convert.ToBoolean(vElm.GetAttribute("useFloatSpacing")));

                if( vLabel.useFloatSpacing )
                {
                    if (vElm.HasAttribute("floatSpacingX"))
                        vLabel.floatSpacingX = float.Parse(vElm.GetAttribute("floatSpacingX"));
                    if (vElm.HasAttribute("floatSpacingY"))
                        vLabel.floatSpacingY = float.Parse(vElm.GetAttribute("floatSpacingY"));
                }
                else
                {
                    if (vElm.HasAttribute("spacingX"))
                        vLabel.spacingX = int.Parse(vElm.GetAttribute("spacingX"));
                    if (vElm.HasAttribute("spacingY"))
                        vLabel.spacingY = int.Parse(vElm.GetAttribute("spacingY"));
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
        }
    }

    static void LoadWidget(UIWidget vWidget, XmlElement vElm)
    {
        try
        {
            
            if (vWidget != null)
            {
                if (vElm.HasAttribute("pivot"))
                    vWidget.pivot = (UIWidget.Pivot)(int.Parse(vElm.GetAttribute("pivot")));

                if (vElm.HasAttribute("wDepth"))
                    vWidget.depth = int.Parse(vElm.GetAttribute("wDepth"));


                if (vElm.HasAttribute("keepAspectRatio"))
                    vWidget.keepAspectRatio = (UIWidget.AspectRatioSource)(int.Parse(vElm.GetAttribute("keepAspectRatio")));

                if (vElm.HasAttribute("aspectRatio"))
                    vWidget.aspectRatio = float.Parse(vElm.GetAttribute("aspectRatio"));

                if (vElm.HasAttribute("width"))
                    vWidget.width = int.Parse(vElm.GetAttribute("width"));
                if (vElm.HasAttribute("height"))
                    vWidget.height = int.Parse(vElm.GetAttribute("height"));

                if (vElm.HasAttribute("bcAutoResize"))
                    vWidget.autoResizeBoxCollider = System.Convert.ToBoolean(vElm.GetAttribute("bcAutoResize"));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
        }
    }

    private static void FindChildOfName(string[] name, Transform go ,int current,int max)
    {
        for (int i = 0; i < go.childCount; i++)
        {
            if (go.GetChild(i).name == name[current - 1] && current == max)
            {
                m_Child = go.GetChild(i);
            }
            else if(go.GetChild(i).name == name[current -1])
            {
                current++;
                FindChildOfName(name, go.GetChild(i), current, max);
            }
        }
    }

    //判断字符串是否为整数
    public static bool IsInteger(string str)
    {
        try
        {
            int i = System.Convert.ToInt32(str);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void WriteDefaultData()
    {
        File.WriteAllText(m_PrefabChangePath, "<Items>\r\n</Items>");
    }

    public static XmlDocument LoadData()
    {
        XmlDocument doc = new XmlDocument();
        try
        {
            string filePath = m_PrefabChangePath;
            if (!File.Exists(filePath))
            {
                Debug.LogError("配置文件FLConfight.xml不存在，请检查!");
                return null;
            }

            doc.Load(filePath);
            return doc;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("读配置文件异常:" + ex.Message);
            return null;
        }
    }

    [MenuItem("Tools/ChangePrefab/ChangePrefabSave")]
    public static void SavePrefabData()
    {
        Object parentObj = PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeObject);
        if (parentObj == null)
        {
            Debug.LogError("请选中目标");
            return;
        }

        string selectObjPath = AssetDatabase.GetAssetPath(parentObj);

        string[] strs = selectObjPath.Split('/');
        string parentName = strs[strs.Length - 1];
        parentName = parentName.Substring(0, parentName.Length - 7);
        Transform selectChild = Selection.activeTransform;     
        string selectChildPath = selectChild.name;
        while (selectChild.parent != null && selectChild.parent.name != parentName)
        {
            selectChild = selectChild.parent;
            selectChildPath = string.Format("{0}/{1}", selectChild.name, selectChildPath);
        }
        XmlDocument vDoc = null;
        if (File.Exists(m_PrefabChangePath))
        {
            vDoc = LoadData();
        }
        else
        {
            WriteDefaultData();
            vDoc = LoadData();
        }

        XmlElement vNewElm = MakeXmlNode(vDoc, Selection.activeTransform, selectObjPath, selectChildPath);
        XmlElement vOldEml = null;
        if ( IsElmExist(vDoc, vNewElm, out vOldEml))
        {
            //新的信息复制到旧的节点
            CopyToElm(vDoc, vNewElm, vOldEml);
        }
        else
        {
            //追加
            vDoc.DocumentElement.AppendChild(vNewElm);
        }
        vDoc.Save(m_PrefabChangePath);
    }
    private static bool RootObj(GameObject obj)
    {
        string[] rootnames = new string[] { "UI Root", "BaseUIRoot", "PopUIRoot", "StoryUIRoot", "TipUIRoot", "TipUIRoot" };

        for (int i = 0; i < rootnames.Length; i++)
        {
            if (obj.transform.parent != null && obj.transform.parent.name.Equals(rootnames[i]))
            {
                return true;
            }
        }
        return false;
    }
    //[MenuItem("ChangePrefab/ChangePrefabSave2")]
    //public static void SavePrefabData2()
    //{
    //    string selectObjPath= Selection.activeGameObject.transform.GetFullHierarchyPath();
    //    //string selectObjPath = AssetDatabase.GetAssetPath(parentObj);
    //    //string[] strs = selectObjPath.Split('/');
    //    //string parentName = strs[strs.Length - 1];
    //    //parentName = parentName.Substring(0, parentName.Length - 7);
    //    Transform selectChild = Selection.activeTransform;
    //    //string selectChildPath = selectChild.name;
    //    //while (selectChild.parent != null && selectChild.parent.name != parentName)
    //    //{
    //    //    selectChild = selectChild.parent;
    //    //    selectChildPath = string.Format("{0}/{1}", selectChild.name, selectChildPath);
    //    //}
    //    GameObject obj = null;
    //    while (RootObj(selectChild.gameObject))
    //    {
    //        selectChild = selectChild.parent;
    //    }
    //    obj = selectChild.gameObject;
    //    string prefabpath = AssetDatabase.GetAssetPath(obj);
    //    XmlDocument vDoc = null;
    //    if (File.Exists(m_PrefabChangePath))
    //    {
    //        vDoc = LoadData();
    //    }
    //    else
    //    {
    //        WriteDefaultData();
    //        vDoc = LoadData();
    //    }

    //    XmlElement vNewElm = MakeXmlNode(vDoc, Selection.activeTransform, prefabpath, selectObjPath);
    //    XmlElement vOldEml = null;
    //    if (IsElmExist(vDoc, vNewElm, out vOldEml))
    //    {
    //        //新的信息复制到旧的节点
    //        CopyToElm(vDoc, vNewElm, vOldEml);
    //    }
    //    else
    //    {
    //        //追加
    //        vDoc.DocumentElement.AppendChild(vNewElm);
    //    }
    //    vDoc.Save(m_PrefabChangePath);
    //}
    enum EType
    {
        Normal,
        Label,
    }

    static bool IsElmExist(XmlDocument doc, XmlElement vNewElm, out XmlElement vOldElm)
    {
        vOldElm = null;
        XmlElement root = doc.DocumentElement;
        XmlNodeList elmList = (XmlNodeList)root.SelectNodes("/Items/Item");
        XmlElement theElem;
        string sOldId;
        string sNewId = vNewElm.GetAttribute("path") + "/" + vNewElm.GetAttribute("subPath");
        foreach (XmlNode xnf in elmList)
        {
            theElem = (XmlElement)xnf;
            //读取检测目录属性
            if (theElem != null && theElem.InnerText != null)
            {
                sOldId = theElem.GetAttribute("path").Trim() + "/" + theElem.GetAttribute("subPath").Trim();
                if(sNewId == sOldId)
                {
                    vOldElm = theElem;
                    return true;
                }
            }
        }
        return false;
    }

    static void CopyToElm(XmlDocument vDoc, XmlElement vSrc, XmlElement vTgt)
    {
        vSrc.SetAttribute("id", vTgt.GetAttribute("id"));
        vDoc.DocumentElement.ReplaceChild(vSrc, vTgt);
    }
}
