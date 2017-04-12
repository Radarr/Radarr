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
  VERSION="`date +%H:%M:%S`"
  YEAR="`date +%Y`"
  MONTH="`date +%m`"
  DAY="`date +%d`"
else
  VERSION=$1
fi
outputFolder='./_output'
outputFolderMono='./_output_mono'
outputFolderOsx='./_output_osx'
outputFolderOsxApp='./_output_osx_app'

tr -d "\r" < $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr > $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr2
rm $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr
chmod +x $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr2
mv $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr2 $outputFolderOsxApp/Radarr.app/Contents/MacOS/Sonarr >& error.log

cp -r $outputFolder/ Radarr_Windows_$VERSION
cp -r $outputFolderMono/ Radarr_Mono_$VERSION
cp -r $outputFolderOsx/ Radarr_OSX_$VERSION

if [ $runtime = "dotnet" ] ; then
  ./7za.exe a Radarr_Windows_$VERSION.zip ./Radarr_Windows_$VERSION/*
  ./7za.exe a -ttar -so Radarr_Mono_$VERSION.tar ./Radarr_Mono_$VERSION/* | ./7za.exe a -si Radarr_Mono_$VERSION.tar.gz
  ./7za.exe a -ttar -so Radarr_OSX_$VERSION.tar ./_output_osx/* | ./7za.exe a -si Radarr_OSX_$VERSION.tar.gz
  ./7za.exe a -ttar -so Radarr_OSX_App_$VERSION.tar ./_output_osx_app/* | ./7za.exe a -si Radarr_OSX_App_$VERSION.tar.gz
else
zip -r Radarr_Windows_$VERSION.zip Radarr_Windows_$VERSION/*
tar -zcvf Radarr_Mono_$VERSION.tar.gz Radarr_Mono_$VERSION  #TODO update for tar.gz
tar -zcvf Radarr_OSX_$VERSION.tar.gz Radarr_OSX_$VERSION
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
