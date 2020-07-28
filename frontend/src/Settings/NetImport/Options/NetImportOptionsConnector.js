import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchNetImportOptions, saveNetImportOptions, setNetImportOptionsValue } from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import NetImportOptions from './NetImportOptions';

const SECTION = 'netImportOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchNetImportOptions: fetchNetImportOptions,
  dispatchSetNetImportOptionsValue: setNetImportOptionsValue,
  dispatchSaveNetImportOptions: saveNetImportOptions,
  dispatchClearPendingChanges: clearPendingChanges
};

class NetImportOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchNetImportOptions,
      dispatchSaveNetImportOptions,
      onChildMounted
    } = this.props;

    dispatchFetchNetImportOptions();
    onChildMounted(dispatchSaveNetImportOptions);
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: SECTION });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetNetImportOptionsValue({ name, value });
  }

  //
  // Render

  render() {
    return (
      <NetImportOptions
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

NetImportOptionsConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchNetImportOptions: PropTypes.func.isRequired,
  dispatchSetNetImportOptionsValue: PropTypes.func.isRequired,
  dispatchSaveNetImportOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(NetImportOptionsConnector);
