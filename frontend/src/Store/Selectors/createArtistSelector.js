import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createArtistSelector() {
  return createSelector(
    (state, { artistId }) => artistId,
    createAllSeriesSelector(),
    (artistId, series) => {
      return _.find(series, { id: artistId });
    }
  );
}

export default createArtistSelector;
