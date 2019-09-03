#! /bin/bash
msBuildVersion='15.0'
outputFolder='./_output'
outputFolderLinux='./_output_linux'
outputFolderMacOS='./_output_macos'
outputFolderMacOSApp='./_output_macos_app'
testPackageFolder='./_tests/'
sourceFolder='./src'
slnFile=$sourceFolder/Radarr.sln
updateFolder=$outputFolder/Radarr.Update
updateFolderMono=$outputFolderLinux/Radarr.Update

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

UpdateVersionNumber()
{
    if [ "$RADARRVERSION" != "" ]; then
        echo "Updating Version Info"
        sed -i "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$RADARRVERSION<\/AssemblyVersion>/g" ./src/Directory.Build.props
        sed -i "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BUILD_SOURCEBRANCHNAME}<\/AssemblyConfiguration>/g" ./src/Directory.Build.props
    fi
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
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Debug //p:Platform=x86 //t:Clean //m
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Clean //m
    $nuget locals all -clear
    $nuget restore $slnFile
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Build //m //p:AllowedReferenceRelatedFileExtensions=.pdb
}

BuildWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode msbuild /p:Configuration=Debug /t:Clean $slnFile
    CheckExitCode msbuild /p:Configuration=Release /t:Clean $slnFile
    mono $nuget locals all -clear
    mono $nuget restore $slnFile
    CheckExitCode msbuild /p:Configuration=Release /p:Platform=x86 /t:Build /p:AllowedReferenceRelatedFileExtensions=.pdb $slnFile
}

LintUI()
{
    ProgressStart 'ESLint'
    CheckExitCode yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    CheckExitCode yarn stylelint
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

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
    yarn install --frozen-lockfile
    ProgressEnd 'yarn install'

    LintUI

    ProgressStart 'Running gulp'
    CheckExitCode yarn run build --production
    ProgressEnd 'Running gulp'
}

PackageMono()
{
    ProgressStart 'Creating Mono Package'

    rm -rf $outputFolderLinux

    echo "Copying Binaries"
    cp -r $outputFolder $outputFolderLinux

    echo "Removing Service helpers"
    rm -f $outputFolderLinux/ServiceUninstall.*
    rm -f $outputFolderLinux/ServiceInstall.*

    echo "Removing native windows binaries Sqlite, MediaInfo"
    rm -f $outputFolderLinux/sqlite3.*
    rm -f $outputFolderLinux/MediaInfo.*

    echo "Adding Radarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Radarr.Core.dll.config $outputFolderLinux


    echo "Renaming Radarr.Console.exe to Radarr.exe"
    rm $outputFolderLinux/Radarr.exe*
    for file in $outputFolderLinux/Radarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing Radarr.Windows"
    rm $outputFolderLinux/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $outputFolderLinux/Radarr.Mono.* $updateFolderMono

    ProgressEnd 'Creating Mono Package'
}

PackageMacOS()
{
    ProgressStart 'Creating MacOS Package'

    rm -rf $outputFolderMacOS
    mkdir $outputFolderMacOS

    echo "Adding Startup script"
    cp ./macOS/Radarr $outputFolderMacOS
    dos2unix $outputFolderMacOS/Radarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOS

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderMacOS

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    ProgressStart 'Creating macOS App Package'

    rm -rf $outputFolderMacOSApp
    mkdir $outputFolderMacOSApp
    cp -r ./macOS/Radarr.app $outputFolderMacOSApp
    mkdir -p $outputFolderMacOSApp/Radarr.app/Contents/MacOS

    echo "Adding Startup script"
    cp ./macOS/Radarr $outputFolderMacOSApp/Radarr.app/Contents/MacOS
    dos2unix $outputFolderMacOSApp/Radarr.app/Contents/MacOS/Radarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOSApp/Radarr.app/Contents/MacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOSApp/Radarr.app/Contents/MacOS

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderMacOSApp/Radarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $outputFolderMacOSApp/Radarr.app/Contents/MacOS/Radarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageTests()
{
    ProgressStart 'Creating Test Package'

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder
    else
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder
    fi

    cp ./test.sh $testPackageFolder

    rm -f $testPackageFolder/*.log.config

    CleanFolder $testPackageFolder true

    echo "Adding Radarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Radarr.Core.dll.config $testPackageFolder

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $testPackageFolder

    ProgressEnd 'Creating Test Package'
}

CleanupWindowsPackage()
{
    ProgressStart 'Cleaning Windows Package'

    echo "Removing Radarr.Mono"
    rm -f $outputFolder/Radarr.Mono.*

    echo "Adding Radarr.Windows to UpdatePackage"
    cp $outputFolder/Radarr.Windows.* $updateFolder

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
    mkdir $artifactsFolderWindows/Radarr
    mkdir $artifactsFolderMacOS/Radarr
    mkdir $artifactsFolderLinux/Radarr
    mkdir $artifactsFolderMacOSApp
    
    cp -r $outputFolder/* $artifactsFolderWindows/Radarr
    cp -r $outputFolderMacOSApp/* $artifactsFolderMacOSApp
    cp -r $outputFolderMacOS/* $artifactsFolderMacOS/Radarr
    cp -r $outputFolderLinux/* $artifactsFolderLinux/Radarr
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

POSITIONAL=()
while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --only-backend)
        ONLY_BACKEND=YES
        shift # past argument
        ;;
    --only-frontend)
        ONLY_FRONTEND=YES
        shift # past argument
        ;;
    --only-packages)
        ONLY_PACKAGES=YES
        shift # past argument
        ;;
    *)    # unknown option
        POSITIONAL+=("$1") # save it in an array for later
        shift # past argument
        ;;
esac
done
set -- "${POSITIONAL[@]}" # restore positional parameters

# Only build backend if we haven't set only-frontend or only-packages
if [ -z "$ONLY_FRONTEND" ] && [ -z "$ONLY_PACKAGES" ];
then
    UpdateVersionNumber
    Build
    PackageTests
fi

# Only build frontend if we haven't set only-backend or only-packages
if [ -z "$ONLY_BACKEND" ] && [ -z "$ONLY_PACKAGES" ];
then
   RunGulp
fi

# Only package if we haven't set only-backend or only-frontend
if [ -z "$ONLY_BACKEND" ] && [ -z "$ONLY_FRONTEND" ];
then
    PackageMono
    PackageMacOS
    PackageMacOSApp
    CleanupWindowsPackage
    PackageArtifacts
fi
