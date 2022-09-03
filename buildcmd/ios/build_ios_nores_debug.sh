#! /bin/bash
################ Path Configure ################
export PATH=/usr/local/bin:$PATH
CURPATH=$(cd $(dirname $0);pwd)
PROJ_PATH=/Volumes/SSD/dlc4_ios/shengwang
UNITY_PATH=/Volumes/SSD/Applications/Unity/Unity.app/Contents/MacOS/Unity
OUT_PUT_DIR=$HOME/Desktop/Build/iOS
RELEASE_DIR=/Volumes/SSD/www/ws/iOS/dlc4
XCODE_PROJECT_PATH=/Volumes/SSD/dlc4_ios/xcodeproject

################ Your Command ################
echo ''
echo "****************** Your Command: *************************"
echo "Build ProjectC iOS $*, `date`"
echo "*********************************************************"
echo ''

################ Delete Old ################
# echo "Start Clean...    `date`"
# rm -rf $PROJ_PATH/assets
# rm -rf $PROJ_PATH/ProjectSettings
#rm -rf $PROJ_PATH/BuildLog
rm -rf $OUT_PUT_DIR/$1#
rm -rf $XCODE_PROJECT_PATH
rm $PROJ_PATH/Library/ScriptAssemblies/*
#rm -rf $PROJ_PATH/Assets/ResMS/UI/Atlas/TPProject
# echo "Clean Done.   # `date`"#

echo "Create Path ...    `date`"
mkdir -p $OUT_PUT_DIR
mkdir -p $XCODE_PROJECT_PATH
echo "Create Path Done.    `date`"

################ Check Out From  SVN ################
echo "Start update...    `date`"
cd $PROJ_PATH
#svn cleanup
#svn revert -R .
#svn update  --username lisongyu --password 25285 --no-auth-cache || exit
echo "Update Done.    `date`"

version="$(date +%Y%m%d%H%M)"
echo ${version} > ${PROJ_PATH}"/version"
echo $XCODE_PROJECT_PATH> $PROJ_PATH"/buildLocation"
################ Build Unity Project ################
echo "Start Build Unity-Xcode Project.    `date`"
rm -rf $PROJ_PATH/Assets/Script/*
cp -rf $PROJ_PATH/hotfix/hotfix/iLScript/* $PROJ_PATH/Assets/Script/
$UNITY_PATH -projectPath $PROJ_PATH -disable-assembly-updater -executeMethod PlayerGenerator.BuildPlayerIOSNoResDebug -quit -batchmode -logFile ${XCODE_PROJECT_PATH}/BuildLog/build_ios.log || exit
echo "Build Unity-Xcode Project Done.    `date`"

security unlock-keychain -p "111111" ~/Library/Keychains/login.keychain

################ Build Xcode PROJECT ################
echo "Start Build Xcode Project.    `date`"
cd $XCODE_PROJECT_PATH
echo "Start Build Xcode Project -- 1.    `date`"
#xcodebuild clean
xcodebuild || exit
echo "Start Build Xcode Project -- 2.    `date`"

ipa_name="ws_chujian_""$(date +%Y.%m.%d_%H%M)""_debug.ipa"
echo "Start Build Xcode Project -- 3.    `date`"
xcrun -sdk iphoneos PackageApplication -v ${XCODE_PROJECT_PATH}/build/Release-iphoneos/*.app -o ${OUT_PUT_DIR}/${ipa_name}
echo "Start Build Xcode Project -- 4 .    `date`"
echo "Build Xcode Project Done.    `date`"

# scp ${OUT_PUT_DIR}/${ipa_name}/${ipa_name}.ipa ${SCP_SERVER}
# scp ${PROJ_PATH}/Assets/StreamingAssets/ResFile/*.* ${SCP_SERVER}/proc/ios_beta

echo "begin copy $ipa_name to $RELEASE_DIR"
cd $OUT_PUT_DIR
mv $ipa_name $RELEASE_DIR/


echo "Done!"

exit 0
