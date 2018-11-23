import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createExistingMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    createAllMoviesSelector(),
    (tmdbId, movies) => {
      return _.some(movies, { tmdbId });
    }
  );
}

export default createExistingMovieSelector;
