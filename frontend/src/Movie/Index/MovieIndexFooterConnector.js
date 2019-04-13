import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieIndexFooter from './MovieIndexFooter';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies.items,
    (movies) => {
      return {
        movies
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexFooter);