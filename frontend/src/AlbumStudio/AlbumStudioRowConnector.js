import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { toggleSeriesMonitored, toggleSeasonMonitored } from 'Store/Actions/artistActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import AlbumStudioRow from './AlbumStudioRow';

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    (series) => {
      return _.pick(series, [
        'status',
        'nameSlug',
        'artistName',
        'monitored',
        'albums',
        'isSaving'
      ]);
    }
  );
}

const mapDispatchToProps = {
  toggleSeriesMonitored,
  toggleSeasonMonitored,
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

    this.props.toggleSeriesMonitored({
      artistId,
      monitored: !monitored
    });
  }

  onAlbumMonitoredPress = (episodeId, monitored) => {
    this.props.toggleEpisodeMonitored({
      episodeId,
      monitored: !monitored
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
  toggleSeriesMonitored: PropTypes.func.isRequired,
  toggleSeasonMonitored: PropTypes.func.isRequired,
  toggleEpisodeMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumStudioRowConnector);
