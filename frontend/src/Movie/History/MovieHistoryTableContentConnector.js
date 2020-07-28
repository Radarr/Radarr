import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearMovieHistory, fetchMovieHistory, movieHistoryMarkAsFailed } from 'Store/Actions/movieHistoryActions';
import MovieHistoryTableContent from './MovieHistoryTableContent';

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

class MovieHistoryTableContentConnector extends Component {

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

  componentDidUpdate(prevProps) {
    const {
      movieId
    } = this.props;

    // If the id has changed we need to clear the history
    if (prevProps.movieId !== movieId) {
      this.props.clearMovieHistory();
      this.props.fetchMovieHistory({
        movieId
      });
    }
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
  }

  //
  // Render

  render() {
    return (
      <MovieHistoryTableContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

MovieHistoryTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  fetchMovieHistory: PropTypes.func.isRequired,
  clearMovieHistory: PropTypes.func.isRequired,
  movieHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieHistoryTableContentConnector);
