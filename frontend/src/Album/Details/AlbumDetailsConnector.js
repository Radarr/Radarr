/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { toggleAlbumsMonitored } from 'Store/Actions/albumActions';
import { fetchTrackFiles, clearTrackFiles } from 'Store/Actions/trackFileActions';
import { clearReleases, cancelFetchReleases } from 'Store/Actions/releaseActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import AlbumDetails from './AlbumDetails';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';

const selectTrackFiles = createSelector(
  (state) => state.trackFiles,
  (trackFiles) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = trackFiles;

    const hasTrackFiles = !!items.length;

    return {
      isTrackFilesFetching: isFetching,
      isTrackFilesPopulated: isPopulated,
      trackFilesError: error,
      hasTrackFiles
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectTrackFiles,
    (state) => state.albums,
    createAllArtistSelector(),
    createCommandsSelector(),
    createUISettingsSelector(),
    (titleSlug, trackFiles, albums, artists, commands, uiSettings) => {
      const sortedAlbums = _.orderBy(albums.items, 'releaseDate');
      const albumIndex = _.findIndex(sortedAlbums, { titleSlug });
      const album = sortedAlbums[albumIndex];
      const artist = _.find(artists, { id: album.authorId });

      if (!album) {
        return {};
      }

      const {
        isTrackFilesFetching,
        isTrackFilesPopulated,
        trackFilesError,
        hasTrackFiles
      } = trackFiles;

      const previousAlbum = sortedAlbums[albumIndex - 1] || _.last(sortedAlbums);
      const nextAlbum = sortedAlbums[albumIndex + 1] || _.first(sortedAlbums);
      const isSearchingCommand = findCommand(commands, { name: commandNames.ALBUM_SEARCH });
      const isSearching = (
        isCommandExecuting(isSearchingCommand) &&
        isSearchingCommand.body.bookIds.indexOf(album.id) > -1
      );

      const isFetching = isTrackFilesFetching;
      const isPopulated = isTrackFilesPopulated;

      return {
        ...album,
        shortDateFormat: uiSettings.shortDateFormat,
        artist,
        isSearching,
        isFetching,
        isPopulated,
        trackFilesError,
        hasTrackFiles,
        previousAlbum,
        nextAlbum
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand,
  fetchTrackFiles,
  clearTrackFiles,
  clearReleases,
  cancelFetchReleases,
  toggleAlbumsMonitored
};

class AlbumDetailsConnector extends Component {

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    // If the id has changed we need to clear the albums
    // files and fetch from the server.

    if (prevProps.id !== this.props.id) {
      this.unpopulate();
      this.populate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.populate);
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const bookId = this.props.id;

    this.props.fetchTrackFiles({ bookId });
  }

  unpopulate = () => {
    this.props.cancelFetchReleases();
    this.props.clearReleases();
    this.props.clearTrackFiles();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleAlbumsMonitored({
      bookIds: [this.props.id],
      monitored
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.ALBUM_SEARCH,
      bookIds: [this.props.id]
    });
  }

  //
  // Render

  render() {
    return (
      <AlbumDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

AlbumDetailsConnector.propTypes = {
  id: PropTypes.number,
  anyReleaseOk: PropTypes.bool,
  isAlbumFetching: PropTypes.bool,
  isAlbumPopulated: PropTypes.bool,
  titleSlug: PropTypes.string.isRequired,
  fetchTrackFiles: PropTypes.func.isRequired,
  clearTrackFiles: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  cancelFetchReleases: PropTypes.func.isRequired,
  toggleAlbumsMonitored: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumDetailsConnector);
