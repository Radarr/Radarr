import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import createExclusionMovieSelector from 'Store/Selectors/createExclusionMovieSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createExclusionMovieSelector(),
    createDimensionsSelector(),
    (isExistingMovie, isExclusionMovie, dimensions) => {
      return {
        isExistingMovie,
        isExclusionMovie,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
