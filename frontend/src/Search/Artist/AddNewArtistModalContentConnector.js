import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAddDefault, addArtist } from 'Store/Actions/searchActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewArtistModalContent from './AddNewArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.search,
    (state) => state.settings.metadataProfiles,
    createDimensionsSelector(),
    (searchState, metadataProfiles, dimensions) => {
      const {
        isAdding,
        addError,
        defaults
      } = searchState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(defaults, {}, addError);

      return {
        isAdding,
        addError,
        showMetadataProfile: metadataProfiles.items.length > 2, // NONE (not allowed for artists) and one other
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setAddDefault,
  addArtist
};

class AddNewArtistModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddDefault({ [name]: value });
  }

  onAddArtistPress = (searchForMissingAlbums) => {
    const {
      foreignAuthorId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      metadataProfileId,
      tags
    } = this.props;

    this.props.addArtist({
      foreignAuthorId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      metadataProfileId: metadataProfileId.value,
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
  foreignAuthorId: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  metadataProfileId: PropTypes.object,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddDefault: PropTypes.func.isRequired,
  addArtist: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewArtistModalContentConnector);
