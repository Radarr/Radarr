import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllTVShowsSelector() {
  return createSelector(
    (state: AppState) => state.tvShows,
    (tvShows) => {
      return tvShows.items;
    }
  );
}

export default createAllTVShowsSelector;
