import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (state: AppState) => state.movies.error,
    (state: AppState) => state.movies.isFetching,
    (state: AppState) => state.movies.isPopulated,
    (movies, error, isFetching, isPopulated) => {
      return {
        count: movies.length,
        error,
        isFetching,
        isPopulated,
      };
    }
  );
}

export default createMovieCountSelector;
