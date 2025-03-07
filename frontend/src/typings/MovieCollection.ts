import ModelBase from 'App/ModelBase';
import Movie from 'Movie/Movie';

interface MovieCollection extends ModelBase {
  title: string;
  sortTitle: string;
  tmdbId: number;
  overview: string;
  monitored: boolean;
  rootFolderPath: string;
  qualityProfileId: number;
  movies: Movie[];
  missingMovies: number;
  tags: number[];
  isSaving?: boolean;
}

export default MovieCollection;
