import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { fetchEpisodes, clearEpisodes } from 'Store/Actions/episodeActions';
import { fetchTrackFiles, clearTrackFiles } from 'Store/Actions/trackFileActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import ArtistDetails from './ArtistDetails';

function createMapStateToProps() {
  return createSelector(
    (state, { nameSlug }) => nameSlug,
    (state) => state.episodes,
    (state) => state.trackFiles,
    createAllArtistSelector(),
    createCommandsSelector(),
    (nameSlug, episodes, trackFiles, allArtists, commands) => {
      const sortedArtist = _.orderBy(allArtists, 'sortName');
      const artistIndex = _.findIndex(sortedArtist, { nameSlug });
      const series = sortedArtist[artistIndex];

      if (!series) {
        return {};
      }

      const previousArtist = sortedArtist[artistIndex - 1] || _.last(sortedArtist);
      const nextArtist = sortedArtist[artistIndex + 1] || _.first(sortedArtist);
      const isArtistRefreshing = !!findCommand(commands, { name: commandNames.REFRESH_ARTIST, artistId: series.id });
      const allArtistRefreshing = _.some(commands, (command) => command.name === commandNames.REFRESH_ARTIST && !command.body.artistId);
      const isRefreshing = isArtistRefreshing || allArtistRefreshing;
      const isSearching = !!findCommand(commands, { name: commandNames.ARTIST_SEARCH, artistId: series.id });
      const isRenamingFiles = !!findCommand(commands, { name: commandNames.RENAME_FILES, artistId: series.id });
      const isRenamingArtistCommand = findCommand(commands, { name: commandNames.RENAME_ARTIST });
      const isRenamingArtist = !!(isRenamingArtistCommand && isRenamingArtistCommand.body.artistId.indexOf(series.id) > -1);

      const isFetching = episodes.isFetching || trackFiles.isFetching;
      const isPopulated = episodes.isPopulated && trackFiles.isPopulated;
      const episodesError = episodes.error;
      const trackFilesError = trackFiles.error;
      const alternateTitles = _.reduce(series.alternateTitles, (acc, alternateTitle) => {
        if ((alternateTitle.seasonNumber === -1 || alternateTitle.seasonNumber === undefined) &&
            (alternateTitle.sceneSeasonNumber === -1 || alternateTitle.sceneSeasonNumber === undefined)) {
          acc.push(alternateTitle.title);
        }

        return acc;
      }, []);

      return {
        ...series,
        alternateTitles,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingArtist,
        isFetching,
        isPopulated,
        episodesError,
        trackFilesError,
        previousArtist,
        nextArtist
      };
    }
  );
}

const mapDispatchToProps = {
  fetchEpisodes,
  clearEpisodes,
  fetchTrackFiles,
  clearTrackFiles,
  fetchQueueDetails,
  clearQueueDetails,
  executeCommand
};

class ArtistDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this._populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isRefreshing,
      isRenamingFiles,
      isRenamingArtist
    } = this.props;

    if (
      (prevProps.isRefreshing && !isRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingArtist && !isRenamingArtist)
    ) {
      this._populate();
    }

    // If the id has changed we need to clear the episodes/episode
    // files and fetch from the server.

    if (prevProps.id !== id) {
      this._unpopulate();
      this._populate();
    }
  }

  componentWillUnmount() {
    this._unpopulate();
  }

  //
  // Control

  _populate() {
    const artistId = this.props.id;

    this.props.fetchEpisodes({ artistId });
    this.props.fetchTrackFiles({ artistId });
    this.props.fetchQueueDetails({ artistId });
  }

  _unpopulate() {
    this.props.clearEpisodes();
    this.props.clearTrackFiles();
    this.props.clearQueueDetails();
  }

  //
  // Listeners

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
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

ArtistDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  nameSlug: PropTypes.string.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingArtist: PropTypes.bool.isRequired,
  fetchEpisodes: PropTypes.func.isRequired,
  clearEpisodes: PropTypes.func.isRequired,
  fetchTrackFiles: PropTypes.func.isRequired,
  clearTrackFiles: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsConnector);
