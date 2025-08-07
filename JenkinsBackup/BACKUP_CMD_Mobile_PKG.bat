REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=jenkins
SET PWD=jenkins
SET SAVEDIR=Mobile_PKG
MKDIR %SAVEDIR%
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/Dot1_QY/androidbuild > %SAVEDIR%/AndroidTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/Dot1_QY/iosbuild > %SAVEDIR%/IosTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/Dot1_QY/autobuild > %SAVEDIR%/BuildResTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/Dot1_QY/androidVerBuild > %SAVEDIR%/BuildAndroidPkg.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/Dot1_QY/iosVerBuild > %SAVEDIR%/BuildIosPkg.xml
