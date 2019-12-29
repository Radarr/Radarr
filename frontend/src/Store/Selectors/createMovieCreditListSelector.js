import _ from 'lodash';
import { createSelector } from 'reselect';

function createMovieCreditListSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.netImports.items,
    (tmdbId, netImports) => {
      const netImportIds = _.reduce(netImports, (acc, list) => {
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

      let netImportId = 0;

      if (netImportIds.length > 0) {
        netImportId = netImportIds[0].id;
      }

      return {
        netImportId
      };
    }
  );
}

export default createMovieCreditListSelector;
