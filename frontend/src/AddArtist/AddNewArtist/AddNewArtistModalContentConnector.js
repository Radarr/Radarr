import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAddArtistDefault, addArtist } from 'Store/Actions/addArtistActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewArtistModalContent from './AddNewArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addArtist,
    (state) => state.settings.metadataProfiles,
    createDimensionsSelector(),
    (addArtistState, metadataProfiles, dimensions) => {
      const {
        isAdding,
        addError,
        defaults
      } = addArtistState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(defaults, {}, addError);

      return {
        isAdding,
        addError,
        showMetadataProfile: metadataProfiles.items.length > 1,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setAddArtistDefault,
  addArtist
};

class AddNewArtistModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddArtistDefault({ [name]: value });
  }

  onAddArtistPress = (searchForMissingAlbums) => {
    const {
      foreignArtistId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      metadataProfileId,
      albumFolder,
      tags
    } = this.props;

    this.props.addArtist({
      foreignArtistId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      metadataProfileId: metadataProfileId.value,
      albumFolder: albumFolder.value,
      tags: tags.value,
      searchForMissingAlbums
    });
  }

  //
  // Render

  render() {
    return (
      <AddNewArtistModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddArtistPress={this.onAddArtistPress}
      />
    );
  }
}

AddNewArtistModalContentConnector.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  metadataProfileId: PropTypes.object,
  albumFolder: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddArtistDefault: PropTypes.func.isRequired,
  addArtist: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewArtistModalContentConnector);
