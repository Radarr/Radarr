import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createImportMovieItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addMovie,
    (state) => state.importMovie,
    createAllMoviesSelector(),
    (id, addMovie, importMovie, movies) => {
      const item = _.find(importMovie.items, { id }) || {};
      const selectedMovie = item && item.selectedMovie;
      const isExistingMovie = !!selectedMovie && _.some(movies, { tmdbId: selectedMovie.tmdbId });

      return {
        defaultMonitor: addMovie.defaults.monitor,
        defaultQualityProfileId: addMovie.defaults.qualityProfileId,
        ...item,
        isExistingMovie
      };
    }
  );
}

export default createImportMovieItemSelector;
