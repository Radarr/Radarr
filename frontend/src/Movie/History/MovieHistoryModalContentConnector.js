import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchMovieHistory, clearMovieHistory, seriesHistoryMarkAsFailed } from 'Store/Actions/movieHistoryActions';
import MovieHistoryModalContent from './MovieHistoryModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.moviesHistory,
    (seriesHistory) => {
      return seriesHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchMovieHistory,
  clearMovieHistory,
  seriesHistoryMarkAsFailed
};

class MovieHistoryModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.fetchMovieHistory({
      seriesId,
      seasonNumber
    });
  }

  componentWillUnmount() {
    this.props.clearMovieHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.seriesHistoryMarkAsFailed({
      historyId,
      seriesId,
      seasonNumber
    });
  }

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
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number,
  fetchMovieHistory: PropTypes.func.isRequired,
  clearMovieHistory: PropTypes.func.isRequired,
  seriesHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieHistoryModalContentConnector);
