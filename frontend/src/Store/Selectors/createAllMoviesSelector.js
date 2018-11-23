import { createSelector } from 'reselect';

function createAllMoviesSelector() {
  return createSelector(
    (state) => state.movies,
    (movies) => {
      return movies.items;
    }
  );
}

export default createAllMoviesSelector;
