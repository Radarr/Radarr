import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setMoviePosterOption } from 'Store/Actions/movieIndexActions';
import MovieIndexPosterOptionsModalContent from './MovieIndexPosterOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieIndex,
    (movieIndex) => {
      return movieIndex.posterOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangePosterOption(payload) {
      dispatch(setMoviePosterOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieIndexPosterOptionsModalContent);
