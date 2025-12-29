import ModelBase from 'App/ModelBase';

interface Season extends ModelBase {
  tvShowId?: number;
  seasonNumber: number;
  title?: string;
  overview?: string;
  monitored: boolean;
  isSaving?: boolean;
}

export default Season;
