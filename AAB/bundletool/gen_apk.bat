SET AAB_NAME=288_vietnam_GooglePlay_1.1.8.aab
SET APK_PATH=my_app.apks
DEL %APK_PATH%
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH%
java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH% --connected-device
java -jar bundletool-all-1.8.0.jar install-apks --apks=%APK_PATH%
PAUSE