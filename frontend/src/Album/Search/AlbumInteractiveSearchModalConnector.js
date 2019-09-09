import { connect } from 'react-redux';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import AlbumInteractiveSearchModal from './AlbumInteractiveSearchModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    onModalClose() {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
      props.onModalClose();
    }
  };
}

export default connect(null, createMapDispatchToProps)(AlbumInteractiveSearchModal);
