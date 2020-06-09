import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMovieOverviewOption } from 'Store/Actions/discoverMovieActions';
import AddListMovieOverviewOptionsModalContent from './AddListMovieOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.discoverMovie,
    (discoverMovie) => {
      return discoverMovie.overviewOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setListMovieOverviewOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AddListMovieOverviewOptionsModalContent);
