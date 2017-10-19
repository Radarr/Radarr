import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearReleases } from 'Store/Actions/releaseActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import episodeEntities from 'Album/episodeEntities';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import { fetchTrackFiles, clearTrackFiles } from 'Store/Actions/trackFileActions';
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

const mapDispatchToProps = {
  clearReleases,
  fetchTracks,
  clearTracks,
  fetchTrackFiles,
  clearTrackFiles,
  toggleEpisodeMonitored
};

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
    this.props.clearReleases();
  }

  //
  // Control

  _populate() {
    const artistId = this.props.artistId;
    const albumId = this.props.albumId;
    this.props.fetchTracks({ artistId, albumId });
    // this.props.fetchTrackFiles({ artistId, albumId });
  }

  _unpopulate() {
    this.props.clearTracks();
    // this.props.clearTrackFiles();
  }

  //
  // Listeners

  onMonitorAlbumPress = (monitored) => {
    const {
      albumId,
      episodeEntity
    } = this.props;

    this.props.toggleEpisodeMonitored({
      episodeEntity,
      albumId,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <EpisodeDetailsModalContent
        {...this.props}
        onMonitorAlbumPress={this.onMonitorAlbumPress}
      />
    );
  }
}

EpisodeDetailsModalContentConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  artistId: PropTypes.number.isRequired,
  fetchTracks: PropTypes.func.isRequired,
  clearTracks: PropTypes.func.isRequired,
  fetchTrackFiles: PropTypes.func.isRequired,
  clearTrackFiles: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  toggleEpisodeMonitored: PropTypes.func.isRequired
};

EpisodeDetailsModalContentConnector.defaultProps = {
  episodeEntity: episodeEntities.EPISODES
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeDetailsModalContentConnector);
