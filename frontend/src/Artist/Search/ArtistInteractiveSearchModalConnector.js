import { connect } from 'react-redux';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import ArtistInteractiveSearchModal from './ArtistInteractiveSearchModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    onModalClose() {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
      props.onModalClose();
    }
  };
}

export default connect(null, createMapDispatchToProps)(ArtistInteractiveSearchModal);
