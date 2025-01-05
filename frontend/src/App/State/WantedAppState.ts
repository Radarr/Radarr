import AppSectionState, {
  AppSectionFilterState,
  PagedAppSectionState,
  TableAppSectionState,
} from 'App/State/AppSectionState';
import Movie from 'Movie/Movie';

interface WantedMovie extends Movie {
  isSaving?: boolean;
}

interface WantedCutoffUnmetAppState
  extends AppSectionState<WantedMovie>,
    AppSectionFilterState<WantedMovie>,
    PagedAppSectionState,
    TableAppSectionState {}

interface WantedMissingAppState
  extends AppSectionState<WantedMovie>,
    AppSectionFilterState<WantedMovie>,
    PagedAppSectionState,
    TableAppSectionState {}

interface WantedAppState {
  cutoffUnmet: WantedCutoffUnmetAppState;
  missing: WantedMissingAppState;
}

export default WantedAppState;
