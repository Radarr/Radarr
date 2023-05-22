import AppSectionState, {
  AppSectionFilterState,
} from 'App/State/AppSectionState';
import Movie from 'Movie/Movie';

interface CalendarAppState
  extends AppSectionState<Movie>,
    AppSectionFilterState<Movie> {}

export default CalendarAppState;
