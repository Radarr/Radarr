import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createImportListSelector from 'Store/Selectors/createImportListSelector';
import ImportListList from './ImportListList';

function createMapStateToProps() {
  return createSelector(
    createImportListSelector(),
    (importListList) => {
      return {
        importListList
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportListList);
