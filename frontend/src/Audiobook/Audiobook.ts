import ModelBase from 'App/ModelBase';

interface Audiobook extends ModelBase {
  title: string;
  sortTitle: string;
  description: string;
  foreignAudiobookId: string;
  isbn: string;
  asin: string;
  narrator: string;
  durationMinutes: number;
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
  seriesId?: number;
  seriesPosition?: number;
  isSaving?: boolean;
}

export default Audiobook;
