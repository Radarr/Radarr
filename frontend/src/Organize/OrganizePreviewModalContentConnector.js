import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { fetchOrganizePreview } from 'Store/Actions/organizePreviewActions';
import { fetchNamingSettings } from 'Store/Actions/settingsActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import OrganizePreviewModalContent from './OrganizePreviewModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.organizePreview,
    (state) => state.settings.naming,
    createArtistSelector(),
    (organizePreview, naming, artist) => {
      const props = { ...organizePreview };
      props.isFetching = organizePreview.isFetching || naming.isFetching;
      props.isPopulated = organizePreview.isPopulated && naming.isPopulated;
      props.error = organizePreview.error || naming.error;
      props.trackFormat = naming.item.standardTrackFormat;
      props.path = artist.path;

      return props;
    }
  );
}

const mapDispatchToProps = {
  fetchOrganizePreview,
  fetchNamingSettings,
  executeCommand
};

class OrganizePreviewModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId,
      albumId
    } = this.props;

    this.props.fetchOrganizePreview({
      artistId,
      albumId
    });

    this.props.fetchNamingSettings();
  }

  //
  // Listeners

  onOrganizePress = (files) => {
    this.props.executeCommand({
      name: commandNames.RENAME_FILES,
      artistId: this.props.artistId,
      files
    });

    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <OrganizePreviewModalContent
        {...this.props}
        onOrganizePress={this.onOrganizePress}
      />
    );
  }
}

OrganizePreviewModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number,
  fetchOrganizePreview: PropTypes.func.isRequired,
  fetchNamingSettings: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OrganizePreviewModalContentConnector);
