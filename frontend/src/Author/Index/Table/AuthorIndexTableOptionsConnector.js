import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import AuthorIndexTableOptions from './AuthorIndexTableOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorIndex.tableOptions,
    (tableOptions) => {
      return tableOptions;
    }
  );
}

export default connect(createMapStateToProps)(AuthorIndexTableOptions);
