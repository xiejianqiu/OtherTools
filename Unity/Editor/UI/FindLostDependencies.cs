using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;

public class FindLostDependencies:EditorWindow
{
    static List<Object> lst = new List<Object>();
    static Object baseObj = null;
    //日志文件的文件流
    private static StreamWriter logStreamWriter;
    //日志文件路径
    private static string basePath = Application.dataPath.Replace("/Assets", "/");
    private static int startIndex;
    private static string configName = "FLConfig.xml";
    private static string logName = "FindLostLog.txt";
    private static List<string> subDirPathList = new List<string>();

    [MenuItem("Tools/Find Lost Dependencies In Project", false, 10)]
    static private void FindLost()
    {
        subDirPathList.Clear();
        if (!IsConfigExist())
        {
            subDirPathList.Add("Assets");
            WriteDefaultConfig();
        }

        EditorWindow.GetWindow<FindLostDependencies>(false, "FindLostDependencies", true).Show();
    }

    private static void OpenLogFile()
    {
        try
        {
            if (logStreamWriter != null)
            {
                logStreamWriter.Close();
                logStreamWriter = null;
            }
            //GB2312
            logStreamWriter = new StreamWriter(basePath + logName, false, Encoding.UTF8);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine("打开日志文件报错了!!!!!!!!!!!");
        }
    }

    public static bool IsConfigExist()
    {
        return File.Exists(basePath + configName);
    }

    public static void WriteDefaultConfig()
    {
        File.WriteAllText(basePath + configName, "<Paths>\r\n<SubPath value=\"Assets\"/>\r\n</Paths>");
    }

    public static void LoadConfig()
    {
        XmlElement theElem = null, root = null;
        XmlDocument doc = new XmlDocument();
        try
        {
            string filePath = basePath + configName;
            if (!File.Exists(filePath))
            {
                writeLog("配置文件FLConfight.xml不存在，请检查!");
                return;
            }

            doc.Load(filePath);
            root = doc.DocumentElement;
            XmlNodeList elmList = (XmlNodeList)root.SelectNodes("/Paths/SubPath");
            subDirPathList.Clear();
            string path;
            foreach (XmlNode xnf in elmList)
            {
                theElem = (XmlElement)xnf;
                //读取检测目录属性
                if (theElem != null && theElem.InnerText != null)
                {
                    path = theElem.GetAttribute("value");
                    path = path.Trim();
                    subDirPathList.Add(path);
                }
            }
        }
        catch (System.Exception ex)
        {
            writeLog("读配置文件异常:" + ex.Message);
        }
    }

    /// <summary>
    /// 开始工作
    /// </summary>
    public static void StartDoTheJob(string subDirPath)
    {
        int i = 0;
        writeLog("查找资源缺失工具v1.0.0");
        writeLog("最后修改日期:2017-7.12 17:00");
        writeLog("作者:ydz");
        
        //writeLog("basePath=" + basePath);
        List<string> filter = new List<string>();
        filter.Add(".prefab");
        //filter.Add(".unity");
        //filter.Add(".mat");
        //filter.Add(".asset");
        string fullPath = basePath + subDirPath;
        if( !Directory.Exists(fullPath))
        {
            writeLog("异常,目录不存在:" + fullPath);
            return;
        }
        writeLog("开始遍历目录:" + fullPath);

        string[] files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories).Where(s => filter.Contains(Path.GetExtension(s).ToLower())).ToArray(); //找指定的预设
        //UI_shengxing.prefab
        //RealmPanelRoot.prefab
        //string[] files = Directory.GetFiles(Application.dataPath, "UI_shengxing.prefab", SearchOption.AllDirectories)
        //    .Where(s => filter.Contains(Path.GetExtension(s).ToLower())).ToArray(); //找指定的预设

        for (i = 0; i < files.Length; i++)
        {
            string file = files[i];
            writeLog("Index=" + ((startIndex++) + 1) + " file=" + file);
            EditorUtility.DisplayCancelableProgressBar("Processing", file, (float)i / (float)files.Length);
            string mainAssetPath = string.Empty;
            string subAssetPath = string.Empty;
            Object mainAsset = null;
            int j = 0;
            int nIdx = file.IndexOf("Assets");
            if (-1 != nIdx)
            {
                file = file.Substring(nIdx);
                try
                {
                    mainAsset = AssetDatabase.LoadMainAssetAtPath(file);
                    mainAssetPath = AssetDatabase.GetAssetPath(mainAsset.GetInstanceID());
                    if (mainAsset != null)
                    {
                        Component[] vecCmps = (mainAsset as GameObject).GetComponentsInChildren<Component>();
                        foreach (var cmp in vecCmps)
                        {
                            SerializedObject so = null;
                            if (cmp is Renderer)
                            {
                                Renderer rd = cmp as Renderer;
                                for (j = 0; j < rd.sharedMaterials.Length; ++j)
                                {
                                    if (rd.sharedMaterials[j] == null)
                                    {
                                        writeLog("--GameObject:" + rd.gameObject.name + "丢失材质,请检查");
                                        continue;
                                    }
                                    so = new SerializedObject(rd.sharedMaterials[j]);
                                    CheckProperty(so);
                                }
                            }
                            else if (cmp is UISprite)
                            {

                                UISprite sprite = cmp as UISprite;
                                //if (sprite.gameObject.name == "Big")
                                //    Debug.Log("aaa");
                                UISpriteData sd = sprite.GetAtlasSprite();
                                if (sprite.atlas == null && !string.IsNullOrEmpty(sprite.spriteName))
                                {
                                    writeLog("--UISprite:" + sprite.gameObject.name + "可能丢失图集,请检查");
                                }
                                else
                                {
                                    if (sprite.atlas != null && sprite.atlas.GetSprite(sprite.spriteName) == null)
                                    {
                                        writeLog("--UISprite:" + sprite.gameObject.name + "在图集:" + sprite.atlas.name + " 找不到图片:" + sprite.spriteName);
                                    }
                                }
                            }
                            else if (cmp is UILabel)
                            {
                                UILabel label = cmp as UILabel;
                                if (label.material == null)
                                {
                                    writeLog("--UILabel:" + label.gameObject.name + "丢失字体,请检查");
                                }
                            }
                            else if (cmp == null)
                            {
                                writeLog("--丢失了脚本,请检查");
                            }
                            else
                            {
                                so = new SerializedObject(cmp);
                                CheckProperty(so);
                            }
                        }
                    }
                    else
                    {
                        //Debug.LogError("Cant Find " + file);
                        writeLog("--找不到文件=" + file); //找不到文件
                    }
                }
                catch (System.Exception ex)
                {
                    //异常处理
                    writeLog("异常了,ex=" + ex.Message + " mainAssetPath=" + mainAssetPath + " subAssetPath=" + subAssetPath);
                }
            }
        }

        EditorUtility.ClearProgressBar();
        writeLog("结束遍历当前目录...");

        if (logStreamWriter != null)
        {
            logStreamWriter.Close();
            logStreamWriter = null;
        }
    }

    public static void CheckProperty(SerializedObject so)
    {
        var sp = so.GetIterator();
        while(sp.NextVisible(true))
        {
            if( sp.propertyType == SerializedPropertyType.ObjectReference)
            {
                if( sp.objectReferenceValue == null )
                {
                    if( sp.objectReferenceInstanceIDValue != 0 )
                        writeLog("--" + ObjectNames.NicifyVariableName(sp.name) + "丢失了组件引用,请检查");
                    else
                    {
                        //if ("Big Level Desc Label" == ObjectNames.NicifyVariableName(sp.name))
                        //    Debug.Log("haha");
                        //if (sp.objectReferenceInstanceIDValue == 0)
                        //    writeLog("--Component Reference null:" + ObjectNames.NicifyVariableName(sp.name));
                    }
                        
                }
            }
        }
    }

    public static void ShowProgress(float val, int total, int cur)
    {
        EditorUtility.DisplayProgressBar("Searching", string.Format("Finding ({0}/{1}), please wait...", cur, total), val);
    }

    static void writeLog(string msg)
    {
        string fullMsg = System.DateTime.Now + "  " + msg;
        System.Console.WriteLine(fullMsg);

        if(logStreamWriter != null)
        {
            logStreamWriter.WriteLine(fullMsg);
            logStreamWriter.Flush();
        }
    }

    bool bPrefab = true;
    bool bUnity = false;
    bool bMat = false;
    bool bAsset = false;

    void OnGUI()
    {
        GUILayout.BeginVertical();
        EditorGUILayout.TextField("目录配置路径: ", basePath + configName);
        EditorGUILayout.TextField("查找结果路径: ", basePath + logName);
        GUILayout.Space(10);
        string rearTxt = string.Empty;
        string defaultTxt = "点击该工具菜单会判断,如果配置文件不存在则生成默认配置于上述路径下.\r\n下述是设置配置文件的例子:\r\n单一目录设置:\r\n<Paths>\r\n<SubPath value=\"Assets\"/>\r\n</Paths>\r\n\r\n多个目录设置,添加多个SubPath节点即可:\r\n<Paths>\r\n<SubPath value=\"Assets\\BundleData\"/>\r\n<SubPath value=\"Assets\\Effect\"/>\r\n</Paths>";
        if (IsConfigExist())
        {
            rearTxt = File.ReadAllText(basePath + configName);
            defaultTxt += "\r\n\r\n\r\n\r\n你当前的目录配置如下:\r\n" + rearTxt;
        }

        GUILayout.TextArea(defaultTxt, 2000);
        GUILayout.Space(10);
        if (GUILayout.Button("开始查找", GUILayout.Width(150)))
        {
            startIndex = 0;
            subDirPathList.Clear();
            OpenLogFile();
            if ( !IsConfigExist() )
            {
                subDirPathList.Add("Assets");
                WriteDefaultConfig();
            }
            else
            {
                LoadConfig();
            }
                
            for(int i = 0; i < subDirPathList.Count; ++i)
            {
                StartDoTheJob(subDirPathList[i]);
            }
            writeLog("所有操作已结束...");
            Repaint();
        }
        GUILayout.EndVertical();
    }


}