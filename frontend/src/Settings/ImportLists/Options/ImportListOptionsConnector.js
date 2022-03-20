import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchImportListOptions, saveImportListOptions, setImportListOptionsValue } from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import ImportListOptions from './ImportListOptions';

const SECTION = 'importListOptions';

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
  dispatchFetchImportListOptions: fetchImportListOptions,
  dispatchSetImportListOptionsValue: setImportListOptionsValue,
  dispatchSaveImportListOptions: saveImportListOptions,
  dispatchClearPendingChanges: clearPendingChanges
};

class ImportListOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchImportListOptions,
      dispatchSaveImportListOptions,
      onChildMounted
    } = this.props;

    dispatchFetchImportListOptions();
    onChildMounted(dispatchSaveImportListOptions);
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
    this.props.dispatchSetImportListOptionsValue({ name, value });
  };

  //
  // Render

  render() {
    return (
      <ImportListOptions
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

ImportListOptionsConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchImportListOptions: PropTypes.func.isRequired,
  dispatchSetImportListOptionsValue: PropTypes.func.isRequired,
  dispatchSaveImportListOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportListOptionsConnector);
