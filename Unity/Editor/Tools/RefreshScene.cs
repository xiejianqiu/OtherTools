using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RefreshScene : EditorWindow
{

    [MenuItem("Tools/RefreshEditorSceneSetting")]
   public static void RefreshEditorSceneSetting()
    {
        List<string> dirs = new List<string>();
        GetDirs("Assets/Scene/Scene", ref dirs);
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[dirs.Count+2];
        for (int i = 0; i < 2; i++)
        {
            newSettings[i] = EditorBuildSettings.scenes[i];
        }
        for (int i = 2; i < newSettings.Length; i++)
        {
            newSettings[i] = new EditorBuildSettingsScene(dirs[i-2], true);
        }
        EditorBuildSettings.scenes = newSettings;
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/RefreshABSceneSetting")]
    public static void RefreshABSceneSetting() 
    {
        if (Directory.Exists("Assets/BundleData/Scene"))
        {
            Directory.Delete("Assets/BundleData/Scene", true);
        }
        CopyDirectory("Assets/Scene/Scene", "Assets/BundleData/Scene/");
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        for (int i = 0; i < 2; i++)
        {
            newSettings[i] = EditorBuildSettings.scenes[i];
        }
        EditorBuildSettings.scenes = newSettings;
        AssetDatabase.SaveAssets();
    }
    private static void GetDirs(string dirPath, ref List<string> dirs)
    {
        foreach (string path in Directory.GetFiles(dirPath))
        {
            if (System.IO.Path.GetExtension(path) == ".unity")
            {
                dirs.Add(path.Substring(path.IndexOf("Assets/Scene/Scene")));
            }
        }
        if (Directory.GetDirectories(dirPath).Length > 0)
        {
            foreach (string path in Directory.GetDirectories(dirPath))
                GetDirs(path, ref dirs);
        }
    }
  
    public static void CopyDirectory(string sourceDirName, string destDirName)
    {
        try
        {

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));

            }

//            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
//                destDirName = destDirName + Path.DirectorySeparatorChar;

            string[] files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                if (file.EndsWith(".unity") || file.EndsWith(".unity.meta"))
                {
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }
                //total++;
            }

            string[] dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName );
            }
        }
        catch (Exception ex)
        {
            //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\log.txt", true);
            //sw.Write(ex.Message + "     " + DateTime.Now + "\r\n");
            //sw.Close();
            Console.WriteLine(ex.Message);
        }
    }

}
