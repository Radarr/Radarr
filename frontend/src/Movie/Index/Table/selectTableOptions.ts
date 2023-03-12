import { createSelector } from 'reselect';

const selectTableOptions = createSelector(
  (state) => state.movieIndex.tableOptions,
  (tableOptions) => tableOptions
);

export default selectTableOptions;
