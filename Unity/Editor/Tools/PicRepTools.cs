using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Drawing;
using System.Text;
using System;

public class PicRepTools : EditorWindow
{
    // Start is called before the first frame update
    [MenuItem("UIEditor/同名图片替换工具", false, 10)]
    static public void OpenCameraWizard()
    {
        EditorWindow.GetWindow<PicRepTools>(false, "图片替换工具", true).Show();
    }
    string newResPath = "";
    string projResPath = "";
    static bool igoneSize = false;
    //static string dir1 = Application.dataPath + "/ResMS/UI";
    //static string dir2 = Application.dataPath + "/BundleData/Texture";
    static double mSpanTime = 180;
    DateTime beginImporttime;
    static DateTime exportDT;
    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("新资源目录：", GUILayout.ExpandWidth(false));
        newResPath = GUILayout.TextField(newResPath, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("选取目录", GUILayout.ExpandWidth(false), GUILayout.MinWidth(60))) {
            newResPath = EditorUtility.OpenFolderPanel("资源目录", "", "");
        }
        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("原资源目录：", GUILayout.ExpandWidth(false));
        //projResPath = GUILayout.TextField(projResPath, GUILayout.ExpandWidth(true));
        //if (GUILayout.Button("选取目录", GUILayout.ExpandWidth(false), GUILayout.MinWidth(60)))
        //{
        //    projResPath = EditorUtility.OpenFolderPanel("资源目录", "", "");
        //}
        //GUILayout.EndHorizontal();
        GUILayout.Label("默认替换路径 :", GUILayout.ExpandWidth(false));
        GUILayout.Label(Application.dataPath + "/ResMS/UI", GUILayout.ExpandWidth(false));
        GUILayout.Label(Application.dataPath + "/BundleData/Texture", GUILayout.ExpandWidth(false));
        //igoneSize = GUILayout.Toggle(igoneSize, new GUIContent("忽略大小"));
        if (GUILayout.Button("开始替换")) {
            beginImporttime = DateTime.Now;
            BeginReplace(newResPath, projResPath);
        }
        if (GUILayout.Button("导出同名UI"))
        {
            GetSameNameFile();
        }
        if (GUILayout.Button("导出未修改过的图片"))
        {
            exportDT = DateTime.Now;
            mSpanTime = (exportDT - beginImporttime).TotalSeconds;
            Debug.Log($"mSpanTime:{mSpanTime}");
            GetOneMinuteNoModify();
        }
        GUILayout.EndVertical();
    }
    public static void BeginReplace(string newResPaht, string projResPath) {
        List<string> repLogLst = new List<string>();
        //List<string> notRepLogLst = new List<string>();
        Dictionary<string, string> notRepLogLst = new Dictionary<string, string>();
        //List<string> notRepLog = new List<string>();
        var newPicLst = GetAllPics(newResPaht);
        var projPicDict = GetAllPicsDict();
        int index = 0;
        try
        {
            foreach (var newFilePath in newPicLst)
            {
                index += 1;
                EditorUtility.DisplayProgressBar("资源替换", "正在替换资源...", index * 1f / newPicLst.Count);
                var fileName = Path.GetFileName(newFilePath);

                if (projPicDict.TryGetValue(fileName, out var projflst))
                {
                    int index2 = 0;
                    foreach (var projFilePath in projflst)
                    {
                        index2 += 1;
                        if (File.Exists(projFilePath))
                        {
                            Image image = Image.FromFile(newFilePath);
                            int width = image.Width;
                            int heigt = image.Height;
                            Image image2 = Image.FromFile(projFilePath);
                            int width2 = image2.Width;
                            int heigt2 = image2.Height;
                            image.Dispose();
                            image2.Dispose();
                            if (Mathf.Abs(width - width2) <= 5 && Mathf.Abs(heigt - heigt2) <= 5 || igoneSize)
                            {
                                File.Copy(newFilePath, projFilePath, true);
                                //repLogLst.Add($"{newFilePath} -> {projFilePath}");
                                FileInfo fileInfo = new FileInfo(projFilePath);
                                if (null != fileInfo) {
                                    fileInfo.LastWriteTime = DateTime.Now;
                                }
                                repLogLst.Add(newFilePath);
                            }
                            else
                            {
                                if (!notRepLogLst.ContainsKey(newFilePath))
                                    notRepLogLst.Add(newFilePath, "因为尺寸和以前不一样");
                            }
                            //File.Copy(newFilePath, projFilePath, true);
                            //repLogLst.Add($"{newFilePath} -> {projFilePath}");
                        }
                        else
                        {
                            if (!notRepLogLst.ContainsKey(newFilePath))
                                notRepLogLst.Add(newFilePath, "因为在工程里没找到资源");
                        }
                    }
                }
                else
                {
                    if (!notRepLogLst.ContainsKey(newFilePath))
                        notRepLogLst.Add(newFilePath, "因为命名和工程里不一样");
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
        finally {
            EditorUtility.ClearProgressBar();
        }
        AssetDatabase.Refresh();
        Debug.Log("图片替换完成!!!");
        for (int i = 0; i < repLogLst.Count; i++)
        {
            if (notRepLogLst.ContainsKey(repLogLst[i]))
            {
                notRepLogLst.Remove(repLogLst[i]);
            }
        }
        StringBuilder str = new StringBuilder();
        if (notRepLogLst.Count > 0)
        {
            str.AppendLine("未替换的图片如下：");
            foreach (var log in notRepLogLst)
            {
                str.AppendLine(log.Key + "___" + log.Value);
                //Debug.Log($"<color=red>{log}</color>");
            }
        }
        UnityEngine.Debug.LogError(str.ToString());
        str.Clear();
        //if (repLogLst.Count > 0)
        //{
        //    Debug.Log("已替换图片如下：");
        //    foreach (var log in repLogLst)
        //    {
        //        Debug.Log($"<color=green>{log}</color>");
        //    }
        //}
        string logPath = $"{Application.dataPath}\\..\\Log\\";
        if (!string.IsNullOrEmpty(logPath))
        {
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            if (notRepLogLst.Count > 0)
            {
                var notRepFilePath = $"{logPath}\\not_rep_log_{System.DateTime.Now.ToString("yyyyMMDDHHmmss")}.txt";
                using (var fs = File.OpenWrite(notRepFilePath))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    foreach (var log in notRepLogLst)
                    {
                        writer.WriteLine(log);
                    }
                    writer.Flush();
                }
            }
            if (repLogLst.Count > 0)
            {
                var repFilePath = $"{logPath}\\rep_log_{System.DateTime.Now.ToString("yyyyMMDDHHmmss")}.txt";
                using (var fs = File.OpenWrite(repFilePath))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    foreach (var log in repLogLst)
                    {
                        writer.WriteLine(log);
                    }
                    writer.Flush();
                }
            }
        }
        System.GC.Collect();
    }

    public static void GetSameNameFile() {

        string logPath = $"{Application.dataPath}\\..\\Log\\";
        var repFilePath = $"{logPath}\\samename_{System.DateTime.Now.ToString("yyyyMMDDHHmmss")}.txt";
        if (!string.IsNullOrEmpty(logPath))
        {
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }
        string SavePath = $"{Application.dataPath}\\..\\Log\\SameName".Replace("\\", "/");
        if (Directory.Exists(SavePath))
        {
            Directory.Delete(SavePath, true);
        }

        System.GC.Collect();
        var dict = GetAllPicsDict();
        var iter = dict.GetEnumerator();
        using (var fs = File.OpenWrite(repFilePath))
        {
            StreamWriter writer = new StreamWriter(fs);
            StringBuilder str = new StringBuilder();
            while (iter.MoveNext())
            {
                if (iter.Current.Value.Count > 1)
                {
                    str.Clear();
                    str.AppendLine(iter.Current.Key);
                    foreach (var filePath in iter.Current.Value)
                    {
                        str.AppendLine(filePath);
                        string newfilepath = filePath.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), SavePath);
                        string newFoloder = Path.GetDirectoryName(newfilepath);
                        if (!Directory.Exists(newFoloder)) {
                            Directory.CreateDirectory(newFoloder);
                        }
                        if (!File.Exists(newfilepath))
                        {
                            File.Copy(filePath, newfilepath);
                        }
                        else {
                            Debug.Log($"file Exist: {newfilepath}");
                        }
                    }
                    str.AppendLine();
                    str.AppendLine();
                    writer.WriteLine(str);
                }
            }
        }
    }
    public static Dictionary<string, HashSet<string>> GetAllPicsDict()
    {
        Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>();
        AddToDict(Application.dataPath + "/ResMS/UI", dict);
        AddToDict(Application.dataPath + "/BundleData/Texture", dict);
        AddToDict(Application.dataPath + "/Effect/Textures", dict);
        return dict;
    }
    public static Dictionary<string, HashSet<string>> GetResMSUI()
    {
        Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>();
        AddToDict(Application.dataPath + "/ResMS/UI", dict);
        return dict;
    }
    public void GetOneMinuteNoModify(){
        string logPath = $"{Application.dataPath}\\..\\Log\\";
        var repFilePath = $"{logPath}\\nomodify_1M_{System.DateTime.Now.ToString("yyyyMMDDHHmmss")}.txt";
        if (!string.IsNullOrEmpty(logPath))
        {
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }
        string SavePath = $"{Application.dataPath}\\..\\Log\\NoModify".Replace("\\", "/");
        if (Directory.Exists(SavePath))
        {
            Directory.Delete(SavePath, true);
        }

        try
        {
            var dict = GetAllPicsDict();
            var iter = dict.GetEnumerator();
            using (var fs = File.OpenWrite(repFilePath))
            {
                StreamWriter writer = new StreamWriter(fs);
                int nCount = 0;
                while (iter.MoveNext())
                {
                    nCount += 1;
                    EditorUtility.DisplayProgressBar("资源获取中...", "Copying...", nCount * 1f / dict.Count);
                    foreach (var filePath in iter.Current.Value)
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        var modifyTime = fileInfo.LastWriteTime;
                        if ((exportDT - modifyTime).TotalSeconds >= mSpanTime + 5)
                        {
                            string newfilepath = filePath.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), SavePath);
                            string newFoloder = Path.GetDirectoryName(newfilepath);
                            if (!Directory.Exists(newFoloder))
                            {
                                Directory.CreateDirectory(newFoloder);
                            }
                            if (!File.Exists(newfilepath))
                            {
                                File.Copy(filePath, newfilepath);
                            }
                            else
                            {
                                Debug.Log($"file Exist: {newfilepath}");
                            }
                            writer.WriteLine(filePath);
                        }
                    }

                }
                writer.Flush();
                writer.Close();
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
            System.GC.Collect();
        }
    }
    public static List<string> GetAllPics(string dir)
    {
        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        var result = from f in files where f.EndsWith(".png") || f.EndsWith(".PNG") || f.EndsWith(".jpg") || f.EndsWith(".JPG") select f.Replace(@"/", @"\");
        return result.ToList();
    }
    public static void AddToDict(string path, Dictionary<string, HashSet<string>> dict) {
        List<string> lst = GetAllPics(path);
        foreach (var filePath in lst)
        {
            var fileName = Path.GetFileName(filePath);
            if (!dict.TryGetValue(fileName, out var list))
            {
                dict[fileName] = new HashSet<string>();
                list = dict[fileName];
            }
            list.Add(filePath);
        }
    }
}
