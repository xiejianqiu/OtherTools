REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=jenkins
SET PWD=jenkins
MKDIR WebGL
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/KoreaWebGame/buildWebPlayer > WebGL/BaseTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/KoreaWebGame/LY100WebGLAutoBuildRes > WebGL/BuildResTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/KoreaWebGame/buildWebPlayer> WebGL/BuildPkG.xml
