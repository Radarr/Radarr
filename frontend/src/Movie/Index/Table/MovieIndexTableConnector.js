import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { setMovieSort } from 'Store/Actions/movieIndexActions';
import MovieIndexTable from './MovieIndexTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.dimensions,
    createClientSideCollectionSelector('movies', 'movieIndex'),
    (dimensions, movies) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        ...movies
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSortPress(sortKey) {
      dispatch(setMovieSort({ sortKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieIndexTable);
