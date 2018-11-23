import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHistory, markAsFailed } from 'Store/Actions/historyActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieHistoryRow from './MovieHistoryRow';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      return {
        movie
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHistory,
  markAsFailed
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieHistoryRow);
