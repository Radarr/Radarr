import { createSelector } from 'reselect';

function createCollectionSelector() {
  return createSelector(
    (state, { collectionId }) => collectionId,
    (state) => state.movieCollections.itemMap,
    (state) => state.movieCollections.items,
    (collectionId, itemMap, allCollections) => {
      if (allCollections && itemMap && collectionId in itemMap) {
        return allCollections[itemMap[collectionId]];
      }
      return undefined;
    }
  );
}

export default createCollectionSelector;
