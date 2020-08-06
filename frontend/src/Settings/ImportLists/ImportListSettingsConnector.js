import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { testAllImportList } from 'Store/Actions/settingsActions';
import ImportListSettings from './ImportListSettings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importLists.isTestingAll,
    (isTestingAll) => {
      return {
        isTestingAll
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchTestAllImportList: testAllImportList
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportListSettings);
