import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Season from 'TVShow/Season';

interface SeasonsAppState
  extends AppSectionState<Season>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<Season>;
}

export default SeasonsAppState;
