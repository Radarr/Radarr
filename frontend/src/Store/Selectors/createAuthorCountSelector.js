import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createAuthorCountSelector() {
  return createSelector(
    createAllAuthorsSelector(),
    (state) => state.authors.error,
    (authors, error) => {
      return {
        count: authors.length,
        error
      };
    }
  );
}

export default createAuthorCountSelector;
