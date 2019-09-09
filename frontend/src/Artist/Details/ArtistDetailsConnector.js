/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import { fetchTrackFiles, clearTrackFiles } from 'Store/Actions/trackFileActions';
import { toggleArtistMonitored } from 'Store/Actions/artistActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import ArtistDetails from './ArtistDetails';

const selectAlbums = createSelector(
  (state) => state.albums,
  (albums) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = albums;

    const hasAlbums = !!items.length;
    const hasMonitoredAlbums = items.some((e) => e.monitored);

    return {
      isAlbumsFetching: isFetching,
      isAlbumsPopulated: isPopulated,
      albumsError: error,
      hasAlbums,
      hasMonitoredAlbums
    };
  }
);

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
    (state, { foreignArtistId }) => foreignArtistId,
    selectAlbums,
    selectTrackFiles,
    (state) => state.settings.metadataProfiles,
    createAllArtistSelector(),
    createCommandsSelector(),
    (foreignArtistId, albums, trackFiles, metadataProfiles, allArtists, commands) => {
      const sortedArtist = _.orderBy(allArtists, 'sortName');
      const artistIndex = _.findIndex(sortedArtist, { foreignArtistId });
      const artist = sortedArtist[artistIndex];
      const metadataProfile = _.find(metadataProfiles.items, { id: artist.metadataProfileId });
      const albumTypes = _.reduce(metadataProfile.primaryAlbumTypes, (acc, primaryType) => {
        if (primaryType.allowed) {
          acc.push(primaryType.albumType.name);
        }
        return acc;
      }, []);

      if (!artist) {
        return {};
      }

      const {
        isAlbumsFetching,
        isAlbumsPopulated,
        albumsError,
        hasAlbums,
        hasMonitoredAlbums
      } = albums;

      const {
        isTrackFilesFetching,
        isTrackFilesPopulated,
        trackFilesError,
        hasTrackFiles
      } = trackFiles;

      const sortedAlbumTypes = _.orderBy(albumTypes);

      const previousArtist = sortedArtist[artistIndex - 1] || _.last(sortedArtist);
      const nextArtist = sortedArtist[artistIndex + 1] || _.first(sortedArtist);
      const isArtistRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_ARTIST, artistId: artist.id }));
      const artistRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_ARTIST });
      const allArtistRefreshing = (
        isCommandExecuting(artistRefreshingCommand) &&
        !artistRefreshingCommand.body.artistId
      );
      const isRefreshing = isArtistRefreshing || allArtistRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.ARTIST_SEARCH, artistId: artist.id }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, artistId: artist.id }));

      const isRenamingArtistCommand = findCommand(commands, { name: commandNames.RENAME_ARTIST });
      const isRenamingArtist = (
        isCommandExecuting(isRenamingArtistCommand) &&
        isRenamingArtistCommand.body.artistIds.indexOf(artist.id) > -1
      );

      const isFetching = isAlbumsFetching || isTrackFilesFetching;
      const isPopulated = isAlbumsPopulated && isTrackFilesPopulated;

      const alternateTitles = _.reduce(artist.alternateTitles, (acc, alternateTitle) => {
        if ((alternateTitle.seasonNumber === -1 || alternateTitle.seasonNumber === undefined) &&
            (alternateTitle.sceneSeasonNumber === -1 || alternateTitle.sceneSeasonNumber === undefined)) {
          acc.push(alternateTitle.title);
        }

        return acc;
      }, []);

      return {
        ...artist,
        albumTypes: sortedAlbumTypes,
        alternateTitles,
        isArtistRefreshing,
        allArtistRefreshing,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingArtist,
        isFetching,
        isPopulated,
        albumsError,
        trackFilesError,
        hasAlbums,
        hasMonitoredAlbums,
        hasTrackFiles,
        previousArtist,
        nextArtist
      };
    }
  );
}

const mapDispatchToProps = {
  fetchAlbums,
  clearAlbums,
  fetchTrackFiles,
  clearTrackFiles,
  toggleArtistMonitored,
  fetchQueueDetails,
  clearQueueDetails,
  executeCommand
};

class ArtistDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isArtistRefreshing,
      allArtistRefreshing,
      isRenamingFiles,
      isRenamingArtist
    } = this.props;

    if (
      (prevProps.isArtistRefreshing && !isArtistRefreshing) ||
      (prevProps.allArtistRefreshing && !allArtistRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingArtist && !isRenamingArtist)
    ) {
      this.populate();
    }

    // If the id has changed we need to clear the albums
    // files and fetch from the server.

    if (prevProps.id !== id) {
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
    const artistId = this.props.id;

    this.props.fetchAlbums({ artistId });
    this.props.fetchTrackFiles({ artistId });
    this.props.fetchQueueDetails({ artistId });
  }

  unpopulate = () => {
    this.props.clearAlbums();
    this.props.clearTrackFiles();
    this.props.clearQueueDetails();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleArtistMonitored({
      artistId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_ARTIST,
      artistId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.ARTIST_SEARCH,
      artistId: this.props.id
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

ArtistDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  isArtistRefreshing: PropTypes.bool.isRequired,
  allArtistRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingArtist: PropTypes.bool.isRequired,
  fetchAlbums: PropTypes.func.isRequired,
  clearAlbums: PropTypes.func.isRequired,
  fetchTrackFiles: PropTypes.func.isRequired,
  clearTrackFiles: PropTypes.func.isRequired,
  toggleArtistMonitored: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsConnector);
