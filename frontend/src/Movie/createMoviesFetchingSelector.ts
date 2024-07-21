import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createMoviesFetchingSelector() {
  return createSelector(
    (state: AppState) => state.movies,
    (movies) => {
      return {
        isMoviesFetching: movies.isFetching,
        isMoviesPopulated: movies.isPopulated,
        moviesError: movies.error,
      };
    }
  );
}

export default createMoviesFetchingSelector;
