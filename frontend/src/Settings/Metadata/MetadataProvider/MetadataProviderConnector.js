import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { setMetadataProviderValue, saveMetadataProvider, fetchMetadataProvider } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import MetadataProvider from './MetadataProvider';

const SECTION = 'metadataProvider';

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
  dispatchFetchMetadataProvider: fetchMetadataProvider,
  dispatchSetMetadataProviderValue: setMetadataProviderValue,
  dispatchSaveMetadataProvider: saveMetadataProvider,
  dispatchClearPendingChanges: clearPendingChanges
};

class MetadataProviderConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchMetadataProvider,
      dispatchSaveMetadataProvider,
      onChildMounted
    } = this.props;

    dispatchFetchMetadataProvider();
    onChildMounted(dispatchSaveMetadataProvider);
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
    this.props.dispatchClearPendingChanges({ section: 'settings.metadataProvider' });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetMetadataProviderValue({ name, value });
  }

  //
  // Render

  render() {
    return (
      <MetadataProvider
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

MetadataProviderConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchMetadataProvider: PropTypes.func.isRequired,
  dispatchSetMetadataProviderValue: PropTypes.func.isRequired,
  dispatchSaveMetadataProvider: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataProviderConnector);
