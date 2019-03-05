/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { toggleAlbumsMonitored, setAlbumsTableOption, setAlbumsSort } from 'Store/Actions/albumActions';
import { executeCommand } from 'Store/Actions/commandActions';
import ArtistDetailsSeason from './ArtistDetailsSeason';

function createMapStateToProps() {
  return createSelector(
    (state, { label }) => label,
    createClientSideCollectionSelector('albums'),
    createArtistSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    createUISettingsSelector(),
    (label, albums, artist, commands, dimensions, uiSettings) => {

      const albumsInGroup = _.filter(albums.items, { albumType: label });

      let sortDir = 'asc';

      if (albums.sortDirection === 'descending') {
        sortDir = 'desc';
      }

      const sortedAlbums = _.orderBy(albumsInGroup, albums.sortKey, sortDir);

      return {
        items: sortedAlbums,
        columns: albums.columns,
        sortKey: albums.sortKey,
        sortDirection: albums.sortDirection,
        artistMonitored: artist.monitored,
        isSmallScreen: dimensions.isSmallScreen,
        uiSettings
      };
    }
  );
}

const mapDispatchToProps = {
  toggleAlbumsMonitored,
  setAlbumsTableOption,
  dispatchSetAlbumSort: setAlbumsSort,
  executeCommand
};

class ArtistDetailsSeasonConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setAlbumsTableOption(payload);
  }

  onSortPress = (sortKey) => {
    this.props.dispatchSetAlbumSort({ sortKey });
  }

  onMonitorAlbumPress = (albumIds, monitored) => {
    this.props.toggleAlbumsMonitored({
      albumIds,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <ArtistDetailsSeason
        {...this.props}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
        onMonitorAlbumPress={this.onMonitorAlbumPress}
      />
    );
  }
}

ArtistDetailsSeasonConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  toggleAlbumsMonitored: PropTypes.func.isRequired,
  setAlbumsTableOption: PropTypes.func.isRequired,
  dispatchSetAlbumSort: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsSeasonConnector);
