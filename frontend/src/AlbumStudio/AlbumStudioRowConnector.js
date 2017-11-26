import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { toggleArtistMonitored } from 'Store/Actions/artistActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import AlbumStudioRow from './AlbumStudioRow';

function createMapStateToProps() {
  return createSelector(
    (state) => state.episodes,
    createArtistSelector(),
    (episodes, artist) => {
      const albumsInArtist = _.filter(episodes.items, { artistId: artist.id });
      const sortedAlbums = _.orderBy(albumsInArtist, 'releaseDate', 'desc');

      return {
        ...artist,
        artistId: artist.id,
        artistName: artist.artistName,
        nameSlug: artist.nameSlug,
        monitored: artist.monitored,
        status: artist.status,
        isSaving: artist.isSaving,
        albums: sortedAlbums
      };
    }
  );
}

const mapDispatchToProps = {
  toggleArtistMonitored,
  toggleEpisodeMonitored
};

class AlbumStudioRowConnector extends Component {

  //
  // Listeners

  onArtistMonitoredPress = () => {
    const {
      artistId,
      monitored
    } = this.props;

    this.props.toggleArtistMonitored({
      artistId,
      monitored: !monitored
    });
  }

  onAlbumMonitoredPress = (albumId, monitored) => {
    this.props.toggleEpisodeMonitored({
      albumId,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <AlbumStudioRow
        {...this.props}
        onArtistMonitoredPress={this.onArtistMonitoredPress}
        onAlbumMonitoredPress={this.onAlbumMonitoredPress}
      />
    );
  }
}

AlbumStudioRowConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleArtistMonitored: PropTypes.func.isRequired,
  toggleEpisodeMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumStudioRowConnector);
