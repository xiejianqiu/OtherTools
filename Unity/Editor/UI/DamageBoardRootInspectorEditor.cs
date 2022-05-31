using UnityEditor;
using UnityEngine;
using System.Collections;

//[CustomEditor(typeof(DamageBoardRootEditor))]
public class DamageBoardRootInspectorEditor : Editor {

    static public DamageBoardRootInspectorEditor instance;

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create DamageBoard"))
        {
            CreateNewDamageBoard();
        }

        if (GUILayout.Button("Delete All DamageBoard"))
        {
            DeleteAllDamageBoard();
        }
    }

    private void CreateNewDamageBoard()
    {
        // DamageBoardRootEditor editor = target as DamageBoardRootEditor;
        // if (editor == null)
        // {
        //     Debug.LogError("Error ! Editor = null;");
        // }
        //
        // GameObject go = GameObject.Instantiate(editor.DamageBoard, editor.transform);

    }

    private void DeleteAllDamageBoard()
    {
        // DamageBoardRootEditor editor = target as DamageBoardRootEditor;
        // if (editor == null)
        // {
        //     Debug.LogError("Error ! Editor = null;");
        // }
        // Transform t = editor.transform;
        // t.DestroyChildren();
    }
}
