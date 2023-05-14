#! /usr/bin/env bash
set -e

outputFolder='_output'
testPackageFolder='_tests'
artifactsFolder="_artifacts";

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
        sed -i'' -e "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$RADARRVERSION<\/AssemblyVersion>/g" src/Directory.Build.props
        sed -i'' -e "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BUILD_SOURCEBRANCHNAME}<\/AssemblyConfiguration>/g" src/Directory.Build.props
        sed -i'' -e "s/<string>10.0.0.0<\/string>/<string>$RADARRVERSION<\/string>/g" distribution/osx/Radarr.app/Contents/Info.plist
    fi
}

EnableExtraPlatformsInSDK()
{
    SDK_PATH=$(dotnet --list-sdks | grep -P '6\.\d\.\d+' | head -1 | sed 's/\(6\.[0-9]*\.[0-9]*\).*\[\(.*\)\]/\2\/\1/g')
    BUNDLEDVERSIONS="${SDK_PATH}/Microsoft.NETCoreSdk.BundledVersions.props"
    if grep -q freebsd-x64 $BUNDLEDVERSIONS; then
        echo "Extra platforms already enabled"
    else
        echo "Enabling extra platform support"
        sed -i.ORI 's/osx-x64/osx-x64;freebsd-x64;linux-x86/' $BUNDLEDVERSIONS
    fi
}

EnableExtraPlatforms()
{
    if grep -qv freebsd-x64 src/Directory.Build.props; then
        sed -i'' -e "s^<RuntimeIdentifiers>\(.*\)</RuntimeIdentifiers>^<RuntimeIdentifiers>\1;freebsd-x64;linux-x86</RuntimeIdentifiers>^g" src/Directory.Build.props
    fi
}

LintUI()
{
    ProgressStart 'ESLint'
    yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    if [ "$os" = "windows" ]; then
        yarn stylelint-windows
    else
        yarn stylelint-linux
    fi
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

    slnFile=src/Radarr.sln

    if [ $os = "windows" ]; then
        platform=Windows
    else
        platform=Posix
    fi

    dotnet clean $slnFile -c Debug
    dotnet clean $slnFile -c Release

    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -t:PublishAllRids
    else
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -p:RuntimeIdentifiers=$RID -t:PublishAllRids
    fi

    ProgressEnd 'Build'
}

YarnInstall()
{
    ProgressStart 'yarn install'
    yarn install --frozen-lockfile --network-timeout 120000
    ProgressEnd 'yarn install'
}

RunWebpack()
{
    ProgressStart 'Running webpack'
    yarn run build --env production
    ProgressEnd 'Running webpack'
}

PackageFiles()
{
    local folder="$1"
    local framework="$2"
    local runtime="$3"

    rm -rf $folder
    mkdir -p $folder
    cp -r $outputFolder/$framework/$runtime/publish/* $folder
    cp -r $outputFolder/Radarr.Update/$framework/$runtime/publish $folder/Radarr.Update
    cp -r $outputFolder/UI $folder

    echo "Adding LICENSE"
    cp LICENSE $folder
}

PackageLinux()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Radarr

    PackageFiles "$folder" "$framework" "$runtime"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Radarr.Windows"
    rm $folder/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $folder/Radarr.Mono.* $folder/Radarr.Update
    if [ "$framework" = "net6.0" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Radarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Radarr.Update
    fi

    ProgressEnd "Creating $runtime Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    local runtime="$2"
    
    ProgressStart "Creating MacOS Package for $framework $runtime"

    local folder=$artifactsFolder/$runtime/$framework/Radarr

    PackageFiles "$folder" "$framework" "$runtime"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Radarr.Windows"
    rm $folder/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $folder/Radarr.Mono.* $folder/Radarr.Update
    if [ "$framework" = "net6.0" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Radarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Radarr.Update
    fi

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    local framework="$1"
    local runtime="$2"
    
    ProgressStart "Creating macOS App Package for $framework $runtime"

    local folder="$artifactsFolder/$runtime-app/$framework"

    rm -rf $folder
    mkdir -p $folder
    cp -r distribution/osx/Radarr.app $folder
    mkdir -p $folder/Radarr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $artifactsFolder/$runtime/$framework/Radarr/* $folder/Radarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $folder/Radarr.app/Contents/MacOS/Radarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageWindows()
{
    local framework="$1"
    local runtime="$2"
    
    ProgressStart "Creating Windows Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Radarr
    
    PackageFiles "$folder" "$framework" "$runtime"
    cp -r $outputFolder/$framework-windows/$runtime/publish/* $folder

    echo "Removing Radarr.Mono"
    rm -f $folder/Radarr.Mono.*
    rm -f $folder/Mono.Posix.NETStandard.*
    rm -f $folder/libMonoPosixHelper.*

    echo "Adding Radarr.Windows to UpdatePackage"
    cp $folder/Radarr.Windows.* $folder/Radarr.Update

    ProgressEnd 'Creating Windows Package'
}

Package()
{
    local framework="$1"
    local runtime="$2"
    local SPLIT

    IFS='-' read -ra SPLIT <<< "$runtime"

    case "${SPLIT[0]}" in
        linux|freebsd*)
            PackageLinux "$framework" "$runtime"
            ;;
        win)
            PackageWindows "$framework" "$runtime"
            ;;
        osx)
            PackageMacOS "$framework" "$runtime"
            PackageMacOSApp "$framework" "$runtime"
            ;;
    esac
}

BuildInstaller()
{
    local framework="$1"
    local runtime="$2"
    
    ./_inno/ISCC.exe distribution/windows/setup/radarr.iss "//DFramework=$framework" "//DRuntime=$runtime"
}

InstallInno()
{
    ProgressStart "Installing portable Inno Setup"
    
    rm -rf _inno
    curl -s --output innosetup.exe "https://files.jrsoftware.org/is/6/innosetup-${INNOVERSION:-6.2.0}.exe"
    mkdir _inno
    ./innosetup.exe //portable=1 //silent //currentuser //dir=.\\_inno
    rm innosetup.exe
    
    ProgressEnd "Installed portable Inno Setup"
}

RemoveInno()
{
    rm -rf _inno
}

PackageTests()
{
    local framework="$1"
    local runtime="$2"

    cp test.sh "$testPackageFolder/$framework/$runtime/publish"

    rm -f $testPackageFolder/$framework/$runtime/*.log.config

    ProgressEnd 'Creating Test Package'
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        os="windows"
        ;;
    *)
        # otherwise use mono
        os="posix"
        ;;
esac

POSITIONAL=()

if [ $# -eq 0 ]; then
    echo "No arguments provided, building everything"
    BACKEND=YES
    FRONTEND=YES
    PACKAGES=YES
    INSTALLER=NO
    LINT=YES
    ENABLE_EXTRA_PLATFORMS=NO
    ENABLE_EXTRA_PLATFORMS_IN_SDK=NO
fi

while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --backend)
        BACKEND=YES
        shift # past argument
        ;;
    --enable-bsd|--enable-extra-platforms)
        ENABLE_EXTRA_PLATFORMS=YES
        shift # past argument
        ;;
    --enable-extra-platforms-in-sdk)
        ENABLE_EXTRA_PLATFORMS_IN_SDK=YES
        shift # past argument
        ;;
    -r|--runtime)
        RID="$2"
        shift # past argument
        shift # past value
        ;;
    -f|--framework)
        FRAMEWORK="$2"
        shift # past argument
        shift # past value
        ;;
    --frontend)
        FRONTEND=YES
        shift # past argument
        ;;
    --packages)
        PACKAGES=YES
        shift # past argument
        ;;
    --installer)
        INSTALLER=YES
        shift # past argument
        ;;
    --lint)
        LINT=YES
        shift # past argument
        ;;
    --all)
        BACKEND=YES
        FRONTEND=YES
        PACKAGES=YES
        LINT=YES
        shift # past argument
        ;;
    *)    # unknown option
        POSITIONAL+=("$1") # save it in an array for later
        shift # past argument
        ;;
esac
done
set -- "${POSITIONAL[@]}" # restore positional parameters

if [ "$ENABLE_EXTRA_PLATFORMS_IN_SDK" = "YES" ];
then
    EnableExtraPlatformsInSDK
fi

if [ "$BACKEND" = "YES" ];
then
    UpdateVersionNumber
    if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
    then
        EnableExtraPlatforms
    fi
    Build
    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        PackageTests "net6.0" "win-x64"
        PackageTests "net6.0" "win-x86"
        PackageTests "net6.0" "linux-x64"
        PackageTests "net6.0" "linux-musl-x64"
        PackageTests "net6.0" "osx-x64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            PackageTests "net6.0" "freebsd-x64"
            PackageTests "net6.0" "linux-x86"
        fi
    else
        PackageTests "$FRAMEWORK" "$RID"
    fi
fi

if [ "$FRONTEND" = "YES" ];
then
    YarnInstall
    RunWebpack
fi

if [ "$LINT" = "YES" ];
then
    if [ -z "$FRONTEND" ];
    then
        YarnInstall
    fi
    
    LintUI
fi

if [ "$PACKAGES" = "YES" ];
then
    UpdateVersionNumber

    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        Package "net6.0" "win-x64"
        Package "net6.0" "win-x86"
        Package "net6.0" "linux-x64"
        Package "net6.0" "linux-musl-x64"
        Package "net6.0" "linux-arm64"
        Package "net6.0" "linux-musl-arm64"
        Package "net6.0" "linux-arm"
        Package "net6.0" "linux-musl-arm"
        Package "net6.0" "osx-x64"
        Package "net6.0" "osx-arm64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            Package "net6.0" "freebsd-x64"
            Package "net6.0" "linux-x86"
        fi
    else
        Package "$FRAMEWORK" "$RID"
    fi
fi

if [ "$INSTALLER" = "YES" ];
then
    InstallInno
    BuildInstaller "net6.0" "win-x64"
    BuildInstaller "net6.0" "win-x86"
    RemoveInno
fi
