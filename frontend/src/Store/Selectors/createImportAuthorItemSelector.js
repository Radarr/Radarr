import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllAuthorsSelector from './createAllAuthorsSelector';

function createImportAuthorItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addAuthor,
    (state) => state.importAuthor,
    createAllAuthorsSelector(),
    (id, addAuthor, importAuthor, author) => {
      const item = _.find(importAuthor.items, { id }) || {};
      const selectedAuthor = item && item.selectedAuthor;
      const isExistingAuthor = !!selectedAuthor && _.some(author, { titleSlug: selectedAuthor.titleSlug });

      return {
        defaultMonitor: addAuthor.defaults.monitor,
        defaultQualityProfileId: addAuthor.defaults.qualityProfileId,
        ...item,
        isExistingAuthor
      };
    }
  );
}

export default createImportAuthorItemSelector;
