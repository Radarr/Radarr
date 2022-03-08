import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setMovieCollectionsOption, setMovieCollectionsOverviewOption } from 'Store/Actions/movieCollectionActions';
import CollectionOverviewOptionsModalContent from './CollectionOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCollections,
    (movieCollections) => {
      return {
        ...movieCollections.options,
        ...movieCollections.overviewOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setMovieCollectionsOverviewOption(payload));
    },
    onChangeOption(payload) {
      dispatch(setMovieCollectionsOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(CollectionOverviewOptionsModalContent);
