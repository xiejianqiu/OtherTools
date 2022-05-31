using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;
using Shark;

public class SelectModel:EditorWindow
{
    static List<Object> lst = new List<Object>();
    static Object baseObj = null;
    //日志文件的文件流
    private static StreamWriter logStreamWriter;
    //日志文件路径
    private static string basePath = Application.dataPath.Replace("/Assets", "/");
    private static string logName = "SelectModelLog.txt";
    private static string m_strInputId = "";
    private static bool m_isLoadedTable = false;
    private static bool m_isShowLog = false;

    static Dictionary<int, string> dicContainer = new Dictionary<int, string>();

    [MenuItem("Tools/Select Model In Project", false, 10)]
    static private void SelectMode()
    {
        LoadTables();
        EditorWindow.GetWindow<SelectModel>(false, "SelectModel", true).Show();
    }

    static void LoadTables()
    {
        //如果UNITY没有运行并且未加载过表则加载一次
        //if (!EditorApplication.isPlaying && !m_isLoadedTable)
        //{
        //    ShowLog("如果UNITY没有运行并且未加载过表则加载一次");
        //    m_isLoadedTable = true;
        string textStr = ReadTxtTable("Client/CharModel.txt");
        Load(textStr, dicContainer);
        EditorUtility.ClearProgressBar();
        //}
    }

    public static string ReadTxtTable(string strPath)
    {
        var localPath = basePath + "/Assets/BundleData/Tables/" + strPath;
        EditorUtility.DisplayCancelableProgressBar("加载表", localPath, 0);
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
            //Console.WriteLine(strData);
            sr.Close();
        }
        catch (System.Exception e)
        {
            ShowLog("File Can't find!!!!");
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



    public static void ShowLog(string tips)
    {
        if(m_isShowLog)
        {
            OutLog.Log(123, tips);
        }
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
            ShowLog("打开日志文件报错了!!!!!!!!!!!");
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
    
    string GetCharModleDataById(int nId)
    {
        foreach (var VARIABLE in dicContainer)
        {
            if (VARIABLE.Key == nId)
            {
                return VARIABLE.Value;
            }
        }
        return null;
    }


    private static void Load(string dataTable, Dictionary<int, string> dicContainer)
    {
        string[] rows = dataTable.Split('\n');
        int len = rows.Length;
        for (int i = 0; i < len; i++)
        {
            if (rows[i].StartsWith("#") || rows[i].Equals(""))
            {
                continue;
            }
            string[] strArray = rows[i].Split('\t');
            int m_Id = int.Parse(strArray[0]);
            string m_Str = strArray[4];
            if (m_Id != -1)
            {
                if (dicContainer.ContainsKey(m_Id))
                {
                    Debug.Log("字典ID重复了, id : " + m_Id);
                }
                else
                {
                    dicContainer.Add(m_Id, m_Str);
                }
            }
        }
    }

    private static string ReadTxtTableStr(string strPath)
    {
        string localPath = Application.dataPath + "/BundleData/Tables/" + strPath;
        string strData = null;
        if (File.Exists(localPath))
        {
            StreamReader sr = null;
            sr = File.OpenText(localPath);
            strData = sr.ReadToEnd();
            sr.Close();
        }
        return strData;
    }


    void OnGUI()
    {
        GUILayout.BeginVertical();
        m_strInputId = EditorGUILayout.TextField("请输入CharModel表中的ID: ", m_strInputId);
        m_isShowLog = GUILayout.Toggle(m_isShowLog, "显示日志(只在Unity运行状态有效)");
        GUILayout.Space(10);
        if (GUILayout.Button("开始查找", GUILayout.Width(150)))
        {
            int ID = -1;
            try
            {
                ShowLog("输入的ID=" + m_strInputId);
                ID = int.Parse(m_strInputId.Trim());
                string vCharModel = GetCharModleDataById(ID);
                if (vCharModel != null)
                {
                    //basePath + 
                    string path = @"Assets/BundleData/Prefab/Model/" + vCharModel;
                    if (!path.EndsWith(".prefab"))
                        path += ".prefab";
                    if (File.Exists(path))
                    {
                        ShowLog("文件存在:" + path);
                    }
                    else
                    {
                        ShowLog("文件不存在:" + path);
                    }

                    Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (mainAsset != null)
                    {
                        ShowLog("打开文件成功:" + path);
                    }
                    else
                    {
                        ShowLog("打开文件失败:" + path);
                    }
                    Selection.activeObject = mainAsset;
                }
                else
                {
                    ShowLog("CharModel表中不存在ID:" + ID);
                }
            }
            catch(System.Exception ex)
            {
                //OutLog.Log(123, "异常了 ex=" + ex.Message);
                ShowLog("异常了 ex=" + ex.Message);
            }

            Repaint();
        }
        GUILayout.EndVertical();
    }


}