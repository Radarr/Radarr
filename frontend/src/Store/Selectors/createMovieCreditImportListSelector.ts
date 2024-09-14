import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import ImportList from 'typings/ImportList';

function createMovieCreditImportListSelector(tmdbId: number) {
  return createSelector(
    (state: AppState) => state.settings.importLists.items,
    (importLists) => {
      const importListIds = importLists.reduce(
        (acc: ImportList[], importList) => {
          if (importList.implementation === 'TMDbPersonImport') {
            const personIdValue = importList.fields.find(
              (field) => field.name === 'personId'
            )?.value as string | null;

            if (personIdValue && parseInt(personIdValue) === tmdbId) {
              acc.push(importList);

              return acc;
            }
          }

          return acc;
        },
        []
      );

      if (importListIds.length === 0) {
        return undefined;
      }

      return importListIds[0];
    }
  );
}

export default createMovieCreditImportListSelector;
