import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const selectTableOptions = createSelector(
  (state: AppState) => state.movieIndex.tableOptions,
  (tableOptions) => tableOptions
);

export default selectTableOptions;
