import { createSelector } from 'reselect';
import createDeepEqualSelector from './createDeepEqualSelector';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('movies', uiSection),
    (movies) => {
      const items = movies.items.map((s) => {
        const {
          id,
          sortTitle
        } = s;

        return {
          id,
          sortTitle
        };
      });

      return {
        ...movies,
        items
      };
    }
  );
}

function createMovieClientSideCollectionItemsSelector(uiSection) {
  return createDeepEqualSelector(
    createUnoptimizedSelector(uiSection),
    (movies) => movies
  );
}

export default createMovieClientSideCollectionItemsSelector;
