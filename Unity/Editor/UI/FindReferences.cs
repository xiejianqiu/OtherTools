using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class FindReferences:EditorWindow
{
    static List<Object> lst = new List<Object>();
    static Object baseObj = null;

    [MenuItem("Tools/Find References In Project", false, 10)]
    static private void Find()
    {
        baseObj = Selection.activeObject;

        EditorWindow.GetWindow<FindReferences>(false, "FindReferences", true).Show();
    }

    bool bPrefab = true;
    bool bUnity = false;
    bool bMat = false;
    bool bAsset = false;

    void OnGUI()
    {

        GUILayout.BeginVertical();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        bPrefab = GUILayout.Toggle(bPrefab,".prefab");
        bUnity = GUILayout.Toggle(bUnity,".unity");
        bMat = GUILayout.Toggle(bMat,".mat");
        bAsset = GUILayout.Toggle(bAsset, ".asset");

        GUILayout.Space(50);
        if(GUILayout.Button("Refresh",GUILayout.Width(150)))
        {
            List<string> filter = new List<string>();
            if(bPrefab)
            {
                filter.Add(".prefab");
            }
            if (bUnity)
            {
                filter.Add(".unity");
            }
            if (bMat)
            {
                filter.Add(".mat");
            }
            if (bAsset)
            {
                filter.Add(".asset");
            }
            FindAllRefs(filter);
            Repaint();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Referrenced Obj:");
        GUILayout.Space(10);

        if(GUILayout.Button(baseObj.name,GUILayout.Width(150)))
        {
            Selection.activeObject = baseObj;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(50);

        for(int i = 0; i< lst.Count/4+1; i++)
        {
            DrawLine(i);
        }

        GUILayout.EndVertical();
    }

    static void DrawLine(int nIdx)
    {
        GUILayout.BeginHorizontal();

        for(int i = nIdx*4; i<nIdx*4+4&& i<lst.Count;i++)
        {
            if (GUILayout.Button(lst[i].name, GUILayout.Width(150)))
            {
                Selection.activeObject = lst[i];
            }
        }

        GUILayout.EndHorizontal();
    }
        

    static void FindAllRefs(List<string> filterLst)
    {
        lst.Clear();
        if(baseObj==null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(baseObj);
        if (!string.IsNullOrEmpty(path))
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => filterLst.Contains(Path.GetExtension(s).ToLower())).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                EditorUtility.DisplayCancelableProgressBar("Processing", file, (float)i / (float)files.Length);

                int nIdx = file.IndexOf("Assets");
                if (-1 != nIdx)
                {
                    file = file.Substring(nIdx);
                    Object asset = AssetDatabase.LoadMainAssetAtPath(file);
                    if (asset != null)
                    {
                        Object[] deps = EditorUtility.CollectDependencies(new Object[] { asset });

                        for (int j = 0; j < deps.Length; j++)
                        {
                            if (deps[j] == baseObj)
                            {
                                lst.Add(asset);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Cant Find " + file);
                    }

                }
            }

            EditorUtility.ClearProgressBar();

        }
    }

    [MenuItem("Assets/Find References In Project", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}