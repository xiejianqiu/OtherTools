#!/bin/bash

PROJ_PATH=/Volumes/SSD/dlc4_ios/shengwang
UNITY_PATH=/Volumes/SSD/Applications/Unity/Unity.app/Contents/MacOS/Unity
OUT_PUT_DIR=$PROJ_PATH/PatchBuild
FILE_OUT_PUT_DIR=$OUT_PUT_DIR/Res
DIR_SAVE_PATCHFILE=/Volumes/SSD/dlc4_ios/patch
DIR_PATCHHISTORY=$PROJ_PATH/PatchHistory/ios/20210728

echo "Step 01 ---> Create Path `date`"       
rm -rf $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE
mkdir -p $DIR_SAVE_PATCHFILE/Res
rm -rf $OUT_PUT_DIR
mkdir -p $OUT_PUT_DIR
mkdir -p $FILE_OUT_PUT_DIR
echo "Step 01 Done!"

echo "Step 02 ---> Start Copy the Base version files... `date`" 
cd $OUT_PUT_DIR
cp $DIR_PATCHHISTORY/abdetail.info $OUT_PUT_DIR/baseabdetail
echo "Step 02 Done!"

echo "Step 03 ---> Start Copy the New version files... `date`" 
cp $PROJ_PATH/Assets/abdetail.info $OUT_PUT_DIR/newabdetail
cp $PROJ_PATH/Assets/StreamingAssets/AssetBundles/dep.all  $OUT_PUT_DIR/newdep
echo "Step 03 Done!"

echo "Step 04 --->Start Build UpdateInfo $2  `date`"
$UNITY_PATH -projectPath $PROJ_PATH -executeMethod PatchBuilder.BuildPatchFile  -quit -batchmode -logFile ${PROJ_PATH}/BuildLog/build_ios.log || exit
echo "Step 04 Done!"

echo "Step 05 ---> copy patch files..."
cp $OUT_PUT_DIR/UpdateInfo.txt $DIR_SAVE_PATCHFILE/
cp $OUT_PUT_DIR/dep.all $DIR_SAVE_PATCHFILE/Res/dep.all
cp $FILE_OUT_PUT_DIR/*.ab $DIR_SAVE_PATCHFILE/Res/
cp $PROJ_PATH/Assets/StreamingAssets/VersionInfo.txt $DIR_SAVE_PATCHFILE/
echo "Step 05 Done!"
