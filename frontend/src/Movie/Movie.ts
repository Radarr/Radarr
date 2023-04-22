import ModelBase from 'App/ModelBase';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Language {
  id: number;
  name: string;
}

interface Movie extends ModelBase {
  tmdbId: number;
  imdbId: string;
  youTubeTrailerId: string;
  monitored: boolean;
  status: string;
  title: string;
  titleSlug: string;
  collection: object;
  studio: string;
  qualityProfile: object;
  added: Date;
  year: number;
  inCinemas: Date;
  physicalRelease: Date;
  originalLanguage: Language;
  originalTitle: string;
  digitalRelease: Date;
  runtime: number;
  minimumAvailability: string;
  path: string;
  sizeOnDisk: number;
  genres: string[];
  ratings: object;
  certification: string;
  tags: number[];
  images: Image;
  isSaving?: boolean;
}

export default Movie;
