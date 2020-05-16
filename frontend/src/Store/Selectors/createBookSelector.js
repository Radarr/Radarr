import _ from 'lodash';
import { createSelector } from 'reselect';
import bookEntities from 'Book/bookEntities';

function createBookSelector() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state, { bookEntity = bookEntities.BOOKS }) => _.get(state, bookEntity, { items: [] }),
    (bookId, books) => {
      return _.find(books.items, { id: bookId });
    }
  );
}

export default createBookSelector;
