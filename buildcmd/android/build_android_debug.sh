#! /bin/bash
################# Path Configure ################
#export PATH=/usr/local/bin:'/Applications/Android Studio.app/Contents/gradle/gradle-2.14.1/bin':$PATH
#SVN_PATH=http://195.168.5.200/SVN/TaiGuShenWang/Project/shengwang
CURPATH=$(cd $(dirname $0);pwd)
PROJ_PATH=/Volumes/SSD/dlc4_android/shengwang
UNITY_PATH=/Volumes/SSD/Applications/Unity/Unity.app/Contents/MacOS/Unity

RELEASE_DIR=$HOME/Documents/www/ws/android/dlc4
JH_DIR=$HOME/CJSDK/jh_tool_new
ANDROID_PROJECT_PATH=$PROJ_PATH/AndroidSDK/BuildModelProject/tgsw
#OUT_PUT_DIR=$ANDROID_PROJECT_PATH/output
OUT_PUT_DIR=$ANDROID_PROJECT_PATH/build/outputs/apk/chujian/release
SCP_SERVER=shengwang@192.168.5.200:/太古神王共享/Demo/iOS
FTP_SERVER=192.168.5.200
USER="FTP_Admin"
PASSWORD="Password11"
FTP_PATH="/shengwang"

################ Your Command ################
echo ''
echo "****************** Your Command: *************************"
echo "Build ProjectC Android $*, `date`"
echo "*********************************************************"
echo ''

################ Delete Old ################
# echo "Start Clean...    `date`"
#rm -rf $PROJ_PATH/assets
#rm -rf $PROJ_PATH/ProjectSettings
rm $PROJ_PATH/BuildLog
rm -rf $OUT_PUT_DIR
echo "Clean Done.    `date`"
rm $PROJ_PATH/Library/ScriptAssemblies/*

killall Unity

echo "Create Path ...    `date`"
rm -rf $OUT_PUT_DIR
mkdir -p $OUT_PUT_DIR
echo "Create Path Done.    `date`"

# ################ Check Out From  SVN ################
echo "Start update...    `date`"
cd $PROJ_PATH
svn cleanup
svn revert -R .
svn update  || exit
echo "Update Done.    `date`"

version="$(date +%Y%m%d)"
FILE_NAME="Demo_""$(date +%Y.%m.%d)"".apk"
echo ${version} > ${PROJ_PATH}"/version"
echo $OUT_PUT_DIR/$FILE_NAME > $PROJ_PATH"/buildLocation"
################ Build Android Package ################
echo "Start Build Android Package.    `date`"
rm -rf $PROJ_PATH/Assets/Script/*
cp -rf $PROJ_PATH/hotfix/hotfix/iLScript/* $PROJ_PATH/Assets/Script/
$UNITY_PATH -projectPath $PROJ_PATH -executeMethod PlayerGenerator.ExportGoogleProjectDebug -quit -batchmode -logFile ${PROJ_PATH}/BuildLog/build_android.log || exit

#进入安卓工程目录
cd  $ANDROID_PROJECT_PATH
rm -f src/main/assets/bin/Data/Managed/*.mdb
rm -f src/main/assets/bin/Data/Managed/*.pdb
#dll加密，复制解密的mono库，用于安卓代码热更
chmod +x Encry
./Encry
cp -f libmonobdwgc-2.0.so src/main/jniLibs/armeabi-v7a

#开始gradle继承
/opt/gradle/gradle-5.6.4/bin/gradle clean
/opt/gradle/gradle-5.6.4/bin/gradle aR
echo "Build Android Package Done.    `date`"

# scp ${OUT_PUT_DIR}/${packageName}/${packageName}.apk ${SCP_SERVER}
# #上传版本文件到服务器
# scp ${PROJ_PATH}/Assets/StreamingAssets/ResFile/*.* ${SCP_SERVER}/proc/android_beta

cd $OUT_PUT_DIR
rm -rf ${JH_DIR}/input/tg2/chujian.apk
rm -rf ${JH_DIR}/output/tg2/oppo/tg2_oppo*
cp tgsw* ${JH_DIR}/input/tg2/chujian.apk
cp tgsw* $RELEASE_DIR/"ws_juhe_debug_""$(date +%Y.%m.%d)"".apk"
cd $JH_DIR
python ./scripts/pack.py -g 14 -c oppo -r

cp ${JH_DIR}/output/tg2/oppo/tg2_oppo* $RELEASE_DIR/"ws_oppo_debug_""$(date +%Y.%m.%d)"".apk"

#cd $RELEASE_DIR
#ln -f $FILE_NAME "Demo.apk"

#ftp -n <<- EOF
#open 192.168.5.200
#user FTP_Admin Password11
#cd "/"
#bin
#put "Demo_""$(date +%Y.%m.%d)"".apk"
#put "Demo_""$(date +%Y.%m.%d)"".apk" "Demo.apk"
#bye
10
echo "Done!"
echo "Version Auto build succeed !  `date`"

exit 0
