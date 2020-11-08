import { createSelector } from 'reselect';

function createImportListSelector() {
  return createSelector(
    (state) => state.settings.importLists.items,
    (lists) => {
      return lists;
    }
  );
}

export default createImportListSelector;
