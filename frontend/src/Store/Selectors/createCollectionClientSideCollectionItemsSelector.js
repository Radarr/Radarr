import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('movieCollections', uiSection),
    (movies) => {
      const items = movies.items.map((s) => {
        const {
          id,
          tmdbId,
          sortTitle
        } = s;

        return {
          id,
          tmdbId,
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

function createCollectionClientSideCollectionItemsSelector(uiSection) {
  return createMovieEqualSelector(
    createUnoptimizedSelector(uiSection),
    (movies) => movies
  );
}

export default createCollectionClientSideCollectionItemsSelector;
