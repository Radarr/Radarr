import ModelBase from 'App/ModelBase';

export type ExtraFileType = 'subtitle' | 'metadata' | 'other';

export interface ExtraFile extends ModelBase {
  movieId: number;
  movieFileId?: number;
  relativePath: string;
  extension: string;
  type: ExtraFileType;
}
