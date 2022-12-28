REM Platform,//Android,iOS,WebGL
REM BuildRes,//构建打包所有资源
REM resVer,//资源版本号
REM IsDebug,//是否为debug包
REM IsAAB,//是否为AAB包
REM IsBuildPkg, //构建安装包或者导出xcode或者webgl工程
REM versionName,//版本号
REM versionCode,//版本Code
REM pkgType,//0：整包（带有全部资源），1：中包（带部分资源），2：小包（不带资源）
SET CUR_PATH=%~dp0
SET UnityExe="C:\Program Files\Unity20180419f1\Editor\Unity.exe"
SET PROJECT_PATH=%CUR_PATH%..\
SET Method=Jenkins.JenkinsBuild.BuildMain
set Log_OUPUT_File=%CUR_PATH%%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%.log
SET Platform=WebGL
SET BuildRes=false
SET resVer=1.0.0.0
SET IsDebug=true
SET IsBuildPkg=true
SET versionName=1.0.0
SET versionCode=1
SET pkgType=2
SET outputPath=%CUR_PATH%..\webgame
rd /S /Q %outputPath%
mkdir %outputPath%
SET CUSTOM_PARGAM=Platform %Platform% BuildRes %BuildRes% resVer %resVer% IsDebug %IsDebug% IsBuildPkg %IsBuildPkg% versionName %versionName% versionCode %versionCode%  pkgType %pkgType% outputPath %outputPath%
call %UnityExe% -projectPath %PROJECT_PATH% -batchMode  -disable-assembly-updater -executeMethod %Method% -quit -batchmode -logFile %Log_OUPUT_File% %CUSTOM_PARGAM%
ECHO "构建完毕!!!!"