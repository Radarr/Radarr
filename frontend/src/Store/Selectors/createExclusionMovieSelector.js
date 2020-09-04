import _ from 'lodash';
import { createSelector } from 'reselect';

function createExclusionMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.importExclusions,
    (tmdbId, importExclusions) => {
      return _.some(importExclusions.items, { tmdbId });
    }
  );
}

export default createExclusionMovieSelector;
