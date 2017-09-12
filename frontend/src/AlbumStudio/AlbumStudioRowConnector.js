import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { toggleSeriesMonitored, toggleSeasonMonitored } from 'Store/Actions/artistActions';
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
  toggleSeasonMonitored
};

class AlbumStudioRowConnector extends Component {

  //
  // Listeners

  onSeriesMonitoredPress = () => {
    const {
      artistId,
      monitored
    } = this.props;

    this.props.toggleSeriesMonitored({
      artistId,
      monitored: !monitored
    });
  }

  onSeasonMonitoredPress = (seasonNumber, monitored) => {
    this.props.toggleSeasonMonitored({
      artistId: this.props.artistId,
      seasonNumber,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <AlbumStudioRow
        {...this.props}
        onSeriesMonitoredPress={this.onSeriesMonitoredPress}
        onSeasonMonitoredPress={this.onSeasonMonitoredPress}
      />
    );
  }
}

AlbumStudioRowConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleSeriesMonitored: PropTypes.func.isRequired,
  toggleSeasonMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumStudioRowConnector);
