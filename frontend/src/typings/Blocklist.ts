import ModelBase from 'App/ModelBase';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import Language from 'Language/Language';
import Movie from 'Movie/Movie';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

interface Blocklist extends ModelBase {
  movie: Movie;
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  title: string;
  date?: string;
  protocol: DownloadProtocol;
  sourceTitle: string;
  movieId?: number;
  indexer?: string;
  message?: string;
}

export default Blocklist;
