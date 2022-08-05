using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSBuildGame;

public class JenkinsBuild
{
    private static PargmaTypeCheck mPargmaTypeCheck = new PargmaTypeCheck();
    public static void Build() {
        mPargmaTypeCheck.Init(Environment.GetCommandLineArgs());
        bool buildRes = mPargmaTypeCheck.IsTrue(PargmaType.BRes);
        bool IsDebug = mPargmaTypeCheck.IsTrue(PargmaType.IsDebug);
        bool IsAAB = mPargmaTypeCheck.IsTrue(PargmaType.IsAAB);
        bool IsMin = mPargmaTypeCheck.IsTrue(PargmaType.IsMin);
        if (IsAAB) {
            PlayerGenerator.Is_Build_Min_AAB = IsMin;
        }
        else {
            PlayerGenerator.Is_Build_Min_APK = IsMin;
        }
        PlayerGenerator.ExportGoogleProject(IsDebug, buildRes, IsAAB, false);
    }
}
