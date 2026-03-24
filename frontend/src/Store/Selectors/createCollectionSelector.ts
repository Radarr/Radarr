import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export function createCollectionSelectorForHook(tmdbId: number) {
  return createSelector(
    (state: AppState) => state.movieCollections.items,
    (collections) => {
      return collections.find((item) => item.tmdbId === tmdbId);
    }
  );
}

function createCollectionSelector() {
  return createSelector(
    (_: AppState, { collectionId }: { collectionId: number }) => collectionId,
    (state: AppState) => state.movieCollections.itemMap,
    (state: AppState) => state.movieCollections.items,
    (collectionId, itemMap, allCollections) => {
      return allCollections && itemMap && collectionId in itemMap
        ? allCollections[itemMap[collectionId]]
        : undefined;
    }
  );
}

export default createCollectionSelector;
