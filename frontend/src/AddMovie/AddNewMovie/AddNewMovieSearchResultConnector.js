import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createDimensionsSelector(),
    (isExistingMovie, dimensions) => {
      return {
        isExistingMovie,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
