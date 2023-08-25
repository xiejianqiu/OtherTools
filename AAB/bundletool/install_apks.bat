SET AAB_NAME=124_korea_GooglePlay_1.0.7_signed.aab
SET APK_PATH=my_app.apks
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH%
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH% --connected-device
java -jar bundletool-all-1.8.0.jar install-apks --apks=%APK_PATH%

PAUSE