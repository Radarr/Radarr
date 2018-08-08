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
    this.props.clearPendingChanges({ section: SECTION });
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
  hasPendingChanges: PropTypes.bool.isRequired,
  setMetadataProviderValue: PropTypes.func.isRequired,
  saveMetadataProvider: PropTypes.func.isRequired,
  fetchMetadataProvider: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired,
  onHasPendingChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataProviderConnector);
