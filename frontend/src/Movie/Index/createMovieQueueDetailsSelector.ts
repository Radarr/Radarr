import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export interface MovieQueueDetails {
  count: number;
}

function createMovieQueueDetailsSelector(movieId: number) {
  return createSelector(
    (state: AppState) => state.queue.details.items,
    (queueItems) => {
      return queueItems.reduce(
        (acc: MovieQueueDetails, item) => {
          if (item.movieId !== movieId) {
            return acc;
          }

          acc.count++;

          return acc;
        },
        {
          count: 0,
        }
      );
    }
  );
}

export default createMovieQueueDetailsSelector;
