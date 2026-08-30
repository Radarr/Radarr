import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllMoviesSelector from './createAllMoviesSelector';

function createExistingMovieSelector() {
  return createSelector(
    (_: AppState, { tmdbId }: { tmdbId: number }) => tmdbId,
    (_: AppState, { internalId }: { internalId?: number }) => internalId,
    (state: AppState) => state.movies.facets.tmdbIds,
    createAllMoviesSelector(),
    (tmdbId, internalId, tmdbIds = [], movies) => {
      return (
        !!internalId || tmdbIds.includes(tmdbId) || some(movies, { tmdbId })
      );
    }
  );
}

export default createExistingMovieSelector;
