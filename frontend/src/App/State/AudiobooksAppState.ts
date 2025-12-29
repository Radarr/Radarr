import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Audiobook from 'Audiobook/Audiobook';

interface AudiobooksAppState
  extends
    AppSectionState<Audiobook>,
    AppSectionDeleteState,
    AppSectionSaveState {
  pendingChanges: Partial<Audiobook>;
}

export default AudiobooksAppState;
