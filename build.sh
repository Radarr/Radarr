#! /bin/bash
outputFolder='./_output'
testPackageFolder='./_tests/'
sourceFolder='./src'
slnFile=$sourceFolder/Radarr.sln

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
    if [ "$RADARRVERSION" != "" ]; then
        echo "Updating Version Info"
        sed -i "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$RADARRVERSION<\/AssemblyVersion>/g" ./src/Directory.Build.props
        sed -i "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BUILD_SOURCEBRANCHNAME}<\/AssemblyConfiguration>/g" ./src/Directory.Build.props
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
    CheckExitCode yarn stylelint
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

    CheckExitCode dotnet clean $slnFile -c Debug
    CheckExitCode dotnet clean $slnFile -c Release
    CheckExitCode dotnet msbuild -restore src/Radarr.sln -p:Configuration=Release -t:PublishAllRids

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

    ProgressStart "Creating Linux Package for $framework"

    local folder=$artifactsFolder/linux/$framework/Radarr

    PackageFiles "$folder" $framework $runtime "linux-x64"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Renaming Radarr.Console.exe to Radarr.exe"
    rm $folder/Radarr.exe*
    for file in $folder/Radarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing Radarr.Windows"
    rm $folder/Radarr.Windows.*

    echo "Adding Radarr.Mono to UpdatePackage"
    cp $folder/Radarr.Mono.* $folder/Radarr.Update

    ProgressEnd "Creating Linux Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    
    ProgressStart "Creating MacOS Package for $framework"

    local folder=$artifactsFolder/macos/$framework/Radarr

    PackageFiles "$folder" "$framework" "osx-x64"

    echo "Adding Startup script"
    cp ./macOS/Radarr $folder
    dos2unix $folder/Radarr

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Renaming Radarr.Console.exe to Radarr.exe"
    rm $folder/Radarr.exe*
    for file in $folder/Radarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

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
    cp -r ./macOS/Radarr.app $folder
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

    local folder=$artifactsFolder/windows/$framework/Radarr
    
    PackageFiles "$folder" "net462" "win-x64"

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
    PackageWindows "net462"
    PackageLinux "net462"
    PackageMacOS "net462"
    PackageMacOSApp "net462"
fi
