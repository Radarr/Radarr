import { createSelector } from 'reselect';

const selectPosterOptions = createSelector(
  (state) => state.movieIndex.posterOptions,
  (posterOptions) => posterOptions
);

export default selectPosterOptions;
