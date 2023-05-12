import AppSectionState from 'App/State/AppSectionState';
import Movie from 'Movie/Movie';
import { FilterBuilderProp } from './AppState';

interface CalendarAppState extends AppSectionState<Movie> {
  filterBuilderProps: FilterBuilderProp<Movie>[];
}

export default CalendarAppState;
