import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Collection {
  title: string;
}

export interface Ratings {
  imdb: object;
  tmdb: object;
  metacritic: object;
  rottenTomatoes: object;
}

export interface Statistics {
  movieFileCount: number;
  releaseGroups: string[];
  sizeOnDisk: number;
}

interface Movie extends ModelBase {
  tmdbId: number;
  imdbId: string;
  sortTitle: string;
  overview: string;
  youTubeTrailerId: string;
  monitored: boolean;
  status: string;
  title: string;
  titleSlug: string;
  collection: Collection;
  studio: string;
  qualityProfileIds: number[];
  qualityProfile: object;
  added: string;
  year: number;
  inCinemas: string;
  physicalRelease: string;
  originalLanguage: Language;
  originalTitle: string;
  digitalRelease: string;
  runtime: number;
  minimumAvailability: string;
  path: string;
  genres: string[];
  ratings: Ratings;
  popularity: number;
  certification: string;
  tags: number[];
  images: Image[];
  statistics: Statistics;
  isAvailable: boolean;
  isSaving?: boolean;
}

export default Movie;
