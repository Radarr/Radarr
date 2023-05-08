import ModelBase from 'App/ModelBase';
import { AppSectionItemState } from 'App/State/AppSectionState';
import Language from 'Language/Language';
import Movie from 'Movie/Movie';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

export interface ParsedMovieInfo {
  releaseTitle: string;
  originalTitle: string;
  movieTitle: string;
  movieTitles: string[];
  year: number;
  quality: QualityModel;
  languages: Language[];
  releaseHash: string;
  releaseGroup?: string;
  edition?: string;
  tmdbId?: number;
  imdbId?: string;
}

export interface ParseModel extends ModelBase {
  title: string;
  parsedMovieInfo: ParsedMovieInfo;
  movie?: Movie;
  languages?: Language[];
  customFormats?: CustomFormat[];
  customFormatScore?: number;
}

type ParseAppState = AppSectionItemState<ParseModel>;

export default ParseAppState;
