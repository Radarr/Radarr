import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setArtistSort } from 'Store/Actions/artistIndexActions';
import ArtistIndexTable from './ArtistIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    (state) => state.artistIndex.tableOptions,
    (state) => state.artistIndex.columns,
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
      dispatch(setArtistSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ArtistIndexTable);
