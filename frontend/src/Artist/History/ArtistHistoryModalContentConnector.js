import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchArtistHistory, clearArtistHistory, artistHistoryMarkAsFailed } from 'Store/Actions/artistHistoryActions';
import ArtistHistoryModalContent from './ArtistHistoryModalContent';

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

class ArtistHistoryModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId,
      albumId
    } = this.props;

    this.props.fetchArtistHistory({
      artistId,
      albumId
    });
  }

  componentWillUnmount() {
    this.props.clearArtistHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      artistId,
      albumId
    } = this.props;

    this.props.artistHistoryMarkAsFailed({
      historyId,
      artistId,
      albumId
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistHistoryModalContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

ArtistHistoryModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number,
  fetchArtistHistory: PropTypes.func.isRequired,
  clearArtistHistory: PropTypes.func.isRequired,
  artistHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistHistoryModalContentConnector);
