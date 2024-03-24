import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Movie from 'Movie/Movie';

function createMultiMoviesSelector(movieIds: number[]) {
  return createSelector(
    (state: AppState) => state.movies.itemMap,
    (state: AppState) => state.movies.items,
    (itemMap, allMovies) => {
      return movieIds.reduce((acc: Movie[], movieId) => {
        const movie = allMovies[itemMap[movieId]];

        if (movie) {
          acc.push(movie);
        }

        return acc;
      }, []);
    }
  );
}

export default createMultiMoviesSelector;
