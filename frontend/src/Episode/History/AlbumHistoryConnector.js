import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchAlbumHistory, clearAlbumHistory, albumHistoryMarkAsFailed } from 'Store/Actions/albumHistoryActions';
import AlbumHistory from './AlbumHistory';

function createMapStateToProps() {
  return createSelector(
    (state) => state.albumHistory,
    (albumHistory) => {
      return albumHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchAlbumHistory,
  clearAlbumHistory,
  albumHistoryMarkAsFailed
};

class AlbumHistoryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchAlbumHistory({ albumId: this.props.albumId });
  }

  componentWillUnmount() {
    this.props.clearAlbumHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    this.props.albumHistoryMarkAsFailed({ historyId, albumId: this.props.albumId });
  }

  //
  // Render

  render() {
    return (
      <AlbumHistory
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

AlbumHistoryConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  fetchAlbumHistory: PropTypes.func.isRequired,
  clearAlbumHistory: PropTypes.func.isRequired,
  albumHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumHistoryConnector);
