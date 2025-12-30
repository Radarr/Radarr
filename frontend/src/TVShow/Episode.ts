import ModelBase from 'App/ModelBase';

interface Episode extends ModelBase {
  tvShowId?: number;
  seasonId?: number;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  title?: string;
  overview?: string;
  airDate?: string;
  airDateUtc?: string;
  runtime?: number;
  isSpecial: boolean;
  unverifiedSceneNumbering: boolean;
  episodeFileId?: number;
  monitored: boolean;
  qualityProfileId: number;
  path?: string;
  rootFolderPath?: string;
  added: string;
  tags: number[];
  lastSearchTime?: string;
  isSaving?: boolean;
}

export default Episode;
