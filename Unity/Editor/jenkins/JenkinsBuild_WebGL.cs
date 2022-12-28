using UnityEditor;
using WeChatWASM;
namespace WeChatWASM
{
    public partial class WXEditorWindow : EditorWindow
    {
        public static void Export(bool IsDebug, string output) {

            WXEditorWindow window = new WXEditorWindow();
            window.developBuild = IsDebug;
            window.profilingFuncs = IsDebug;
            window.autoProfile = IsDebug;
            window.OnEnable();
            if (!string.IsNullOrEmpty(output))
            {
                WXEditorWindow.dst = output;
            }
            window.DoExport(true);
        }

    }
}
namespace Jenkins
{
    
    public partial class JenkinsBuild
    {
        private static void BuildWebGl()
        {
            bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
            string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
            WXEditorWindow.Export(IsDebug, outputPath);
        }
    }
}
