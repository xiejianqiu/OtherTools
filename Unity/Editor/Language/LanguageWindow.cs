using UnityEngine;
using UnityEditor;
using System;
namespace Language
{
    public class LanguageWindow : EditorWindow
    {
        private LANGUAGE lastLang;
        private LANGUAGE selctLang;
        private bool IsInit = false;
        [MenuItem("Tools/多语言/1.语种设置",priority =1)]
        static void Open()
        {
            GetWindow<LanguageWindow>("语种选择", true);
        }
        private void InitLangOp()
        {
            if (Enum.TryParse<LANGUAGE>(GfxPlayerPrefsData.LastSelectLanguage, out var lang))
            {
                this.lastLang = lang;
            }
            this.IsInit = true;
        }
        string tip = string.Empty;
        private void OnGUI()
        {
            if (!IsInit)
            {
                this.InitLangOp();
            }
            GUILayout.Label($"当前语种:{lastLang}");
            selctLang = (LANGUAGE)EditorGUILayout.EnumPopup("请选择语种:", selctLang);
            if (GUILayout.Button("设置"))
            {
                GfxPlayerPrefsData.LastSelectLanguage = selctLang.ToString();
                lastLang = selctLang;
            }
        }
    }
}