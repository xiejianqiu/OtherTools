using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEditor;
public class ExportNavMesh : UnityEditor.EditorWindow
{
    [UnityEditor.MenuItem("Tools/ExportNavMesh/window")]
    static void show()
    {
        UnityEditor.EditorWindow.GetWindow<ExportNavMesh>().Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Export NavMesh OBJ"))
        {
            string outstring = GenNavMesh();
            string outfile = Application.dataPath + "\\ExportedObj\\" + SceneManager.GetActiveScene().name + ".obj";
            System.IO.File.WriteAllText(outfile, outstring);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

    }

    string GenNavMesh()
    {
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
        Dictionary<int, int> indexMap = new Dictionary<int, int>();
        List<Vector3> repos = new List<Vector3>();
        for (int i = 0; i < navMeshTriangulation.vertices.Length; i++)
        {
            int ito = -1;
            for (int j = 0; j < repos.Count; j++)
            {
                if (Vector3.Distance(navMeshTriangulation.vertices[i], repos[j]) < 0.01)
                {
                    ito = j;
                    break;
                }
            }

            if (ito < 0)
            {
                indexMap[i] = repos.Count;
                repos.Add(navMeshTriangulation.vertices[i]);
            }
            else
            {
                indexMap[i] = ito;
            }
        }

        List<int> list = new List<int>();
        List<int[]> polys = new List<int[]>();
        for (int i = 0; i < navMeshTriangulation.indices.Length / 3; i++)
        {
            int i0 = navMeshTriangulation.indices[i * 3 + 0];
            int i1 = navMeshTriangulation.indices[i * 3 + 1];
            int i2 = navMeshTriangulation.indices[i * 3 + 2];

            if (list.Contains(i0) || list.Contains(i1) || list.Contains(i2))
            {
                if (list.Contains(i0) == false)
                    list.Add(i0);
                if (list.Contains(i1) == false)
                    list.Add(i1);
                if (list.Contains(i2) == false)
                    list.Add(i2);
            }
            else
            {
                if (list.Count > 0)
                {
                    polys.Add(list.ToArray());
                }

                list.Clear();
                list.Add(i0);
                list.Add(i1);
                list.Add(i2);
            }
        }

        if (list.Count > 0)
            polys.Add(list.ToArray());

        string genNavMesh = "";

        for (int i = 0; i < repos.Count; i++)
        {
            genNavMesh += "v " + repos[i].x * 1 + " " + repos[i].y + " " + repos[i].z + "\r\n";
        }

        genNavMesh += "\r\n";

        for (int i = 0; i < polys.Count; i++)
        {
            genNavMesh += "f";
            for (int j = polys[i].Length - 1; j >= 0; j--)
            {
                genNavMesh += " " + (indexMap[polys[i][polys[i].Length - 1 - j]] + 1);
            }

            genNavMesh += "\r\n";
        }

        return genNavMesh;
    }
}