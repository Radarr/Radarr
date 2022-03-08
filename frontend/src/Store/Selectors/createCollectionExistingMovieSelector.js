import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createCollectionExistingMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    createAllMoviesSelector(),
    (tmdbId, allMovies) => {
      return allMovies.find((movie) => movie.tmdbId === tmdbId);
    }
  );
}

export default createCollectionExistingMovieSelector;
