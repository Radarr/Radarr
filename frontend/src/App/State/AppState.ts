import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CommandAppState from './CommandAppState';
import HistoryAppState from './HistoryAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import MovieCollectionAppState from './MovieCollectionAppState';
import MovieCreditAppState from './MovieCreditAppState';
import MovieFilesAppState from './MovieFilesAppState';
import MoviesAppState, { MovieIndexAppState } from './MoviesAppState';
import ParseAppState from './ParseAppState';
import QueueAppState from './QueueAppState';
import RootFolderAppState from './RootFolderAppState';
import SettingsAppState from './SettingsAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';

interface FilterBuilderPropOption {
  id: string;
  name: string;
}

export interface FilterBuilderProp<T> {
  name: string;
  label: string;
  type: string;
  valueType?: string;
  optionsSelector?: (items: T[]) => FilterBuilderPropOption[];
}

export interface PropertyFilter {
  key: string;
  value: boolean | string | number | string[] | number[];
  type: string;
}

export interface Filter {
  key: string;
  label: string;
  filers: PropertyFilter[];
}

export interface CustomFilter {
  id: number;
  type: string;
  label: string;
  filers: PropertyFilter[];
}

export interface AppSectionState {
  isConnected: boolean;
  isReconnecting: boolean;
  version: string;
  dimensions: {
    isSmallScreen: boolean;
    width: number;
    height: number;
  };
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  calendar: CalendarAppState;
  commands: CommandAppState;
  history: HistoryAppState;
  interactiveImport: InteractiveImportAppState;
  movieCollections: MovieCollectionAppState;
  movieCredits: MovieCreditAppState;
  movieFiles: MovieFilesAppState;
  movieIndex: MovieIndexAppState;
  movies: MoviesAppState;
  parse: ParseAppState;
  queue: QueueAppState;
  rootFolders: RootFolderAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
}

export default AppState;
