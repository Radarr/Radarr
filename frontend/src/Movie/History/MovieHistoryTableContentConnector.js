import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { movieHistoryMarkAsFailed } from 'Store/Actions/movieHistoryActions';
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
  movieHistoryMarkAsFailed
};

class MovieHistoryTableContentConnector extends Component {

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
      <MovieHistoryTableContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

MovieHistoryTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  movieHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieHistoryTableContentConnector);
