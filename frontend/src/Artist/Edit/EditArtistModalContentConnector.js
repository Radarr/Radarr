import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { setArtistValue, saveArtist } from 'Store/Actions/artistActions';
import EditArtistModalContent from './EditArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist,
    (state) => state.settings.languageProfiles,
    (state) => state.settings.metadataProfiles,
    createArtistSelector(),
    (artistState, languageProfiles, metadataProfiles, artist) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = artistState;

      const artistSettings = _.pick(artist, [
        'monitored',
        'albumFolder',
        'qualityProfileId',
        'languageProfileId',
        'metadataProfileId',
        'path',
        'tags'
      ]);

      const settings = selectSettings(artistSettings, pendingChanges, saveError);

      return {
        artistName: artist.artistName,
        isSaving,
        saveError,
        pendingChanges,
        item: settings.settings,
        showLanguageProfile: languageProfiles.items.length > 1,
        showMetadataProfile: metadataProfiles.items.length > 1,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setArtistValue,
  saveArtist
};

class EditArtistModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setArtistValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveArtist({ id: this.props.artistId });
  }

  //
  // Render

  render() {
    return (
      <EditArtistModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

EditArtistModalContentConnector.propTypes = {
  artistId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  setArtistValue: PropTypes.func.isRequired,
  saveArtist: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditArtistModalContentConnector);
