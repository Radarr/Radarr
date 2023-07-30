import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createQueueItemSelector() {
  return createSelector(
    (_: AppState, { movieId }: { movieId: number }) => movieId,
    (state: AppState) => state.queue.details.items,
    (movieId, details) => {
      if (!movieId || !details) {
        return null;
      }

      return details.find((item) => item.movieId === movieId);
    }
  );
}

export default createQueueItemSelector;
