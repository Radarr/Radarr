import ModelBase from 'App/ModelBase';
import { MovieFile } from 'MovieFile/MovieFile';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Language {
  id: number;
  name: string;
}

export interface Collection {
  title: string;
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
  sizeOnDisk: number;
  genres: string[];
  ratings: object;
  certification: string;
  tags: number[];
  images: Image[];
  movieFile: MovieFile;
  hasFile: boolean;
  isAvailable: boolean;
  isSaving?: boolean;
}

export default Movie;
