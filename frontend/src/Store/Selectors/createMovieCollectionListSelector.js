import _ from 'lodash';
import { createSelector } from 'reselect';

function createMovieCollectionListSelector() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.settings.netImports.items,
    (tmdbId, netImports) => {
      const netImportIds = _.reduce(netImports, (acc, list) => {
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

      if (netImportIds.length === 0) {
        return undefined;
      }

      return netImportIds[0];
    }
  );
}

export default createMovieCollectionListSelector;
