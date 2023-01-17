REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=jenkins
SET PWD=jenkins
MKDIR Test
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.3.195:9000/ -webSocket get-job Test/BuildProject_IOS > Test/BuildProject_IOS.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.3.195:9000/ -webSocket get-job Test/NewBuildProject > Test/NewBuildProject.xml