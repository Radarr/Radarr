import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import ImportMovieSearchResult from './ImportMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    (isExistingMovie) => {
      return {
        isExistingMovie
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportMovieSearchResult);
