import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setMovieSort } from 'Store/Actions/movieIndexActions';
import MovieIndexTable from './MovieIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    (state) => state.movieIndex.tableOptions,
    (state) => state.movieIndex.columns,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (dimensions, tableOptions, columns, movieRuntimeFormat) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        showBanners: tableOptions.showBanners,
        columns,
        movieRuntimeFormat
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setMovieSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieIndexTable);
