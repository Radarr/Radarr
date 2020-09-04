import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMovieOption, setListMovieOverviewOption } from 'Store/Actions/discoverMovieActions';
import DiscoverMovieOverviewOptionsModalContent from './DiscoverMovieOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.discoverMovie,
    (discoverMovie) => {
      return {
        ...discoverMovie.options,
        ...discoverMovie.overviewOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setListMovieOverviewOption(payload));
    },
    onChangeOption(payload) {
      dispatch(setListMovieOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DiscoverMovieOverviewOptionsModalContent);
