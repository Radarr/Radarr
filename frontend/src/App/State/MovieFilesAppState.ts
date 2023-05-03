import AppSectionState, {
  AppSectionDeleteState,
} from 'App/State/AppSectionState';
import { MovieFile } from 'MovieFile/MovieFile';

interface MovieFilesAppState
  extends AppSectionState<MovieFile>,
    AppSectionDeleteState {}

export default MovieFilesAppState;
