import getMonitoringOptions from 'Utilities/Series/getMonitoringOptions';

function getNewSeries(series, payload) {
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

  // const {
    // seasons,
    // options: addOptions
  // } = getMonitoringOptions(series.seasons, monitor);

  // addOptions.searchForMissingAlbums = searchForMissingAlbums;
  // series.addOptions = addOptions;
  // series.seasons = seasons;
  series.monitored = true;
  series.qualityProfileId = qualityProfileId;
  series.languageProfileId = languageProfileId;
  series.rootFolderPath = rootFolderPath;
  series.artistType = artistType;
  series.albumFolder = albumFolder;
  series.primaryAlbumTypes = primaryAlbumTypes;
  series.secondaryAlbumTypes = secondaryAlbumTypes;
  series.tags = tags;

  return series;
}

export default getNewSeries;
