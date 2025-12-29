import ModelBase from 'App/ModelBase';

interface Series extends ModelBase {
  title: string;
  sortTitle: string;
  description: string;
  foreignSeriesId: string;
  authorId?: number;
  monitored: boolean;
  isSaving?: boolean;
}

export default Series;
