import Movie, { MovieAvailability, MovieMonitor } from 'Movie/Movie';

interface NewMoviePayload {
  rootFolderPath: string;
  monitor: MovieMonitor;
  qualityProfileId: number;
  minimumAvailability: MovieAvailability;
  tags: number[];
  searchForMovie?: boolean;
}

function getNewMovie(movie: Movie, payload: NewMoviePayload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    minimumAvailability,
    tags,
    searchForMovie = false,
  } = payload;

  const addOptions = {
    monitor,
    searchForMovie,
  };

  movie.addOptions = addOptions;
  movie.monitored = monitor !== 'none';
  movie.qualityProfileId = qualityProfileId;
  movie.minimumAvailability = minimumAvailability;
  movie.rootFolderPath = rootFolderPath;
  movie.tags = tags;

  return movie;
}

export default getNewMovie;
