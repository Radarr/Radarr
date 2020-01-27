#! /bin/bash
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
        sed -i'' -e "s/<string>10.0.0.0<\/string>/<string>$RADARRVERSION<\/string>/g" macOS/Radarr.app/Contents/Info.plist
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
    dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -t:PublishAllRids

    ProgressEnd 'Build'
}

YarnInstall()
{
    ProgressStart 'yarn install'
    yarn install --frozen-lockfile
    ProgressEnd 'yarn install'
}

RunGulp()
{
    ProgressStart 'Running gulp'
    yarn run build --production
    ProgressEnd 'Running gulp'
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
    if [ "$framework" = "netcoreapp3.1" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Radarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Radarr.Update
    fi

    ProgressEnd "Creating $runtime Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    
    ProgressStart "Creating MacOS Package for $framework"

    local folder=$artifactsFolder/macos/$framework/Radarr

    PackageFiles "$folder" "$framework" "osx-x64"

    if [ "$framework" = "net462" ]; then
        echo "Adding Startup script"
        cp macOS/Radarr $folder
    fi

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Radarr.Windows"
    rm $folder/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $folder/Radarr.Mono.* $folder/Radarr.Update
    if [ "$framework" = "netcoreapp3.1" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Radarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Radarr.Update
    fi

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    local framework="$1"
    
    ProgressStart "Creating macOS App Package for $framework"

    local folder=$artifactsFolder/macos-app/$framework

    rm -rf $folder
    mkdir -p $folder
    cp -r macOS/Radarr.app $folder
    mkdir -p $folder/Radarr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $artifactsFolder/macos/$framework/Radarr/* $folder/Radarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $folder/Radarr.app/Contents/MacOS/Radarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageTests()
{
    ProgressStart 'Creating Test Package'

    cp test.sh $testPackageFolder/net462/linux-x64/publish
    cp test.sh $testPackageFolder/netcoreapp3.1/win-x64/publish
    cp test.sh $testPackageFolder/netcoreapp3.1/linux-x64/publish
    cp test.sh $testPackageFolder/netcoreapp3.1/osx-x64/publish

    rm -f $testPackageFolder/*.log.config

    # geckodriver.exe isn't copied by dotnet publish
    curl -Lo gecko.zip "https://github.com/mozilla/geckodriver/releases/download/v0.26.0/geckodriver-v0.26.0-win64.zip"
    unzip -o gecko.zip
    cp geckodriver.exe $testPackageFolder/netcoreapp3.1/win-x64/publish

    ProgressEnd 'Creating Test Package'
}

PackageWindows()
{
    local framework="$1"
    
    ProgressStart "Creating Windows Package for $framework"

    local folder=$artifactsFolder/windows/$framework/Radarr
    
    PackageFiles "$folder" "$framework" "win-x64"

    echo "Removing Radarr.Mono"
    rm -f $folder/Radarr.Mono.*
    rm -f $folder/Mono.Posix.NETStandard.*
    rm -f $folder/libMonoPosixHelper.*

    echo "Adding Radarr.Windows to UpdatePackage"
    cp $folder/Radarr.Windows.* $folder/Radarr.Update

    ProgressEnd 'Creating Windows Package'
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
    LINT=YES
fi

while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --backend)
        BACKEND=YES
        shift # past argument
        ;;
    --frontend)
        FRONTEND=YES
        shift # past argument
        ;;
    --packages)
        PACKAGES=YES
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

if [ "$BACKEND" = "YES" ];
then
    UpdateVersionNumber
    Build
    PackageTests
fi

if [ "$FRONTEND" = "YES" ];
then
    YarnInstall
    RunGulp
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
    PackageWindows "netcoreapp3.1"
    PackageLinux "net462" "linux-x64"
    PackageLinux "netcoreapp3.1" "linux-x64"
    PackageLinux "netcoreapp3.1" "linux-arm64"
    PackageLinux "netcoreapp3.1" "linux-arm"
    PackageMacOS "netcoreapp3.1"
    PackageMacOSApp "netcoreapp3.1"
fi
