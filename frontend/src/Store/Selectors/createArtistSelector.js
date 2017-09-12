import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createArtistSelector() {
  return createSelector(
    (state, { artistId }) => artistId,
    createAllArtistSelector(),
    (artistId, series) => {
      return _.find(series, { id: artistId });
    }
  );
}

export default createArtistSelector;
