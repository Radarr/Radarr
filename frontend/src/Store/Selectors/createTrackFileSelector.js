import { createSelector } from 'reselect';

function createTrackFileSelector() {
  return createSelector(
    (state, { trackFileId }) => trackFileId,
    (state) => state.trackFiles,
    (trackFileId, trackFiles) => {
      if (!trackFileId) {
        return;
      }

      return trackFiles.items.find((trackFile) => trackFile.id === trackFileId);
    }
  );
}

export default createTrackFileSelector;
