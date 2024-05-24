SET AAB_NAME=tgsw1.aab
SET APK_PATH=my_app.apks
DEL %APK_PATH%
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH%
java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH% --connected-device
adb.exe connect 127.0.0.1 5555
java -jar bundletool-all-1.8.0.jar install-apks --apks=%APK_PATH%
PAUSE