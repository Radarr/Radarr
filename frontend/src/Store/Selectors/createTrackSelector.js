import _ from 'lodash';
import { createSelector } from 'reselect';

function createTrackSelector() {
  return createSelector(
    (state, { trackId }) => trackId,
    (state) => state.tracks,
    (trackId, tracks) => {
      return _.find(tracks.items, { id: trackId });
    }
  );
}

export default createTrackSelector;
