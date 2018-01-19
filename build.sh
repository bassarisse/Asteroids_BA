#!/bin/bash

GAME_NAME=${PWD##*/}
MAIN_PATH=~/Desktop
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity
FINAL_PATH=$MAIN_PATH/$GAME_NAME

CURRENT_PATH=$(pwd)

rm -rf $FINAL_PATH
mkdir $FINAL_PATH

cd $FINAL_PATH

mkdir linux
mkdir win32
mkdir mac

cd $CURRENT_PATH

cp -f ./ProjectSettings/InputManager-linux.asset ./ProjectSettings/InputManager.asset
$UNITY_PATH -quit -batchmode -buildLinux32Player $FINAL_PATH/linux/$GAME_NAME.x86
cp -f ./ProjectSettings/InputManager-win32.asset ./ProjectSettings/InputManager.asset
$UNITY_PATH -quit -batchmode -buildWindowsPlayer $FINAL_PATH/win32/$GAME_NAME.exe
cp -f ./ProjectSettings/InputManager-mac.asset ./ProjectSettings/InputManager.asset
$UNITY_PATH -quit -batchmode -buildOSXUniversalPlayer $FINAL_PATH/mac/$GAME_NAME.app

cd $FINAL_PATH

rm -rf win32/*.pdb

cd $FINAL_PATH/linux
zip -r -X -q ../$GAME_NAME-linux *
cd $FINAL_PATH/win32
zip -r -X -q ../$GAME_NAME-win32 *
cd $FINAL_PATH/mac
zip -r -X -q ../$GAME_NAME-mac *

cd $FINAL_PATH

rm -rf linux
rm -rf win32
rm -rf mac