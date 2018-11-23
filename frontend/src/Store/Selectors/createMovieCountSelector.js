import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (movies) => {
      return movies.length;
    }
  );
}

export default createMovieCountSelector;
