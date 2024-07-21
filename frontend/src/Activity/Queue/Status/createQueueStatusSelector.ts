import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createQueueStatusSelector() {
  return createSelector(
    (state: AppState) => state.queue.status.isPopulated,
    (state: AppState) => state.queue.status.item,
    (state: AppState) => state.queue.options.includeUnknownMovieItems,
    (isPopulated, status, includeUnknownMovieItems) => {
      const {
        errors,
        warnings,
        unknownErrors,
        unknownWarnings,
        count,
        totalCount,
      } = status;

      return {
        ...status,
        isPopulated,
        count: includeUnknownMovieItems ? totalCount : count,
        errors: includeUnknownMovieItems ? errors || unknownErrors : errors,
        warnings: includeUnknownMovieItems
          ? warnings || unknownWarnings
          : warnings,
      };
    }
  );
}

export default createQueueStatusSelector;
