import { createSelector } from 'reselect';

export function createMovieSelectorForHook(movieId) {
  return createSelector(
    (state) => state.movies.itemMap,
    (state) => state.movies.items,
    (itemMap, allMovies) => {

      return movieId ? allMovies[itemMap[movieId]]: undefined;
    }
  );
}

function createMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.movies.itemMap,
    (state) => state.movies.items,
    (movieId, itemMap, allMovies) => {
      return allMovies[itemMap[movieId]];
    }
  );
}

export default createMovieSelector;
