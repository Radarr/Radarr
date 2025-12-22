import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllAuthorsSelector() {
  return createSelector(
    (state: AppState) => state.authors,
    (authors) => {
      return authors.items;
    }
  );
}

export default createAllAuthorsSelector;
