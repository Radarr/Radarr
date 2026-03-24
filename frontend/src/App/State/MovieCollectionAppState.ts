import AppSectionState, {
  AppSectionFilterState,
  AppSectionSaveState,
  Error,
} from 'App/State/AppSectionState';
import MovieCollection from 'typings/MovieCollection';

interface MovieCollectionAppState
  extends AppSectionState<MovieCollection>,
    AppSectionFilterState<MovieCollection>,
    AppSectionSaveState {
  itemMap: Record<number, number>;

  isAdding: boolean;
  addError: Error;

  pendingChanges: Partial<MovieCollection>;
}

export default MovieCollectionAppState;
