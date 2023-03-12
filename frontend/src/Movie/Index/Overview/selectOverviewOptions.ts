import { createSelector } from 'reselect';

const selectOverviewOptions = createSelector(
  (state) => state.movieIndex.overviewOptions,
  (overviewOptions) => overviewOptions
);

export default selectOverviewOptions;
