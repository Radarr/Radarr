import getMonitoringOptions from 'Utilities/Series/getMonitoringOptions';

function getNewSeries(artist, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    languageProfileId,
    artistType,
    albumFolder,
    primaryAlbumTypes,
    secondaryAlbumTypes,
    tags,
    searchForMissingAlbums = false
  } = payload;

  const {
    // seasons,
    options: addOptions
  } = getMonitoringOptions(monitor);

  addOptions.searchForMissingAlbums = searchForMissingAlbums;
  artist.addOptions = addOptions;
  // artist.seasons = seasons;
  artist.monitored = true;
  artist.qualityProfileId = qualityProfileId;
  artist.languageProfileId = languageProfileId;
  artist.rootFolderPath = rootFolderPath;
  artist.artistType = artistType;
  artist.albumFolder = albumFolder;
  artist.primaryAlbumTypes = primaryAlbumTypes;
  artist.secondaryAlbumTypes = secondaryAlbumTypes;
  artist.tags = tags;

  return artist;
}

export default getNewSeries;
