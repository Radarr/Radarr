import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchArtistHistory, clearArtistHistory, artistHistoryMarkAsFailed } from 'Store/Actions/artistHistoryActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistHistory,
    (artistHistory) => {
      return artistHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchArtistHistory,
  clearArtistHistory,
  artistHistoryMarkAsFailed
};

class ArtistHistoryContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      authorId,
      bookId
    } = this.props;

    this.props.fetchArtistHistory({
      authorId,
      bookId
    });
  }

  componentWillUnmount() {
    this.props.clearArtistHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      authorId,
      bookId
    } = this.props;

    this.props.artistHistoryMarkAsFailed({
      historyId,
      authorId,
      bookId
    });
  }

  //
  // Render

  render() {
    const {
      component: ViewComponent,
      ...otherProps
    } = this.props;

    return (
      <ViewComponent
        {...otherProps}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

ArtistHistoryContentConnector.propTypes = {
  component: PropTypes.elementType.isRequired,
  authorId: PropTypes.number.isRequired,
  bookId: PropTypes.number,
  fetchArtistHistory: PropTypes.func.isRequired,
  clearArtistHistory: PropTypes.func.isRequired,
  artistHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistHistoryContentConnector);
