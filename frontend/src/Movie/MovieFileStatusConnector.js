import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import MovieFileStatus from './MovieFileStatus';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createUISettingsSelector(),
    (movie, uiSettings) => {
      return {
        inCinemas: movie.inCinemas,
        isAvailable: movie.isAvailable,
        monitored: movie.monitored,
        grabbed: movie.grabbed,
        movieFile: movie.movieFile,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

const mapDispatchToProps = {
};

class MovieFileStatusConnector extends Component {

  //
  // Render

  render() {
    return (
      <MovieFileStatus
        {...this.props}
      />
    );
  }
}

MovieFileStatusConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  queueStatus: PropTypes.string,
  queueState: PropTypes.string
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieFileStatusConnector);
