import _ from 'lodash';
import { createSelector } from 'reselect';

function createTrackFileSelector() {
  return createSelector(
    (state, { trackFileId }) => trackFileId,
    (state) => state.trackFiles,
    (trackFileId, trackFiles) => {
      if (!trackFileId) {
        return null;
      }

      return _.find(trackFiles.items, { id: trackFileId });
    }
  );
}

export default createTrackFileSelector;
