import AppSectionState, {
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import MovieCollection from 'typings/MovieCollection';

interface MovieCollectionAppState
  extends AppSectionState<MovieCollection>,
    AppSectionSaveState {
  itemMap: Record<number, number>;
}

export default MovieCollectionAppState;
