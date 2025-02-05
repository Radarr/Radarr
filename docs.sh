#!/bin/bash
set -e

FRAMEWORK="net6.0"
PLATFORM=$1
ARCHITECTURE="${2:-x64}"

if [ "$PLATFORM" = "Windows" ]; then
  RUNTIME="win-$ARCHITECTURE"
elif [ "$PLATFORM" = "Linux" ]; then
  RUNTIME="linux-$ARCHITECTURE"
elif [ "$PLATFORM" = "Mac" ]; then
  RUNTIME="osx-$ARCHITECTURE"
else
  echo "Platform must be provided as first argument: Windows, Linux or Mac"
  exit 1
fi

outputFolder='_output'
testPackageFolder='_tests'

rm -rf $outputFolder
rm -rf $testPackageFolder

slnFile=src/Radarr.sln

platform=Posix

if [ "$PLATFORM" = "Windows" ]; then
  application=Radarr.Console.dll
else
  application=Radarr.dll
fi

dotnet clean $slnFile -c Debug
dotnet clean $slnFile -c Release

dotnet msbuild -restore $slnFile -p:Configuration=Debug -p:Platform=$platform -p:RuntimeIdentifiers=$RUNTIME -t:PublishAllRids

dotnet new tool-manifest
dotnet tool install --version 6.6.2 Swashbuckle.AspNetCore.Cli

dotnet tool run swagger tofile --output ./src/Radarr.Api.V3/openapi.json "$outputFolder/$FRAMEWORK/$RUNTIME/$application" v3 &

sleep 45

kill %1

exit 0
