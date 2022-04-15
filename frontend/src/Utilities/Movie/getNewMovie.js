
function getNewMovie(movie, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileIds,
    minimumAvailability,
    tags,
    searchForMovie = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMovie
  };

  movie.addOptions = addOptions;
  movie.monitored = monitor !== 'none';
  movie.qualityProfileIds = qualityProfileIds;
  movie.minimumAvailability = minimumAvailability;
  movie.rootFolderPath = rootFolderPath;
  movie.tags = tags;

  return movie;
}

export default getNewMovie;
