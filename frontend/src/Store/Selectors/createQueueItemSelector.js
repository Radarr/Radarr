import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.queue.details.items,
    (movieId, details) => {
      if (!movieId || !details) {
        return null;
      }

      return details.find((item) => {
        return item.movieId === movieId;
      });
    }
  );
}

export default createQueueItemSelector;
