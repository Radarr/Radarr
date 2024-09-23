import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { MovieFile } from 'MovieFile/MovieFile';

export type MovieStatus =
  | 'tba'
  | 'announced'
  | 'inCinemas'
  | 'released'
  | 'deleted';

export type CoverType = 'poster' | 'fanart' | 'headshot';

export interface Image {
  coverType: CoverType;
  url: string;
  remoteUrl: string;
}

export interface Collection {
  title: string;
}

export interface Statistics {
  movieFileCount: number;
  releaseGroups: string[];
  sizeOnDisk: number;
}

export interface RatingValues {
  votes: number;
  value: number;
}

export interface Ratings {
  imdb: RatingValues;
  tmdb: RatingValues;
  metacritic: RatingValues;
  rottenTomatoes: RatingValues;
  trakt: RatingValues;
}

export interface AlternativeTitle extends ModelBase {
  sourceType: string;
  title: string;
}

interface Movie extends ModelBase {
  tmdbId: number;
  imdbId?: string;
  sortTitle: string;
  overview: string;
  youTubeTrailerId?: string;
  monitored: boolean;
  status: MovieStatus;
  title: string;
  titleSlug: string;
  originalTitle: string;
  originalLanguage: Language;
  collection: Collection;
  alternateTitles: AlternativeTitle[];
  studio: string;
  qualityProfileId: number;
  added: string;
  year: number;
  inCinemas?: string;
  physicalRelease?: string;
  digitalRelease?: string;
  releaseDate?: string;
  runtime: number;
  minimumAvailability: string;
  path: string;
  genres: string[];
  ratings: Ratings;
  popularity: number;
  certification: string;
  statistics: Statistics;
  tags: number[];
  images: Image[];
  movieFile: MovieFile;
  hasFile: boolean;
  lastSearchTime?: string;
  isAvailable: boolean;
  isSaving?: boolean;
}

export default Movie;
