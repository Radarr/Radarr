import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createExistingArtistSelector() {
  return createSelector(
    (state, { foreignArtistId }) => foreignArtistId,
    createAllArtistSelector(),
    (foreignArtistId, artist) => {
      return _.some(artist, { foreignArtistId });
    }
  );
}

export default createExistingArtistSelector;
