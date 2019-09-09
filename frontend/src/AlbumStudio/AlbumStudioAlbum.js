import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import styles from './AlbumStudioAlbum.css';

class AlbumStudioAlbum extends Component {

  //
  // Listeners

  onAlbumMonitoredPress = () => {
    const {
      id,
      monitored
    } = this.props;

    this.props.onAlbumMonitoredPress(id, !monitored);
  }

  //
  // Render

  render() {
    const {
      title,
      disambiguation,
      albumType,
      monitored,
      statistics,
      isSaving
    } = this.props;

    const {
      trackFileCount,
      totalTrackCount,
      percentOfTracks
    } = statistics;

    return (
      <div className={styles.album}>
        <div className={styles.info}>
          <MonitorToggleButton
            monitored={monitored}
            isSaving={isSaving}
            onPress={this.onAlbumMonitoredPress}
          />

          <span>
            {
              disambiguation ? `${title} (${disambiguation})` : `${title}`
            }
          </span>
        </div>

        <div className={styles.albumType}>
          <span>
            {
              `${albumType}`
            }
          </span>
        </div>

        <div
          className={classNames(
            styles.tracks,
            percentOfTracks < 100 && monitored && styles.missingWanted,
            percentOfTracks === 100 && styles.allTracks
          )}
          title={`${trackFileCount}/${totalTrackCount} tracks downloaded`}
        >
          {
            totalTrackCount === 0 ? '0/0' : `${trackFileCount}/${totalTrackCount}`
          }
        </div>
      </div>
    );
  }
}

AlbumStudioAlbum.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  albumType: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  onAlbumMonitoredPress: PropTypes.func.isRequired
};

AlbumStudioAlbum.defaultProps = {
  isSaving: false,
  statistics: {
    trackFileCount: 0,
    totalTrackCount: 0,
    percentOfTracks: 0
  }
};

export default AlbumStudioAlbum;
