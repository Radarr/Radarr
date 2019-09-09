
function getNewArtist(artist, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    metadataProfileId,
    artistType,
    albumFolder,
    tags,
    searchForMissingAlbums = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingAlbums
  };

  artist.addOptions = addOptions;
  artist.monitored = true;
  artist.qualityProfileId = qualityProfileId;
  artist.metadataProfileId = metadataProfileId;
  artist.rootFolderPath = rootFolderPath;
  artist.artistType = artistType;
  artist.albumFolder = albumFolder;
  artist.tags = tags;

  return artist;
}

export default getNewArtist;
