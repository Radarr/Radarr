import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createNetImportSelector from 'Store/Selectors/createNetImportSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    createNetImportSelector(),
    (netImportList) => {
      return {
        tagList: netImportList.map((netImport) => {
          const {
            id,
            name
          } = netImport;

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
