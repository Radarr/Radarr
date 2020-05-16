
function getNewAuthor(author, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    metadataProfileId,
    authorType,
    tags,
    searchForMissingBooks = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingBooks
  };

  author.addOptions = addOptions;
  author.monitored = true;
  author.qualityProfileId = qualityProfileId;
  author.metadataProfileId = metadataProfileId;
  author.rootFolderPath = rootFolderPath;
  author.authorType = authorType;
  author.tags = tags;

  return author;
}

export default getNewAuthor;
