import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAuthorSort } from 'Store/Actions/authorIndexActions';
import AuthorIndexTable from './AuthorIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    (state) => state.authorIndex.tableOptions,
    (state) => state.authorIndex.columns,
    (dimensions, tableOptions, columns) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        showBanners: tableOptions.showBanners,
        columns
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setAuthorSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AuthorIndexTable);
