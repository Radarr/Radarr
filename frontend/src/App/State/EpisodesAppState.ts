import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Episode from 'TVShow/Episode';

interface EpisodesAppState
  extends AppSectionState<Episode>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<Episode>;
}

export default EpisodesAppState;
