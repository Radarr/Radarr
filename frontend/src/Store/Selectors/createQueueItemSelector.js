import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state) => state.queue.details.items,
    (bookId, details) => {
      if (!bookId || !details) {
        return null;
      }

      return details.find((item) => {
        if (item.book) {
          return item.book.id === bookId;
        }

        return false;
      });
    }
  );
}

export default createQueueItemSelector;
