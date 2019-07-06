import _ from 'lodash';
import { createSelector } from 'reselect';

function createExclusionMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.netImportExclusions,
    (tmdbId, netImportExclusions) => {
      return _.some(netImportExclusions.items, { tmdbId });
    }
  );
}

export default createExclusionMovieSelector;
