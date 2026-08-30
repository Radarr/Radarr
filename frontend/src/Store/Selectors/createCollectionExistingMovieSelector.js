import { createSelector } from 'reselect';
import createAllMoviesSelector from './createAllMoviesSelector';

function createCollectionExistingMovieSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state, props) => props,
    createAllMoviesSelector(),
    (tmdbId, props, allMovies) => {
      return allMovies.find((movie) => movie.tmdbId === tmdbId) ||
        (props.id ? props : undefined);
    }
  );
}

export default createCollectionExistingMovieSelector;
