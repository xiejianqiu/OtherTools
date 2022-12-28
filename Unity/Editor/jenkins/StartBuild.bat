REM Platform,//Android,iOS,WebGL
REM BuildRes,//构建打包所有资源
REM ResVer,//资源版本号
REM IsDebug,//是否为debug包
REM IsAAB,//是否为AAB包
REM IsBuildPkg, //构建安装包或者导出xcode或者webgl工程
REM VersionName,//版本号
REM VersionCode,//版本Code
REM PkgType,//0：整包（带有全部资源），1：中包（带部分资源），2：小包（不带资源）
REM OutputPath,//导出路径
REM RemoveManifest,//是否删除资源清单
SET CUR_PATH=%~dp0
SET UnityExe="C:\Program Files\Unity20180419f1\Editor\Unity.exe"
SET PROJECT_PATH=%CUR_PATH%..\
SET Method=Jenkins.JenkinsBuild.BuildMain
set Log_OUPUT_File=%CUR_PATH%%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%.log
SET Platform=WebGL
SET BuildRes=true
SET ResVer=1.0.0.0
SET IsDebug=true
SET IsBuildPkg=false
SET VersionName=1.0.0
SET VersionCode=1
SET PkgType=2
SET OutputPath=%CUR_PATH%..\webgame
SET RemoveManifest=false
rd /S /Q %outputPath%
mkdir %outputPath%
SET CUSTOM_PARGAM=Platform %Platform% BuildRes %BuildRes% ResVer %ResVer% IsDebug %IsDebug% IsBuildPkg %IsBuildPkg% VersionName %VersionName% VersionCode %VersionCode%  PkgType %PkgType% OutputPath %OutputPath% RemoveManifest %RemoveManifest%
call %UnityExe% -projectPath %PROJECT_PATH% -batchMode  -disable-assembly-updater -executeMethod %Method% -quit -batchmode -logFile %Log_OUPUT_File% %CUSTOM_PARGAM%
ECHO "构建完毕!!!!"