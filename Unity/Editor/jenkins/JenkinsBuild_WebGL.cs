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
            if (!string.IsNullOrEmpty(output))
            {
                WXEditorWindow.dst = output;
            }
            window.OnEnable();
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
            string outputPath = mPargmaTypeCheck.GetValue(PargmaType.outputPath);
            WXEditorWindow.Export(IsDebug, outputPath);
        }
    }
}
