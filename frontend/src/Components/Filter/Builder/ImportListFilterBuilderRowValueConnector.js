import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createImportListSelector from 'Store/Selectors/createImportListSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    createImportListSelector(),
    (importLists) => {
      return {
        tagList: importLists.map((importList) => {
          const {
            id,
            name
          } = importList;

          return {
            id,
            name
          };
        })
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
