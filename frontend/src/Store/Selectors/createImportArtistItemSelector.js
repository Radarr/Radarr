import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllArtistSelector from './createAllArtistSelector';

function createImportArtistItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addArtist,
    (state) => state.importArtist,
    createAllArtistSelector(),
    (id, addArtist, importArtist, series) => {
      const item = _.find(importArtist.items, { id }) || {};
      const selectedArtist = item && item.selectedArtist;
      const isExistingArtist = !!selectedArtist && _.some(series, { foreignArtistId: selectedArtist.foreignArtistId });

      return {
        defaultMonitor: addArtist.defaults.monitor,
        defaultQualityProfileId: addArtist.defaults.qualityProfileId,
        defaultSeriesType: addArtist.defaults.seriesType,
        defaultSeasonFolder: addArtist.defaults.seasonFolder,
        ...item,
        isExistingArtist
      };
    }
  );
}

export default createImportArtistItemSelector;
