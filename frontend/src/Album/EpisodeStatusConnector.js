import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAlbumSelector from 'Store/Selectors/createAlbumSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import EpisodeStatus from './EpisodeStatus';

function createMapStateToProps() {
  return createSelector(
    createAlbumSelector(),
    createQueueItemSelector(),
    createTrackFileSelector(),
    (album, queueItem, trackFile) => {
      const result = _.pick(album, [
        'airDateUtc',
        'monitored',
        'grabbed'
      ]);

      result.queueItem = queueItem;
      result.trackFile = trackFile;

      return result;
    }
  );
}

const mapDispatchToProps = {
};

class EpisodeStatusConnector extends Component {

  //
  // Render

  render() {
    return (
      <EpisodeStatus
        {...this.props}
      />
    );
  }
}

EpisodeStatusConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  trackFileId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeStatusConnector);
