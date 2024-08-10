import _ from 'lodash';
import { createSelector } from 'reselect';

function createExclusionMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.importListExclusions,
    (tmdbId, importListExclusions) => {
      return _.some(importListExclusions.items, { tmdbId });
    }
  );
}

export default createExclusionMovieSelector;
