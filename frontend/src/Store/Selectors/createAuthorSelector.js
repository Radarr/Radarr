import { createSelector } from 'reselect';

function createAuthorSelector() {
  return createSelector(
    (state, { authorId }) => authorId,
    (state) => state.authors.itemMap,
    (state) => state.authors.items,
    (authorId, itemMap, allAuthors) => {
      return allAuthors[itemMap[authorId]];
    }
  );
}

export default createAuthorSelector;
