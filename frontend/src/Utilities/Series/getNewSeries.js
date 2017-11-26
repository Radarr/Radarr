import getMonitoringOptions from 'Utilities/Series/getMonitoringOptions';

function getNewSeries(artist, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    languageProfileId,
    metadataProfileId,
    artistType,
    albumFolder,
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
  artist.metadataProfileId = metadataProfileId;
  artist.rootFolderPath = rootFolderPath;
  artist.artistType = artistType;
  artist.albumFolder = albumFolder;
  artist.tags = tags;

  return artist;
}

export default getNewSeries;
