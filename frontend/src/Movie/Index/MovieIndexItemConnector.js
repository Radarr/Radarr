import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';

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
    createQualityProfileSelector(),
    selectShowSearchAction(),
    createCommandsSelector(),
    (
      movie,
      qualityProfile,
      showSearchAction,
      commands
    ) => {
      const isRefreshingMovie = commands.some((command) => {
        return (
          command.name === commandNames.REFRESH_MOVIE &&
          command.body.movieId === movie.id &&
          isCommandExecuting(command)
        );
      });

      const isSearchingMovie = commands.some((command) => {
        return (
          command.name === commandNames.MOVIE_SEARCH &&
          command.body.movieId === movie.id &&
          isCommandExecuting(command)
        );
      });

      return {
        ...movie,
        qualityProfile,
        showSearchAction,
        isRefreshingMovie,
        isSearchingMovie
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class MovieIndexItemConnector extends Component {

  //
  // Listeners

  onRefreshMoviePress = () => {
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
    const {
      component: ItemComponent,
      ...otherProps
    } = this.props;

    return (
      <ItemComponent
        {...otherProps}
        onRefreshMoviePress={this.onRefreshMoviePress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

MovieIndexItemConnector.propTypes = {
  id: PropTypes.number.isRequired,
  component: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieIndexItemConnector);
