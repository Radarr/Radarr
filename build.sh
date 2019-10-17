#! /bin/bash
outputFolder='./_output'
testPackageFolder='./_tests/'
sourceFolder='./src'

#Artifact variables
artifactsFolder="./_artifacts";

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
    if [ "$LIDARRVERSION" != "" ]; then
        echo "Updating Version Info"
        sed -i "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$LIDARRVERSION<\/AssemblyVersion>/g" ./src/Directory.Build.props
        sed -i "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BUILD_SOURCEBRANCHNAME}<\/AssemblyConfiguration>/g" ./src/Directory.Build.props
        sed -i "s/<string>10.0.0.0<\/string>/<string>$LIDARRVERSION<\/string>/g" ./macOS/Lidarr.app/Contents/Info.plist
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
    CheckExitCode yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    if [ "$os" = "windows" ]; then
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
    rm -rf $testPackageFolder

    if [ $os = "windows" ]; then
        slnFile=$sourceFolder/Lidarr.Windows.sln
    else
        slnFile=$sourceFolder/Lidarr.Posix.sln
    fi

    CheckExitCode dotnet clean $slnFile -c Debug
    CheckExitCode dotnet clean $slnFile -c Release
    CheckExitCode dotnet msbuild -restore $slnFile -p:Configuration=Release -t:PublishAllRids

    ProgressEnd 'Build'
}

YarnInstall()
{
    ProgressStart 'yarn install'
    yarn install
    #npm-cache install npm || CheckExitCode npm install --no-optional --no-bin-links
    ProgressEnd 'yarn install'
}

RunGulp()
{
    ProgressStart 'Running gulp'
    CheckExitCode yarn run build --production
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
    cp -r $outputFolder/Lidarr.Update/$framework/$runtime/publish $folder/Lidarr.Update
    cp -r $outputFolder/UI $folder

    CleanFolder $folder

    echo "Adding LICENSE"
    cp LICENSE.md $folder
}

PackageLinux()
{
    local framework="$1"

    ProgressStart "Creating Linux Package for $framework"

    local folder=$artifactsFolder/linux/$framework/Lidarr

    PackageFiles "$folder" $framework $runtime "linux-x64"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Lidarr.Windows"
    rm $folder/Lidarr.Windows.*

    echo "Adding Lidarr.Mono to UpdatePackage"
    cp $folder/Lidarr.Mono.* $folder/Lidarr.Update

    ProgressEnd "Creating Linux Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    
    ProgressStart "Creating MacOS Package for $framework"

    local folder=$artifactsFolder/macos/$framework/Lidarr

    PackageFiles "$folder" "$framework" "osx-x64"

    echo "Adding Startup script"
    cp ./macOS/Lidarr $folder

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Lidarr.Windows"
    rm $folder/Lidarr.Windows.*

    echo "Adding Lidarr.Mono to UpdatePackage"
    cp $folder/Lidarr.Mono.* $folder/Lidarr.Update

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    local framework="$1"
    
    ProgressStart "Creating macOS App Package for $framework"

    local folder=$artifactsFolder/macos-app/$framework

    rm -rf $folder
    mkdir -p $folder
    cp -r ./macOS/Lidarr.app $folder
    mkdir -p $folder/Lidarr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $artifactsFolder/macos/$framework/Lidarr/* $folder/Lidarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $folder/Lidarr.app/Contents/MacOS/Lidarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageTests()
{
    ProgressStart 'Creating Test Package'

    cp ./test.sh $testPackageFolder/net462/win-x64/publish
    cp ./test.sh $testPackageFolder/net462/linux-x64/publish
    cp ./test.sh $testPackageFolder/net462/osx-x64/publish

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

    local folder=$artifactsFolder/windows/$framework/Lidarr
    
    PackageFiles "$folder" "$framework" "win-x64"

    echo "Removing Lidarr.Mono"
    rm -f $folder/Lidarr.Mono.*

    echo "Adding Lidarr.Windows to UpdatePackage"
    cp $folder/Lidarr.Windows.* $folder/Lidarr.Update

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
    PackageLinux "net462"
    PackageMacOS "net462"
    PackageMacOSApp "net462"
fi
