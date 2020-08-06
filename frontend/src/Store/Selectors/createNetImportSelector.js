import { createSelector } from 'reselect';

function createNetImportSelector() {
  return createSelector(
    (state) => state.settings.netImports.items,
    (lists) => {
      return lists;
    }
  );
}

export default createNetImportSelector;
