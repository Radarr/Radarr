import { connect } from 'react-redux';
import { setListMovieTableOption } from 'Store/Actions/addMovieActions';
import AddListMovieHeader from './AddListMovieHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setListMovieTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(AddListMovieHeader);
