@echo off
cd %~dp0

CALL dnu pack --configuration release src\TwentyTwenty.Storage
CALL dnu pack --configuration release src\TwentyTwenty.Storage.Azure
CALL dnu pack --configuration release src\TwentyTwenty.Storage.Amazon
CALL dnu pack --configuration release src\TwentyTwenty.Storage.Local

mkdir artifacts 2> NUL
mkdir artifacts\pack 2> NUL

move /y "src\TwentyTwenty.Storage\bin\release\*.nupkg" "artifacts\pack\"
move /y "src\TwentyTwenty.Storage.Azure\bin\release\*.nupkg" "artifacts\pack\"
move /y "src\TwentyTwenty.Storage.Amazon\bin\release\*.nupkg" "artifacts\pack\"
move /y "src\TwentyTwenty.Storage.Local\bin\release\*.nupkg" "artifacts\pack\"