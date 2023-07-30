import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllMoviesSelector from './createAllMoviesSelector';

function createExistingMovieSelector() {
  return createSelector(
    (_: AppState, { tmdbId }: { tmdbId: number }) => tmdbId,
    createAllMoviesSelector(),
    (tmdbId, movies) => {
      return some(movies, { tmdbId });
    }
  );
}

export default createExistingMovieSelector;
