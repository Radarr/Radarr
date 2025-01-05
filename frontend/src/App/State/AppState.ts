import { Error } from './AppSectionState';
import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CaptchaAppState from './CaptchaAppState';
import CommandAppState from './CommandAppState';
import CustomFiltersAppState from './CustomFiltersAppState';
import ExtraFilesAppState from './ExtraFilesAppState';
import HistoryAppState, { MovieHistoryAppState } from './HistoryAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import MessagesAppState from './MessagesAppState';
import MovieBlocklistAppState from './MovieBlocklistAppState';
import MovieCollectionAppState from './MovieCollectionAppState';
import MovieCreditAppState from './MovieCreditAppState';
import MovieFilesAppState from './MovieFilesAppState';
import MoviesAppState, { MovieIndexAppState } from './MoviesAppState';
import OAuthAppState from './OAuthAppState';
import OrganizePreviewAppState from './OrganizePreviewAppState';
import ParseAppState from './ParseAppState';
import PathsAppState from './PathsAppState';
import ProviderOptionsAppState from './ProviderOptionsAppState';
import QueueAppState from './QueueAppState';
import ReleasesAppState from './ReleasesAppState';
import RootFolderAppState from './RootFolderAppState';
import SettingsAppState from './SettingsAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';
import WantedAppState from './WantedAppState';

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
  label: string | (() => string);
  filters: PropertyFilter[];
}

export interface CustomFilter {
  id: number;
  type: string;
  label: string;
  filters: PropertyFilter[];
}

export interface AppSectionState {
  isUpdated: boolean;
  isConnected: boolean;
  isDisconnected: boolean;
  isReconnecting: boolean;
  isSidebarVisible: boolean;
  version: string;
  prevVersion?: string;
  dimensions: {
    isSmallScreen: boolean;
    width: number;
    height: number;
  };
  translations: {
    error?: Error;
    isPopulated: boolean;
  };
  messages: MessagesAppState;
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  calendar: CalendarAppState;
  captcha: CaptchaAppState;
  commands: CommandAppState;
  customFilters: CustomFiltersAppState;
  extraFiles: ExtraFilesAppState;
  history: HistoryAppState;
  interactiveImport: InteractiveImportAppState;
  movieBlocklist: MovieBlocklistAppState;
  movieCollections: MovieCollectionAppState;
  movieCredits: MovieCreditAppState;
  movieFiles: MovieFilesAppState;
  movieHistory: MovieHistoryAppState;
  movieIndex: MovieIndexAppState;
  movies: MoviesAppState;
  oAuth: OAuthAppState;
  organizePreview: OrganizePreviewAppState;
  parse: ParseAppState;
  paths: PathsAppState;
  providerOptions: ProviderOptionsAppState;
  queue: QueueAppState;
  releases: ReleasesAppState;
  rootFolders: RootFolderAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
  wanted: WantedAppState;
}

export default AppState;
