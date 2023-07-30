import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Movie from 'Movie/Movie';
import ImportList from 'typings/ImportList';
import MovieCollection from 'typings/MovieCollection';
import createAllMoviesSelector from './createAllMoviesSelector';

function createProfileInUseSelector(profileProp: string) {
  return createSelector(
    (_: AppState, { id }: { id: number }) => id,
    createAllMoviesSelector(),
    (state: AppState) => state.settings.importLists.items,
    (state: AppState) => state.movieCollections.items,
    (id, movies, lists, collections) => {
      if (!id) {
        return false;
      }

      return (
        movies.some((m) => m[profileProp as keyof Movie] === id) ||
        lists.some((list) => list[profileProp as keyof ImportList] === id) ||
        collections.some(
          (collection) =>
            collection[profileProp as keyof MovieCollection] === id
        )
      );
    }
  );
}

export default createProfileInUseSelector;
