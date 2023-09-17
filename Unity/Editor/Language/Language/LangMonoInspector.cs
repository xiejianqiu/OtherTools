using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Language
{
    [CustomEditor(typeof(LangMono))]
    internal class LangMonoInspector:Editor
    {

        SerializedProperty assetPathProperty;

        void OnEnable()
        {
            assetPathProperty = serializedObject.FindProperty("assetPath");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(assetPathProperty, new GUIContent("资源路径"));
        }
    }
}
