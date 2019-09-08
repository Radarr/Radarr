import { createSelector } from 'reselect';

function createAddListMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.addMovie,
    (movieId, allMovies) => {
      return allMovies.items.find((movie) => movie.tmdbId === movieId);
    }
  );
}

export default createAddListMovieSelector;
