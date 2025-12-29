import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllBookSeriesSelector() {
  return createSelector(
    (state: AppState) => state.bookSeries,
    (bookSeries) => {
      return bookSeries.items;
    }
  );
}

export default createAllBookSeriesSelector;
