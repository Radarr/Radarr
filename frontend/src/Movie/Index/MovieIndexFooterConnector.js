import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import MovieIndexFooter from './MovieIndexFooter';

function createUnoptimizedSelector() {
  return createSelector(
    createClientSideCollectionSelector('movies', 'movieIndex'),
    (movies) => {
      return movies.items.map((s) => {
        const {
          monitored,
          status,
          statistics,
          sizeOnDisk,
          hasFile
        } = s;

        return {
          monitored,
          status,
          statistics,
          sizeOnDisk,
          hasFile
        };
      });
    }
  );
}

function createMoviesSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSelector(),
    (movies) => movies
  );
}

function createMapStateToProps() {
  return createSelector(
    createMoviesSelector(),
    (movies) => {
      return {
        movies
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexFooter);
