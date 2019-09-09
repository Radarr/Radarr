import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { setTracksTableOption } from 'Store/Actions/trackActions';
import { executeCommand } from 'Store/Actions/commandActions';
import AlbumDetailsMedium from './AlbumDetailsMedium';

function createMapStateToProps() {
  return createSelector(
    (state, { mediumNumber }) => mediumNumber,
    (state) => state.tracks,
    createDimensionsSelector(),
    (mediumNumber, tracks, dimensions) => {

      const tracksInMedium = _.filter(tracks.items, { mediumNumber });
      const sortedTracks = _.orderBy(tracksInMedium, ['absoluteTrackNumber'], ['asc']);

      return {
        items: sortedTracks,
        columns: tracks.columns,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  setTracksTableOption,
  executeCommand
};

class AlbumDetailsMediumConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setTracksTableOption(payload);
  }

  //
  // Render

  render() {
    return (
      <AlbumDetailsMedium
        {...this.props}
        onTableOptionChange={this.onTableOptionChange}
      />
    );
  }
}

AlbumDetailsMediumConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  albumMonitored: PropTypes.bool.isRequired,
  mediumNumber: PropTypes.number.isRequired,
  setTracksTableOption: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumDetailsMediumConnector);
