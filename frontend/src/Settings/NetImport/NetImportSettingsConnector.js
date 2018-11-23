import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { testAllNetImport } from 'Store/Actions/settingsActions';
import NetImportSettings from './NetImportSettings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.netImports.isTestingAll,
    (isTestingAll) => {
      return {
        isTestingAll
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchTestAllNetImport: testAllNetImport
};

export default connect(createMapStateToProps, mapDispatchToProps)(NetImportSettings);
