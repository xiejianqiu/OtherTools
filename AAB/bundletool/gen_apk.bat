SET AAB_NAME=107_FRXX_KOREA_android_kr_1.0.0.1_1.0.7_7_dtrue_vtrue_efalse.aab
SET APK_PATH=my_app.apks
DEL %APK_PATH%
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH%
REM adb connect 127.0.0.1:5555
REM java -jar bundletool-all-1.8.0.jar build-apks --bundle=%AAB_NAME% --output=%APK_PATH% --connected-device
java -jar bundletool-all-1.8.0.jar build-apks  --mode=universal --bundle=%AAB_NAME% --output=%APK_PATH%
REM java -jar bundletool-all-1.8.0.jar install-apks --apks=%APK_PATH%
REM java -jar bundletool-all-x.x.x.jar build-apks --bundle=/path/to/your/app.aab --output=/path/to/output.apks --mode=universal --ks=/path/to/your/keystore.jks --ks-pass=pass:your-keystore-password --ks-key-alias=your-key-alias --key-pass=pass:your-key-password

PAUSE