import _ from 'lodash';
import { createSelector } from 'reselect';

function createMovieCollectionListSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.importLists.items,
    (tmdbId, importLists) => {
      const importListIds = _.reduce(importLists, (acc, list) => {
        if (list.implementation === 'TMDbCollectionImport') {
          const collectionIdField = list.fields.find((field) => {
            return field.name === 'collectionId';
          });

          if (collectionIdField && parseInt(collectionIdField.value) === tmdbId) {
            acc.push(list);
            return acc;
          }
        }

        return acc;
      }, []);

      if (importListIds.length === 0) {
        return undefined;
      }

      return importListIds[0];
    }
  );
}

export default createMovieCollectionListSelector;
