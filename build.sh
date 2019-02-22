#! /bin/bash
msBuildVersion='15.0'
outputFolder='./_output'
outputFolderLinux='./_output_linux'
outputFolderMacOS='./_output_macos'
outputFolderMacOSApp='./_output_macos_app'
testPackageFolder='./_tests/'
testSearchPattern='*.Test/bin/x86/Release/*'
sourceFolder='./src'
slnFile=$sourceFolder/Lidarr.sln
updateFolder=$outputFolder/Lidarr.Update
updateFolderMono=$outputFolderLinux/Lidarr.Update

#Artifact variables
artifactsFolder="./_artifacts";
artifactsFolderWindows=$artifactsFolder/windows
artifactsFolderLinux=$artifactsFolder/linux
artifactsFolderMacOS=$artifactsFolder/macos
artifactsFolderMacOSApp=$artifactsFolder/macos-app

nuget='tools/nuget/nuget.exe';
vswhere='tools/vswhere/vswhere.exe';

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

ProgressStart()
{
    echo "Start '$1'"
}

ProgressEnd()
{
    echo "Finish '$1'"
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

    echo "Removing vshost files"
    find $path -name "*.vshost.exe" -exec rm "{}" \;

    echo "Removing dylib files"
    find $path -name "*.dylib" -exec rm "{}" \;

    echo "Removing Empty folders"
    find $path -depth -empty -type d -exec rm -r "{}" \;
}

BuildWithMSBuild()
{
    installationPath=`$vswhere -latest -products \* -requires Microsoft.Component.MSBuild -property installationPath`
    installationPath=${installationPath/C:\\/\/c\/}
    installationPath=${installationPath//\\/\/}
    msBuild="$installationPath/MSBuild/$msBuildVersion/Bin"
    echo $msBuild

    export PATH=$msBuild:$PATH
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Clean //m
    $nuget restore $slnFile
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Build //m //p:AllowedReferenceRelatedFileExtensions=.pdb
}

BuildWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode msbuild /p:Configuration=Debug /t:Clean $slnFile
    CheckExitCode msbuild /p:Configuration=Release /t:Clean $slnFile
    mono $nuget restore $slnFile
    CheckExitCode msbuild /p:Configuration=Release /p:Platform=x86 /t:Build /p:AllowedReferenceRelatedFileExtensions=.pdb $slnFile
}

LintUI()
{
    ProgressStart 'ESLint'
    CheckExitCode yarn eslint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    if [ $runtime = "dotnet" ] ; then
        CheckExitCode yarn stylelint-windows
    else
        CheckExitCode yarn stylelint-linux
    fi
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder

    if [ $runtime = "dotnet" ] ; then
        BuildWithMSBuild
    else
        BuildWithXbuild
    fi

    CleanFolder $outputFolder false

    echo "Removing Mono.Posix.dll"
    rm $outputFolder/Mono.Posix.dll

    echo "Adding LICENSE.md"
    cp LICENSE.md $outputFolder

    ProgressEnd 'Build'
}

RunGulp()
{
    ProgressStart 'yarn install'
    yarn install
    #npm-cache install npm || CheckExitCode npm install --no-optional --no-bin-links
    ProgressEnd 'yarn install'

    LintUI

    ProgressStart 'Running gulp'
    CheckExitCode yarn run build --production
    ProgressEnd 'Running gulp'
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
    ProgressStart 'Creating Mono Package'

    rm -rf $outputFolderLinux

    echo "Copying Binaries"
    cp -r $outputFolder $outputFolderLinux

    echo "Creating MDBs"
    CreateMdbs $outputFolderLinux

    echo "Removing PDBs"
    find $outputFolderLinux -name "*.pdb" -exec rm "{}" \;

    echo "Removing Service helpers"
    rm -f $outputFolderLinux/ServiceUninstall.*
    rm -f $outputFolderLinux/ServiceInstall.*

    echo "Removing native windows binaries Sqlite, fpcalc"
    rm -f $outputFolderLinux/sqlite3.*
    rm -f $outputFolderLinux/fpcalc*

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $outputFolderLinux

    echo "Renaming Lidarr.Console.exe to Lidarr.exe"
    rm $outputFolderLinux/Lidarr.exe*
    for file in $outputFolderLinux/Lidarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing Lidarr.Windows"
    rm $outputFolderLinux/Lidarr.Windows.*

    echo "Adding Lidarr.Mono to UpdatePackage"
    cp $outputFolderLinux/Lidarr.Mono.* $updateFolderMono

    ProgressEnd 'Creating Mono Package'
}

PackageMacOS()
{
    ProgressStart 'Creating MacOS Package'

    rm -rf $outputFolderMacOS
    mkdir $outputFolderMacOS

    echo "Adding Startup script"
    cp ./macOS/Lidarr $outputFolderMacOS
    dos2unix $outputFolderMacOS/Lidarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOS
    cp $outputFolder/fpcalc $outputFolderMacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOS

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    ProgressStart 'Creating macOS App Package'

    rm -rf $outputFolderMacOSApp
    mkdir $outputFolderMacOSApp
    cp -r ./macOS/Lidarr.app $outputFolderMacOSApp
    mkdir -p $outputFolderMacOSApp/Lidarr.app/Contents/MacOS

    echo "Adding Startup script"
    cp ./macOS/Lidarr $outputFolderMacOSApp/Lidarr.app/Contents/MacOS
    dos2unix $outputFolderMacOSApp/Lidarr.app/Contents/MacOS/Lidarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOSApp/Lidarr.app/Contents/MacOS
    cp $outputFolder/fpcalc $outputFolderMacOSApp/Lidarr.app/Contents/MacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOSApp/Lidarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $outputFolderMacOSApp/Lidarr.app/Contents/MacOS/Lidarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageTests()
{
    ProgressStart 'Creating Test Package'

    rm -rf $testPackageFolder
    mkdir $testPackageFolder

    find . -maxdepth 6 -path $testSearchPattern -exec cp -r "{}" $testPackageFolder \;

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.7.0 -Output $testPackageFolder
    else
        nuget install NUnit.ConsoleRunner -Version 3.7.0 -Output $testPackageFolder
    fi

    cp $outputFolder/*.dll $testPackageFolder
    cp $outputFolder/*.exe $testPackageFolder
    cp $outputFolder/fpcalc $testPackageFolder
    cp ./*.sh $testPackageFolder

    echo "Creating MDBs for tests"
    CreateMdbs $testPackageFolder

    rm -f $testPackageFolder/*.log.config

    CleanFolder $testPackageFolder true

    echo "Adding Lidarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Lidarr.Core.dll.config $testPackageFolder

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $testPackageFolder

    echo "Copying CurlSharp libraries"
    cp $sourceFolder/ExternalModules/CurlSharp/libs/i386/* $testPackageFolder

    echo "Copying dylibs"
    cp -r $outputFolderMacOS/*.dylib $testPackageFolder

    ProgressEnd 'Creating Test Package'
}

CleanupWindowsPackage()
{
    ProgressStart 'Cleaning Windows Package'

    echo "Removing Lidarr.Mono"
    rm -f $outputFolder/Lidarr.Mono.*

    echo "Adding Lidarr.Windows to UpdatePackage"
    cp $outputFolder/Lidarr.Windows.* $updateFolder

    echo "Removing MacOS fpcalc"
    rm $outputFolder/fpcalc

    ProgressEnd 'Cleaning Windows Package'
}

PackageArtifacts()
{
    echo "Creating Artifact Directories"
    
    rm -rf $artifactsFolder
    mkdir $artifactsFolder
    
    mkdir $artifactsFolderWindows
    mkdir $artifactsFolderMacOS
    mkdir $artifactsFolderLinux
    mkdir $artifactsFolderWindows/Lidarr
    mkdir $artifactsFolderMacOS/Lidarr
    mkdir $artifactsFolderLinux/Lidarr
    mkdir $artifactsFolderMacOSApp
    
    cp -r $outputFolder/* $artifactsFolderWindows/Lidarr
    cp -r $outputFolderMacOSApp/* $artifactsFolderMacOSApp
    cp -r $outputFolderMacOS/* $artifactsFolderMacOS/Lidarr
    cp -r $outputFolderLinux/* $artifactsFolderLinux/Lidarr
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        runtime="dotnet"
        ;;
    *)
        # otherwise use mono
        runtime="mono"
        ;;
esac

Build
RunGulp
PackageMono
PackageMacOS
PackageMacOSApp
PackageTests
CleanupWindowsPackage
PackageArtifacts
