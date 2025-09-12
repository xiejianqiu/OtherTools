using UnityEditor;
using WeChatWASM;
namespace WeChatWASM
{
    //public partial class WXEditorWindow : EditorWindow
    //{
    //    public static void Export(bool IsDebug, string output, int assetLoadType) {

    //        WXEditorWindow window = new WXEditorWindow();
    //        window.developBuild = IsDebug;
    //        window.profilingFuncs = IsDebug;
    //        window.autoProfile = IsDebug;
    //        window.assetLoadType = assetLoadType;
    //        window.OnEnable();
    //        if (!string.IsNullOrEmpty(output))
    //        {
    //            WXEditorWindow.dst = output;
    //        }
    //        window.DoExport(true);
    //    }

    //}
}
namespace Jenkins
{
    
    public partial class JenkinsBuild
    {
        private static void BuildWebGl()
        {
            bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
            string outputPath = mPargmaTypeCheck.GetValue(PargmaType.OutputPath);
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = $"webgame{(EnvUtils.IsDEVELOPMENT_BUILD() ? "_debug" : "_release")}";
            }
            int assetLoadType = 0;
            //if(int.TryParse(mPargmaTypeCheck.GetValue(PargmaType.AssetLoadType), out assetLoadType))
            //WXEditorWindow.Export(IsDebug, outputPath, assetLoadType);
        }
    }
}
