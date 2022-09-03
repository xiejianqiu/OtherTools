#!/bin/bash

SVN_PATH=http://195.168.5.200/SVN/TaiGuShenWang/Project/shengwang
PROJ_PATH=//Volumes/SSD/dlc4_android/shengwang
UNITY_PATH=/Volumes/SSD/Applications/Unity/Unity.app/Contents/MacOS/Unity
OUT_PUT_DIR=$PROJ_PATH/PatchBuild
DLL_OUT_PUT_DIR=$OUT_PUT_DIR/pathdll
DLL_BASE_DIR=$OUT_PUT_DIR/basedll
DLL_NEW_DIR=$OUT_PUT_DIR/newdll
FILE_OUT_PUT_DIR=$OUT_PUT_DIR/Res
DIR_SAVE_PATCHFILE=/Volumes/SSD/dlc4_android/patch/
#DIR_LAST_VERSION=$PROJ_PATH/LastAppVersion/android/abdetail.info
DIR_MANAGED=$PROJ_PATH/AndroidSDK/BuildModelProject/tgsw/src/main/assets/bin/Data/Managed
DIR_PATCHHISTORY=$PROJ_PATH/PatchHistory/android/20210716


echo "Create Path ...    `date`"
rm -rf $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE/Res
rm -rf $OUT_PUT_DIR
mkdir -p $OUT_PUT_DIR
mkdir -p $FILE_OUT_PUT_DIR
cd $OUT_PUT_DIR
echo "Create Path Done.    `date`"

echo "Start Copy the Base version files... `date`" 
mkdir -p $DLL_BASE_DIR
#cp $DIR_PATCHHISTORY/*.dll $DLL_BASE_DIR/
cp $DIR_PATCHHISTORY/abdetail.info $OUT_PUT_DIR/baseabdetail
#cp $DIR_PATCHHISTORY/dep.all $OUT_PUT_DIR/basedep
echo "Copy Done."

echo "Start Copy the New version files... `date`" 
mkdir -p $DLL_NEW_DIR
cp $DIR_MANAGED/*.dll $DLL_NEW_DIR/
cp $PROJ_PATH/Assets/abdetail.info $OUT_PUT_DIR/newabdetail
cp $PROJ_PATH/Assets/StreamingAssets/AssetBundles/dep.all  $OUT_PUT_DIR/newdep
echo "Copy Done"

mkdir -p $DLL_OUT_PUT_DIR
echo "Start Build UpdateInfo $2  `date`"
$UNITY_PATH -projectPath $PROJ_PATH -executeMethod PatchBuilder.BuildPatchFile  -quit -batchmode -logFile ${PROJ_PATH}/BuildLog/build_android.log || exit
echo "Build Patch file Success.."


echo "Start copy patch files..."
rm -rf $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE/Res
cp $OUT_PUT_DIR/UpdateInfo.txt $DIR_SAVE_PATCHFILE/
cp $OUT_PUT_DIR/UpdateInfo.txt $DIR_SAVE_PATCHFILE/
cp $OUT_PUT_DIR/dep.all $DIR_SAVE_PATCHFILE/Res/dep.all
cp $DLL_OUT_PUT_DIR/*.dll $DIR_SAVE_PATCHFILE/Res/
cp $FILE_OUT_PUT_DIR/*.ab $DIR_SAVE_PATCHFILE/Res/
cp $PROJ_PATH/Assets/StreamingAssets/VersionInfo.txt $DIR_SAVE_PATCHFILE/
echo "Copy Success"
