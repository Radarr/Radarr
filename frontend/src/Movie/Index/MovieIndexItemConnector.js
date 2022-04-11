import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createMovieQualityProfileSelector from 'Store/Selectors/createMovieQualityProfileSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';

function selectShowSearchAction() {
  return createSelector(
    (state) => state.movieIndex,
    (movieIndex) => {
      const view = movieIndex.view;

      switch (view) {
        case 'posters':
          return movieIndex.posterOptions.showSearchAction;
        case 'overview':
          return movieIndex.overviewOptions.showSearchAction;
        default:
          return movieIndex.tableOptions.showSearchAction;
      }
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createMovieQualityProfileSelector(),
    selectShowSearchAction(),
    createExecutingCommandsSelector(),
    (state) => state.queue.details.items,
    (
      movie,
      qualityProfile,
      showSearchAction,
      executingCommands,
      queueItems
    ) => {

      // If a movie is deleted this selector may fire before the parent
      // selecors, which will result in an undefined movie, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a movie that has no information available.

      if (!movie) {
        return {};
      }

      const isRefreshingMovie = executingCommands.some((command) => {
        return (
          command.name === commandNames.REFRESH_MOVIE &&
          command.body.movieIds.includes(movie.id)
        );
      });

      const isSearchingMovie = executingCommands.some((command) => {
        return (
          command.name === commandNames.MOVIE_SEARCH &&
          command.body.movieIds.includes(movie.id)
        );
      });

      const firstQueueItem = queueItems.find((q) => q.movieId === movie.id);

      return {
        ...movie,
        qualityProfile,
        showSearchAction,
        isRefreshingMovie,
        isSearchingMovie,
        queueStatus: firstQueueItem ? firstQueueItem.status : null,
        queueState: firstQueueItem ? firstQueueItem.trackedDownloadState : null
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchExecuteCommand: executeCommand
};

class MovieIndexItemConnector extends Component {

  //
  // Listeners

  onRefreshMoviePress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.REFRESH_MOVIE,
      movieIds: [this.props.id]
    });
  };

  onSearchPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVIE_SEARCH,
      movieIds: [this.props.id]
    });
  };

  //
  // Render

  render() {
    const {
      id,
      component: ItemComponent,
      ...otherProps
    } = this.props;

    if (!id) {
      return null;
    }

    return (
      <ItemComponent
        {...otherProps}
        id={id}
        onRefreshMoviePress={this.onRefreshMoviePress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

MovieIndexItemConnector.propTypes = {
  id: PropTypes.number,
  component: PropTypes.elementType.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieIndexItemConnector);
