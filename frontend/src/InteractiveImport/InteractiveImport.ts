import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import MovieVersion from 'Movie/Details/Versions/MovieVersion';
import Movie from 'Movie/Movie';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import Rejection from 'typings/Rejection';

export interface InteractiveImportCommandOptions {
  path: string;
  folderName: string;
  movieId: number;
  movieVersionId?: number;
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
  movieVersionId?: number;
  movieVersions?: MovieVersion[];
  qualityWeight: number;
  customFormats: CustomFormat[];
  indexerFlags: number;
  rejections: Rejection[];
  movieFileId?: number;
}

export default InteractiveImport;
