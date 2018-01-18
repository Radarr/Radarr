import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { toggleAlbumMonitored } from 'Store/Actions/albumActions';
import createAlbumSelector from 'Store/Selectors/createAlbumSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import albumEntities from 'Album/albumEntities';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import AlbumDetailsModalContent from './AlbumDetailsModalContent';

function createMapStateToProps() {
  return createSelector(
    createAlbumSelector(),
    createArtistSelector(),
    (album, artist) => {
      const {
        artistName,
        foreignArtistId,
        monitored: artistMonitored
      } = artist;

      return {
        artistName,
        foreignArtistId,
        artistMonitored,
        ...album
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },

    dispatchClearReleases() {
      dispatch(clearReleases());
    },

    dispatchFetchTracks({ artistId, albumId }) {
      dispatch(fetchTracks({ artistId, albumId }));
    },

    dispatchClearTracks() {
      dispatch(clearTracks());
    },

    onMonitorAlbumPress(monitored) {
      const {
        albumId,
        albumEntity
      } = this.props;

      dispatch(toggleAlbumMonitored({
        albumEntity,
        albumId,
        monitored
      }));
    }
  };
}

class AlbumDetailsModalContentConnector extends Component {

  //
  // Lifecycle
  componentDidMount() {
    this._populate();
  }

  componentWillUnmount() {
    // Clear pending releases here so we can reshow the search
    // results even after switching tabs.
    this._unpopulate();
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearReleases();
  }

  //
  // Control

  _populate() {
    const artistId = this.props.artistId;
    const albumId = this.props.albumId;
    this.props.dispatchFetchTracks({ artistId, albumId });
  }

  _unpopulate() {
    this.props.dispatchClearTracks();
  }

  //
  // Render

  render() {
    const {
      dispatchClearReleases,
      ...otherProps
    } = this.props;

    return (
      <AlbumDetailsModalContent {...otherProps} />
    );
  }
}

AlbumDetailsModalContentConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  albumEntity: PropTypes.string.isRequired,
  artistId: PropTypes.number.isRequired,
  dispatchFetchTracks: PropTypes.func.isRequired,
  dispatchClearTracks: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired
};

AlbumDetailsModalContentConnector.defaultProps = {
  albumEntity: albumEntities.ALBUMS
};

export default connect(createMapStateToProps, createMapDispatchToProps)(AlbumDetailsModalContentConnector);
