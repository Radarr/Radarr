import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearReleases } from 'Store/Actions/releaseActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import episodeEntities from 'Episode/episodeEntities';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import { fetchEpisodeFiles, clearEpisodeFiles } from 'Store/Actions/episodeFileActions';
import EpisodeDetailsModalContent from './EpisodeDetailsModalContent';

function createMapStateToProps() {
  return createSelector(
    createEpisodeSelector(),
    createArtistSelector(),
    (episode, series) => {
      const {
        artistName,
        nameSlug,
        monitored: artistMonitored,
        seriesType
      } = series;

      return {
        artistName,
        nameSlug,
        artistMonitored,
        seriesType,
        ...episode
      };
    }
  );
}

const mapDispatchToProps = {
  clearReleases,
  fetchTracks,
  clearTracks,
  fetchEpisodeFiles,
  clearEpisodeFiles,
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
    const albumId = this.props.episodeId;
    this.props.fetchTracks({ artistId, albumId });
    // this.props.fetchEpisodeFiles({ artistId, albumId });
  }

  _unpopulate() {
    this.props.clearTracks();
    // this.props.clearEpisodeFiles();
  }


  //
  // Listeners

  onMonitorAlbumPress = (monitored) => {
    const {
      episodeId,
      episodeEntity
    } = this.props;

    this.props.toggleEpisodeMonitored({
      episodeEntity,
      episodeId,
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
  episodeId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  artistId: PropTypes.number.isRequired,
  fetchTracks: PropTypes.func.isRequired,
  clearTracks: PropTypes.func.isRequired,
  fetchEpisodeFiles: PropTypes.func.isRequired,
  clearEpisodeFiles: PropTypes.func.isRequired,
  clearReleases: PropTypes.func.isRequired,
  toggleEpisodeMonitored: PropTypes.func.isRequired
};

EpisodeDetailsModalContentConnector.defaultProps = {
  episodeEntity: episodeEntities.EPISODES
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeDetailsModalContentConnector);
