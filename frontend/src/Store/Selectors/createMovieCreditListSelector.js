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

      let importListId = 0;

      if (importListIds.length > 0) {
        importListId = importListIds[0].id;
      }

      return {
        importListId
      };
    }
  );
}

export default createMovieCreditListSelector;
