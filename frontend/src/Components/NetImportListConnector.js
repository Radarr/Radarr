import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createNetImportSelector from 'Store/Selectors/createNetImportSelector';
import NetImportList from './NetImportList';

function createMapStateToProps() {
  return createSelector(
    createNetImportSelector(),
    (netImportList) => {
      return {
        netImportList
      };
    }
  );
}

export default connect(createMapStateToProps)(NetImportList);
