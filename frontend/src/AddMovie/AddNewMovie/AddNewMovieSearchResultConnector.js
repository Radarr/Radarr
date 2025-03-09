import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createDimensionsSelector(),
    (state) => state.movieFiles.items,
    (state, { internalId }) => internalId,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (isExistingMovie, dimensions, movieFiles, internalId, movieRuntimeFormat) => {
      const movieFile = movieFiles.find((item) => internalId > 0 && item.movieId === internalId);

      return {
        existingMovieId: internalId,
        isExistingMovie,
        isSmallScreen: dimensions.isSmallScreen,
        movieFile,
        movieRuntimeFormat
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
