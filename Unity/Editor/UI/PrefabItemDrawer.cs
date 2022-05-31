using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;

[CustomPropertyDrawer(typeof(PrefabItem))]
public class PrefabItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var amountRect = new Rect(position.x, position.y, 150, position.height);
        var unitRect = new Rect(amountRect.x + amountRect.width + 5, position.y, 100, position.height);
        var countRect = new Rect(unitRect.x + unitRect.width + 5, position.y, 50, position.height);
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("mPrefab"), GUIContent.none);
        EditorGUI.PropertyField(countRect, property.FindPropertyRelative("nCount"), GUIContent.none);
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("compName"), GUIContent.none);
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
