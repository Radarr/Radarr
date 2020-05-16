import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createExistingAuthorSelector() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    createAllAuthorsSelector(),
    (titleSlug, author) => {
      return _.some(author, { titleSlug });
    }
  );
}

export default createExistingAuthorSelector;
