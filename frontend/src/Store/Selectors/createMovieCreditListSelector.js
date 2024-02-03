import _ from 'lodash';
import { createSelector } from 'reselect';

function createMovieCreditListSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.importLists.items,
    (tmdbId, importLists) => {
      const importListIds = _.reduce(importLists, (acc, list) => {
        if (list.implementation === 'TMDbPersonImport') {
          const personIdField = list.fields.find((field) => {
            return field.name === 'personId';
          });

          if (personIdField && parseInt(personIdField.value) === tmdbId) {
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

export default createMovieCreditListSelector;
