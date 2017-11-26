import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchMetadataProfiles, deleteMetadataProfile } from 'Store/Actions/settingsActions';
import MetadataProfiles from './MetadataProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    (state) => state.settings.metadataProfiles,
    (advancedSettings, metadataProfiles) => {
      return {
        advancedSettings,
        ...metadataProfiles
      };
    }
  );
}

const mapDispatchToProps = {
  fetchMetadataProfiles,
  deleteMetadataProfile
};

class MetadataProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchMetadataProfiles();
  }

  //
  // Listeners

  onConfirmDeleteMetadataProfile = (id) => {
    this.props.deleteMetadataProfile({ id });
  }

  //
  // Render

  render() {
    return (
      <MetadataProfiles
        onConfirmDeleteMetadataProfile={this.onConfirmDeleteMetadataProfile}
        {...this.props}
      />
    );
  }
}

MetadataProfilesConnector.propTypes = {
  fetchMetadataProfiles: PropTypes.func.isRequired,
  deleteMetadataProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataProfilesConnector);
