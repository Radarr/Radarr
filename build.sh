#! /bin/bash
msBuild='/MSBuild/15.0/Bin'
outputFolder='./_output'
outputFolderMono='./_output_mono'
outputFolderOsx='./_output_osx'
outputFolderOsxApp='./_output_osx_app'
testPackageFolder='./_tests/'
testSearchPattern='*.Test/bin/x86/Release'
sourceFolder='./src'
slnFile=$sourceFolder/Radarr.sln
updateFolder=$outputFolder/Radarr.Update
updateFolderMono=$outputFolderMono/Radarr.Update

nuget='tools/nuget/nuget.exe';
CheckExitCode()
{
    "$@"
    local status=$?
    if [ $status -ne 0 ]; then
        echo "error with $1" >&2
        exit 1
    fi
    return $status
}

CleanFolder()
{
    local path=$1
    local keepConfigFiles=$2


    find $path -name "*.transform" -exec rm "{}" \;

    if [ $keepConfigFiles != true ] ; then
        find $path -name "*.dll.config" -exec rm "{}" \;
    fi

    echo "Removing FluentValidation.Resources files"
    find $path -name "FluentValidation.resources.dll" -exec rm "{}" \;
    find $path -name "App.config" -exec rm "{}" \;

    echo "Removing .less files"
    find $path -name "*.less" -exec rm "{}" \;

    echo "Removing vshost files"
    find $path -name "*.vshost.exe" -exec rm "{}" \;

    echo "Removing dylib files"
    find $path -name "*.dylib" -exec rm "{}" \;

    echo "Removing Empty folders"
    find $path -depth -empty -type d -exec rm -r "{}" \;
}



AddJsonNet()
{
    rm $outputFolder/Newtonsoft.Json.*
    cp $sourceFolder/packages/Newtonsoft.Json.*/lib/net35/*.dll $outputFolder
    cp $sourceFolder/packages/Newtonsoft.Json.*/lib/net35/*.dll $outputFolder/NzbDrone.Update
}

BuildWithMSBuild()
{
    export PATH=$msBuild:$PATH
	echo $PATH
    CheckExitCode MSBuild.exe $slnFile //t:Clean //m
    $nuget restore $slnFile
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Build //m //p:AllowedReferenceRelatedFileExtensions=.pdb
}

RestoreNuget()
{
    export MONO_IOMAP=case
    mono $nuget restore $slnFile
}

CleanWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode msbuild /t:Clean $slnFile
}

BuildWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode msbuild /p:Configuration=Release /p:Platform=x86 /t:Build /p:AllowedReferenceRelatedFileExtensions=.pdb /maxcpucount:3 $slnFile
}

LintUI()
{
    echo "ESLint"
    CheckExitCode yarn eslint
    echo "ESLint"

    echo "Stylelint"
    CheckExitCode yarn stylelint
    echo "Stylelint"
}

Build()
{
    echo "Start Build"

    rm -rf $outputFolder

    if [ $runtime = "dotnet" ] ; then
        BuildWithMSBuild
    else
        CleanWithXbuild
        RestoreNuget
        BuildWithXbuild
    fi

    CleanFolder $outputFolder false

    AddJsonNet

    echo "Removing Mono.Posix.dll"
    rm $outputFolder/Mono.Posix.dll

    echo "Finish Build"
}

RunGulp()
{
    echo "Start yarn install"
    yarn install
    echo "Finish yarn install"

    LintUI

    echo "Start Running gulp"
    CheckExitCode yarn run build --production
    echo "Finish Running gulp"
}

CreateMdbs()
{
    local path=$1
    if [ $runtime = "dotnet" ] ; then
        local pdbFiles=( $(find $path -name "*.pdb") )
        for filename in "${pdbFiles[@]}"
        do
          if [ -e ${filename%.pdb}.dll ]  ; then
            tools/pdb2mdb/pdb2mdb.exe ${filename%.pdb}.dll
          fi
          if [ -e ${filename%.pdb}.exe ]  ; then
            tools/pdb2mdb/pdb2mdb.exe ${filename%.pdb}.exe
          fi
        done
    fi
}

PackageMono()
{
    echo "##teamcity[progressStart 'Creating Mono Package']"
    rm -rf $outputFolderMono
    cp -r $outputFolder $outputFolderMono

    echo "Creating MDBs"
    CreateMdbs $outputFolderMono

    echo "Removing PDBs"
    find $outputFolderMono -name "*.pdb" -exec rm "{}" \;

    echo "Removing Service helpers"
    rm -f $outputFolderMono/ServiceUninstall.*
    rm -f $outputFolderMono/ServiceInstall.*

    echo "Removing native windows binaries Sqlite, MediaInfo"
    rm -f $outputFolderMono/sqlite3.*
    rm -f $outputFolderMono/MediaInfo.*

    echo "Adding NzbDrone.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Radarr.Core.dll.config $outputFolderMono

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $outputFolderMono

    echo "Renaming Radarr.Console.exe to Radarr.exe"
    rm $outputFolderMono/Radarr.exe*
    for file in $outputFolderMono/Radarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing Radarr.Windows"
    rm $outputFolderMono/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $outputFolderMono/Radarr.Mono.* $updateFolderMono

    echo "##teamcity[progressFinish 'Creating Mono Package']"
}

PackageOsx()
{
    echo "##teamcity[progressStart 'Creating OS X Package']"
    rm -rf $outputFolderOsx
    cp -r $outputFolderMono $outputFolderOsx

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderOsx

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderOsx

    echo "Adding Startup script"
    cp  ./osx/Radarr $outputFolderOsx

    echo "##teamcity[progressFinish 'Creating OS X Package']"
}

PackageOsxApp()
{
    echo "##teamcity[progressStart 'Creating OS X App Package']"
    rm -rf $outputFolderOsxApp
    mkdir $outputFolderOsxApp

    cp -r ./osx/Radarr.app $outputFolderOsxApp
    cp -r $outputFolderOsx $outputFolderOsxApp/Radarr.app/Contents/MacOS

    echo "##teamcity[progressFinish 'Creating OS X App Package']"
}

PackageTests()
{
    echo "Packaging Tests"
    echo "##teamcity[progressStart 'Creating Test Package']"
    rm -rf $testPackageFolder
    mkdir $testPackageFolder

    find $sourceFolder -path $testSearchPattern -exec cp -r -u -T "{}" $testPackageFolder \;

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.Runners -Version 3.2.1 -Output $testPackageFolder
    else
        mono $nuget install NUnit.Runners -Version 3.2.1 -Output $testPackageFolder
    fi

    cp $outputFolder/*.dll $testPackageFolder
    cp ./*.sh $testPackageFolder

    echo "Creating MDBs for tests"
    CreateMdbs $testPackageFolder

    rm -f $testPackageFolder/*.log.config

    CleanFolder $testPackageFolder true

    echo "Adding Radarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Radarr.Core.dll.config $testPackageFolder

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $testPackageFolder

    echo "Copying CurlSharp libraries"
    cp $sourceFolder/ExternalModules/CurlSharp/libs/i386/* $testPackageFolder

    echo "##teamcity[progressFinish 'Creating Test Package']"
}

CleanupWindowsPackage()
{
    echo "Removing Radarr.Mono"
    rm -f $outputFolder/Radarr.Mono.*

    echo "Adding Radarr.Windows to UpdatePackage"
    cp $outputFolder/Radarr.Windows.* $updateFolder
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        runtime="dotnet"
		vsLoc=$(./tools/vswhere/vswhere.exe -property installationPath)
		vsLoc=$(echo "/$vsLoc" | sed -e 's/\\/\//g' -e 's/://')
		msBuild="$vsLoc$msBuild"
        ;;
    *)
        # otherwise use mono
        runtime="mono"
        ;;
esac

if [ $# -eq 0 ]
  then
    Build
    RunGulp
    PackageMono
    PackageOsx
    PackageOsxApp
    PackageTests
    CleanupWindowsPackage
fi

if [ "$1" = "CleanXbuild" ]
then rm -rf $outputFolder
    CleanWithXbuild
fi

if [ "$1" = "NugetMono" ]
then rm -rf $outputFolder
    RestoreNuget
fi

if [ "$1" = "Build" ]
then BuildWithXbuild
  CleanFolder $outputFolder false
  AddJsonNet
  rm $outputFolder/Mono.Posix.dll
fi

if [ "$1" = "Gulp" ]
then RunGulp
fi

if [ "$1" = "Package" ]
then PackageMono
  PackageOsx
  PackageOsxApp
  PackageTests
  CleanupWindowsPackage
fi
