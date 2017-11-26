import _ from 'lodash';
import { createSelector } from 'reselect';

function createMetadataProfileSelector() {
  return createSelector(
    (state, { metadataProfileId }) => metadataProfileId,
    (state) => state.settings.metadataProfiles.items,
    (metadataProfileId, metadataProfiles) => {
      return _.find(metadataProfiles, { id: metadataProfileId });
    }
  );
}

export default createMetadataProfileSelector;
