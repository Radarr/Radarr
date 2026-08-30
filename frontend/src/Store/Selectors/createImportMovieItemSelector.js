import _ from 'lodash';
import { createSelector } from 'reselect';

function createImportMovieItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addMovie,
    (state) => state.importMovie,
    (id, addMovie, importMovie) => {
      const item = _.find(importMovie.items, { id }) || {};
      const selectedMovie = item && item.selectedMovie;
      const isExistingMovie = !!selectedMovie?.id;

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
