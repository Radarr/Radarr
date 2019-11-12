import { createSelector } from 'reselect';

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
