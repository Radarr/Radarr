import { createSelector } from 'reselect';

function createBookFileSelector() {
  return createSelector(
    (state, { bookFileId }) => bookFileId,
    (state) => state.bookFiles,
    (bookFileId, bookFiles) => {
      if (!bookFileId) {
        return;
      }

      return bookFiles.items.find((bookFile) => bookFile.id === bookFileId);
    }
  );
}

export default createBookFileSelector;
