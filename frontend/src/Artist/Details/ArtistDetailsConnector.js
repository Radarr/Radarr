/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import { fetchSeries, clearSeries } from 'Store/Actions/seriesActions';
import { fetchTrackFiles, clearTrackFiles } from 'Store/Actions/trackFileActions';
import { toggleArtistMonitored } from 'Store/Actions/artistActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { clearReleases, cancelFetchReleases } from 'Store/Actions/releaseActions';
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

const selectSeries = createSelector(
  createSortedSectionSelector('series', (a, b) => a.title.localeCompare(b.title)),
  (state) => state.series,
  (series) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = series;

    const hasSeries = !!items.length;

    return {
      isSeriesFetching: isFetching,
      isSeriesPopulated: isPopulated,
      seriesError: error,
      hasSeries,
      series: series.items
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
    (state, { titleSlug }) => titleSlug,
    selectAlbums,
    selectSeries,
    selectTrackFiles,
    createAllArtistSelector(),
    createCommandsSelector(),
    (titleSlug, albums, series, trackFiles, allArtists, commands) => {
      const sortedArtist = _.orderBy(allArtists, 'sortName');
      const artistIndex = _.findIndex(sortedArtist, { titleSlug });
      const artist = sortedArtist[artistIndex];

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
        isSeriesFetching,
        isSeriesPopulated,
        seriesError,
        hasSeries,
        series: seriesItems
      } = series;

      const {
        isTrackFilesFetching,
        isTrackFilesPopulated,
        trackFilesError,
        hasTrackFiles
      } = trackFiles;

      const previousArtist = sortedArtist[artistIndex - 1] || _.last(sortedArtist);
      const nextArtist = sortedArtist[artistIndex + 1] || _.first(sortedArtist);
      const isArtistRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_ARTIST, authorId: artist.id }));
      const artistRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_ARTIST });
      const allArtistRefreshing = (
        isCommandExecuting(artistRefreshingCommand) &&
        !artistRefreshingCommand.body.authorId
      );
      const isRefreshing = isArtistRefreshing || allArtistRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.ARTIST_SEARCH, authorId: artist.id }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, authorId: artist.id }));

      const isRenamingArtistCommand = findCommand(commands, { name: commandNames.RENAME_ARTIST });
      const isRenamingArtist = (
        isCommandExecuting(isRenamingArtistCommand) &&
        isRenamingArtistCommand.body.authorIds.indexOf(artist.id) > -1
      );

      const isFetching = isAlbumsFetching || isSeriesFetching || isTrackFilesFetching;
      const isPopulated = isAlbumsPopulated && isSeriesPopulated && isTrackFilesPopulated;

      const alternateTitles = _.reduce(artist.alternateTitles, (acc, alternateTitle) => {
        if ((alternateTitle.seasonNumber === -1 || alternateTitle.seasonNumber === undefined) &&
            (alternateTitle.sceneSeasonNumber === -1 || alternateTitle.sceneSeasonNumber === undefined)) {
          acc.push(alternateTitle.title);
        }

        return acc;
      }, []);

      return {
        ...artist,
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
        seriesError,
        trackFilesError,
        hasAlbums,
        hasMonitoredAlbums,
        hasSeries,
        series: seriesItems,
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
  fetchSeries,
  clearSeries,
  fetchTrackFiles,
  clearTrackFiles,
  toggleArtistMonitored,
  fetchQueueDetails,
  clearQueueDetails,
  clearReleases,
  cancelFetchReleases,
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
    const authorId = this.props.id;

    this.props.fetchAlbums({ authorId });
    this.props.fetchSeries({ authorId });
    this.props.fetchTrackFiles({ authorId });
    this.props.fetchQueueDetails({ authorId });
  }

  unpopulate = () => {
    this.props.cancelFetchReleases();
    this.props.clearAlbums();
    this.props.clearSeries();
    this.props.clearTrackFiles();
    this.props.clearQueueDetails();
    this.props.clearReleases();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleArtistMonitored({
      authorId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_ARTIST,
      authorId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.ARTIST_SEARCH,
      authorId: this.props.id
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
  titleSlug: PropTypes.string.isRequired,
  isArtistRefreshing: PropTypes.bool.isRequired,
  allArtistRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingArtist: PropTypes.bool.isRequired,
  fetchAlbums: PropTypes.func.isRequired,
  clearAlbums: PropTypes.func.isRequired,
  fetchSeries: PropTypes.func.isRequired,
  clearSeries: PropTypes.func.isRequired,
  fetchTrackFiles: PropTypes.func.isRequired,
  clearTrackFiles: PropTypes.func.isRequired,
  toggleArtistMonitored: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  cancelFetchReleases: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsConnector);
