SET AAB_NAME=107_FRXX_KOREA_android_kr_1.0.0.1_1.0.7_7_dtrue_vtrue_efalse.aab
SET APK_PATH=my_app.apks
SET JKS=key_Korea.jks
SET ALIAS=bellatorm
SET PASSWORD=gamerepublickorlodastra
SET SIGN_PARGMA=--ks=%JKS% --ks-key-alias=%ALIAS% --ks-pass=pass:%PASSWORD% --key-pass=pass:%PASSWORD% 
DEL %APK_PATH%
java -jar bundletool-all-1.8.0.jar build-apks --mode=universal --bundle=%AAB_NAME% --output=%APK_PATH% %SIGN_PARGMA%
PAUSE