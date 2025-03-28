REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=jenkins
SET PWD=jenkins
MKDIR Mobile
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/tg2ovquick/buildAndroid > Mobile/AndroidTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/tg2ovquick/iosbuild > Mobile/IosTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/tg2ovquick/resautobuild > Mobile/BuildResTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/tg2ovquick/autobuildandroidpkg > Mobile/BuildAndroidPkg.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/tg2ovquick/iosverbuild > Mobile/BuildIosPkg.xml
