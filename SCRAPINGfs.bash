#/bin/bash

lib1='./packages/FSharp.Data/lib/net40/FSharp.Data.dll'
lib2='./packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll'

declare -a arr=(

    '--nologo'
    '--simpleresolution'
    '--resident'

    '-r:'$lib1
    '-r:'$lib2

    '-I:./packages/FSharp.Data/lib/net40/'
    '-r:FSharp.Data.dll'
    '-r:FSharp.Data.DesignTime.dll'

)
echo ${arr[@]}

if [ -e ./bin ]
then
    rm -rf ./bin
fi

mkdir bin
cp $lib1 ./bin
cp $lib2 ./bin
