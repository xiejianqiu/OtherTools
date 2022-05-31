using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AtlasTools : EditorWindow
{

    private string m_strPathProcess = "Assets/BundleData/TPProject";
    private string m_strPathSave = "c:/";
    private string m_strPathUI = "Assets/BundleData/UI";
    private string m_strPathJQUI = "Assets/BundleData/Prefab/Story";
    private string m_strPathIcon = "Assets/ResMS/UI/Atlas/Material";


    private List<UIAtlas> m_lstAtlas = new List<UIAtlas>();

    private Dictionary<string, int> m_dicSpriteName = new Dictionary<string, int>();
    private Dictionary<string, List<string>> m_dicAtlasSprite = new Dictionary<string, List<string>>();

    private string PathSpriteGen { get { return m_strPathSave + "/SpriteGen"; } }
    private string PathSpriteAnalyze { get { return m_strPathSave + "/SpriteAnalyze"; } }

    [MenuItem("Tools/AtlasTools", false, 10)]
    static public void OpenSpriteFinderWindow()
    {
        EditorWindow.GetWindow<AtlasTools>(false, "AtlasTools", true).Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("图集目录 : ");
        m_strPathProcess = EditorGUILayout.TextField(m_strPathProcess);
        EditorGUILayout.EndHorizontal(); 

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("UI目录 : ");
        m_strPathUI = EditorGUILayout.TextField(m_strPathUI);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输出目录 : ");
        m_strPathSave = EditorGUILayout.TextField(m_strPathSave);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        if (GUILayout.Button("导出图集素材"))
        {
            ExportPNGFromAtlas();
        }

        if (GUILayout.Button("图集使用分析"))
        {
            AnalyzeAtlasUsage();
        }

        if (GUILayout.Button("图集交叉使用分析"))
        {
            AnalyzeUI();
        }

        if (GUILayout.Button("重复图片分析"))
        {
            CheckCopyIcon();
        }


    }

    void CheckCopyIcon()
    {
        string strFilePath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string strTargetMD5 = GfxUtils.GetMD5Hash(strFilePath);

        StreamWriter sw = CreateTxt(m_strPathSave + "/UIAnalyzeCopyIcon.txt");
        sw.WriteLine("重复的图片如下：");
        sw.WriteLine("");
        sw.WriteLine("");

        string[] vFolderPaths = Directory.GetDirectories(m_strPathIcon);
        for (int i = 0; i < vFolderPaths.Length; i++)
        {
            string [] strIconPaths = Directory.GetFiles(vFolderPaths[i]);
            for (int j = 0; j < strIconPaths.Length; j++)
            {
                if (!strIconPaths[j].Contains(".meta"))
                {
                    string strTmpMD5 = GfxUtils.GetMD5Hash(strIconPaths[j]);
                    if (strTargetMD5.Equals(strTmpMD5))
                    {
                        sw.WriteLine(strIconPaths[j]);
                    }
                }
            }
        }

        sw.Flush();
        sw.Close();
        sw.Dispose();
    }

    void ExportPNGFromAtlas()
    {
        CollectAtlas();

        CreateDir(m_strPathSave);
        CreateDir(PathSpriteGen);

        for (int i = 0; i < m_lstAtlas.Count; i++)
        {
            ExportPNGFromAtlas(PathSpriteGen + "/" + m_lstAtlas[i].name, m_lstAtlas[i]);
        }
    }

    void ExportPNGFromAtlas(string folderPath, UIAtlas atlas)
    {
        DltDir(folderPath);
        CreateDir(folderPath);

        List<UISpriteData> exitSpritesList = atlas.spriteList;
        Texture2D atlasTexture = NGUIEditorTools.ImportTexture(atlas.texture, true, false, !atlas.premultipliedAlpha);
        int oldwith = atlasTexture.width;
        int oldHeight = atlasTexture.height;
        Color32[] oldPixels = null;
        foreach (var es in exitSpritesList)
        {
            int xmin = Mathf.Clamp(es.x, 0, oldwith);
            int ymin = Mathf.Clamp(es.y, 0, oldHeight);
            int newWidth = Mathf.Clamp(es.width, 0, oldwith);
            int newHeight = Mathf.Clamp(es.height, 0, oldHeight);
            if (newWidth == 0 || newHeight == 0) continue;
            if (oldPixels == null) oldPixels = atlasTexture.GetPixels32();
            newWidth += (es.paddingLeft + es.paddingRight);
            newHeight += (es.paddingTop + es.paddingBottom);
            Color32[] newPixels = new Color32[newWidth * newHeight];
            for(int i = 0; i < newWidth*newHeight;i++)
            {
                newPixels[i] = new Color32(255,255,255,0);
            }
            int nTopPadding = es.paddingTop > 0 ? es.paddingTop : 0;
            int nRightPadding = es.paddingRight > 0 ? es.paddingRight : 0;
            int nLeftPadding = es.paddingLeft < 0 ? -es.paddingLeft : 0;
            int nBottomPadding = es.paddingBottom < 0 ? -es.paddingBottom : 0;
            if (es.paddingTop < 0||es.paddingRight<0||es.paddingLeft<0||es.paddingRight<0)
            {
                Debug.LogError(atlas.name + "  " + es.name);
            }
            int nStarRefixX = es.paddingLeft > 0 ? es.paddingLeft : 0;
            int nStarRefixY = es.paddingBottom > 0 ? es.paddingBottom : 0;
            int nEndRefixX = es.paddingRight > 0 ? es.paddingRight : 0;
            int nEndRefixY = es.paddingTop > 0 ? es.paddingTop : 0;
            
            for (int y = nStarRefixY; y < newHeight- nEndRefixY; ++y)
            {
                for (int x = nStarRefixX; x < newWidth- nEndRefixX; ++x)
                {
                    int newIndex = (newHeight - 1 - y) * newWidth + x;
                    int oldIndex = (oldHeight - 1 - (ymin + y- es.paddingBottom)) * oldwith + (xmin + x- es.paddingLeft);

                    Color32 nValue = oldPixels[oldIndex];
                    
                    if(newIndex < newPixels.Length)
                    {
                        newPixels[newIndex] = nValue;
                    }
                    else
                    {
                        Debug.LogError(es.name+"   " +newIndex + " " + newHeight + "   " + newWidth + "   " + x + "   " + y);
                        return;
                    }
                }
            }
            Texture2D t = new Texture2D(newWidth, newHeight);
            t.SetPixels32(newPixels);
            t.Apply();
            byte[] bytes = t.EncodeToPNG();
            Texture2D.DestroyImmediate(t);
            t = null;

            string filename = folderPath + "/" + es.name + ".png";
            if(File.Exists(filename))
            {
                Debug.LogError(atlas.name+"    " +  filename);
                continue;
            }
            using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write(bytes);
            }
        }
    }

    void AnalyzeAtlasUsage()
    {
        CollectAtlas();
        DltDir(PathSpriteAnalyze);
        CreateDir(PathSpriteAnalyze);
        CollectSprite();
        for (int i = 0; i < m_lstAtlas.Count; i++)
        {
            AnalyzeOneAtlase(m_lstAtlas[i]);
        }
    }
    void AnalyzeUI()
    {
        StreamWriter sw = CreateTxt(m_strPathSave+"/UIAnalyze.txt");
        sw.WriteLine("存在交叉使用图集的UI如下：");
        sw.WriteLine("");
        sw.WriteLine("");
        sw.WriteLine("");

        string filter = "t:Prefab";
        string[] searchPath = { m_strPathUI };
        string[] guidLst = AssetDatabase.FindAssets(filter, searchPath);

        Dictionary<string, int> dicPublicAtlas = new Dictionary<string, int>();
        dicPublicAtlas.Add("mainui", 1);
        dicPublicAtlas.Add("public", 1);
        dicPublicAtlas.Add("public2", 1);
        dicPublicAtlas.Add("skillicon", 1);
        dicPublicAtlas.Add("itemicon", 1);
        dicPublicAtlas.Add("itemicon2", 1);

        for (int i = 0; i < guidLst.Length; i++)
        {
            GameObject vGo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidLst[i]), typeof(GameObject)) as GameObject;
            if (vGo != null)
            {
                Dictionary<string, int> dicAtlas = new Dictionary<string, int>();
                UISprite[] vecSprite = vGo.GetComponentsInChildren<UISprite>(true);
                for (int j = 0; j < vecSprite.Length; j++)
                {
                    if (vecSprite[j].atlas != null)
                    {
                        if(!dicPublicAtlas.ContainsKey(vecSprite[j].atlas.name))
                        {
                            if(!dicAtlas.ContainsKey(vecSprite[j].atlas.name))
                            {
                                dicAtlas.Add(vecSprite[j].atlas.name, 0);
                            }
                        }
                    }
                }

                if(dicAtlas.Count>1)
                {
                    string str = "UI = " + vGo.name + ",使用了图集  ";
                    var enu = dicAtlas.GetEnumerator();
                    while(enu.MoveNext())
                    {
                        str = str + enu.Current.Key + ",  ";
                    }
                    sw.WriteLine(str);
                }
            }
        }

        sw.Flush();
        sw.Close();
        sw.Dispose();
    }
    void AnalyzeOneAtlase(UIAtlas atlas)
    {
        Dictionary<string, int> dicSpInAtlas = new Dictionary<string, int>();
        List<UISpriteData> lstSpData = atlas.spriteList;
        List<string> lstUnused = new List<string>();
        List<string> lstLost = new List<string>();
        List<string> lstUsed = new List<string>();

        for (int i = 0; i < lstSpData.Count; i++)
        {
            dicSpInAtlas.Add(lstSpData[i].name, 0);
        }
        List<string> lstSpName = null;
        if(!m_dicAtlasSprite.TryGetValue(atlas.name,out lstSpName))
        {
            Debug.LogError("No UI use this atlas !!!  atlas name ="+atlas.name);
            return;
        }
        for (int i = 0; i < lstSpName.Count; i++)
        {
            if (!dicSpInAtlas.ContainsKey(lstSpName[i]))
            {
                lstLost.Add(lstSpName[i]);
            }
            else
            {
                dicSpInAtlas[lstSpName[i]]++;
            }
        }
        
        var enu = dicSpInAtlas.GetEnumerator();
        while (enu.MoveNext())
        {
            if (enu.Current.Value == 0)
            {
                lstUnused.Add(enu.Current.Key);
            }
        }

        OutputAnalyze(atlas.name, lstLost, lstUnused);
    }

    void OutputAnalyze(string nameAtlas, List<string> lstLost, List<string> lstUnused)
    {
        StreamWriter fi = CreateTxt(PathSpriteAnalyze + "/" + nameAtlas+".txt");
        fi.WriteLine("丢失的Sprite:");

        for (int i = 0; i < lstLost.Count; i++)
        {
            fi.WriteLine(lstLost[i]);
        }

        fi.WriteLine("");
        fi.WriteLine("");
        fi.WriteLine("");
        fi.WriteLine("不再使用的Sprite:");
        for (int i = 0; i < lstUnused.Count; i++)
        {
            fi.WriteLine(lstUnused[i]);
        }
        fi.Flush();
        fi.Close();
        fi.Dispose();
    }

    void CollectAtlas()
    {
        m_lstAtlas.Clear();

        string filter = "t:Prefab";
        string[] searchPath = { m_strPathProcess };
        string[] guidLst = AssetDatabase.FindAssets(filter, searchPath);

        for (int i = 0; i < guidLst.Length; i++)
        {

            GameObject vGo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidLst[i]), typeof(GameObject)) as GameObject;
            if (vGo != null)
            {
                UIAtlas vAtlas = vGo.GetComponent<UIAtlas>();
                if (vAtlas != null)
                {
                    m_lstAtlas.Add(vAtlas);
                }
            }
        }

        if (m_lstAtlas.Count == 0)
        {
            Debug.LogError("No atlas find!!!");
        }
    }
    StreamWriter CreateTxt(string path)
    {
        if(File.Exists(path))
        {
            File.Delete(path);
        }

        StreamWriter sf = File.CreateText(path);
        return sf;
    }
    void CollectSprite()
    {
        m_dicAtlasSprite.Clear();
        if (m_lstAtlas.Count == 0)
        {
            return;
        }

        string filter = "t:Prefab";
        string[] searchPath = { m_strPathUI, m_strPathJQUI };
        string[] guidLst = AssetDatabase.FindAssets(filter, searchPath);

        for (int i = 0; i < guidLst.Length; i++)
        {
            GameObject vGo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidLst[i]), typeof(GameObject)) as GameObject;
            if (vGo != null)
            {
                UISprite[] vecSprite = vGo.GetComponentsInChildren<UISprite>(true);
                for(int j = 0; j < vecSprite.Length; j++)
                {
                    if(vecSprite[j].atlas!=null)
                    {
                        AddAtlasAndSpriteName(vecSprite[j].atlas.name, vecSprite[j].spriteName);
                    }
                }

                UIButton[] vecButton = vGo.GetComponentsInChildren<UIButton>(true);
                for(int j =0; j < vecButton.Length; j++)
                {
                    UISprite vSp = vecButton[j].gameObject.GetComponent<UISprite>();
                    if(vSp!=null&&vSp.atlas!=null)
                    {
                        if(!string.IsNullOrEmpty(vecButton[j].pressedSprite))
                        {
                            AddAtlasAndSpriteName(vSp.atlas.name, vecButton[j].pressedSprite);
                        }
                    }
                }

            }
        }

        if (m_dicAtlasSprite.Count == 0)
        {
            Debug.LogError("No uisprite find!!!");
        }

    }

    void AddAtlasAndSpriteName(string nameAtlas,string nameSprite)
    {
        List<string> lstSpName = null;
        if(!m_dicAtlasSprite.TryGetValue(nameAtlas,out lstSpName))
        {
            lstSpName = new List<string>();
            m_dicAtlasSprite.Add(nameAtlas, lstSpName);
        }

        if(!lstSpName.Contains(nameSprite))
        {
            lstSpName.Add(nameSprite);
        }
        
    }

    void CreateDir(string strDir)
    {
        if (!Directory.Exists(strDir))
        {
            Directory.CreateDirectory(strDir);
        }
    }

    void DltDir(string strDic)
    {
        if (Directory.Exists(strDic))
        {
            Directory.Delete(strDic, true);
        }
    }
}
