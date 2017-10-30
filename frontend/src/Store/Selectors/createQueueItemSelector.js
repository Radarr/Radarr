import _ from 'lodash';
import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { albumId }) => albumId,
    (state) => state.queue.details,
    (albumId, details) => {
      if (!albumId) {
        return null;
      }

      return _.find(details.items, (item) => {
        return item.album.id === albumId;
      });
    }
  );
}

export default createQueueItemSelector;
