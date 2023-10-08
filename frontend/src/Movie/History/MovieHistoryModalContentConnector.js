import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearMovieHistory, fetchMovieHistory, movieHistoryMarkAsFailed } from 'Store/Actions/movieHistoryActions';
import MovieHistoryModalContent from './MovieHistoryModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieHistory,
    (movieHistory) => {
      return movieHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchMovieHistory,
  clearMovieHistory,
  movieHistoryMarkAsFailed
};

class MovieHistoryModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      movieId
    } = this.props;

    this.props.fetchMovieHistory({
      movieId
    });
  }

  componentWillUnmount() {
    this.props.clearMovieHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      movieId
    } = this.props;

    this.props.movieHistoryMarkAsFailed({
      historyId,
      movieId
    });
  };

  //
  // Render

  render() {
    return (
      <MovieHistoryModalContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

MovieHistoryModalContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  fetchMovieHistory: PropTypes.func.isRequired,
  clearMovieHistory: PropTypes.func.isRequired,
  movieHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieHistoryModalContentConnector);
