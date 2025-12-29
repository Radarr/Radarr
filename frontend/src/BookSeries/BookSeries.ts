import ModelBase from 'App/ModelBase';

interface BookSeries extends ModelBase {
  title: string;
  sortTitle: string;
  description: string;
  foreignSeriesId: string;
  authorId?: number;
  monitored: boolean;
  isSaving?: boolean;
}

export default BookSeries;
