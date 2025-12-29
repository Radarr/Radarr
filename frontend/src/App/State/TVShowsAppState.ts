import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import TVShow from 'TVShow/TVShow';

interface TVShowsAppState
  extends AppSectionState<TVShow>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<TVShow>;
}

export default TVShowsAppState;
