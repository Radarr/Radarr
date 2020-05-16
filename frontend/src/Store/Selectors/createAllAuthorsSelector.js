import { createSelector } from 'reselect';

function createAllAuthorsSelector() {
  return createSelector(
    (state) => state.authors,
    (author) => {
      return author.items;
    }
  );
}

export default createAllAuthorsSelector;
