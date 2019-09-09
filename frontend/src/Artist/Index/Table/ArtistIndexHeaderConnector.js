import { connect } from 'react-redux';
import { setArtistTableOption } from 'Store/Actions/artistIndexActions';
import ArtistIndexHeader from './ArtistIndexHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setArtistTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(ArtistIndexHeader);
