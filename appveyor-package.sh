#! /bin/bash

artifactsFolder="./_artifacts";
artifactsFolderWindows=$artifactsFolder/windows
artifactsFolderLinux=$artifactsFolder/linux
artifactsFolderMacOS=$artifactsFolder/macos
artifactsFolderMacOSApp=$artifactsFolder/macos-app

PublishArtifacts()
{
    7z a $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.windows.zip $artifactsFolderWindows/*
    
    7z a $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.osx-app.zip $artifactsFolderMacOSApp/*
    
    7z a -ttar $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.osx.tar $artifactsFolderMacOS/*
    7z a -tgzip $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.osx.tar.gz $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.osx.tar
    rm -f $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.osx.tar
    
    7z a -ttar $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.linux.tar $artifactsFolderLinux/*
    7z a -tgzip $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.linux.tar.gz $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.linux.tar
    rm -f $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.linux.tar

    if [ "${CI_WINDOWS}" = "True" ]; then
        ./setup/inno/ISCC.exe "./setup/lidarr.iss"
        cp ./setup/output/Lidarr.*windows.exe $artifactsFolder/Lidarr.${APPVEYOR_REPO_BRANCH}.${APPVEYOR_BUILD_VERSION}.windows-installer.exe        
    fi
}

PublishArtifacts
