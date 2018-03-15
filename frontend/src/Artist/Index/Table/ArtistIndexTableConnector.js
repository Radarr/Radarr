import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setArtistSort } from 'Store/Actions/artistIndexActions';
import ArtistIndexTable from './ArtistIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector(),
    (dimensions, artist) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        ...artist
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

export default connectSection(
  createMapStateToProps,
  createMapDispatchToProps,
  undefined,
  undefined,
  { section: 'artist', uiSection: 'artistIndex' }
)(ArtistIndexTable);
