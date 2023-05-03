import InteractiveImportAppState from 'App/State/InteractiveImportAppState';
import MovieFilesAppState from './MovieFilesAppState';
import MoviesAppState, { MovieIndexAppState } from './MoviesAppState';
import QueueAppState from './QueueAppState';
import SettingsAppState from './SettingsAppState';
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

interface AppState {
  movieFiles: MovieFilesAppState;
  interactiveImport: InteractiveImportAppState;
  movieIndex: MovieIndexAppState;
  settings: SettingsAppState;
  movies: MoviesAppState;
  tags: TagsAppState;
  queue: QueueAppState;
}

export default AppState;
