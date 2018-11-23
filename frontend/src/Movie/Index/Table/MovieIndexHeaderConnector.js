import { connect } from 'react-redux';
import { setMovieTableOption } from 'Store/Actions/movieIndexActions';
import MovieIndexHeader from './MovieIndexHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setMovieTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(MovieIndexHeader);
