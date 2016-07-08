#/bin/bash

lib1='./packages/FSharp.Data/lib/net40/FSharp.Data.dll'
lib2='./packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll'
lib3='./packages/Persimmon.Console/tools/Persimmon.dll'
lib4='/Users/kohei/Documents/myProgramming/myFSharp/SCRAPINGfs/SCRAPINGfs.dll'


declare -a arr=(
    '-a'
    '--nologo'
    '--simpleresolution'
    '--resident'
    '-r:'$lib1
    '-r:'$lib2
    '-r:'$lib3
    '-r:'$lib4
)

echo ${arr[@]}

if [ -e ./bin ]
then
    rm -rf ./bin
fi

mkdir bin
cp $lib1 ./bin
cp $lib2 ./bin
cp $lib4 ./bin

