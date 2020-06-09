import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMovieSort } from 'Store/Actions/discoverMovieActions';
import AddListMovieTable from './AddListMovieTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    (state) => state.discoverMovie.columns,
    (dimensions, columns) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        columns
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setListMovieSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AddListMovieTable);
