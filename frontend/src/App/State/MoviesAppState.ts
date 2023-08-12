import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Column from 'Components/Table/Column';
import SortDirection from 'Helpers/Props/SortDirection';
import Movie from 'Movie/Movie';
import { Filter, FilterBuilderProp } from './AppState';

export interface MovieIndexAppState {
  sortKey: string;
  sortDirection: SortDirection;
  secondarySortKey: string;
  secondarySortDirection: SortDirection;
  view: string;

  posterOptions: {
    detailedProgressBar: boolean;
    size: string;
    showTitle: boolean;
    showMonitored: boolean;
    showQualityProfile: boolean;
    showReleaseDate: boolean;
    showCinemaRelease: boolean;
    showTmdbRating: boolean;
    showImdbRating: boolean;
    showRottenTomatoesRating: boolean;
    showSearchAction: boolean;
  };

  overviewOptions: {
    detailedProgressBar: boolean;
    size: string;
    showMonitored: boolean;
    showStudio: boolean;
    showQualityProfile: boolean;
    showAdded: boolean;
    showPath: boolean;
    showSizeOnDisk: boolean;
    showSearchAction: boolean;
  };

  tableOptions: {
    showSearchAction: boolean;
  };

  selectedFilterKey: string;
  filterBuilderProps: FilterBuilderProp<Movie>[];
  filters: Filter[];
  columns: Column[];
}

interface MoviesAppState
  extends AppSectionState<Movie>,
    AppSectionDeleteState,
    AppSectionSaveState {
  itemMap: Record<number, number>;

  deleteOptions: {
    addImportExclusion: boolean;
  };
}

export default MoviesAppState;
