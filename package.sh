if [ $# -eq 0 ]; then
  if [ "$TRAVIS_PULL_REQUEST" != false ]; then
    echo "Need to supply version argument" && exit;
  fi
fi

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

cp -r $outputFolder Radarr_Windows_$VERSION
cp -r $outputFolderMono Radarr_Mono_$VERSION
cp -r $outputFolderOsxApp Radarr_OSX_$VERSION

zip -r Radarr_Windows_$VERSION.zip Radarr_Windows_$VERSION >& /dev/null
zip -r Radarr_Mono_$VERSION.zip Radarr_Mono_$VERSION >& /dev/null
zip -r Radarr_OSX_$VERSION.zip Radarr_OSX_$VERSION >& /dev/null

ftp -n ftp.leonardogalli.ch << END_SCRIPT
passive
quote USER $FTP_USER
quote PASS $FTP_PASS
mkdir builds
cd builds
mkdir $YEAR
cd $YEAR
mkdir $MONTH
cd $MONTH
mkdir $DAY
cd $DAY
binary
put Radarr_Windows_$VERSION.zip
put Radarr_Mono_$VERSION.zip
put Radarr_OSX_$VERSION.zip
quit
END_SCRIPT
