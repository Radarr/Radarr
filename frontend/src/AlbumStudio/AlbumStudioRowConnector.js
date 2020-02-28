import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { toggleArtistMonitored } from 'Store/Actions/artistActions';
import { toggleAlbumsMonitored } from 'Store/Actions/albumActions';
import AlbumStudioRow from './AlbumStudioRow';

// Use a const to share the reselect cache between instances
const getAlbumMap = createSelector(
  (state) => state.albums.items,
  (albums) => {
    return albums.reduce((acc, curr) => {
      (acc[curr.artistId] = acc[curr.artistId] || []).push(curr);
      return acc;
    }, {});
  }
);

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    getAlbumMap,
    (artist, albumMap) => {
      const albumsInArtist = albumMap.hasOwnProperty(artist.id) ? albumMap[artist.id] : [];
      const sortedAlbums = _.orderBy(albumsInArtist, 'releaseDate', 'desc');

      return {
        ...artist,
        artistId: artist.id,
        artistName: artist.artistName,
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
  toggleAlbumsMonitored
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
    const albumIds = [albumId];
    this.props.toggleAlbumsMonitored({
      albumIds,
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
  toggleAlbumsMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumStudioRowConnector);
