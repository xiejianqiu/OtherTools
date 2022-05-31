using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
public class ChildNameChanger : EditorWindow
{
    private string text = null;

    [MenuItem("UIEditor/ResetChildName")]
    static private void Find()
    {
        EditorWindow.GetWindow<ChildNameChanger>(false, "Child Name Changer", true).Show();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        text = EditorGUILayout.TextField(text);
        if(GUILayout.Button("Change Name",GUILayout.Width(150)))
        {
            ResetChildName();
        }
    }

    public void ResetChildName()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            return;
        }

        Transform t = go.transform;
        for (int i = 0; i < t.childCount; ++i)
        {
            t.GetChild(i).name = text + (i >= 10 ? i.ToString() : "0" + i.ToString());
        }
    }
}