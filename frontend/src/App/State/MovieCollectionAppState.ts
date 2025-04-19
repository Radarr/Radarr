import AppSectionState, {
  AppSectionFilterState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import MovieCollection from 'typings/MovieCollection';

interface MovieCollectionAppState
  extends AppSectionState<MovieCollection>,
    AppSectionFilterState<MovieCollection>,
    AppSectionSaveState {
  itemMap: Record<number, number>;

  pendingChanges: Partial<MovieCollection>;
}

export default MovieCollectionAppState;
