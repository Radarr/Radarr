import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setArtistSort } from 'Store/Actions/artistIndexActions';
import ArtistIndexTable from './ArtistIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector('artist', 'artistIndex'),
    (dimensions, artist) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        ...artist,
        showBanners: artist.tableOptions.showBanners
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
