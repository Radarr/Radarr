import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    createAllMoviesSelector(),
    (movieId, movies) => {
      return _.find(movies, { id: movieId });
    }
  );
}

export default createMovieSelector;
