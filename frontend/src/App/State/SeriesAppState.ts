import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Series from 'Series/Series';

interface SeriesAppState
  extends AppSectionState<Series>, AppSectionDeleteState, AppSectionSaveState {
  pendingChanges: Partial<Series>;
}

export default SeriesAppState;
