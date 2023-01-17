SET USER=jenkins
SET PWD=jenkins
MKDIR Test
REM java -jar jenkins-cli.jar -s http://localhost:9000/ -webSocket create-job NAME
REM java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.3.195:9000/ -webSocket create-job Test/AutoCreateJob < Test/BuildProject.xml
REM java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.3.195:9000/ -webSocket create-job Test/tchineseIos < taigu2/TChinese/tchineseIos.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.3.195:9000/ -webSocket create-job Test/tchineseandroid2 < taigu2/TChinese/tchineseandroid2.xml
pause
pause