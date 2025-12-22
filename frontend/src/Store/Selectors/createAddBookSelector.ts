import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAddBookSelector() {
  return createSelector(
    (state: AppState) => state.addBook,
    (addBook) => {
      return addBook;
    }
  );
}

export default createAddBookSelector;
