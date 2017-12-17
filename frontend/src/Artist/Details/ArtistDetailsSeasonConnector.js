/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { toggleEpisodesMonitored, setEpisodesTableOption } from 'Store/Actions/episodeActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import ArtistDetailsSeason from './ArtistDetailsSeason';

function createMapStateToProps() {
  return createSelector(
    (state, { label }) => label,
    (state) => state.episodes,
    createArtistSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (label, episodes, artist, commands, dimensions) => {

      const episodesInSeason = _.filter(episodes.items, { albumType: label });
      const sortedEpisodes = _.orderBy(episodesInSeason, 'releaseDate', 'desc');

      return {
        items: sortedEpisodes,
        columns: episodes.columns,
        artistMonitored: artist.monitored,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  toggleEpisodesMonitored,
  setEpisodesTableOption,
  executeCommand
};

class ArtistDetailsSeasonConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setEpisodesTableOption(payload);
  }

  onMonitorAlbumPress = (albumIds, monitored) => {
    this.props.toggleEpisodesMonitored({
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
        onTableOptionChange={this.onTableOptionChange}
        onMonitorAlbumPress={this.onMonitorAlbumPress}
      />
    );
  }
}

ArtistDetailsSeasonConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  toggleEpisodesMonitored: PropTypes.func.isRequired,
  setEpisodesTableOption: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsSeasonConnector);
