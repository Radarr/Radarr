VERSION=$1
outputFolder='./_output'
outputFolderMono='./_output_mono'
outputFolderOsx='./_output_osx'
outputFolderOsxApp='./_output_osx_app'

cp -r $outputFolder Radarr_Windows_$VERSION
cp -r $outputFolderMono Radarr_Mono_$VERSION
cp -r $outputFolderOsxApp Radarr_OSX_$VERSION
