#/bin/bash

file='SCRAPINGfs'
lib1='./packages/FSharp.Data/lib/net40/FSharp.Data.dll'
lib2='./packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll'

declare -a arr=(
    'fsharpc'
    '-a'
    '--nologo'
    '--simpleresolution'

    '-I:'${lib1%/*.*}/

    '-r:'${lib1##./*/}
    '-r:'${lib2##./*/}

    $file'.fsx'
)
${arr[@]}


