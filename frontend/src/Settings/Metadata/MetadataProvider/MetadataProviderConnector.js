import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { setMetadataProviderValue, saveMetadataProvider, fetchMetadataProvider } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import connectSection from 'Store/connectSection';
import MetadataProvider from './MetadataProvider';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  setMetadataProviderValue,
  saveMetadataProvider,
  fetchMetadataProvider,
  clearPendingChanges
};

class MetadataProviderConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchMetadataProvider();
  }

  componentDidUpdate(prevProps) {
    if (this.props.hasPendingChanges !== prevProps.hasPendingChanges) {
      this.props.onHasPendingChange(this.props.hasPendingChanges);
    }
  }

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: this.props.section });
  }

  //
  // Control

  save = () => {
    this.props.saveMetadataProvider();
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setMetadataProviderValue({ name, value });
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
  section: PropTypes.string.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  setMetadataProviderValue: PropTypes.func.isRequired,
  saveMetadataProvider: PropTypes.func.isRequired,
  fetchMetadataProvider: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired,
  onHasPendingChange: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  { withRef: true },
  { section: 'settings.metadataProvider' }
)(MetadataProviderConnector);
