import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (state) => state.movies.error,
    (state) => state.movies.isFetching,
    (state) => state.movies.isPopulated,
    (movies, error, isFetching, isPopulated) => {
      return {
        count: movies.length,
        error,
        isFetching,
        isPopulated
      };
    }
  );
}

export default createMovieCountSelector;
