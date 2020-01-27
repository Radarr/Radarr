import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchMovieFiles, clearMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchMovieCredits, clearMovieCredits } from 'Store/Actions/movieCreditsActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { clearReleases, cancelFetchReleases } from 'Store/Actions/releaseActions';
import { fetchNetImportSchema } from 'Store/Actions/settingsActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
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

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectMovieFiles,
    selectMovieCredits,
    createAllMoviesSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (titleSlug, movieFiles, movieCredits, allMovies, commands, dimensions) => {
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

      const previousMovie = sortedMovies[movieIndex - 1] || _.last(sortedMovies);
      const nextMovie = sortedMovies[movieIndex + 1] || _.first(sortedMovies);
      const isMovieRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_MOVIE, movieId: movie.id }));
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

      const isFetching = isMovieFilesFetching && isMovieCreditsFetching;
      const isPopulated = isMovieFilesPopulated && isMovieCreditsPopulated;
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
        hasMovieFiles,
        sizeOnDisk,
        previousMovie,
        nextMovie,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  fetchMovieFiles,
  clearMovieFiles,
  fetchMovieCredits,
  clearMovieCredits,
  clearReleases,
  cancelFetchReleases,
  fetchNetImportSchema,
  toggleMovieMonitored,
  fetchQueueDetails,
  clearQueueDetails,
  executeCommand
};

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

    this.props.fetchMovieFiles({ movieId });
    this.props.fetchMovieCredits({ movieId });
    this.props.fetchQueueDetails({ movieId });
    this.props.fetchNetImportSchema();
  }

  unpopulate = () => {
    this.props.cancelFetchReleases();
    this.props.clearMovieFiles();
    this.props.clearMovieCredits();
    this.props.clearQueueDetails();
    this.props.clearReleases();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleMovieMonitored({
      movieId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_MOVIE,
      movieId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
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
  fetchMovieFiles: PropTypes.func.isRequired,
  clearMovieFiles: PropTypes.func.isRequired,
  fetchMovieCredits: PropTypes.func.isRequired,
  clearMovieCredits: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  cancelFetchReleases: PropTypes.func.isRequired,
  toggleMovieMonitored: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  fetchNetImportSchema: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieDetailsConnector);
