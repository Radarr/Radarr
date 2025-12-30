import ModelBase from 'App/ModelBase';

interface Book extends ModelBase {
  title: string;
  sortTitle: string;
  description: string;
  foreignBookId: string;
  isbn: string;
  isbn13: string;
  asin: string;
  pageCount: number;
  releaseDate: string;
  publisher: string;
  language: string;
  monitored: boolean;
  qualityProfileId: number;
  path: string;
  rootFolderPath: string;
  added: string;
  tags: number[];
  lastSearchTime?: string;
  authorId?: number;
  bookSeriesId?: number;
  seriesPosition?: number;
  isSaving?: boolean;
}

export default Book;
