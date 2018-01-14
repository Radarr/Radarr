import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchMetadataProfiles, deleteMetadataProfile, cloneMetadataProfile } from 'Store/Actions/settingsActions';
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
  dispatchFetchMetadataProfiles: fetchMetadataProfiles,
  dispatchDeleteMetadataProfile: deleteMetadataProfile,
  dispatchCloneMetadataProfile: cloneMetadataProfile
};

class MetadataProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchMetadataProfiles();
  }

  //
  // Listeners

  onConfirmDeleteMetadataProfile = (id) => {
    this.props.dispatchDeleteMetadataProfile({ id });
  }

  onCloneMetadataProfilePress = (id) => {
    this.props.dispatchCloneMetadataProfile({ id });
  }

  //
  // Render

  render() {
    return (
      <MetadataProfiles
        onConfirmDeleteMetadataProfile={this.onConfirmDeleteMetadataProfile}
        onCloneMetadataProfilePress={this.onCloneMetadataProfilePress}
        {...this.props}
      />
    );
  }
}

MetadataProfilesConnector.propTypes = {
  dispatchFetchMetadataProfiles: PropTypes.func.isRequired,
  dispatchDeleteMetadataProfile: PropTypes.func.isRequired,
  dispatchCloneMetadataProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataProfilesConnector);
