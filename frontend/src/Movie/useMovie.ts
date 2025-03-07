import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export type MovieEntity =
  | 'calendar'
  | 'movies'
  | 'interactiveImport.movies'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

export function createMovieSelector(movieId?: number) {
  return createSelector(
    (state: AppState) => state.movies.itemMap,
    (state: AppState) => state.movies.items,
    (itemMap, allMovies) => {
      return movieId ? allMovies[itemMap[movieId]] : undefined;
    }
  );
}

function useMovie(movieId: number | undefined) {
  return useSelector(createMovieSelector(movieId));
}

export default useMovie;
