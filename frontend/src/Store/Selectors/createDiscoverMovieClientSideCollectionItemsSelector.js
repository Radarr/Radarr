import { createSelector } from 'reselect';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';
import createDeepEqualSelector from './createDeepEqualSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('movies', uiSection),
    (movies) => {
      const items = movies.items.map((s) => {
        const {
          tmdbId,
          sortTitle
        } = s;

        return {
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

function createDiscoverMovieClientSideCollectionItemsSelector(uiSection) {
  return createDeepEqualSelector(
    createUnoptimizedSelector(uiSection),
    (movies) => movies
  );
}

export default createDiscoverMovieClientSideCollectionItemsSelector;
