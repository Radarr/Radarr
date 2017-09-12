import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createExistingArtistSelector() {
  return createSelector(
    (state, { foreignArtistId }) => foreignArtistId,
    createAllArtistSelector(),
    (foreignArtistId, series) => {
      return _.some(series, { foreignArtistId });
    }
  );
}

export default createExistingArtistSelector;
