import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieStatus from 'Movie/MovieStatus';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import { createMovieByEntitySelector } from 'Store/Selectors/createMovieSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';

function createMapStateToProps() {
  return createSelector(
    createMovieByEntitySelector(),
    createQueueItemSelector(),
    createMovieFileSelector(),
    (movie, queueItem, movieFile) => {
      const result = _.pick(movie, [
        'isAvailable',
        'monitored',
        'grabbed'
      ]);

      result.queueItem = queueItem;
      result.movieFile = movieFile;

      return result;
    }
  );
}

class MovieStatusConnector extends Component {

  //
  // Render

  render() {
    return (
      <MovieStatus
        {...this.props}
      />
    );
  }
}

MovieStatusConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  movieFileId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps, null)(MovieStatusConnector);
