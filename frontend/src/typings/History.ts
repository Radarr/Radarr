import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from './CustomFormat';

export type HistoryEventType =
  | 'grabbed'
  | 'downloadFolderImported'
  | 'downloadFailed'
  | 'movieFileDeleted'
  | 'movieFolderImported'
  | 'movieFileRenamed'
  | 'downloadIgnored';

export interface GrabbedHistoryData {
  indexer: string;
  nzbInfoUrl: string;
  releaseGroup: string;
  age: string;
  ageHours: string;
  ageMinutes: string;
  publishedDate: string;
  downloadClient: string;
  downloadClientName: string;
  size: string;
  downloadUrl: string;
  guid: string;
  tmdbId: string;
  imdbId: string;
  protocol: string;
  customFormatScore?: string;
  movieMatchType: string;
  releaseSource: string;
  indexerFlags: string;
}

export interface DownloadFailedHistory {
  message: string;
}

export interface DownloadFolderImportedHistory {
  customFormatScore?: string;
  droppedPath: string;
  importedPath: string;
}

export interface MovieFileDeletedHistory {
  customFormatScore?: string;
  reason: 'Manual' | 'MissingFromDisk' | 'Upgrade';
}

export interface MovieFileRenamedHistory {
  sourcePath: string;
  sourceRelativePath: string;
  path: string;
  relativePath: string;
}

export interface DownloadIgnoredHistory {
  message: string;
}

export type HistoryData =
  | GrabbedHistoryData
  | DownloadFailedHistory
  | DownloadFolderImportedHistory
  | MovieFileDeletedHistory
  | MovieFileRenamedHistory
  | DownloadIgnoredHistory;

export default interface History {
  movieId: number;
  sourceTitle: string;
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  customFormatScore: number;
  qualityCutoffNotMet: boolean;
  date: string;
  downloadId: string;
  eventType: HistoryEventType;
  data: HistoryData;
  id: number;
}
