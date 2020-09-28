import _ from 'lodash';
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
      const result = _.pick(movie, [
        'inCinemas',
        'isAvailable',
        'monitored',
        'grabbed'
      ]);

      result.movieFile = movie.movieFile;

      return result;
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
