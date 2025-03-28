REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=jenkins
SET PWD=jenkins
MKDIR PC
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/TG3QQ/QQBaseTask > PC/BaseTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/TG3QQ/QQResTask > PC/BuildResTask.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.32.77:9000/ -webSocket get-job ClientBuild/TG3QQ/QQVersionTask> PC/BuildPkG.xml
