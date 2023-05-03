import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const selectPosterOptions = createSelector(
  (state: AppState) => state.movieIndex.posterOptions,
  (posterOptions) => posterOptions
);

export default selectPosterOptions;
