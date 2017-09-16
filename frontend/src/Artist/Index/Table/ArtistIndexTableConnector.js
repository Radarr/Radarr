import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setArtistSort } from 'Store/Actions/artistIndexActions';
import ArtistIndexTable from './ArtistIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector(),
    (dimensions, series) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        ...series
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
                { withRef: true },
                { section: 'series', uiSection: 'artistIndex' }
              )(ArtistIndexTable);
