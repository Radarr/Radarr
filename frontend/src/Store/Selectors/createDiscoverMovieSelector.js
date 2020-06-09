import { createSelector } from 'reselect';

function createDiscoverMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.discoverMovie,
    (movieId, allMovies) => {
      return allMovies.items.find((movie) => movie.tmdbId === movieId);
    }
  );
}

export default createDiscoverMovieSelector;
