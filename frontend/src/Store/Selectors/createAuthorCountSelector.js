import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createAuthorCountSelector() {
  return createSelector(
    createAllAuthorsSelector(),
    (state) => state.authors.error,
    (state) => state.authors.isFetching,
    (state) => state.authors.isPopulated,
    (authors, error, isFetching, isPopulated) => {
      return {
        count: authors.length,
        error,
        isFetching,
        isPopulated
      };
    }
  );
}

export default createAuthorCountSelector;
