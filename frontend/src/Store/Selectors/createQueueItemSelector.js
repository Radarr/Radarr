import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state) => state.queue.details.items,
    (bookId, details) => {
      if (!bookId) {
        return null;
      }

      return details.find((item) => {
        if (item.album) {
          return item.album.id === bookId;
        }

        return false;
      });
    }
  );
}

export default createQueueItemSelector;
