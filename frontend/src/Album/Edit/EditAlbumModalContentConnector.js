import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createAlbumSelector from 'Store/Selectors/createAlbumSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { setAlbumValue, saveAlbum } from 'Store/Actions/albumActions';
import EditAlbumModalContent from './EditAlbumModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.albums,
    createAlbumSelector(),
    createArtistSelector(),
    (albumState, album, artist) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = albumState;

      const albumSettings = _.pick(album, [
        'monitored',
        'anyReleaseOk',
        'releases'
      ]);

      const settings = selectSettings(albumSettings, pendingChanges, saveError);

      return {
        title: album.title,
        artistName: artist.artistName,
        albumType: album.albumType,
        statistics: album.statistics,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetAlbumValue: setAlbumValue,
  dispatchSaveAlbum: saveAlbum
};

class EditAlbumModalContentConnector extends Component {

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
    this.props.dispatchSetAlbumValue({ name, value });
  }

  onSavePress = () => {
    this.props.dispatchSaveAlbum({
      id: this.props.albumId
    });
  }

  //
  // Render

  render() {
    return (
      <EditAlbumModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

EditAlbumModalContentConnector.propTypes = {
  albumId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetAlbumValue: PropTypes.func.isRequired,
  dispatchSaveAlbum: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditAlbumModalContentConnector);
