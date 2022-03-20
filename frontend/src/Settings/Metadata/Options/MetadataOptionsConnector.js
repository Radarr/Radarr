import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchMetadataOptions, saveMetadataOptions, setMetadataOptionsValue } from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import MetadataOptions from './MetadataOptions';

const SECTION = 'metadataOptions';

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
  dispatchFetchMetadataOptions: fetchMetadataOptions,
  dispatchSetMetadataOptionsValue: setMetadataOptionsValue,
  dispatchSaveMetadataOptions: saveMetadataOptions,
  dispatchClearPendingChanges: clearPendingChanges
};

class MetadataOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchMetadataOptions,
      dispatchSaveMetadataOptions,
      onChildMounted
    } = this.props;

    dispatchFetchMetadataOptions();
    onChildMounted(dispatchSaveMetadataOptions);
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
    this.props.dispatchSetMetadataOptionsValue({ name, value });
  };

  //
  // Render

  render() {
    return (
      <MetadataOptions
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

MetadataOptionsConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchMetadataOptions: PropTypes.func.isRequired,
  dispatchSetMetadataOptionsValue: PropTypes.func.isRequired,
  dispatchSaveMetadataOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataOptionsConnector);
