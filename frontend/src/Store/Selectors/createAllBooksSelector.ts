import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllBooksSelector() {
  return createSelector(
    (state: AppState) => state.books,
    (books) => {
      return books.items;
    }
  );
}

export default createAllBooksSelector;
