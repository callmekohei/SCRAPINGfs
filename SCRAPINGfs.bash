#/bin/bash

declare -a arr=(

    '--nologo'
    '--simpleresolution'
    '--resident'

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
cp './packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll' ./bin
cp './packages/FSharp.Data/lib/net40/FSharp.Data.dll' ./bin

