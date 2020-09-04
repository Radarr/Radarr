import { connect } from 'react-redux';
import { setListMovieTableOption } from 'Store/Actions/discoverMovieActions';
import DiscoverMovieHeader from './DiscoverMovieHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setListMovieTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(DiscoverMovieHeader);
