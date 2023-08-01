import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import Movie from 'Movie/Movie';
import { QualityModel } from 'Quality/Quality';
import Rejection from 'typings/Rejection';

export interface InteractiveImportCommandOptions {
  path: string;
  folderName: string;
  movieId: number;
  releaseGroup?: string;
  quality: QualityModel;
  languages: Language[];
  indexerFlags: number;
  downloadId?: string;
  movieFileId?: number;
}

interface InteractiveImport extends ModelBase {
  path: string;
  relativePath: string;
  folderName: string;
  name: string;
  size: number;
  releaseGroup: string;
  quality: QualityModel;
  languages: Language[];
  movie?: Movie;
  qualityWeight: number;
  customFormats: object[];
  indexerFlags: number;
  rejections: Rejection[];
  movieFileId?: number;
}

export default InteractiveImport;
