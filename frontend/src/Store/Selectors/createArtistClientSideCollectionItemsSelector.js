import { createSelector } from 'reselect';
import createDeepEqualSelector from './createDeepEqualSelector';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('artist', uiSection),
    (artist) => {
      const items = artist.items.map((s) => {
        const {
          id,
          sortName
        } = s;

        return {
          id,
          sortName
        };
      });

      return {
        ...artist,
        items
      };
    }
  );
}

function createArtistClientSideCollectionItemsSelector(uiSection) {
  return createDeepEqualSelector(
    createUnoptimizedSelector(uiSection),
    (artist) => artist
  );
}

export default createArtistClientSideCollectionItemsSelector;
