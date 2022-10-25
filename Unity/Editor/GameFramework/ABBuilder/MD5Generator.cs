using UnityEngine;
using UnityEditor;
using System.IO;
public class MD5Generator : EditorWindow
{
    [MenuItem("ABSystem/Md5Calc")]
    static void Open()
    {
        string selectedPath = EditorUtility.OpenFilePanel("Path", Application.dataPath+ "/../PatchHistory/", "");


        Debug.LogError(selectedPath);
        Debug.LogError(GfxUtils.GetMD5Hash(selectedPath));

    }
    
    [MenuItem("ILRuntime/CopyDll2Path")]
    public static void CopyDll2Path()
    {
        File.Copy(Application.dataPath+"/../hotfix/hotfix/bin/Release/hotfix.dll", Application.dataPath+"/BundleData/dll/hotfix.dll.bytes",true);
        File.Copy(Application.dataPath+"/../hotfix/hotfix/bin/Release/hotfix.pdb", Application.dataPath+"/BundleData/dll/hotfix.pdb.bytes",true);
        AssetDatabase.Refresh();
        Debug.LogError("Copy succeed!");
    }

}
