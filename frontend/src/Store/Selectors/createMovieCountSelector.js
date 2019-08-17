import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (state) => state.movies.error,
    (movies, error) => {
      return {
        count: movies.length,
        error
      };
    }
  );
}

export default createMovieCountSelector;
