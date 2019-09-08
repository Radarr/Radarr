import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMoviePosterOption } from 'Store/Actions/addMovieActions';
import AddListMoviePosterOptionsModalContent from './AddListMoviePosterOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (addMovie) => {
      return addMovie.posterOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangePosterOption(payload) {
      dispatch(setListMoviePosterOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AddListMoviePosterOptionsModalContent);
