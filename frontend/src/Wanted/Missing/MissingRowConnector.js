import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MissingRow from './MissingRow';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movies) => {
      return {
        movies
      };
    }
  );
}

export default connect(createMapStateToProps)(MissingRow);
