REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -webSocket help
REM java -jar jenkins-cli.jar -s http://192.168.15.211:8080/ -auth common:111111 list-jobs
SET USER=cj
SET PWD=cj888!
MKDIR taigu2\China\
REM 国服
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/China/China_tgsw2Android > taigu2/China/China_tgsw2Android.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/China/China_tgsw2Ios > taigu2/China/China_tgsw2Ios.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/China/China_tgsw2Ios136 > taigu2/China/China_tgsw2Ios136.xml

MKDIR taigu2\Korea\
REM 韩国分支
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Korea/koreaAndroid2 > taigu2/Korea/koreaAndroid2.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Korea/koreaAndroid2 > taigu2/Korea/KoreaBuildIos.xml

REM 韩国主干
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Korea/koreaAndroid_tcptest > taigu2/Korea/koreaAndroid_tcptest.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Korea/koreaAndroid2 > taigu2/Korea/KoreaBuildIos2.xml

MKDIR taigu2\Vietnam\
REM  越南
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Vietnam/vietnamAndroid > taigu2/Vietnam/koreaAndroid2.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/Vietnam/vietnamIos > taigu2/Vietnam/KoreaBuildIos.xml

MKDIR taigu2\TChinese\
REM  繁体
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/TChinese/tchineseandroid2 > taigu2/TChinese/tchineseandroid2.xml
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket get-job taigu2/TChinese/tchineseIos > taigu2/TChinese/tchineseIos.xml

REM 导出查找
java -jar jenkins-cli.jar -auth %USER%:%PWD% -s http://192.168.15.211:8080/ -webSocket list-plugins > jenkins_plugins.xml
PAUSE