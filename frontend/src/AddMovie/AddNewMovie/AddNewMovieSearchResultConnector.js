import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExclusionMovieSelector from 'Store/Selectors/createExclusionMovieSelector';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createExclusionMovieSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state, { internalId }) => internalId,
    (isExistingMovie, isExclusionMovie, dimensions, queueItems, internalId) => {
      const firstQueueItem = queueItems.find((q) => q.movieId === internalId && internalId > 0);

      return {
        isExistingMovie,
        isExclusionMovie,
        isSmallScreen: dimensions.isSmallScreen,
        queueStatus: firstQueueItem ? firstQueueItem.status : null,
        queueState: firstQueueItem ? firstQueueItem.trackedDownloadState : null
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
