import ModelBase from 'App/ModelBase';

export type TVShowStatus = 'continuing' | 'ended' | 'upcoming' | 'canceled';
export type SeriesType = 'standard' | 'daily' | 'anime';

interface TVShow extends ModelBase {
  tvdbId?: number;
  tmdbId?: number;
  imdbId?: string;
  aniDbId?: number;
  title: string;
  sortTitle: string;
  cleanTitle: string;
  overview?: string;
  network?: string;
  status: TVShowStatus;
  runtime?: number;
  airTime?: string;
  certification?: string;
  firstAired?: string;
  year: number;
  genres: string[];
  originalLanguage?: string;
  isAnime: boolean;
  seriesType: SeriesType;
  useSceneNumbering: boolean;
  path?: string;
  rootFolderPath?: string;
  qualityProfileId: number;
  seasonFolder: boolean;
  monitored: boolean;
  monitorNewItems: boolean;
  added: string;
  tags: number[];
  lastSearchTime?: string;
  isSaving?: boolean;
}

export default TVShow;
