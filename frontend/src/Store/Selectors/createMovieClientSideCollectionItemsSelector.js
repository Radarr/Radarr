import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';

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

function movieListEqual(a, b) {
  return hasDifferentItemsOrOrder(a, b);
}

const createMovieEqualSelector = createSelectorCreator(
  defaultMemoize,
  movieListEqual
);

function createMovieClientSideCollectionItemsSelector(uiSection) {
  return createMovieEqualSelector(
    createUnoptimizedSelector(uiSection),
    (movies) => movies
  );
}

export default createMovieClientSideCollectionItemsSelector;
