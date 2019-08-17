import { createSelector } from 'reselect';

function createMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.movies.itemMap,
    (state) => state.movies.items,
    (movieId, itemMap, allMovies) => {
      if (allMovies && itemMap && movieId in itemMap) {
        return allMovies[itemMap[movieId]];
      }
      return undefined;
    }
  );
}

export default createMovieSelector;
