if [ $# -eq 0 ]; then
  if [ "$TRAVIS_PULL_REQUEST" != false ]; then
    echo "Need to supply version argument" && exit;
  fi
fi

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

if [ "$TRAVIS_PULL_REQUEST" = "false" ]; then
  VERSION="$(date +%H:%M:%S)"
  YEAR="$(date +%Y)"
  MONTH="$(date +%m)"
  DAY="$(date +%d)"
else
  VERSION=$1
  BRANCH=$2
  BRANCH=${BRANCH#refs\/heads\/}
  BRANCH=${BRANCH//\//-}
fi
outputFolder='./_output'
outputFolderMono='./_output_mono'
outputFolderOsx='./_output_osx'
outputFolderOsxApp='./_output_osx_app'

tr -d "\r" < $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr > $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr2
rm $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr
chmod +x $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr2
mv $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr2 $outputFolderOsxApp/Radarr.app/Contents/MacOS/Radarr >& error.log

if [ $runtime = "dotnet" ] ; then
  ./tools/7zip/7za.exe a Radarr_Windows_$VERSION.zip ./Radarr_Windows_$VERSION/*
  ./tools/7zip/7za.exe a -ttar -so Radarr_Mono_$VERSION.tar ./Radarr_Mono_$VERSION/* | ./tools/7zip/7za.exe a -si Radarr_Mono_$VERSION.tar.gz
  ./tools/7zip/7za.exe a -ttar -so Radarr_OSX_$VERSION.tar ./_output_osx/* | ./tools/7zip/7za.exe a -si Radarr_OSX_$VERSION.tar.gz
  ./tools/7zip/7za.exe a -ttar -so Radarr_OSX_App_$VERSION.tar ./_output_osx_app/* | ./tools/7zip/7za.exe a -si Radarr_OSX_App_$VERSION.tar.gz
else
  cp -r $outputFolder/ Radarr
  zip -r Radarr.$BRANCH.$VERSION.windows.zip Radarr
  rm -rf Radarr
  cp -r $outputFolderMono/ Radarr
  tar -zcvf Radarr.$BRANCH.$VERSION.linux.tar.gz Radarr
  rm -rf Radarr
  cp -r $outputFolderOsx/ Radarr
  tar -zcvf Radarr.$BRANCH.$VERSION.osx.tar.gz Radarr
  rm -rf Radarr
  #TODO update for tar.gz

  cd _output_osx_app/
  zip -r ../Radarr.$BRANCH.$VERSION.osx-app.zip *
fi
# ftp -n ftp.leonardogalli.ch << END_SCRIPT
# passive
# quote USER $FTP_USER
# quote PASS $FTP_PASS
# mkdir builds
# cd builds
# mkdir $YEAR
# cd $YEAR
# mkdir $MONTH
# cd $MONTH
# mkdir $DAY
# cd $DAY
# binary
# put Radarr_Windows_$VERSION.zip
# put Radarr_Mono_$VERSION.zip
# put Radarr_OSX_$VERSION.zip
# quit
# END_SCRIPT
