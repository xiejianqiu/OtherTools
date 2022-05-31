using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XingYuTuDebugTool))]
public class XingYuTuDebugInspectorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("打印所有子节点坐标"))
        {
            DebugChildPos();
        }
        if (GUILayout.Button("打印所有子节点名称"))
        {
            DebugChildName();
        }
        if (GUILayout.Button("测试模拟占领一颗星"))
        {
            OccupyStar();
        }
    }
    private void DebugChildPos()
    {
        XingYuTuDebugTool editor = target as XingYuTuDebugTool;
        editor.DebugChildPos();
    }
    private void DebugChildName()
    {
        XingYuTuDebugTool editor = target as XingYuTuDebugTool;
        editor.DebugChildName();
    }
    private void OccupyStar()
    {
        XingYuTuDebugTool editor = target as XingYuTuDebugTool;
        editor.OccupyStar();
    }

}
