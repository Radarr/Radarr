import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingArtistSelector() {
  return createSelector(
    (state, { foreignArtistId }) => foreignArtistId,
    createAllSeriesSelector(),
    (foreignArtistId, series) => {
      return _.some(series, { foreignArtistId });
    }
  );
}

export default createExistingArtistSelector;
