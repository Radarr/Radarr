import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export function createMovieCollectionSelector(collectionId?: number) {
  return createSelector(
    (state: AppState) => state.movieCollections.itemMap,
    (state: AppState) => state.movieCollections.items,
    (itemMap, allMovieCollections) => {
      return collectionId
        ? allMovieCollections[itemMap[collectionId]]
        : undefined;
    }
  );
}

function useMovieCollection(collectionId: number | undefined) {
  return useSelector(createMovieCollectionSelector(collectionId));
}

export default useMovieCollection;
