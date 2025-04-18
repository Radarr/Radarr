import ModelBase from 'App/ModelBase';
import Movie, { Image, MovieAvailability } from 'Movie/Movie';

interface MovieCollection extends ModelBase {
  tmdbId: number;
  sortTitle: string;
  title: string;
  overview: string;
  monitored: boolean;
  minimumAvailability: MovieAvailability;
  qualityProfileId: number;
  rootFolderPath: string;
  searchOnAdd: boolean;
  images: Image[];
  movies: Movie[];
  missingMovies: number;
  tags: number[];
  isSaving?: boolean;
}

export default MovieCollection;
