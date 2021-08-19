import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearExtraFiles, fetchExtraFiles } from 'Store/Actions/extraFileActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { clearMovieBlocklist, fetchMovieBlocklist } from 'Store/Actions/movieBlocklistActions';
import { clearMovieCredits, fetchMovieCredits } from 'Store/Actions/movieCreditsActions';
import { clearMovieFiles, fetchMovieFiles } from 'Store/Actions/movieFileActions';
import { clearMovieHistory, fetchMovieHistory } from 'Store/Actions/movieHistoryActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { fetchImportListSchema } from 'Store/Actions/settingsActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import MovieDetails from './MovieDetails';

const selectMovieFiles = createSelector(
  (state) => state.movieFiles,
  (movieFiles) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = movieFiles;

    const hasMovieFiles = !!items.length;

    const sizeOnDisk = items.map((item) => item.size).reduce((prev, curr) => prev + curr, 0);

    return {
      isMovieFilesFetching: isFetching,
      isMovieFilesPopulated: isPopulated,
      movieFilesError: error,
      hasMovieFiles,
      sizeOnDisk
    };
  }
);

const selectMovieCredits = createSelector(
  (state) => state.movieCredits,
  (movieCredits) => {
    const {
      isFetching,
      isPopulated,
      error
    } = movieCredits;

    return {
      isMovieCreditsFetching: isFetching,
      isMovieCreditsPopulated: isPopulated,
      movieCreditsError: error
    };
  }
);

const selectExtraFiles = createSelector(
  (state) => state.extraFiles,
  (extraFiles) => {
    const {
      isFetching,
      isPopulated,
      error
    } = extraFiles;

    return {
      isExtraFilesFetching: isFetching,
      isExtraFilesPopulated: isPopulated,
      extraFilesError: error
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectMovieFiles,
    selectMovieCredits,
    selectExtraFiles,
    createAllMoviesSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state) => state.app.isSidebarVisible,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (titleSlug, movieFiles, movieCredits, extraFiles, allMovies, commands, dimensions, queueItems, isSidebarVisible, movieRuntimeFormat) => {
      const sortedMovies = _.orderBy(allMovies, 'sortTitle');
      const movieIndex = _.findIndex(sortedMovies, { titleSlug });
      const movie = sortedMovies[movieIndex];

      if (!movie) {
        return {};
      }

      const {
        isMovieFilesFetching,
        isMovieFilesPopulated,
        movieFilesError,
        hasMovieFiles,
        sizeOnDisk
      } = movieFiles;

      const {
        isMovieCreditsFetching,
        isMovieCreditsPopulated,
        movieCreditsError
      } = movieCredits;

      const {
        isExtraFilesFetching,
        isExtraFilesPopulated,
        extraFilesError
      } = extraFiles;

      const previousMovie = sortedMovies[movieIndex - 1] || _.last(sortedMovies);
      const nextMovie = sortedMovies[movieIndex + 1] || _.first(sortedMovies);
      const isMovieRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_MOVIE, movieIds: [movie.id] }));
      const movieRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_MOVIE });
      const allMoviesRefreshing = (
        isCommandExecuting(movieRefreshingCommand) &&
        !movieRefreshingCommand.body.movieId
      );
      const isRefreshing = isMovieRefreshing || allMoviesRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.MOVIE_SEARCH, movieIds: [movie.id] }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, movieId: movie.id }));
      const isRenamingMovieCommand = findCommand(commands, { name: commandNames.RENAME_MOVIE });
      const isRenamingMovie = (
        isCommandExecuting(isRenamingMovieCommand) &&
        isRenamingMovieCommand.body.movieIds.indexOf(movie.id) > -1
      );

      const isFetching = isMovieFilesFetching || isMovieCreditsFetching || isExtraFilesFetching;
      const isPopulated = isMovieFilesPopulated && isMovieCreditsPopulated && isExtraFilesPopulated;
      const alternateTitles = _.reduce(movie.alternateTitles, (acc, alternateTitle) => {
        acc.push(alternateTitle.title);
        return acc;
      }, []);

      return {
        ...movie,
        alternateTitles,
        isMovieRefreshing,
        allMoviesRefreshing,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingMovie,
        isFetching,
        isPopulated,
        movieFilesError,
        movieCreditsError,
        extraFilesError,
        hasMovieFiles,
        sizeOnDisk,
        previousMovie,
        nextMovie,
        isSmallScreen: dimensions.isSmallScreen,
        isSidebarVisible,
        queueItems,
        movieRuntimeFormat
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchMovieFiles({ movieId }) {
      dispatch(fetchMovieFiles({ movieId }));
    },
    dispatchClearMovieFiles() {
      dispatch(clearMovieFiles());
    },
    dispatchFetchMovieHistory({ movieId }) {
      dispatch(fetchMovieHistory({ movieId }));
    },
    dispatchClearMovieHistory() {
      dispatch(clearMovieHistory());
    },
    dispatchFetchMovieCredits({ movieId }) {
      dispatch(fetchMovieCredits({ movieId }));
    },
    dispatchClearMovieCredits() {
      dispatch(clearMovieCredits());
    },
    dispatchFetchExtraFiles({ movieId }) {
      dispatch(fetchExtraFiles({ movieId }));
    },
    dispatchClearExtraFiles() {
      dispatch(clearExtraFiles());
    },
    dispatchClearReleases() {
      dispatch(clearReleases());
    },
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },
    dispatchFetchQueueDetails({ movieId }) {
      dispatch(fetchQueueDetails({ movieId }));
    },
    dispatchClearQueueDetails() {
      dispatch(clearQueueDetails());
    },
    dispatchFetchImportListSchema() {
      dispatch(fetchImportListSchema());
    },
    dispatchToggleMovieMonitored(payload) {
      dispatch(toggleMovieMonitored(payload));
    },
    dispatchExecuteCommand(payload) {
      dispatch(executeCommand(payload));
    },
    onGoToMovie(titleSlug) {
      dispatch(push(`${window.Radarr.urlBase}/movie/${titleSlug}`));
    },
    dispatchFetchMovieBlocklist({ movieId }) {
      dispatch(fetchMovieBlocklist({ movieId }));
    },
    dispatchClearMovieBlocklist() {
      dispatch(clearMovieBlocklist());
    }
  };
}

class MovieDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isMovieRefreshing,
      allMoviesRefreshing,
      isRenamingFiles,
      isRenamingMovie
    } = this.props;

    if (
      (prevProps.isMovieRefreshing && !isMovieRefreshing) ||
      (prevProps.allMoviesRefreshing && !allMoviesRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingMovie && !isRenamingMovie)
    ) {
      this.populate();
    }

    // If the id has changed we need to clear the episodes/episode
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
    const movieId = this.props.id;

    this.props.dispatchFetchMovieFiles({ movieId });
    this.props.dispatchFetchMovieBlocklist({ movieId });
    this.props.dispatchFetchMovieHistory({ movieId });
    this.props.dispatchFetchExtraFiles({ movieId });
    this.props.dispatchFetchMovieCredits({ movieId });
    this.props.dispatchFetchQueueDetails({ movieId });
    this.props.dispatchFetchImportListSchema();
  }

  unpopulate = () => {
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearMovieBlocklist();
    this.props.dispatchClearMovieFiles();
    this.props.dispatchClearMovieHistory();
    this.props.dispatchClearExtraFiles();
    this.props.dispatchClearMovieCredits();
    this.props.dispatchClearQueueDetails();
    this.props.dispatchClearReleases();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.dispatchToggleMovieMonitored({
      movieId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.REFRESH_MOVIE,
      movieIds: [this.props.id]
    });
  }

  onSearchPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVIE_SEARCH,
      movieIds: [this.props.id]
    });
  }

  //
  // Render

  render() {
    return (
      <MovieDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

MovieDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isMovieRefreshing: PropTypes.bool.isRequired,
  allMoviesRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingMovie: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  dispatchFetchMovieFiles: PropTypes.func.isRequired,
  dispatchClearMovieFiles: PropTypes.func.isRequired,
  dispatchFetchMovieHistory: PropTypes.func.isRequired,
  dispatchClearMovieHistory: PropTypes.func.isRequired,
  dispatchFetchExtraFiles: PropTypes.func.isRequired,
  dispatchClearExtraFiles: PropTypes.func.isRequired,
  dispatchFetchMovieCredits: PropTypes.func.isRequired,
  dispatchClearMovieCredits: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchToggleMovieMonitored: PropTypes.func.isRequired,
  dispatchFetchQueueDetails: PropTypes.func.isRequired,
  dispatchClearQueueDetails: PropTypes.func.isRequired,
  dispatchFetchImportListSchema: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired,
  dispatchFetchMovieBlocklist: PropTypes.func.isRequired,
  dispatchClearMovieBlocklist: PropTypes.func.isRequired,
  onGoToMovie: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieDetailsConnector);
