import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    createAllMoviesSelector(),
    (movieId, allMovies) => {
      return allMovies.find((movie) => movie.id === movieId);
    }
  );
}

export default createMovieSelector;
