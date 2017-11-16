import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import episodeEntities from 'Album/episodeEntities';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import EpisodeDetailsModalContent from './EpisodeDetailsModalContent';

function createMapStateToProps() {
  return createSelector(
    createEpisodeSelector(),
    createArtistSelector(),
    (album, artist) => {
      const {
        artistName,
        nameSlug,
        monitored: artistMonitored
      } = artist;

      return {
        artistName,
        nameSlug,
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
        episodeEntity
      } = this.props;

      dispatch(toggleEpisodeMonitored({
        episodeEntity,
        albumId,
        monitored
      }));
    }
  };
}

class EpisodeDetailsModalContentConnector extends Component {

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
      <EpisodeDetailsModalContent {...otherProps} />
    );
  }
}

EpisodeDetailsModalContentConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  artistId: PropTypes.number.isRequired,
  dispatchFetchTracks: PropTypes.func.isRequired,
  dispatchClearTracks: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired
};

EpisodeDetailsModalContentConnector.defaultProps = {
  episodeEntity: episodeEntities.EPISODES
};

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeDetailsModalContentConnector);
