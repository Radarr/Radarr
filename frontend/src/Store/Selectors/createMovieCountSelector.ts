import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (state: AppState) => state.movies.error,
    (state: AppState) => state.movies.isFetching,
    (state: AppState) => state.movies.isPopulated,
    (state: AppState) => state.movies.totalRecords,
    (movies, error, isFetching, isPopulated, totalRecords) => {
      return {
        count: totalRecords || movies.length,
        error,
        isFetching,
        isPopulated,
      };
    }
  );
}

export default createMovieCountSelector;
