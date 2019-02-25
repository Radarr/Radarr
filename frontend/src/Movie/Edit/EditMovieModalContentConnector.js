import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import { setMovieValue, saveMovie } from 'Store/Actions/movieActions';
import EditMovieModalContent from './EditMovieModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.movies.pendingChanges,
    createMovieSelector(),
    (pendingChanges, movie) => {
      const path = pendingChanges.path;

      if (path == null) {
        return false;
      }

      return movie.path !== path;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies,
    createMovieSelector(),
    createIsPathChangingSelector(),
    (seriesState, movie, isPathChanging) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = seriesState;

      const seriesSettings = _.pick(movie, [
        'monitored',
        'qualityProfileId',
        'path',
        'tags'
      ]);

      const settings = selectSettings(seriesSettings, pendingChanges, saveError);

      return {
        title: movie.title,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: movie.path,
        item: settings.settings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetMovieValue: setMovieValue,
  dispatchSaveMovie: saveMovie
};

class EditMovieModalContentConnector extends Component {

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
    this.props.dispatchSetMovieValue({ name, value });
  }

  onSavePress = (moveFiles) => {
    this.props.dispatchSaveMovie({
      id: this.props.movieId,
      moveFiles
    });
  }

  //
  // Render

  render() {
    return (
      <EditMovieModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        onMoveSeriesPress={this.onMoveSeriesPress}
      />
    );
  }
}

EditMovieModalContentConnector.propTypes = {
  movieId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetMovieValue: PropTypes.func.isRequired,
  dispatchSaveMovie: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditMovieModalContentConnector);
