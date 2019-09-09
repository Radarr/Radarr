import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setArtistOverviewOption } from 'Store/Actions/artistIndexActions';
import ArtistIndexOverviewOptionsModalContent from './ArtistIndexOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistIndex,
    (artistIndex) => {
      return artistIndex.overviewOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setArtistOverviewOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ArtistIndexOverviewOptionsModalContent);
