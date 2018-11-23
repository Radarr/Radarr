import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieIndexTableOptions from './MovieIndexTableOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieIndex.tableOptions,
    (tableOptions) => {
      return tableOptions;
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexTableOptions);
