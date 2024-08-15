import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state) => state.movieFiles.items,
    (state, { internalId }) => internalId,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (isExistingMovie, dimensions, queueItems, movieFiles, internalId, movieRuntimeFormat) => {
      const queueItem = queueItems.find((item) => internalId > 0 && item.movieId === internalId);
      const movieFile = movieFiles.find((item) => internalId > 0 && item.movieId === internalId);

      return {
        existingMovieId: internalId,
        isExistingMovie,
        isSmallScreen: dimensions.isSmallScreen,
        queueItem,
        movieFile,
        movieRuntimeFormat
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
