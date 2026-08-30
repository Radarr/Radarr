import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import ImportMovieSearchResult from './ImportMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => !!id,
    (isExistingMovie) => {
      return {
        isExistingMovie
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportMovieSearchResult);
