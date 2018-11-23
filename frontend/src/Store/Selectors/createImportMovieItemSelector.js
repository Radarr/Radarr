import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createImportMovieItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addMovie,
    (state) => state.importMovie,
    createAllMoviesSelector(),
    (id, addMovie, importMovie, series) => {
      const item = _.find(importMovie.items, { id }) || {};
      const selectedMovie = item && item.selectedMovie;
      const isExistingMovie = !!selectedMovie && _.some(series, { tvdbId: selectedMovie.tvdbId });

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
