import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setArtistPosterOption } from 'Store/Actions/artistIndexActions';
import ArtistIndexPosterOptionsModalContent from './ArtistIndexPosterOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistIndex,
    (artistIndex) => {
      return artistIndex.posterOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangePosterOption(payload) {
      dispatch(setArtistPosterOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ArtistIndexPosterOptionsModalContent);
