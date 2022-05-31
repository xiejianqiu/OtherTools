using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using System;

public class CheckUpdateFiles : Editor {

    [MenuItem("Tools/CheckUpdateFiles")]
    public static void ChechUpdateFiles()
    {
        Debug.Log("Check begin");

        string dataPath = PlayerPrefs.GetString("patchdir","");
        string selectedPath = EditorUtility.OpenFolderPanel("Path", dataPath, "");
        PlayerPrefs.SetString("patchdir", selectedPath);
        //string fromPath = selectedPath+"/../UpdateInfo.txt";
        string toPath = selectedPath+ "/UpdateInfo.txt";
        string filePath = selectedPath + "/Res/";

        string previousPatch = EditorUtility.OpenFolderPanel("Path", dataPath, "");
        string fromPath = previousPatch + "/UpdateInfo.txt";

        CheckVersionCode(selectedPath + "/VersionInfo.txt", previousPatch + "/VersionInfo.txt");

        Dictionary<string, string> fromDic = null;
        Dictionary<string, string> toDic = null;
        Dictionary<string, string> updateDic = new Dictionary<string, string>();

        fromDic = ReadUpdateInfo(fromPath);
        toDic = ReadUpdateInfo(toPath);

        Debug.Log("fromDic Count = "+fromDic.Count);
        Debug.Log("toDic Count = "+toDic.Count);


        var enu = toDic.GetEnumerator();
        while(enu.MoveNext())
        {
            string crcOld = null;
            if (fromDic.TryGetValue(enu.Current.Key,out crcOld))
            {
                if (crcOld.Equals(enu.Current.Value))
                {
                    continue;
                }
            }

            updateDic.Add(enu.Current.Key,enu.Current.Value);
        }

        enu.Dispose();

        Debug.Log("updateDic Count = "+updateDic.Count);

        enu = updateDic.GetEnumerator();
        
        long nLen = 0;
        
        while (enu.MoveNext())
        {
            if(!File.Exists(filePath+enu.Current.Key))
            {
                Debug.LogError(enu.Current.Key);
                continue;
            }
            if(!GfxUtils.GetMD5Hash(filePath + enu.Current.Key).Equals(enu.Current.Value))
            {
                Debug.LogError(enu.Current.Key);
                continue;
            }

            nLen += new FileInfo(filePath + enu.Current.Key).Length;
        }
        Debug.Log("Size :" + (nLen/1024/1024));
        Debug.Log("Check end");
    }

    private static void CheckVersionCode(string selectedFile, string previousFile)
    {
        int selRevision = 0;
        int selBuild = 0;
        int preRevison = 0;
        int preBuild = 0;
        using (var reader = new StreamReader(selectedFile))
        {
            var content = reader.ReadLine();
            string[] versions = content.Split('.');
            int.TryParse(versions[2], out selRevision);
            int.TryParse(versions[3], out selBuild);
        }
        using (var reader = new StreamReader(previousFile))
        {
            var content = reader.ReadLine();
            string[] versions = content.Split('.');
            int.TryParse(versions[2], out preRevison);
            int.TryParse(versions[3], out preBuild);
        }
        if (selRevision > preRevison)
        {
            Debug.LogErrorFormat("<color=red>Previous revision version is [{0}],</color> Select revision version is [{1}]", preRevison, selRevision);
        }
        else
        {
            Debug.LogFormat("Previous revision version is [{0}], Select revision version is [{1}]", preRevison, selRevision);
        }

        if (selBuild > preBuild)
        {
            Debug.LogFormat("Previous build version is [{0}], Select build version is [{1}]", preBuild, selBuild);
        } else
        {
            Debug.LogErrorFormat("<color=red>Previous build version is [{0}],</" +
                "color> Select build version is [{1}]", preBuild, selBuild);
        }
    }

    public static Dictionary<string, string> ReadUpdateInfo(string strPath)
    {
        Dictionary<string, string> vRet = new Dictionary<string, string>();

        using (FileStream fs = new FileStream(strPath, FileMode.Open))
        {
            StreamReader sr = new StreamReader(fs);
            string strLine = sr.ReadLine();
            while (!string.IsNullOrEmpty(strLine))
            {
                JsonData jd = JsonMapper.ToObject(strLine);
                vRet.Add(jd["fn"].ToString(),jd["md"].ToString());
                strLine = sr.ReadLine();
            }
            sr.Close();
        }

        return vRet;
    }
}
