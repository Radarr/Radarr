import { createSelector } from 'reselect';

function createMovieSelector(id) {
  if (id == null) {
    return createSelector(
      (state, { movieId }) => movieId,
      (state) => state.movies.itemMap,
      (state) => state.movies.items,
      (movieId, itemMap, allMovies) => {
        return allMovies[itemMap[movieId]];
      }
    );
  }

  return createSelector(
    (state) => state.movies.itemMap,
    (state) => state.movies.items,
    (itemMap, allMovies) => {
      return allMovies[itemMap[id]];
    }
  );
}

export default createMovieSelector;
