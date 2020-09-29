import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieFileStatus from './MovieFileStatus';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      return {
        inCinemas: movie.inCinemas,
        isAvailable: movie.isAvailable,
        monitored: movie.monitored,
        grabbed: movie.grabbed,
        movieFile: movie.movieFile
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
