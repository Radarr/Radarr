#! /bin/bash
set -e

outputFolder='_output'
testPackageFolder='_tests'
artifactsFolder="_artifacts";

nuget='tools/nuget/nuget.exe';

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

CleanFolder()
{
    local path=$1

    find $path -name "*.transform" -exec rm "{}" \;

    echo "Removing FluentValidation.Resources files"
    find $path -name "FluentValidation.resources.dll" -exec rm "{}" \;
    find $path -name "App.config" -exec rm "{}" \;

    echo "Removing vshost files"
    find $path -name "*.vshost.exe" -exec rm "{}" \;

    echo "Removing Empty folders"
    find $path -depth -empty -type d -exec rm -r "{}" \;
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

    if [ $os = "windows" ]; then
        slnFile=src/Radarr.Windows.sln
    else
        slnFile=src/Radarr.Posix.sln
    fi

    dotnet clean $slnFile -c Debug
    dotnet clean $slnFile -c Release
    dotnet msbuild -restore $slnFile -p:Configuration=Release -t:PublishAllRids

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

    CleanFolder $folder

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

    cp test.sh $testPackageFolder/net462/win-x64/publish
    cp test.sh $testPackageFolder/net462/linux-x64/publish
    cp test.sh $testPackageFolder/net462/osx-x64/publish

    if [ $os = "windows" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/win-x64/publish
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/linux-x64/publish
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/osx-x64/publish
    else
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/win-x64/publish
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/linux-x64/publish
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolder/net462/osx-x64/publish
    fi
    
    rm -f $testPackageFolder/*.log.config

    # geckodriver.exe isn't copied by dotnet publish
    curl -Lo gecko.zip "https://github.com/mozilla/geckodriver/releases/download/v0.24.0/geckodriver-v0.24.0-win64.zip"
    unzip -o gecko.zip
    cp geckodriver.exe $testPackageFolder/net462/win-x64/publish

    CleanFolder $testPackageFolder

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
    PackageWindows "net462"
    PackageLinux "net462" "linux-x64"
    PackageMacOS "net462"
    PackageMacOSApp "net462"
fi
